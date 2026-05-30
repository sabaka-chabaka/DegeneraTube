import { videosApi, type VideoDto } from '../api/videos';
import { commentsApi, type CommentDto } from '../api/comments';
import { usersApi } from '../api/users';
import { VideoPlayer } from '../components/VideoPlayer';
import { store } from '../store';
import { formatViews, formatRelative, avatarInitial, toast, thumbnailUrl } from '../utils';
import { navigate } from '../router';

export async function VideoPage(params: Record<string, string>): Promise<HTMLElement> {
    const el = document.createElement('div');
    const { id } = params;

    el.innerHTML = `<div class="video-page">
    <div id="main-col"></div>
    <div id="side-col"><div class="sidebar-title">Up next</div><div id="sidebar-videos"></div></div>
  </div>`;

    const mainCol = el.querySelector('#main-col')!;

    try {
        const [video, comments, related] = await Promise.all([
            videosApi.getById(id),
            commentsApi.getByVideo(id),
            videosApi.getPaged(1, 10),
        ]);

        videosApi.registerView(id).catch(() => {});

        mainCol.appendChild(VideoPlayer(id));
        mainCol.appendChild(renderVideoInfo(video));
        mainCol.appendChild(await renderComments(id, comments.items));

        const sideVideos = el.querySelector('#sidebar-videos')!;
        related.items
            .filter(v => v.id !== id)
            .slice(0, 8)
            .forEach(v => sideVideos.appendChild(renderSideCard(v)));

    } catch {
        mainCol.innerHTML = `<div class="empty-state"><div class="empty-state-text">Video not found or not available yet.</div></div>`;
    }

    return el;
}

function renderVideoInfo(video: VideoDto): HTMLElement {
    const el = document.createElement('div');
    el.className = 'video-info';
    const user = store.getUser();
    const isOwner = user?.id === video.userId;

    el.innerHTML = `
    <div class="video-title">${video.title}</div>
    <div class="video-meta-row">
      <div class="video-stats">${formatViews(video.viewCount)} · ${formatRelative(video.createdAt)}</div>
      <div class="video-actions">
        ${isOwner ? `
          <input type="file" id="thumb-input" accept="image/jpeg,image/png,image/webp" style="display:none" />
          <button class="btn btn-ghost" id="thumb-btn">🖼 Change thumbnail</button>
          <button class="btn btn-ghost" id="delete-btn">🗑 Delete</button>
        ` : ''}
        ${!isOwner && user ? `<button class="btn btn-ghost" id="sub-btn">+ Subscribe</button>` : ''}
      </div>
    </div>
    <div class="video-channel">
      <a href="#/profile/${video.userId}" class="avatar">${avatarInitial(video.authorUsername)}</a>
      <div>
        <a href="#/profile/${video.userId}" class="video-channel-name">${video.authorUsername}</a>
      </div>
    </div>
    ${video.description ? `<div class="video-description">${video.description}</div>` : ''}`;

    el.querySelector('#thumb-btn')?.addEventListener('click', () => {
        (el.querySelector('#thumb-input') as HTMLInputElement).click();
    });

    el.querySelector('#thumb-input')?.addEventListener('change', async (e) => {
        const input = e.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file) return;

        const btn = el.querySelector('#thumb-btn') as HTMLButtonElement;
        btn.disabled = true;
        btn.textContent = 'Uploading...';

        try {
            await videosApi.updateThumbnail(video.id, file);
            toast('Thumbnail updated!', 'success');
            btn.textContent = '🖼 Change thumbnail';
        } catch (err: any) {
            toast(err.message ?? 'Failed to update thumbnail', 'error');
            btn.textContent = '🖼 Change thumbnail';
        } finally {
            btn.disabled = false;
            input.value = '';
        }
    });

    el.querySelector('#delete-btn')?.addEventListener('click', async () => {
        if (!confirm('Delete this video?')) return;
        try {
            await videosApi.delete(video.id);
            toast('Video deleted', 'success');
            navigate('/');
        } catch (e: any) {
            toast(e.message, 'error');
        }
    });

    el.querySelector('#sub-btn')?.addEventListener('click', async () => {
        try {
            await usersApi.subscribe(video.userId);
            toast(`Subscribed to ${video.authorUsername}`, 'success');
        } catch (e: any) {
            toast(e.message, 'error');
        }
    });

    return el;
}

