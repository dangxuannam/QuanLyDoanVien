import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { FileAttachment, ExcelParseResult, ExcelSheet } from '../../../core/models/models';

@Component({ selector: 'app-file-upload', templateUrl: './file-upload.component.html' })
export class FileUploadComponent implements OnInit {
  selectedFile: File | null = null;
  description = '';
  module = 'HE_THONG';
  uploading = false;
  dragOver = false;

  uploadResult: any = null;
  previewResult: ExcelParseResult | null = null;
  selectedSheet: ExcelSheet | null = null;
  previewFileObj: any = null;

  recentFiles: FileAttachment[] = [];
  loadingFiles = true;
  previewLoading = false;

  constructor(
    protected api: ApiService,
    protected router: Router,
    protected route: ActivatedRoute,
    protected snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.module = this.route.snapshot.data['module'] || 'HE_THONG';
    this.loadFiles();
  }

  loadFiles() {
    this.loadingFiles = true;
    this.api.getFiles({ page: 1, pageSize: 10 }).subscribe({
      next: r => { this.recentFiles = r.items; this.loadingFiles = false; },
      error: (e) => { 
        this.loadingFiles = false; 
        console.error(e);
        this.snack.open('Lỗi tải danh sách file: ' + (e.message || 'Server error'), 'Đóng');
      }
    });
  }

  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
      this.uploadResult = null;
      this.previewResult = null;
    }
  }

  onDragOver(event: DragEvent) { event.preventDefault(); this.dragOver = true; }
  onDragLeave() { this.dragOver = false; }
  onDrop(event: DragEvent) {
    event.preventDefault(); this.dragOver = false;
    if (event.dataTransfer?.files.length) {
      this.selectedFile = event.dataTransfer.files[0];
      this.uploadResult = null; this.previewResult = null;
    }
  }

  upload() {
    if (!this.selectedFile) return;
    this.uploading = true;
    this.api.uploadFile(this.selectedFile, this.module, this.description).subscribe({
      next: (res) => {
        this.uploading = false;
        this.uploadResult = res;
        this.previewFileObj = res.file;
        if (res.excelData) {
          this.previewResult = res.excelData;
          if (res.excelData.sheets?.length) this.selectedSheet = res.excelData.sheets[0];
        }
        this.snack.open(' Upload thành công!', 'Đóng', { duration: 3000, panelClass: 'snack-success' });
        this.loadFiles();
      },
      error: (e) => {
        this.uploading = false;
        this.snack.open(e.error?.message || ' Upload thất bại', 'Đóng', { duration: 4000, panelClass: 'snack-error' });
      }
    });
  }

  previewFile(file: FileAttachment) {
    this.previewLoading = true;
    this.previewFileObj = file;
    this.previewResult = null; this.selectedSheet = null;
    this.api.parseExcel(file.id).subscribe({
      next: (r) => {
        this.previewResult = r;
        if (r.sheets?.length) this.selectedSheet = r.sheets[0];
        this.previewLoading = false;
      },
      error: () => { this.previewLoading = false; }
    });
  }

  selectSheet(s: ExcelSheet) { this.selectedSheet = s; }

  downloadFile(file: FileAttachment) { window.location.href = this.api.downloadUrl(file.id); }

  deleteFile(file: FileAttachment) {
    if (!confirm(`Xóa file "${file.originalName}"?`)) return;
    this.api.deleteFile(file.id).subscribe({
      next: () => { this.snack.open('Đã xóa', 'Đóng', { duration: 2500 }); this.loadFiles(); }
    });
  }

  formatSize(bytes?: number): string {
    if (!bytes) return '0 B';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return (bytes/1024).toFixed(1) + ' KB';
    return (bytes/1048576).toFixed(1) + ' MB';
  }

  importMembers() {
    if (!this.previewFileObj || !this.selectedSheet) return;
    this.uploading = true;
    const fileId = this.previewFileObj.id || this.previewFileObj.Id;
    const body = { fileId: fileId, sheetName: this.selectedSheet.sheetName };
    this.api.importMembers(body).subscribe({
      next: (res: any) => {
        this.uploading = false;
        if (res.success) {
          this.snack.open(` Đã import thành công ${res.count} đoàn viên!`, 'Đóng', { duration: 5000 });
          this.router.navigate(['/members']);
        } else {
          this.snack.open(' ' + (res.message || 'Lỗi dữ liệu'), 'Đóng', { duration: 10000 });
        }
      },
      error: (e) => {
        this.uploading = false;
        this.snack.open(e.error?.message || ' Import thất bại', 'Đóng', { duration: 5000 });
      }
    });
  }
}
