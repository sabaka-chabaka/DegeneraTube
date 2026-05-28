import { videosApi, type VideoDto } from '../api/videos';
import { VideoCard } from '../components/VideoCard';
import { toast } from '../utils';

export async function HomePage(): Promise<HTMLElement> {
    const el = document.createElement('div');

    const params = new URLSearchParams(window.location.hash.split('?')[1] ?? '');
    const search = params.get('search') ?? undefined;

    el.innerHTML = `
    ${search ? `<div style="padding:20px 16px 0;font-size:13px;color:var(--text-2)">
      Results for <strong style="color:var(--text)">"${search}"</strong>
      <a href="#/" style="margin-left:12px;color:var(--accent);font-size:12px">Clear</a>
    </div>` : ''}
    <div class="video-grid" id="grid">
      ${Array(12).fill(0).map(() => `
        <div>
          <div class="skeleton" style="aspect-ratio:16/9;border-radius:10px;margin-bottom:10px"></div>
          <div style="display:flex;gap:10px">
            <div class="skeleton" style="width:36px;height:36px;border-radius:50%;flex-shrink:0"></div>
            <div style="flex:1">
              <div class="skeleton" style="height:14px;margin-bottom:8px;border-radius:4px"></div>
              <div class="skeleton" style="height:12px;width:60%;border-radius:4px"></div>
            </div>
          </div>
        </div>`).join('')}
    </div>
    <div id="pagination" style="display:flex;justify-content:center;gap:8px;padding:24px 16px"></div>`;

    try {
        const data = await videosApi.getPaged(1, 24, search);
        const grid = el.querySelector('#grid')!;
        grid.innerHTML = '';

        if (data.items.length === 0) {
            grid.innerHTML = `
        <div class="empty-state" style="grid-column:1/-1">
          <div class="empty-state-icon">📭</div>
          <div class="empty-state-text">${search ? 'No videos found' : 'No videos yet'}</div>
          <div class="empty-state-sub">${search ? 'Try different keywords' : 'Be the first to upload!'}</div>
        </div>`;
            return el;
        }

        data.items.forEach((v: VideoDto) => grid.appendChild(VideoCard(v)));

        if (data.totalPages > 1) {
            const pagination = el.querySelector('#pagination')!;
            for (let p = 1; p <= data.totalPages; p++) {
                const btn = document.createElement('button');
                btn.className = `btn ${p === 1 ? 'btn-primary' : 'btn-ghost'}`;
                btn.style.borderRadius = '8px';
                btn.textContent = String(p);
                btn.addEventListener('click', async () => {
                    const paged = await videosApi.getPaged(p, 24, search);
                    grid.innerHTML = '';
                    paged.items.forEach((v: VideoDto) => grid.appendChild(VideoCard(v)));
                    pagination.querySelectorAll('button').forEach((b, i) => {
                        b.className = `btn ${i + 1 === p ? 'btn-primary' : 'btn-ghost'}`;
                        (b as HTMLButtonElement).style.borderRadius = '8px';
                    });
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                });
                pagination.appendChild(btn);
            }
        }
    } catch (e) {
        toast('Failed to load videos', 'error');
    }

    return el;
}