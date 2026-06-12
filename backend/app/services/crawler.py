"""Crawler service - refreshes YouTube channel rows."""

import asyncio
import logging
from datetime import datetime

from sqlalchemy import select

from app.config import settings
from app.database import async_session
from app.models.crawl_job import CrawlJob
from app.models.item import Item
from app.models.metric_snapshot import MetricSnapshot
from app.services.youtube_client import YouTubeApiError, fetch_channel, get_active_api_keys
from app.utils.youtube_urls import ParsedYouTubeChannel

logger = logging.getLogger(__name__)
CRAWL_TASK_SEMAPHORE = asyncio.Semaphore(max(1, settings.CRAWL_TASK_MAX_CONCURRENCY))


def _delta_days(previous: datetime | None, current: datetime) -> int | None:
    if previous is None:
        return None
    return max(1, (current.date() - previous.date()).days)


async def _load_delta_baseline(db, item: Item, checked_at: datetime) -> tuple[int | None, datetime | None]:
    start_of_day = datetime.combine(checked_at.date(), datetime.min.time())
    previous_day_result = await db.execute(
        select(MetricSnapshot)
        .where(
            MetricSnapshot.item_id == item.id,
            MetricSnapshot.view_count.is_not(None),
            MetricSnapshot.checked_at < start_of_day,
        )
        .order_by(MetricSnapshot.checked_at.desc())
        .limit(1)
    )
    previous_day_snapshot = previous_day_result.scalar_one_or_none()
    if previous_day_snapshot is not None:
        return previous_day_snapshot.view_count, previous_day_snapshot.checked_at

    previous_result = await db.execute(
        select(MetricSnapshot)
        .where(
            MetricSnapshot.item_id == item.id,
            MetricSnapshot.view_count.is_not(None),
            MetricSnapshot.checked_at < checked_at,
        )
        .order_by(MetricSnapshot.checked_at.desc())
        .limit(1)
    )
    previous_snapshot = previous_result.scalar_one_or_none()
    if previous_snapshot is not None:
        return previous_snapshot.view_count, previous_snapshot.checked_at

    return item.view_count, item.last_checked


async def crawl_item_task(job_id: str, parsed: ParsedYouTubeChannel):
    async with CRAWL_TASK_SEMAPHORE:
        async with async_session() as db:
            job = None
            try:
                result = await db.execute(select(CrawlJob).where(CrawlJob.id == job_id))
                job = result.scalar_one_or_none()
                if not job:
                    logger.error("Job %s not found", job_id)
                    return

                job.status = "crawling"
                job.started_at = datetime.utcnow()
                await db.commit()

                item_result = await db.execute(select(Item).where(Item.id == job.item_id))
                item = item_result.scalar_one_or_none()
                if not item:
                    raise YouTubeApiError("Không tìm thấy channel row")

                api_keys = await get_active_api_keys(db)
                if not api_keys:
                    raise YouTubeApiError("Chưa có YouTube API key")

                data = None
                last_error: Exception | None = None
                for api_key in api_keys:
                    try:
                        data = await fetch_channel(parsed, api_key.key)
                        api_key.last_status = "ok"
                        api_key.last_error = None
                        api_key.last_checked_at = datetime.utcnow()
                        break
                    except Exception as exc:
                        last_error = exc
                        api_key.last_status = "error"
                        api_key.last_error = str(exc)
                        api_key.last_checked_at = datetime.utcnow()

                if data is None:
                    raise YouTubeApiError(str(last_error) if last_error else "Không gọi được YouTube API")

                checked_at = data["checked_at"]
                baseline_view_count, baseline_checked = await _load_delta_baseline(db, item, checked_at)

                item.youtube_id = data["youtube_id"] or item.youtube_id
                item.name = data["name"] or item.name
                item.image = data["image"] or item.image
                item.banner_image = data["banner_image"] or item.banner_image
                item.video_count = data["video_count"]
                item.subscriber_count = data["subscriber_count"]
                item.view_count = data["view_count"]
                item.last_checked = checked_at
                item.status = "active"
                item.error_code = None
                item.error_message = None

                if baseline_view_count is not None and data["view_count"] is not None:
                    item.view_count_delta = data["view_count"] - baseline_view_count
                    item.delta_days = _delta_days(baseline_checked, checked_at)
                else:
                    item.view_count_delta = None
                    item.delta_days = None

                db.add(MetricSnapshot(item_id=item.id, view_count=data["view_count"], checked_at=checked_at))

                job.status = "completed"
                job.completed_at = datetime.utcnow()
                job.result = {
                    "id": item.id,
                    "youtube_id": item.youtube_id,
                    "type": "channel",
                    "query": item.query,
                    "query_type": item.query_type,
                    "name": item.name,
                    "image": item.image,
                    "banner_image": item.banner_image,
                    "video_count": item.video_count,
                    "subscriber_count": item.subscriber_count,
                    "view_count": item.view_count,
                    "view_count_delta": item.view_count_delta,
                    "delta_days": item.delta_days,
                    "status": item.status,
                    "group": item.group,
                    "user_id": item.user_id,
                    "last_checked": item.last_checked.isoformat() if item.last_checked else None,
                    "created_at": item.created_at.isoformat() if item.created_at else None,
                }
                await db.commit()
            except Exception as exc:
                logger.warning("YouTube crawl failed job=%s error=%s", job_id, exc)
                if job is not None:
                    item = None
                    if job.item_id:
                        item_result = await db.execute(select(Item).where(Item.id == job.item_id))
                        item = item_result.scalar_one_or_none()
                    if item is not None:
                        item.status = "error"
                        item.error_message = str(exc)
                        item.last_checked = datetime.utcnow()
                    job.status = "error"
                    job.error = str(exc)
                    job.completed_at = datetime.utcnow()
                    await db.commit()
