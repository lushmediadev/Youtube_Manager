"""YouTube Manager Backend - FastAPI application entry point."""

import logging
from contextlib import asynccontextmanager
from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles

from app.api.router import router as api_router
from app.config import settings
from app.database import init_db
from app.services.youtube_client import close_youtube_http_client

logging.basicConfig(
    level=logging.DEBUG if settings.DEBUG else logging.INFO,
    format="%(asctime)s | %(levelname)-7s | %(name)s | %(message)s",
    datefmt="%H:%M:%S",
)
logger = logging.getLogger("youtube-manager")


def _resolve_frontend_dir() -> Path | None:
    candidates: list[Path] = []
    configured = (settings.FRONTEND_DIR or "").strip()
    if configured:
        configured_path = Path(configured)
        if not configured_path.is_absolute():
            configured_path = Path(__file__).resolve().parents[1] / configured_path
        candidates.append(configured_path)
    backend_dir = Path(__file__).resolve().parents[1]
    candidates.append(backend_dir.parent / "frontend")

    seen: set[Path] = set()
    for candidate in candidates:
        normalized = candidate.resolve()
        if normalized in seen:
            continue
        seen.add(normalized)
        if (normalized / "index.html").exists():
            return normalized
    return None


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("Starting YouTube Manager API...")
    await init_db()
    yield
    await close_youtube_http_client()
    logger.info("Shutting down YouTube Manager API...")


app = FastAPI(
    title="YouTube Manager API",
    description="YouTube channel monitoring backend",
    version="0.1.0",
    lifespan=lifespan,
)

allowed_origins = [
    settings.FRONTEND_URL,
    "http://localhost:8080",
    "http://localhost:3000",
    "http://127.0.0.1:8080",
    "http://127.0.0.1:8010",
]
allowed_origins = [origin for origin in dict.fromkeys(allowed_origins) if origin]

app.add_middleware(
    CORSMiddleware,
    allow_origins=allowed_origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(api_router)

frontend_dir = _resolve_frontend_dir()
if settings.SERVE_FRONTEND and frontend_dir:
    app.mount("/", StaticFiles(directory=str(frontend_dir), html=True), name="frontend")
    logger.info("Serving frontend from: %s", frontend_dir)
else:
    logger.info("Frontend static serving disabled or directory not found")
