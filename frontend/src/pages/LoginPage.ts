import { authApi } from '../api/auth';
import { usersApi } from '../api/users';
import { store } from '../store';
import { navigate } from '../router';
import { renderNavbar } from '../components/Navbar';
import { toast } from '../utils';

export function LoginPage(): HTMLElement {
    return buildForm('login');
}

export function RegisterPage(): HTMLElement {
    return buildForm('register');
}

function buildForm(mode: 'login' | 'register'): HTMLElement {
    const el = document.createElement('div');
    el.className = 'form-page';

    const isRegister = mode === 'register';

    el.innerHTML = `
    <div class="form-card">
      <div class="form-title">Degenera<span>Tube</span></div>
      <div class="form-subtitle">${isRegister ? 'Create your account' : 'Welcome back'}</div>

      ${isRegister ? `
        <div class="form-group">
          <label class="form-label">Username</label>
          <input class="form-input" id="username" type="text" placeholder="cooluser123" autocomplete="username" />
        </div>` : ''}

      <div class="form-group">
        <label class="form-label">Email</label>
        <input class="form-input" id="email" type="email" placeholder="you@example.com" autocomplete="email" />
      </div>

      <div class="form-group">
        <label class="form-label">Password</label>
        <input class="form-input" id="password" type="password" placeholder="••••••••" autocomplete="${isRegister ? 'new-password' : 'current-password'}" />
        <div class="form-error" id="error"></div>
      </div>

      <button class="btn btn-primary w-full" id="submit" style="justify-content:center;border-radius:10px;padding:11px">
        ${isRegister ? 'Create account' : 'Sign in'}
      </button>

      <div class="form-divider">${isRegister ? 'or' : 'or'}</div>

      <div style="text-align:center;font-size:13px;color:var(--text-2)">
        ${isRegister
        ? `Already have an account? <a href="#/login" style="color:var(--accent)">Sign in</a>`
        : `Don't have an account? <a href="#/register" style="color:var(--accent)">Join</a>`}
      </div>
    </div>`;

    const errorEl = el.querySelector('#error')!;
    const submitBtn = el.querySelector('#submit') as HTMLButtonElement;

    submitBtn.addEventListener('click', async () => {
        errorEl.textContent = '';
        const email = (el.querySelector('#email') as HTMLInputElement).value.trim();
        const password = (el.querySelector('#password') as HTMLInputElement).value;

        if (!email || !password) {
            errorEl.textContent = 'Please fill in all fields.';
            return;
        }

        submitBtn.disabled = true;
        submitBtn.textContent = 'Please wait...';

        try {
            let tokens;
            if (isRegister) {
                const username = (el.querySelector('#username') as HTMLInputElement).value.trim();
                if (!username) { errorEl.textContent = 'Username is required.'; return; }
                tokens = await authApi.register(username, email, password);
            } else {
                tokens = await authApi.login(email, password);
            }

            store.setTokens(tokens.accessToken, tokens.refreshToken);
            const me = await usersApi.getMe();
            store.setUser({ id: me.id, username: me.username, email: me.email, avatarPath: me.avatarPath });
            renderNavbar();
            navigate('/');
            toast(`Welcome, ${me.username}!`, 'success');
        } catch (e: any) {
            errorEl.textContent = e.message;
            submitBtn.disabled = false;
            submitBtn.textContent = isRegister ? 'Create account' : 'Sign in';
        }
    });

    el.querySelectorAll('.form-input').forEach(input => {
        input.addEventListener('keydown', (e) => {
            if ((e as KeyboardEvent).key === 'Enter') submitBtn.click();
        });
    });

    return el;
}