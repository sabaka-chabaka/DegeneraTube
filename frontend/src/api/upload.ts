import { request } from './client';
import type { VideoDto } from './videos';

const CHUNK_SIZE = 5 * 1024 * 1024;
const LARGE_FILE_THRESHOLD = 20 * 1024 * 1024;

export interface UploadOptions {
    title: string;
    description: string;
    tags: string[];
    onProgress?: (percent: number, label: string) => void;
}

async function initUpload(
    fileName: string,
    fileSize: number,
    totalChunks: number,
    title: string,
    description: string,
    tags: string[],
): Promise<{ uploadId: string }> {
    return request('/uploads/init', {
        method: 'POST',
        body: JSON.stringify({ fileName, fileSize, totalChunks, title, description, tags }),
    });
}

async function uploadChunk(
    uploadId: string,
    chunkIndex: number,
    totalChunks: number,
    chunk: Blob,
): Promise<{ receivedChunks: number; totalChunks: number; isComplete: boolean }> {
    const headers: Record<string, string> = {
        'X-Total-Chunks': String(totalChunks),
        'Content-Type': 'application/octet-stream',
    };
    return request(`/uploads/${uploadId}/chunk/${chunkIndex}`, {
        method: 'PUT',
        headers,
        body: chunk,
    });
}

async function finalizeUpload(uploadId: string): Promise<VideoDto> {
    return request(`/uploads/${uploadId}/finalize`, { method: 'POST' });
}

export async function uploadVideo(file: File, opts: UploadOptions): Promise<VideoDto> {
    const { title, description, tags, onProgress } = opts;

    if (file.size <= LARGE_FILE_THRESHOLD) {
        onProgress?.(10, 'Uploading...');
        const fd = new FormData();
        fd.append('title', title);
        fd.append('description', description);
        tags.forEach(t => fd.append('tags', t));
        fd.append('file', file);

        const video = await request<VideoDto>('/videos', { method: 'POST', body: fd });
        onProgress?.(100, 'Upload complete!');
        return video;
    }

    const totalChunks = Math.ceil(file.size / CHUNK_SIZE);
    onProgress?.(0, 'Initialising upload...');

    const { uploadId } = await initUpload(
        file.name, file.size, totalChunks, title, description, tags,
    );

    for (let i = 0; i < totalChunks; i++) {
        const start = i * CHUNK_SIZE;
        const end   = Math.min(start + CHUNK_SIZE, file.size);
        const chunk = file.slice(start, end);

        await uploadChunk(uploadId, i, totalChunks, chunk);

        const percent = Math.round(((i + 1) / totalChunks) * 90); // 0–90%
        onProgress?.(percent, `Uploading part ${i + 1} of ${totalChunks}...`);
    }

    onProgress?.(95, 'Finalising...');
    const video = await finalizeUpload(uploadId);

    onProgress?.(100, 'Upload complete! Processing in background...');
    return video;
}

export { LARGE_FILE_THRESHOLD, CHUNK_SIZE };