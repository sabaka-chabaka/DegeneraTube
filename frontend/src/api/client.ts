import {store} from "../store.ts";

const BASE = '/api';

async function refreshAccessToken(): Promise<boolean>{
    const refresh = store.getRefreshToken();
    if(!refresh) return false;

    const res = await fetch(`${BASE}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: refresh }),
    });

    if (!res.ok) {
        store.clear();
        return false;
    }

    const data = await res.json();
    store.setTokens(data.accessToken, data.refreshToken);
    return true;
}

export async function request<T>(
    path: string,
    options: RequestInit = {},
    retry = true,
): Promise<T> {
    const token = store.getAccessToken();

    const headers: Record<string, string> = {
        ...(options.body && !(options.body instanceof FormData)
            ? { 'Content-Type': 'application/json' }
            : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(options.headers as Record<string, string> ?? {}),
    };

    const res = await fetch(`${BASE}${path}`, { ...options, headers });

    if (res.status === 401 && retry) {
        const ok = await refreshAccessToken();
        if (ok) return request<T>(path, options, false);
        window.location.hash = '#/login';
        throw new Error('Unauthorized');
    }

    if (!res.ok) {
        const err = await res.json().catch(() => ({ error: 'Unknown error' }));
        throw new Error(err.error ?? 'Request failed');
    }

    if (res.status === 204 || res.headers.get('content-length') === '0') {
        return undefined as T;
    }

    return res.json();
}

export const get  = <T>(path: string) => request<T>(path);
export const post = <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) });
export const put  = <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) });
export const del  = <T>(path: string) =>
    request<T>(path, { method: 'DELETE' });
