import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PageEvent } from '@angular/material/paginator';
import { SelectionModel } from '@angular/cdk/collections';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-unit-list',
  templateUrl: './unit-list.component.html'
})
export class UnitListComponent implements OnInit {
  @ViewChild('formTpl')    formTpl!:    TemplateRef<any>;
  @ViewChild('importTpl')  importTpl!:  TemplateRef<any>;
  @ViewChild('summaryTpl') summaryTpl!: TemplateRef<any>;
  private dlgRef?: MatDialogRef<any>;

  /* ── Danh sách đơn vị ───────────────────────────────────────────────── */
  units: any[]  = [];
  total         = 0;
  page          = 1;
  pageSize      = 15;
  search        = '';
  loading       = false;
  displayedColumns = ['select', 'unitName', 'totalMembers', 'lastImportAt', 'actions'];

  /* ── Checkbox chọn hàng loạt ─────────────────────────────────────────── */
  selection = new SelectionModel<any>(true, []);

  /* ── Form tạo / sửa ─────────────────────────────────────────────────── */
  editMode       = false;
  editingId: number | null = null;
  formData       = { unitName: '', description: '' };
  saving         = false;
  createTab      = 'manual';
  createTabIndex = 0;
  inlineFile: File | null = null;
  inlineUploading = false;

  /* ── Upload / Import ─────────────────────────────────────────────────── */
  files: any[]     = [];
  filesLoaded      = false;
  importingUnit: any = null;
  selectedFileIds: number[] = [];
  sheetName      = 'Sheet1';
  importing      = false;
  importResult: any = null;

  /* ── Xem tổng hợp đơn lẻ (qua dialog) ──────────────────────────────── */
  summaryUnit: any  = null;
  summaryData: any  = null;
  loadingSummary    = false;
  selectedMetricDlg = 'all';   // combobox trong dialog

  /* ── TỔNG HỢP GỘP NGAY TRÊN TRANG ─────────────────────────────────── */
  combinedData: any       = null;
  combinedUnitNames: string[] = [];
  loadingCombined         = false;
  showCombinedPanel       = false;
  selectedMetric          = 'all';   // combobox trên trang chính

  /* ── Danh sách chỉ tiêu ─────────────────────────────────────────────── */
  readonly METRICS = [
    { value: 'all',        label: '— Tất cả chỉ tiêu —' },
    { value: 'gender',     label: '👤 Giới tính' },
    { value: 'age',        label: '🎂 Độ tuổi' },
    { value: 'ethnicity',  label: '🌏 Dân tộc' },
    { value: 'religion',   label: '⛪ Tôn giáo' },
    { value: 'profession', label: '💼 Nghề nghiệp' },
    { value: 'education',  label: '📚 Học vấn' },
    { value: 'expertise',  label: '🎓 Chuyên môn' },
    { value: 'political',  label: '⚖️ Lý luận chính trị' },
    { value: 'party',      label: '⭐ Đảng viên' },
    { value: 'communist',  label: '🏛️ Cấp ủy' },
    { value: 'position',   label: '🎖️ Chức vụ chủ chốt' },
  ];

  constructor(
    private api:    ApiService,
    private dialog: MatDialog,
    private snack:  MatSnackBar
  ) {}

  ngOnInit() { this.load(); }

  /* ──────────────────── DANH SÁCH ─────────────────────────────────────── */
  load() {
    this.loading = true;
    this.selection.clear();
    this.showCombinedPanel = false;
    this.combinedData = null;
    this.api.getUnits({ page: this.page, pageSize: this.pageSize, search: this.search }).subscribe({
      next: r => { this.units = r.items; this.total = r.total; this.loading = false; },
      error: () => this.loading = false
    });
  }
  onSearch()    { this.page = 1; this.load(); }
  onPageChange(e: PageEvent) { this.page = e.pageIndex + 1; this.pageSize = e.pageSize; this.load(); }

