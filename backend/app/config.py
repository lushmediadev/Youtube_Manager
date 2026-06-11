"""Application configuration via Pydantic Settings."""

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Central configuration - reads from .env or environment variables."""

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    DATABASE_URL: str = "sqlite+aiosqlite:///./youtube_manager.db"
    YOUTUBE_HTTP_TIMEOUT_SECONDS: float = 12.0

    CRAWL_TASK_MAX_CONCURRENCY: int = 8
    RATE_LIMIT_PER_MINUTE: int = 60

    FRONTEND_URL: str = "http://localhost:8080"
    SERVE_FRONTEND: bool = True
    FRONTEND_DIR: str = "../frontend"

    SECRET_KEY: str = "change-me-in-production"
    DEBUG: bool = False
    AUTO_INIT_DB: bool = True

    JWT_SECRET_KEY: str = "change-me-in-production-jwt"
    JWT_ALGORITHM: str = "HS256"
    JWT_EXPIRY_HOURS: int = 24


settings = Settings()
