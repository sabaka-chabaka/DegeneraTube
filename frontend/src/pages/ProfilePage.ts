import { usersApi } from '../api/users';
import { videosApi } from '../api/videos';
import { store } from '../store';
import { VideoCard } from '../components/VideoCard';
import { avatarInitial, toast } from '../utils';

export async function ProfilePage(params: Record<string, string>): Promise<HTMLElement> {
    const el = document.createElement('div');
    const { id } = params;
    const currentUser = store.getUser();
    const isOwn = currentUser?.id === id;

    try {
        const [profile, videos] = await Promise.all([
            usersApi.getProfile(id),
            videosApi.getByUser(id),
        ]);

        el.innerHTML = `
      <div class="profile-header">
        <div class="profile-header-inner">
          <div class="avatar profile-avatar">${avatarInitial(profile.username)}</div>
          <div>
            <div class="profile-name">${profile.username}</div>
            <div class="profile-meta">
              ${profile.subscriberCount} subscribers · ${profile.videoCount} videos
            </div>
          </div>
          <div style="margin-left:auto;display:flex;gap:8px">
            ${isOwn
            ? `<button class="btn btn-ghost" id="edit-btn">Edit profile</button>`
            : currentUser
                ? `<button class="btn btn-primary" id="sub-btn">Subscribe</button>`
                : ''}
          </div>
        </div>
      </div>

      <div class="profile-page">
        ${videos.items.length === 0
            ? `<div class="empty-state">
               <div class="empty-state-icon">🎬</div>
               <div class="empty-state-text">No videos yet</div>
               ${isOwn ? `<a href="#/upload" class="btn btn-primary mt-16">Upload your first video</a>` : ''}
             </div>`
            : `<div class="video-grid" id="profile-grid"></div>`}
      </div>`;

        const grid = el.querySelector('#profile-grid');
        if (grid) videos.items.forEach(v => grid.appendChild(VideoCard(v)));

        el.querySelector('#sub-btn')?.addEventListener('click', async () => {
            try {
                await usersApi.subscribe(id);
                toast(`Subscribed to ${profile.username}`, 'success');
                (el.querySelector('#sub-btn') as HTMLButtonElement).textContent = 'Subscribed';
            } catch (e: any) {
                toast(e.message, 'error');
            }
        });

        el.querySelector('#edit-btn')?.addEventListener('click', () => {
            showEditModal(el, profile.username);
        });

    } catch {
        el.innerHTML = `<div class="empty-state" style="margin-top:80px">
      <div class="empty-state-icon">👤</div>
      <div class="empty-state-text">User not found</div>
    </div>`;
    }

    return el;
}

function showEditModal(parent: HTMLElement, currentUsername: string): void {
    const overlay = document.createElement('div');
    overlay.style.cssText = `
    position:fixed;inset:0;background:rgba(0,0,0,0.7);
    display:flex;align-items:center;justify-content:center;z-index:200`;

    overlay.innerHTML = `
    <div class="form-card" style="width:400px">
      <div class="form-title" style="font-size:24px;margin-bottom:20px">Edit Profile</div>
      <div class="form-group">
        <label class="form-label">Username</label>
        <input class="form-input" id="new-username" type="text" value="${currentUsername}" />
        <div class="form-error" id="edit-error"></div>
      </div>
      <div style="display:flex;gap:8px;margin-top:16px">
        <button class="btn btn-primary" id="save-btn" style="flex:1;justify-content:center;border-radius:10px">Save</button>
        <button class="btn btn-ghost" id="close-btn" style="border-radius:10px">Cancel</button>
      </div>
    </div>`;

    overlay.querySelector('#close-btn')?.addEventListener('click', () => overlay.remove());
    overlay.addEventListener('click', (e) => { if (e.target === overlay) overlay.remove(); });

    overlay.querySelector('#save-btn')?.addEventListener('click', async () => {
        const username = (overlay.querySelector('#new-username') as HTMLInputElement).value.trim();
        const errorEl = overlay.querySelector('#edit-error')!;
        if (!username) { errorEl.textContent = 'Username required'; return; }

        try {
            const updated = await usersApi.updateProfile({ username });
            const user = store.getUser()!;
            store.setUser({ ...user, username: updated.username });
            toast('Profile updated', 'success');
            overlay.remove();
            window.location.reload();
        } catch (e: any) {
            errorEl.textContent = e.message;
        }
    });

    parent.appendChild(overlay);
}