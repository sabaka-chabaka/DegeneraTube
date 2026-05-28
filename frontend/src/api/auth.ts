import { post } from './client';

export interface TokenResponse {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
}

export const authApi = {
    register: (username: string, email: string, password: string) =>
        post<TokenResponse>('/auth/register', { username, email, password }),

    login: (email: string, password: string) =>
        post<TokenResponse>('/auth/login', { email, password }),

    logout: (refreshToken: string) =>
        post<void>('/auth/logout', { refreshToken }),
};