  /* ──── checkbox ──────────────────────────────────────────────────────── */
  isAllSelected() { return this.units.length > 0 && this.selection.selected.length === this.units.length; }
  masterToggle()  {
    if (this.isAllSelected()) { this.selection.clear(); }
    else { this.units.forEach(u => this.selection.select(u)); }
    this.closeCombinedPanel();
  }
  onRowToggle(u: any) {
    this.selection.toggle(u);
    this.closeCombinedPanel();
  }

  /* ──── TỔng hợp gộp ngay trên trang ─────────────────────────────────── */
  viewCombinedSummary() {
    const selected = this.selection.selected;
    if (!selected.length) return;
    const ids = selected.map(u => u.id);
    this.loadingCombined = true;
    this.showCombinedPanel = true;
    this.combinedData = null;

    this.api.getCombinedSummary(ids).subscribe({
      next: (r) => {
        this.loadingCombined = false;
        if (r.hasSummary) {
          this.combinedData = r.summary;
          this.combinedUnitNames = r.unitNames || [];
        } else {
          this.snack.open(r.message || 'Chưa có dữ liệu.', 'Đóng', { duration: 4000 });
          this.showCombinedPanel = false;
        }
      },
      error: () => {
        this.loadingCombined = false;
        this.showCombinedPanel = false;
        this.snack.open('Lỗi khi tải dữ liệu tổng hợp.', 'Đóng', { duration: 4000 });
      }
    });
  }

  closeCombinedPanel() {
    this.showCombinedPanel = false;
    this.combinedData = null;
  }

  showSection(key: string): boolean {
    return this.selectedMetric === 'all' || this.selectedMetric === key;
  }

  /* ──────────────────── THÊM / SỬA ───────────────────────────────────── */
  openCreate() {
    this.editMode = false; this.editingId = null;
    this.formData = { unitName: '', description: '' };
    this.createTab = 'manual'; this.inlineFile = null; this.importResult = null;
    if (!this.filesLoaded) this.loadFiles();
    this.dlgRef = this.dialog.open(this.formTpl, { width: '540px', maxHeight: '90vh', panelClass: 'unit-mat-dialog' });
  }
  openEdit(u: any) {
    this.editMode = true; this.editingId = u.id;
    this.formData = { unitName: u.unitName, description: u.description || '' };
    this.dlgRef = this.dialog.open(this.formTpl, { width: '540px', maxHeight: '90vh', panelClass: 'unit-mat-dialog' });
  }
  onFileSelected(e: Event) { this.inlineFile = (e.target as HTMLInputElement).files?.[0] ?? null; }

  saveUnit() {
    if (!this.formData.unitName.trim()) { this.snack.open('Vui lòng nhập tên đơn vị.', 'Đóng', { duration: 3000 }); return; }
    this.saving = true;
    if (this.editMode && this.editingId) {
      this.api.updateUnit(this.editingId, this.formData).subscribe({
        next: () => { this.snack.open('Đã cập nhật.', 'Đóng', { duration: 3000 }); this.dlgRef?.close(); this.load(); this.saving = false; },
        error: (e) => { this.snack.open(e.error?.message || 'Lỗi', 'Đóng', { duration: 4000 }); this.saving = false; }
      });
    } else {
      this.api.createUnit(this.formData).subscribe({
        next: (r) => {
          const newId = r.id;
          if (this.createTab === 'file' && this.inlineFile) {
            this.inlineUploading = true;
            this.api.uploadFile(this.inlineFile, 'DON_VI').subscribe({
              next: (fRes) => {
                const fileId = fRes.file?.id || fRes.file?.Id || fRes.id;
                this.api.importUnit(newId, { fileId, sheetName: this.sheetName }).subscribe({
                  next: (iRes) => {
                    this.saving = false; this.inlineUploading = false; this.dlgRef?.close(); this.load();
                    this.snack.open(`Đã tạo và tổng hợp ${iRes.count} bản ghi.`, 'Đóng', { duration: 4000 });
                  },
                  error: () => { this.saving = false; this.inlineUploading = false; this.dlgRef?.close(); this.load(); this.snack.open('Tạo thành công, import lỗi.', 'Đóng', { duration: 4000 }); }
                });
              },
              error: () => { this.saving = false; this.inlineUploading = false; this.dlgRef?.close(); this.load(); this.snack.open('Tạo thành công, upload lỗi.', 'Đóng', { duration: 4000 }); }
            });
          } else {
            this.saving = false; this.dlgRef?.close(); this.load();
            this.snack.open('Đã thêm đơn vị mới.', 'Đóng', { duration: 3000 });
          }
        },
        error: (e) => { this.snack.open(e.error?.message || 'Lỗi', 'Đóng', { duration: 4000 }); this.saving = false; }
      });
    }
  }

