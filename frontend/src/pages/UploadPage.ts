import { uploadVideo, LARGE_FILE_THRESHOLD } from '../api/upload';
import { store } from '../store';
import { navigate } from '../router';
import { toast } from '../utils';

export function UploadPage(): HTMLElement {
    const el = document.createElement('div');
    el.className = 'upload-page';

    if (!store.isLoggedIn()) {
        navigate('/login');
        return el;
    }

    let selectedFile: File | null = null;

    el.innerHTML = `
    <h1 style="font-family:var(--font-display);font-size:36px;margin-bottom:24px">
      Upload <span style="color:var(--accent)">Video</span>
    </h1>

    <div class="upload-drop" id="drop-zone">
      <div class="upload-drop-icon">🎬</div>
      <div class="upload-drop-text">Drag & drop your video here</div>
      <div class="upload-drop-hint">MP4, MOV, AVI, MKV · max 4 GB</div>
      <input type="file" id="file-input" accept="video/*" style="display:none" />
      <button class="btn btn-ghost mt-16" id="browse-btn">Browse files</button>
    </div>

    <div class="upload-progress" id="progress-box" style="display:none">
      <div id="progress-filename" style="font-size:13px;font-weight:500"></div>
      <div class="upload-progress-bar-wrap">
        <div class="upload-progress-bar" id="progress-bar" style="width:0%;transition:width .25s ease"></div>
      </div>
      <div style="display:flex;justify-content:space-between;margin-top:6px">
        <div id="progress-label" style="font-size:12px;color:var(--text-2)">Uploading...</div>
        <div id="progress-percent" style="font-size:12px;color:var(--text-2);font-weight:600">0%</div>
      </div>
    </div>

    <div id="meta-form" style="display:none">
      <div class="form-group">
        <label class="form-label">Title</label>
        <input class="form-input" id="title" type="text" placeholder="Video title" />
      </div>
      <div class="form-group">
        <label class="form-label">Description</label>
        <textarea class="form-input" id="description" rows="4"
          placeholder="Describe your video..." style="resize:vertical"></textarea>
      </div>
      <div class="form-group">
        <label class="form-label">Tags</label>
        <input class="form-input" id="tags" type="text"
          placeholder="gaming, funny, tutorial (comma separated)" />
      </div>
      <div id="upload-mode-hint" style="font-size:12px;color:var(--text-2);margin-bottom:16px"></div>
      <div style="display:flex;gap:12px;margin-top:24px">
        <button class="btn btn-primary" id="upload-btn"
          style="flex:1;justify-content:center;border-radius:10px;padding:12px">
          Upload
        </button>
        <button class="btn btn-ghost" id="cancel-btn" style="border-radius:10px">Cancel</button>
      </div>
    </div>`;

    const dropZone        = el.querySelector('#drop-zone')!;
    const fileInput       = el.querySelector('#file-input') as HTMLInputElement;
    const metaForm        = el.querySelector('#meta-form') as HTMLElement;
    const progressBox     = el.querySelector('#progress-box') as HTMLElement;
    const progressBar     = el.querySelector('#progress-bar') as HTMLElement;
    const progressLabel   = el.querySelector('#progress-label')!;
    const progressPercent = el.querySelector('#progress-percent')!;
    const progressFilename = el.querySelector('#progress-filename')!;
    const uploadModeHint  = el.querySelector('#upload-mode-hint') as HTMLElement;

    el.querySelector('#browse-btn')?.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', () => {
        if (fileInput.files?.[0]) selectFile(fileInput.files[0]);
    });

    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-over');
    });
    dropZone.addEventListener('dragleave', () => dropZone.classList.remove('drag-over'));
    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
        const file = (e as DragEvent).dataTransfer?.files[0];
        if (file?.type.startsWith('video/')) selectFile(file);
        else toast('Please select a video file', 'error');
    });

    function selectFile(file: File) {
        selectedFile = file;
        const sizeMb = (file.size / 1024 / 1024).toFixed(1);
        const isLarge = file.size > LARGE_FILE_THRESHOLD;

        dropZone.innerHTML = `
      <div class="upload-drop-icon">✅</div>
      <div class="upload-drop-text">${file.name}</div>
      <div class="upload-drop-hint">${sizeMb} MB</div>`;

        metaForm.style.display = 'block';
        (metaForm.querySelector('#title') as HTMLInputElement).value =
            file.name.replace(/\.[^/.]+$/, '');

        uploadModeHint.textContent = isLarge
            ? `📦 Large file detected — will upload in chunks (5 MB each)`
            : `⚡ Small file — single-shot upload`;
    }

    el.querySelector('#cancel-btn')?.addEventListener('click', () => navigate('/'));

    el.querySelector('#upload-btn')?.addEventListener('click', async () => {
        if (!selectedFile) return;

        const title       = (el.querySelector('#title') as HTMLInputElement).value.trim();
        const description = (el.querySelector('#description') as HTMLTextAreaElement).value.trim();
        const tagsRaw     = (el.querySelector('#tags') as HTMLInputElement).value;
        const tags        = tagsRaw.split(',').map(t => t.trim()).filter(Boolean);

        if (!title) { toast('Title is required', 'error'); return; }

        metaForm.style.display  = 'none';
        progressBox.style.display = 'block';
        progressFilename.textContent = selectedFile.name;

        try {
            const video = await uploadVideo(selectedFile, {
                title,
                description,
                tags,
                onProgress(percent, label) {
                    progressBar.style.width    = `${percent}%`;
                    progressPercent.textContent = `${percent}%`;
                    progressLabel.textContent  = label;
                },
            });

            toast('Video uploaded! Processing in background.', 'success');
            setTimeout(() => navigate(`/video/${video.id}`), 1200);
        } catch (e: any) {
            toast(e.message, 'error');
            metaForm.style.display    = 'block';
            progressBox.style.display = 'none';
        }
    });

    return el;
}