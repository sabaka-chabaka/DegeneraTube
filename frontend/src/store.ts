export interface CurrentUser{
    id: string;
    username: string;
    email: string;
    avatarPath: string | null;
}

const KEYS = {
    ACCESS:  'dgt_access',
    REFRESH: 'dgt_refresh',
    USER:    'dgt_user',
} as const;

export const store = {
    getAccessToken: (): string | null =>
        localStorage.getItem(KEYS.ACCESS),

    getRefreshToken: (): string | null =>
        localStorage.getItem(KEYS.REFRESH),

    getUser: (): CurrentUser | null => {
        const raw = localStorage.getItem(KEYS.USER);
        return raw ? JSON.parse(raw) : null;
    },

    setTokens: (access: string, refresh: string): void => {
        localStorage.setItem(KEYS.ACCESS, access);
        localStorage.setItem(KEYS.REFRESH, refresh);
    },

    setUser: (user: CurrentUser): void =>
        localStorage.setItem(KEYS.USER, JSON.stringify(user)),

    clear: (): void => {
        localStorage.removeItem(KEYS.ACCESS);
        localStorage.removeItem(KEYS.REFRESH);
        localStorage.removeItem(KEYS.USER);
    },

    isLoggedIn: (): boolean =>
        !!localStorage.getItem(KEYS.ACCESS),
};