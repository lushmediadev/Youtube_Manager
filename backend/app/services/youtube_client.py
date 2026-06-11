"""Small YouTube Data API v3 client."""

from __future__ import annotations

import random
import string
from datetime import datetime

import httpx
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.config import settings
from app.models.api_key import ApiKey
from app.utils.youtube_urls import ParsedYouTubeChannel


YOUTUBE_API_BASE = "https://www.googleapis.com/youtube/v3"


class YouTubeApiError(RuntimeError):
    pass


def _to_int(value) -> int | None:
    if value is None:
        return None
    try:
        return int(value)
    except (TypeError, ValueError):
        return None


def _youtube_banner_url(value: str | None) -> str | None:
    if not value:
        return None
    url = value.strip()
    if not url.startswith("http"):
        return None
    if "=w" in url or "-fcrop64=" in url:
        return url
    return f"{url}=w1707-fcrop64=1,00005a57ffffa5a8-k-c0xffffffff-no-nd-rj"


async def get_active_api_keys(db: AsyncSession) -> list[ApiKey]:
    result = await db.execute(select(ApiKey).where(ApiKey.is_active.is_(True)).order_by(ApiKey.created_at))
    return list(result.scalars().all())


async def get_random_api_key(db: AsyncSession) -> ApiKey:
    keys = await get_active_api_keys(db)
    if not keys:
        raise YouTubeApiError("Chưa có YouTube API key")
    return random.choice(keys)


async def _request(path: str, params: dict, api_key: str) -> dict:
    request_params = dict(params)
    request_params["key"] = api_key
    async with httpx.AsyncClient(timeout=settings.YOUTUBE_HTTP_TIMEOUT_SECONDS) as client:
        res = await client.get(f"{YOUTUBE_API_BASE}/{path}", params=request_params)
    if res.status_code != 200:
        detail = res.text[:500]
        try:
            payload = res.json()
            detail = payload.get("error", {}).get("message") or detail
        except ValueError:
            pass
        raise YouTubeApiError(detail)
    return res.json()


async def check_api_key(api_key: str) -> tuple[bool, str | None]:
    query = "".join(random.choice(string.ascii_letters) for _ in range(5))
    try:
        await _request(
            "search",
            {"part": "snippet", "q": query, "maxResults": 1, "type": "channel"},
            api_key,
        )
        return True, None
    except Exception as exc:
        return False, str(exc)


async def fetch_channel(parsed: ParsedYouTubeChannel, api_key: str) -> dict:
    params: dict[str, str | int] = {
        "part": "snippet,statistics,brandingSettings",
        "maxResults": 1,
    }
    if parsed.query_type == "id":
        params["id"] = parsed.query
    elif parsed.query_type == "handle":
        params["forHandle"] = parsed.query if parsed.query.startswith("@") else f"@{parsed.query}"
    elif parsed.query_type == "username":
        params["forUsername"] = parsed.query
    else:
        search = await _request(
            "search",
            {"part": "snippet", "q": parsed.query, "maxResults": 1, "type": "channel"},
            api_key,
        )
        items = search.get("items") or []
        channel_id = None
        if items:
            channel_id = ((items[0].get("id") or {}).get("channelId"))
        if not channel_id:
            raise YouTubeApiError("Không tìm thấy kênh từ URL này")
        params["id"] = channel_id

    payload = await _request("channels", params, api_key)
    items = payload.get("items") or []
    if not items:
        raise YouTubeApiError("Không tìm thấy kênh")

    item = items[0]
    snippet = item.get("snippet") or {}
    stats = item.get("statistics") or {}
    branding = item.get("brandingSettings") or {}
    branding_image = branding.get("image") or {}
    thumbnails = snippet.get("thumbnails") or {}
    thumb = thumbnails.get("default") or thumbnails.get("medium") or thumbnails.get("high") or {}

    return {
        "youtube_id": item.get("id"),
        "name": snippet.get("title"),
        "image": thumb.get("url"),
        "banner_image": _youtube_banner_url(branding_image.get("bannerExternalUrl")),
        "video_count": _to_int(stats.get("videoCount")),
        "subscriber_count": _to_int(stats.get("subscriberCount")),
        "view_count": _to_int(stats.get("viewCount")),
        "raw": payload,
        "checked_at": datetime.utcnow(),
    }
