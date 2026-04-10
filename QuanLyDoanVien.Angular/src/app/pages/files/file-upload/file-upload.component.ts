import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpEventType } from '@angular/common/http';
import { ApiService } from '../../../core/services/api.service';
import { FileAttachment, ExcelParseResult, ExcelSheet } from '../../../core/models/models';

interface QueuedFile {
  file: File;
  status: 'pending' | 'uploading' | 'done' | 'error';
  progress: number;
  error?: string;
  result?: any;
}

@Component({ selector: 'app-file-upload', templateUrl: './file-upload.component.html' })
export class FileUploadComponent implements OnInit {
  // Multi-file queue
  fileQueue: QueuedFile[] = [];
  description = '';
  module = 'HE_THONG';
  uploading = false;
  uploadProgress = 0;
  dragOver = false;

  // Preview state (shows after clicking preview on a file or after upload)
  previewResult: ExcelParseResult | null = null;
  selectedSheet: ExcelSheet | null = null;
  previewFileObj: any = null;
  previewLoading = false;

  recentFiles: FileAttachment[] = [];
  loadingFiles = true;

  units: any[] = [];
  selectedUnitId: number | null = null;

  constructor(
    protected api: ApiService,
    protected router: Router,
    protected route: ActivatedRoute,
    protected snack: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.module = this.route.snapshot.data['module'] || 'HE_THONG';
    this.loadFiles();
    if (this.module === 'DOAN_VIEN') {
      this.loadUnits();
    }
  }

  loadUnits() {
    this.api.getUnits({ page: 1, pageSize: 500 }).subscribe(r => this.units = r.items || []);
  }

  loadFiles() {
    this.loadingFiles = true;
    this.api.getFiles({ page: 1, pageSize: 10 }).subscribe({
      next: r => { this.recentFiles = r.items; this.loadingFiles = false; },
      error: (e) => {
        this.loadingFiles = false;
        this.snack.open('Lỗi tải danh sách file', 'Đóng', { duration: 3000 });
      }
    });
  }

  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.addFiles(Array.from(input.files));
      input.value = ''; // reset để chọn lại cùng file nếu cần
    }
  }

  onDragOver(event: DragEvent) { event.preventDefault(); this.dragOver = true; }
  onDragLeave() { this.dragOver = false; }
  onDrop(event: DragEvent) {
    event.preventDefault();
    this.dragOver = false;
    if (event.dataTransfer?.files.length) {
      this.addFiles(Array.from(event.dataTransfer.files));
    }
  }

  addFiles(files: File[]) {
    const allowed = ['.xlsx', '.xls'];
    files.forEach(f => {
      const ext = f.name.substring(f.name.lastIndexOf('.')).toLowerCase();
      if (!allowed.includes(ext)) {
        this.snack.open(`"${f.name}" không phải file Excel (.xlsx, .xls)`, 'Đóng', { duration: 3000 });
        return;
      }

      if (this.fileQueue.some(q => q.file.name === f.name && q.file.size === f.size)) return;
      this.fileQueue.push({ file: f, status: 'pending', progress: 0 });
    });
  }

  removeFromQueue(idx: number) {
    this.fileQueue.splice(idx, 1);
  }

  clearQueue() {
    this.fileQueue = this.fileQueue.filter(f => f.status !== 'done');
  }

  get pendingCount() { return this.fileQueue.filter(f => f.status === 'pending').length; }
  get doneCount() { return this.fileQueue.filter(f => f.status === 'done').length; }
  get hasQueue() { return this.fileQueue.length > 0; }

  uploadAll() {
    const pending = this.fileQueue.filter(f => f.status === 'pending');
    if (!pending.length) return;

    this.uploading = true;
    this.uploadProgress = 0;

    pending.forEach(q => q.status = 'uploading');

    this.api.uploadMultipleFiles(pending.map(q => q.file), this.module).subscribe({
      next: (event: any) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress = Math.round(100 * event.loaded / event.total);
          pending.forEach(q => q.progress = this.uploadProgress);
        }
        if (event.type === HttpEventType.Response) {
          const body = event.body;
          this.uploading = false;
          this.uploadProgress = 100;

          let successCount = 0;
          (body.results || []).forEach((res: any, idx: number) => {
            const qf = pending[idx];
            if (!qf) return;
            if (res.success) {
              qf.status = 'done';
              qf.progress = 100;
              qf.result = res;
              successCount++;
              if (pending.length === 1 && res.excelData) {
                this.previewResult = res.excelData;
                this.previewFileObj = res.file;
                if (res.excelData.sheets?.length) this.selectedSheet = res.excelData.sheets[0];
              }
            } else {
              qf.status = 'error';
              qf.error = res.error || 'Upload thất bại';
            }
          });

          this.snack.open(`✅ Upload xong ${successCount}/${pending.length} file!`, 'Đóng', { duration: 3000 });
          this.loadFiles();
        }
      },
      error: (e) => {
        this.uploading = false;
        pending.forEach(q => { q.status = 'error'; q.error = 'Lỗi kết nối server'; });
        this.snack.open('❌ Upload thất bại: ' + (e.error?.message || 'Server error'), 'Đóng', { duration: 4000 });
      }
    });
  }

  previewFile(file: FileAttachment) {
    this.previewLoading = true;
    this.previewFileObj = file;
    this.previewResult = null;
    this.selectedSheet = null;
    this.api.parseExcel(file.id).subscribe({
      next: (r) => {
        this.previewResult = r;
        if (r.sheets?.length) this.selectedSheet = r.sheets[0];
        this.previewLoading = false;
      },
      error: () => { this.previewLoading = false; }
    });
  }

  previewQueued(qf: QueuedFile) {
    if (qf.result?.excelData) {
      this.previewResult = qf.result.excelData;
      this.previewFileObj = qf.result.file;
      if (qf.result.excelData.sheets?.length) this.selectedSheet = qf.result.excelData.sheets[0];
    }
  }

  selectSheet(s: ExcelSheet) { this.selectedSheet = s; }

  downloadFile(file: FileAttachment) { window.location.href = this.api.downloadUrl(file.id); }

  deleteFile(file: FileAttachment) {
    if (!confirm(`Xóa file "${file.originalName}"?`)) return;
    this.api.deleteFile(file.id).subscribe({
      next: () => { this.snack.open('Đã xóa', 'Đóng', { duration: 2500 }); this.loadFiles(); }
    });
  }

  importMembers() {
    if (!this.previewFileObj || !this.selectedSheet) return;
    this.uploading = true;
    const fileId = this.previewFileObj.id || this.previewFileObj.Id;
    const body = { fileId, sheetName: this.selectedSheet.sheetName, unitId: this.selectedUnitId };
    this.api.importMembers(body).subscribe({
      next: (res: any) => {
        this.uploading = false;
        if (res.success) {
          this.snack.open(`✅ Đã import ${res.count} đoàn viên!`, 'Đóng', { duration: 5000 });
          this.router.navigate(['/members']);
        } else {
          this.snack.open('⚠️ ' + (res.message || 'Lỗi dữ liệu'), 'Đóng', { duration: 10000 });
        }
      },
      error: (e) => {
        this.uploading = false;
        this.snack.open('❌ ' + (e.error?.message || 'Import thất bại'), 'Đóng', { duration: 5000 });
      }
    });
  }

  formatSize(bytes?: number): string {
    if (!bytes) return '0 B';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1048576).toFixed(1) + ' MB';
  }
}
