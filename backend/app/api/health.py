"""Health check endpoint."""

from fastapi import APIRouter

from app.config import settings

router = APIRouter()


@router.get("/health")
async def health():
    return {
        "status": "ok",
        "service": "youtube-manager-api",
        "version": "0.1.0",
        "debug": settings.DEBUG,
    }
