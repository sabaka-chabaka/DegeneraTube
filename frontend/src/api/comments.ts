import { get, post, put, del } from './client.ts';

export interface CommentDto {
    id: string;
    videoId: string;
    userId: string;
    authorUsername: string;
    authorAvatarPath: string | null;
    parentId: string | null;
    body: string;
    createdAt: string;
    replyCount: number;
}

export interface CommentPagedResponse {
    items: CommentDto[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export const commentsApi = {
    getByVideo: (videoId: string, page = 1, pageSize = 20) =>
        get<CommentPagedResponse>(`/comments/video/${videoId}?page=${page}&pageSize=${pageSize}`),

    getReplies: (commentId: string) =>
        get<CommentDto[]>(`/comments/${commentId}/replies`),

    create: (videoId: string, body: string, parentId?: string) =>
        post<CommentDto>(`/comments/video/${videoId}`, { body, parentId: parentId ?? null }),

    update: (id: string, body: string) =>
        put<CommentDto>(`/comments/${id}`, { body }),

    delete: (id: string) =>
        del<void>(`/comments/${id}`),
};