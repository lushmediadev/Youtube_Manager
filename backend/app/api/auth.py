"""Auth endpoints - register, login, profile, user management."""

import json
from datetime import datetime

from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy import delete, func, or_, select
from sqlalchemy.ext.asyncio import AsyncSession

from app.database import get_db
from app.models.crawl_job import CrawlJob
from app.models.item import Item
from app.models.user import User
from app.schemas.auth import (
    AdminCreateUserRequest,
    AdminResetPasswordRequest,
    AdminUpdateUserRequest,
    AuthResponse,
    ChangePasswordRequest,
    LoginRequest,
    RegisterRequest,
    UpdateAvatarRequest,
    UserResponse,
)
from app.services.auth import (
    can_manage_user,
    create_access_token,
    get_current_user,
    get_manager_or_admin_user,
    hash_password,
    verify_password,
)

router = APIRouter(prefix="/auth")
_INTERNAL_EMAIL_DOMAIN = "users.youtube-manager.local"
_VALID_ROLES = {"admin", "manager", "user"}


def _build_internal_email(username: str) -> str:
    return f"{username.lower()}@{_INTERNAL_EMAIL_DOMAIN}"


def _resolve_email(username: str, email: str | None) -> str:
    normalized = (email or "").strip().lower()
    return normalized or _build_internal_email(username)


def _public_email(email: str | None) -> str | None:
    normalized = (email or "").strip().lower()
    if not normalized or normalized.endswith("@" + _INTERNAL_EMAIL_DOMAIN):
        return None
    return email


async def _manager_name(db: AsyncSession, manager_id: str | None) -> str | None:
    if not manager_id:
        return None
    result = await db.execute(select(User).where(User.id == manager_id))
    manager = result.scalar_one_or_none()
    return manager.username if manager else None


async def _user_response(user: User, db: AsyncSession) -> UserResponse:
    try:
        custom_groups = json.loads(user.custom_groups) if user.custom_groups else []
    except (json.JSONDecodeError, TypeError):
        custom_groups = []
    if not isinstance(custom_groups, list):
        custom_groups = []
    custom_groups = [str(g).strip() for g in custom_groups if str(g).strip()]

    return UserResponse(
        id=user.id,
        username=user.username,
        email=_public_email(user.email),
        role=user.role,
        manager_id=user.manager_id,
        manager_name=await _manager_name(db, user.manager_id),
        is_active=user.is_active,
        created_at=user.created_at.isoformat() if user.created_at else None,
        last_login=user.last_login.isoformat() if user.last_login else None,
        avatar=user.avatar,
        custom_groups=custom_groups,
    )


async def _scoped_users_query(actor: User):
    query = select(User)
    if actor.role == "manager":
        query = query.where(or_(User.id == actor.id, User.manager_id == actor.id))
    elif actor.role != "admin":
        query = query.where(User.id == actor.id)
    return query


async def _load_target_user(db: AsyncSession, actor: User, user_id: str) -> User:
    result = await db.execute(select(User).where(User.id == user_id))
    target = result.scalar_one_or_none()
    if not target:
        raise HTTPException(status_code=404, detail="User not found")
    if not can_manage_user(actor, target):
        raise HTTPException(status_code=403, detail="Not authorized for this user")
    return target


async def _normalize_role_and_manager(
    db: AsyncSession,
    actor: User,
    role: str,
    manager_id: str | None,
) -> tuple[str, str | None]:
    normalized_role = (role or "user").strip().lower()
    if normalized_role not in _VALID_ROLES:
        raise HTTPException(status_code=400, detail="Role must be admin, manager, or user")

    if actor.role == "manager":
        if normalized_role != "user":
            raise HTTPException(status_code=403, detail="Manager can only create user accounts")
        return "user", actor.id

    if normalized_role == "user":
        if manager_id:
            result = await db.execute(select(User).where(User.id == manager_id, User.role == "manager"))
            manager = result.scalar_one_or_none()
            if not manager:
                raise HTTPException(status_code=400, detail="manager_id must point to a manager")
            return normalized_role, manager.id
        return normalized_role, None
    return normalized_role, None