async function renderComments(videoId: string, items: CommentDto[]): Promise<HTMLElement> {
    const el = document.createElement('div');
    el.className = 'comments';
    const user = store.getUser();

    el.innerHTML = `
    <div class="comments-title">${items.length} Comments</div>
    ${user ? `
      <div class="comment-form">
        <div class="avatar">${avatarInitial(user.username)}</div>
        <div style="flex:1;display:flex;flex-direction:column;gap:8px">
          <textarea id="comment-input" placeholder="Add a comment..."></textarea>
          <div style="display:flex;justify-content:flex-end">
            <button class="btn btn-primary" id="comment-submit">Post</button>
          </div>
        </div>
      </div>` : ''}
    <div id="comments-list"></div>`;

    const list = el.querySelector('#comments-list')!;
    items.forEach(c => list.appendChild(CommentItem(c, videoId)));

    el.querySelector('#comment-submit')?.addEventListener('click', async () => {
        const input = el.querySelector('#comment-input') as HTMLTextAreaElement;
        const body = input.value.trim();
        if (!body) return;
        try {
            const created = await commentsApi.create(videoId, body);
            list.prepend(CommentItem(created, videoId));
            input.value = '';
            toast('Comment posted', 'success');
        } catch (e: any) {
            toast(e.message, 'error');
        }
    });

    return el;
}

function CommentItem(comment: CommentDto, videoId: string): HTMLElement {
    const el = document.createElement('div');
    el.className = 'comment-item';
    const user = store.getUser();
    const isOwner = user?.id === comment.userId;

    el.innerHTML = `
    <div class="avatar">${avatarInitial(comment.authorUsername)}</div>
    <div class="comment-body">
      <div class="comment-author">
        ${comment.authorUsername}
        <span>${formatRelative(comment.createdAt)}</span>
      </div>
      <div class="comment-text">${comment.body}</div>
      <div class="comment-actions">
        ${comment.replyCount > 0
        ? `<button class="comment-action-btn" id="replies-btn-${comment.id}">${comment.replyCount} replies</button>`
        : ''}
        <button class="comment-action-btn reply-btn">Reply</button>
        ${isOwner ? `<button class="comment-action-btn delete-comment-btn">Delete</button>` : ''}
      </div>
      <div id="replies-${comment.id}"></div>
      <div id="reply-form-${comment.id}"></div>
    </div>`;

    el.querySelector(`#replies-btn-${comment.id}`)?.addEventListener('click', async (e) => {
        const btn = e.target as HTMLButtonElement;
        const container = el.querySelector(`#replies-${comment.id}`)!;
        if (container.children.length > 0) {
            container.innerHTML = '';
            btn.textContent = `${comment.replyCount} replies`;
            return;
        }
        const replies = await commentsApi.getReplies(comment.id);
        replies.forEach(r => container.appendChild(CommentItem(r, videoId)));
        btn.textContent = 'Hide replies';
    });

    el.querySelector('.reply-btn')?.addEventListener('click', () => {
        if (!user) { navigate('/login'); return; }
        const formEl = el.querySelector(`#reply-form-${comment.id}`)!;
        if (formEl.children.length > 0) { formEl.innerHTML = ''; return; }

        formEl.innerHTML = `
      <div class="comment-form" style="margin-top:12px">
        <div class="avatar" style="width:28px;height:28px;font-size:11px">${avatarInitial(user.username)}</div>
        <div style="flex:1;display:flex;flex-direction:column;gap:8px">
          <textarea id="reply-input-${comment.id}" placeholder="Reply..." style="height:60px"></textarea>
          <div style="display:flex;justify-content:flex-end;gap:8px">
            <button class="btn btn-ghost cancel-reply" style="padding:6px 14px;font-size:12px">Cancel</button>
            <button class="btn btn-primary submit-reply" style="padding:6px 14px;font-size:12px">Reply</button>
          </div>
        </div>
      </div>`;

        formEl.querySelector('.cancel-reply')?.addEventListener('click', () => formEl.innerHTML = '');
        formEl.querySelector('.submit-reply')?.addEventListener('click', async () => {
            const input = formEl.querySelector(`#reply-input-${comment.id}`) as HTMLTextAreaElement;
            const body = input.value.trim();
            if (!body) return;
            try {
                const created = await commentsApi.create(videoId, body, comment.id);
                const repliesContainer = el.querySelector(`#replies-${comment.id}`)!;
                repliesContainer.appendChild(CommentItem(created, videoId));
                formEl.innerHTML = '';
                toast('Reply posted', 'success');
            } catch (e: any) {
                toast(e.message, 'error');
            }
        });
    });

    el.querySelector('.delete-comment-btn')?.addEventListener('click', async () => {
        try {
            await commentsApi.delete(comment.id);
            el.remove();
            toast('Comment deleted', 'success');
        } catch (e: any) {
            toast(e.message, 'error');
        }
    });

    return el;
}

function renderSideCard(video: VideoDto): HTMLElement {
    const el = document.createElement('div');
    el.className = 'sidebar-card';
    el.innerHTML = `
    <div class="sidebar-card-thumb">
      <img src="${thumbnailUrl(video.id)}" alt="${video.title}" loading="lazy" />
    </div>
    <div class="sidebar-card-info">
      <div class="sidebar-card-title">${video.title}</div>
      <div class="sidebar-card-meta">${video.authorUsername}</div>
      <div class="sidebar-card-meta">${formatViews(video.viewCount)}</div>
    </div>`;
    el.addEventListener('click', () => navigate(`/video/${video.id}`));
    return el;
}