  /* ──────────────────── XÓA ──────────────────────────────────────────── */
  deleteUnit(u: any) {
    if (!confirm(`Xóa đơn vị "${u.unitName}"?`)) return;
    this.api.deleteUnit(u.id).subscribe({ next: () => { this.snack.open('Đã xóa.', 'Đóng', { duration: 3000 }); this.load(); } });
  }
  bulkDelete() {
    const sel = this.selection.selected;
    if (!confirm(`Xóa ${sel.length} đơn vị đã chọn?`)) return;
    this.api.deleteUnits(sel.map(u => u.id)).subscribe({
      next: (r) => { this.snack.open(r.message, 'Đóng', { duration: 4000 }); this.load(); }
    });
  }

  /* ──────────────────── IMPORT ────────────────────────────────────────── */
  loadFiles() {
    this.api.getFiles({ page: 1, pageSize: 200 }).subscribe({ next: r => { this.files = r.items; this.filesLoaded = true; } });
  }
  openImport(u: any) {
    this.importingUnit = u; this.selectedFileIds = [];
    this.sheetName = 'Sheet1'; this.importing = false; this.importResult = null;
    if (!this.filesLoaded) this.loadFiles();
    this.dlgRef = this.dialog.open(this.importTpl, { width: '560px', maxHeight: '85vh', panelClass: 'unit-mat-dialog' });
  }
  doImportMultiple() {
    if (!this.selectedFileIds.length || !this.importingUnit) return;
    this.importing = true; this.importResult = null;
    this.api.importUnitMultiple(this.importingUnit.id, { fileIds: this.selectedFileIds, sheetName: this.sheetName }).subscribe({
      next: (r) => { this.importing = false; this.importResult = r; this.load(); },
      error: (e) => { this.importing = false; this.importResult = { success: false, message: e.error?.message || 'Lỗi.' }; }
    });
  }

  /* ──────────────────── XEM TỔNG HỢP (DIALOG ĐƠN LẺ) ────────────────── */
  openSummary(u: any) {
    this.summaryUnit = u; this.summaryData = null; this.loadingSummary = true; this.selectedMetricDlg = 'all';
    this.dlgRef = this.dialog.open(this.summaryTpl, { width: '900px', maxHeight: '90vh', panelClass: 'unit-mat-dialog' });
    this.api.getUnitSummary(u.id).subscribe({
      next: (r) => { this.loadingSummary = false; this.summaryData = r.hasSummary ? r.summary : null; },
      error: () => { this.loadingSummary = false; }
    });
  }
  showSectionDlg(key: string) { return this.selectedMetricDlg === 'all' || this.selectedMetricDlg === key; }

  /* ──────────────────── XUẤT EXCEL ───────────────────────────────────── */
  exportUnit(u: any) { window.location.href = this.api.exportUnitUrl(u.id); }
  exportAll()        { window.location.href = this.api.exportAllUnitsUrl(); }

  /* ── Helpers ─────────────────────────────────────────────────────────── */
  dictKeys(d: any)  { return d ? Object.keys(d) : []; }
  getFileName(id: number) { return this.files.find(f => f.id === id)?.originalName ?? `File #${id}`; }
  closeDlg()        { this.dlgRef?.close(); }
}