@router.post("/register", response_model=AuthResponse, status_code=201)
async def register(req: RegisterRequest, db: AsyncSession = Depends(get_db)):
    count_result = await db.execute(select(func.count()).select_from(User))
    if (count_result.scalar() or 0) > 0:
        raise HTTPException(status_code=403, detail="Public sign up is disabled")

    username = (req.username or "").strip()
    if not username:
        raise HTTPException(status_code=400, detail="Username is required")

    user = User(
        username=username,
        email=_resolve_email(username, req.email),
        password_hash=hash_password(req.password),
        role="admin",
    )
    db.add(user)
    await db.flush()

    token = create_access_token({"sub": user.id})
    return AuthResponse(access_token=token, user=await _user_response(user, db))


@router.post("/login", response_model=AuthResponse)
async def login(req: LoginRequest, db: AsyncSession = Depends(get_db)):
    result = await db.execute(
        select(User).where((User.username == req.username) | (User.email == req.username))
    )
    user = result.scalar_one_or_none()
    if not user or not verify_password(req.password, user.password_hash):
        raise HTTPException(status_code=401, detail="Invalid credentials")
    if not user.is_active:
        raise HTTPException(status_code=403, detail="Account is deactivated")

    user.last_login = datetime.utcnow()
    await db.flush()

    token = create_access_token({"sub": user.id})
    return AuthResponse(access_token=token, user=await _user_response(user, db))


@router.get("/me", response_model=UserResponse)
async def me(current_user: User = Depends(get_current_user), db: AsyncSession = Depends(get_db)):
    return await _user_response(current_user, db)


@router.get("/users", response_model=list[UserResponse])
async def list_users(
    actor: User = Depends(get_manager_or_admin_user),
    db: AsyncSession = Depends(get_db),
):
    query = await _scoped_users_query(actor)
    result = await db.execute(query.order_by(User.created_at))
    return [await _user_response(user, db) for user in result.scalars().all()]


@router.post("/users", response_model=UserResponse, status_code=201)
async def create_user(
    req: AdminCreateUserRequest,
    actor: User = Depends(get_manager_or_admin_user),
    db: AsyncSession = Depends(get_db),
):
    username = (req.username or "").strip()
    if not username:
        raise HTTPException(status_code=400, detail="Username is required")
    if len(req.password or "") < 4:
        raise HTTPException(status_code=400, detail="Password must be at least 4 characters")

    role, manager_id = await _normalize_role_and_manager(db, actor, req.role, req.manager_id)
    email = _resolve_email(username, req.email)
    existing = await db.execute(select(User).where((User.username == username) | (User.email == email)))
    if existing.scalar_one_or_none():
        raise HTTPException(status_code=409, detail="Username already registered")

    user = User(
        username=username,
        email=email,
        password_hash=hash_password(req.password),
        role=role,
        manager_id=manager_id,
        is_active=True,
    )
    db.add(user)
    await db.flush()
    return await _user_response(user, db)


@router.patch("/users/{user_id}", response_model=UserResponse)
async def update_user(
    user_id: str,
    req: AdminUpdateUserRequest,
    actor: User = Depends(get_manager_or_admin_user),
    db: AsyncSession = Depends(get_db),
):
    user = await _load_target_user(db, actor, user_id)
    if req.username is not None and req.username.strip():
        user.username = req.username.strip()
    if req.role is not None or req.manager_id is not None:
        role, manager_id = await _normalize_role_and_manager(
            db,
            actor,
            req.role or user.role,
            req.manager_id if req.manager_id is not None else user.manager_id,
        )
        user.role = role
        user.manager_id = manager_id
    if req.is_active is not None:
        if user.id == actor.id and not req.is_active:
            raise HTTPException(status_code=400, detail="Cannot deactivate yourself")
        user.is_active = bool(req.is_active)
    await db.flush()
    return await _user_response(user, db)


@router.delete("/users/{user_id}")
async def delete_user(
    user_id: str,
    actor: User = Depends(get_manager_or_admin_user),
    db: AsyncSession = Depends(get_db),
):
    user = await _load_target_user(db, actor, user_id)
    if user.id == actor.id:
        raise HTTPException(status_code=400, detail="Cannot delete yourself")
    item_ids = [row[0] for row in (await db.execute(select(Item.id).where(Item.user_id == user.id))).all()]
    if item_ids:
        await db.execute(delete(CrawlJob).where(CrawlJob.item_id.in_(item_ids)))
    await db.execute(delete(Item).where(Item.user_id == user.id))
    await db.execute(delete(CrawlJob).where(CrawlJob.user_id == user.id))
    await db.execute(delete(User).where(User.id == user.id))
    return {"ok": True}


