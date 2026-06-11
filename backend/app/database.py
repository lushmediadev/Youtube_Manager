"""SQLAlchemy async engine and session factory."""

from sqlalchemy import inspect, text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine
from sqlalchemy.orm import DeclarativeBase

from app.config import settings


def _normalize_database_url(raw_url: str) -> str:
    url = (raw_url or "").strip()
    if not url:
        return url
    if url.startswith("postgres://"):
        url = url.replace("postgres://", "postgresql://", 1)
    if url.startswith("postgresql+asyncpg://"):
        return url
    if url.startswith("postgresql://"):
        return url.replace("postgresql://", "postgresql+asyncpg://", 1)
    return url


DATABASE_URL = _normalize_database_url(settings.DATABASE_URL)
engine_kwargs = {"echo": settings.DEBUG, "pool_pre_ping": True}
if DATABASE_URL.startswith("sqlite"):
    engine_kwargs = {"echo": settings.DEBUG}

engine = create_async_engine(DATABASE_URL, **engine_kwargs)
async_session = async_sessionmaker(engine, class_=AsyncSession, expire_on_commit=False)


class Base(DeclarativeBase):
    pass


async def get_db() -> AsyncSession:
    async with async_session() as session:
        try:
            yield session
            await session.commit()
        except Exception:
            await session.rollback()
            raise


async def init_db():
    # Import models so metadata is populated before create_all.
    import app.models  # noqa: F401

    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
        await conn.run_sync(_ensure_runtime_columns)


def _ensure_runtime_columns(sync_conn):
    inspector = inspect(sync_conn)
    if not inspector.has_table("items"):
        return
    existing = {column["name"] for column in inspector.get_columns("items")}
    for name, column_type in {
        "youtube_id": "VARCHAR(64)",
        "query": "VARCHAR(512)",
        "query_type": "VARCHAR(24)",
        "video_count": "INTEGER",
        "subscriber_count": "BIGINT",
        "view_count": "BIGINT",
        "view_count_delta": "BIGINT",
        "delta_days": "INTEGER",
    }.items():
        if name not in existing:
            sync_conn.execute(text(f"ALTER TABLE items ADD COLUMN {name} {column_type}"))
            existing.add(name)
    if "banner_image" not in existing:
        sync_conn.execute(text("ALTER TABLE items ADD COLUMN banner_image TEXT"))
        existing.add("banner_image")

    if "spotify_id" in existing:
        sync_conn.execute(text("UPDATE items SET youtube_id = spotify_id WHERE youtube_id IS NULL AND spotify_id IS NOT NULL"))
        sync_conn.execute(text("UPDATE items SET query = COALESCE(spotify_id, name, CAST(id AS TEXT)) WHERE query IS NULL"))
    else:
        sync_conn.execute(text("UPDATE items SET query = COALESCE(name, CAST(id AS TEXT)) WHERE query IS NULL"))
    sync_conn.execute(text("UPDATE items SET query_type = CASE WHEN query LIKE 'UC%' THEN 'id' ELSE 'url' END WHERE query_type IS NULL"))

    if "track_count" in existing:
        sync_conn.execute(text("UPDATE items SET video_count = track_count WHERE video_count IS NULL AND track_count IS NOT NULL"))
    if "followers" in existing:
        sync_conn.execute(text("UPDATE items SET subscriber_count = followers WHERE subscriber_count IS NULL AND followers IS NOT NULL"))
    if "playcount" in existing:
        sync_conn.execute(text("UPDATE items SET view_count = playcount WHERE view_count IS NULL AND playcount IS NOT NULL"))
