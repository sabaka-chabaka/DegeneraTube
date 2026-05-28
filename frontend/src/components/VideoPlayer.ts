import Hls from 'hls.js';
import { videosApi } from '../api/videos.ts';

export function VideoPlayer(videoId: string): HTMLElement {
    const wrap = document.createElement('div');
    wrap.className = 'video-player-wrap';

    const video = document.createElement('video');
    video.controls = true;
    video.autoplay = false;
    video.playsInline = true;
    wrap.appendChild(video);

    const src = videosApi.streamUrl(videoId);

    if (Hls.isSupported()) {
        const hls = new Hls({ enableWorker: true, lowLatencyMode: false });
        hls.loadSource(src);
        hls.attachMedia(video);
        hls.on(Hls.Events.MANIFEST_PARSED, () => video.play().catch(() => {}));

        hls.on(Hls.Events.ERROR, (_, data) => {
            if (data.fatal) {
                switch (data.type) {
                    case Hls.ErrorTypes.NETWORK_ERROR:
                        hls.startLoad();
                        break;
                    case Hls.ErrorTypes.MEDIA_ERROR:
                        hls.recoverMediaError();
                        break;
                    default:
                        hls.destroy();
                }
            }
        });
    } else if (video.canPlayType('application/vnd.apple.mpegurl')) {
        video.src = src;
    } else {
        wrap.innerHTML = `<div class="empty-state"><div class="empty-state-text">Your browser doesn't support HLS playback.</div></div>`;
    }

    return wrap;
}