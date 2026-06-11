"""Settings endpoints for YouTube API keys."""

from datetime import datetime

from fastapi import APIRouter, Depends
from sqlalchemy import delete, select
from sqlalchemy.ext.asyncio import AsyncSession

from app.database import get_db
from app.models.api_key import ApiKey
from app.models.user import User
from app.schemas.settings import ApiKeyCheckResponse, ApiKeyCheckResult, ApiKeyRequest, ApiKeyResponse
from app.services.auth import get_admin_user
from app.services.youtube_client import check_api_key

router = APIRouter(prefix="/settings")


def _clean_keys(raw: str) -> list[str]:
    return list(dict.fromkeys(line.strip() for line in (raw or "").splitlines() if line.strip()))


def _preview(key: str) -> str:
    if len(key) <= 10:
        return key
    return f"{key[:6]}...{key[-4:]}"


@router.get("/api-keys", response_model=ApiKeyResponse)
async def get_api_keys(
    actor: User = Depends(get_admin_user),
    db: AsyncSession = Depends(get_db),
):
    result = await db.execute(select(ApiKey).order_by(ApiKey.created_at))
    keys = [row.key for row in result.scalars().all()]
    return ApiKeyResponse(api_keys="\n".join(keys))


@router.put("/api-keys", response_model=ApiKeyResponse)
async def save_api_keys(
    req: ApiKeyRequest,
    actor: User = Depends(get_admin_user),
    db: AsyncSession = Depends(get_db),
):
    keys = _clean_keys(req.api_keys)
    await db.execute(delete(ApiKey))
    for idx, key in enumerate(keys, start=1):
        db.add(ApiKey(key=key, label=f"Key {idx}", is_active=True))
    await db.flush()
    return ApiKeyResponse(api_keys="\n".join(keys))


@router.post("/api-keys/check", response_model=ApiKeyCheckResponse)
async def check_keys(
    actor: User = Depends(get_admin_user),
    db: AsyncSession = Depends(get_db),
):
    result = await db.execute(select(ApiKey).order_by(ApiKey.created_at))
    rows = list(result.scalars().all())
    output: list[ApiKeyCheckResult] = []
    for row in rows:
        ok, error = await check_api_key(row.key)
        row.last_checked_at = datetime.utcnow()
        row.last_status = "ok" if ok else "error"
        row.last_error = error
        output.append(ApiKeyCheckResult(key_preview=_preview(row.key), ok=ok, error=error))
    return ApiKeyCheckResponse(results=output)
