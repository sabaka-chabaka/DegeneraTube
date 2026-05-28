import { store } from '../store';
import { navigate } from '../router';
import { authApi } from '../api/auth';
import { toast } from '../utils';

export function renderNavbar(): void {
    const el = document.getElementById('navbar')!;
    const user = store.getUser();

    el.innerHTML = `
    <nav class="navbar">
      <a href="#/" class="navbar-logo">Degenera<span>Tube</span></a>
 
      <div class="navbar-search">
        <input type="text" id="search-input" placeholder="Search videos..." />
        <button id="search-btn">⌕</button>
      </div>
 
      <div class="navbar-actions">
        ${user ? `
          <a href="#/upload" class="btn btn-ghost">+ Upload</a>
          <a href="#/profile/${user.id}" class="avatar" title="${user.username}">
            ${user.avatarPath
        ? `<img src="/api/storage/${user.avatarPath}" alt="${user.username}" />`
        : user.username.charAt(0).toUpperCase()}
          </a>
          <button class="btn-icon" id="logout-btn" title="Logout">⏻</button>
        ` : `
          <a href="#/login" class="btn btn-ghost">Sign in</a>
          <a href="#/register" class="btn btn-primary">Join</a>
        `}
      </div>
    </nav>`;

    el.querySelector('#search-btn')?.addEventListener('click', () => {
        const q = (el.querySelector('#search-input') as HTMLInputElement).value.trim();
        if (q) navigate(`/?search=${encodeURIComponent(q)}`);
    });

    el.querySelector('#search-input')?.addEventListener('keydown', (e) => {
        if ((e as KeyboardEvent).key === 'Enter') {
            const q = (el.querySelector('#search-input') as HTMLInputElement).value.trim();
            if (q) navigate(`/?search=${encodeURIComponent(q)}`);
        }
    });

    el.querySelector('#logout-btn')?.addEventListener('click', async () => {
        const refresh = store.getRefreshToken();
        if (refresh) await authApi.logout(refresh).catch(() => {});
        store.clear();
        renderNavbar();
        navigate('/');
        toast('Signed out', 'info');
    });
}