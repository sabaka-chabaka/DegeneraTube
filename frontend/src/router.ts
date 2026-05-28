type RouteHandler = (params: Record<string, string>) => HTMLElement | Promise<HTMLElement>;

interface Route {
    pattern: RegExp;
    keys: string[];
    handler: RouteHandler;
}

const routes: Route[] = [];

export function route(path: string, handler: RouteHandler): void {
    const keys: string[] = [];
    const pattern = new RegExp(
        '^' + path.replace(/:([^/]+)/g, (_, k) => { keys.push(k); return '([^/]+)'; }) + '$'
    );
    routes.push({ pattern, keys, handler });
}

async function resolve(): Promise<void> {
    const hash = window.location.hash.slice(1) || '/';
    const app = document.getElementById('app')!;

    for (const r of routes) {
        const match = hash.match(r.pattern);
        if (!match) continue;

        const params: Record<string, string> = {};
        r.keys.forEach((k, i) => params[k] = match[i + 1]);

        app.innerHTML = '';
        const el = await r.handler(params);
        app.appendChild(el);
        window.scrollTo(0, 0);
        return;
    }

    app.innerHTML = `
    <div class="empty-state" style="margin-top:80px">
      <div class="empty-state-icon">404</div>
      <div class="empty-state-text">Page not found</div>
      <a href="#/" class="btn btn-ghost mt-16">Go home</a>
    </div>`;
}

export function navigate(path: string): void {
    window.location.hash = path;
}

export function initRouter(): void {
    window.addEventListener('hashchange', resolve);
    resolve();
}