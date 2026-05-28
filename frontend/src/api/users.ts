import { get, put, post, del } from './client';

export interface UserDto {
    id: string;
    username: string;
    email: string;
    avatarPath: string | null;
    createdAt: string;
}

export interface UserProfileDto {
    id: string;
    username: string;
    avatarPath: string | null;
    videoCount: number;
    subscriberCount: number;
    createdAt: string;
}

export const usersApi = {
    getMe: () => get<UserDto>('/users/me'),
    getProfile: (id: string) => get<UserProfileDto>(`/users/${id}`),
    updateProfile: (body: { username?: string; avatarPath?: string }) =>
        put<UserDto>('/users/me', body),
    subscribe: (id: string) => post<void>(`/users/${id}/subscribe`, {}),
    unsubscribe: (id: string) => del<void>(`/users/${id}/subscribe`),
};