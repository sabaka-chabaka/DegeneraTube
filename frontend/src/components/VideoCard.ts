import type { VideoDto } from '../api/videos';
import { formatViews, formatDuration, formatRelative, thumbnailUrl, avatarInitial } from '../utils';
import { navigate } from '../router';

export function VideoCard(video: VideoDto): HTMLElement {
    const el = document.createElement('div');
    el.className = 'video-card';

    const isProcessing = video.status === 'Processing';

    el.innerHTML = `
    <div class="video-card-thumb">
      <img src="${thumbnailUrl(video.thumbnailPath)}" alt="${video.title}" loading="lazy" />
      ${isProcessing
        ? `<div class="badge-processing" style="position:absolute;top:8px;left:8px">Processing</div>`
        : `<div class="video-card-duration">${formatDuration(video.durationSeconds)}</div>`}
    </div>
    <div class="video-card-body">
      <div class="avatar" style="margin-top:2px">${avatarInitial(video.authorUsername)}</div>
      <div class="video-card-info">
        <div class="video-card-title">${video.title}</div>
        <div class="video-card-meta">
          <a class="video-card-author" href="#/profile/${video.userId}">${video.authorUsername}</a>
          <span>${formatViews(video.viewCount)} · ${formatRelative(video.createdAt)}</span>
        </div>
      </div>
    </div>`;

    el.querySelector('.video-card-thumb')?.addEventListener('click', () => {
        if (!isProcessing) navigate(`/video/${video.id}`);
    });

    el.querySelector('.video-card-title')?.addEventListener('click', () => {
        if (!isProcessing) navigate(`/video/${video.id}`);
    });

    el.querySelector('.video-card-author')?.addEventListener('click', (e) => {
        e.stopPropagation();
    });

    return el;
}