@router.post("/users/{user_id}/reset-password")
async def reset_password(
    user_id: str,
    req: AdminResetPasswordRequest,
    actor: User = Depends(get_manager_or_admin_user),
    db: AsyncSession = Depends(get_db),
):
    if len(req.new_password or "") < 4:
        raise HTTPException(status_code=400, detail="Password must be at least 4 characters")
    user = await _load_target_user(db, actor, user_id)
    user.password_hash = hash_password(req.new_password)
    return {"ok": True}


@router.post("/me/change-password")
async def change_password(
    req: ChangePasswordRequest,
    current_user: User = Depends(get_current_user),
):
    if not verify_password(req.current_password, current_user.password_hash):
        raise HTTPException(status_code=400, detail="Current password is incorrect")
    if len(req.new_password or "") < 4:
        raise HTTPException(status_code=400, detail="Password must be at least 4 characters")
    current_user.password_hash = hash_password(req.new_password)
    return {"ok": True}


@router.put("/me/avatar", response_model=UserResponse)
async def update_avatar(
    req: UpdateAvatarRequest,
    current_user: User = Depends(get_current_user),
    db: AsyncSession = Depends(get_db),
):
    current_user.avatar = req.avatar
    await db.flush()
    return await _user_response(current_user, db)


@router.delete("/me/avatar", response_model=UserResponse)
async def delete_avatar(current_user: User = Depends(get_current_user), db: AsyncSession = Depends(get_db)):
    current_user.avatar = None
    await db.flush()
    return await _user_response(current_user, db)


def _sanitize_ui_preferences(raw: dict | None) -> dict:
    data = raw if isinstance(raw, dict) else {}
    return {
        "row_order": [str(v) for v in data.get("row_order", []) if str(v).strip()][:5000]
        if isinstance(data.get("row_order"), list)
        else [],
        "column_widths": data.get("column_widths", {}) if isinstance(data.get("column_widths"), dict) else {},
    }


@router.get("/me/preferences")
async def get_preferences(current_user: User = Depends(get_current_user)):
    try:
        preferences = json.loads(current_user.ui_preferences) if current_user.ui_preferences else {}
    except (json.JSONDecodeError, TypeError):
        preferences = {}
    return {"preferences": _sanitize_ui_preferences(preferences)}


@router.put("/me/preferences")
async def save_preferences(req: dict, current_user: User = Depends(get_current_user)):
    current_user.ui_preferences = json.dumps(_sanitize_ui_preferences(req.get("preferences")))
    return {"preferences": json.loads(current_user.ui_preferences)}


@router.get("/me/groups")
async def get_my_groups(current_user: User = Depends(get_current_user)):
    try:
        groups = json.loads(current_user.custom_groups) if current_user.custom_groups else []
    except (json.JSONDecodeError, TypeError):
        groups = []
    return {"groups": groups if isinstance(groups, list) else []}


@router.put("/me/groups")
async def save_my_groups(req: dict, current_user: User = Depends(get_current_user)):
    groups = req.get("groups", [])
    if not isinstance(groups, list):
        raise HTTPException(status_code=400, detail="groups must be an array")
    cleaned = list(dict.fromkeys(str(g).strip() for g in groups if str(g).strip()))
    current_user.custom_groups = json.dumps(cleaned)
    return {"groups": cleaned}


@router.get("/users/{user_id}/groups")
async def get_user_groups(
    user_id: str,
    actor: User = Depends(get_manager_or_admin_user),
    db: AsyncSession = Depends(get_db),
):
    user = await _load_target_user(db, actor, user_id)
    try:
        groups = json.loads(user.custom_groups) if user.custom_groups else []
    except (json.JSONDecodeError, TypeError):
        groups = []
    return {"groups": groups if isinstance(groups, list) else []}


@router.put("/users/{user_id}/groups")
async def save_user_groups(
    user_id: str,
    req: dict,
    actor: User = Depends(get_manager_or_admin_user),
    db: AsyncSession = Depends(get_db),
):
    user = await _load_target_user(db, actor, user_id)
    groups = req.get("groups", [])
    if not isinstance(groups, list):
        raise HTTPException(status_code=400, detail="groups must be an array")
    cleaned = list(dict.fromkeys(str(g).strip() for g in groups if str(g).strip()))
    user.custom_groups = json.dumps(cleaned)
    return {"groups": cleaned}
