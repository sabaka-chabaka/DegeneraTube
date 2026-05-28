import { get, put, del, request } from './client';


export interface VideoDto {
    id: string;
    userId: string;
    authorUsername: string;
    authorAvatarPath: string | null;
    title: string;
    description: string | null;
    status: 'Processing' | 'Ready' | 'Failed';
    thumbnailPath: string | null;
    durationSeconds: number;
    viewCount: number;
    resolutions: number[];
    tags: string[];
    createdAt: string;
}

export interface PagedResponse<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export const videosApi = {
    getPaged: (page = 1, pageSize = 20, search?: string) => {
        const q = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
        if (search) q.set('search', search);
        return get<PagedResponse<VideoDto>>(`/videos?${q}`);
    },

    getById: (id: string) =>
        get<VideoDto>(`/videos/${id}`),

    getByUser: (userId: string, page = 1, pageSize = 20) =>
        get<PagedResponse<VideoDto>>(`/videos/user/${userId}?page=${page}&pageSize=${pageSize}`),

    upload: (formData: FormData) =>
        request<VideoDto>('/videos', { method: 'POST', body: formData }),

    update: (id: string, body: { title?: string; description?: string; tags?: string[] }) =>
        put<VideoDto>(`/videos/${id}`, body),

    delete: (id: string) =>
        del<void>(`/videos/${id}`),

    registerView: (id: string) =>
        request<void>(`/videos/${id}/view`, { method: 'POST' }),

    streamUrl: (id: string) =>
        `/api/videos/${id}/stream`,
};
