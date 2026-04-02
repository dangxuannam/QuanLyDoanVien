import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PageEvent } from '@angular/material/paginator';
import { ApiService } from '../../../core/services/api.service';
import { Member, MemberGroup } from '../../../core/models/models';
import { SelectionModel } from '@angular/cdk/collections';

@Component({ selector: 'app-member-list', templateUrl: './member-list.component.html' })
export class MemberListComponent implements OnInit {
  members: Member[] = [];
  selection = new SelectionModel<Member>(true, []);
  total     = 0;
  page      = 1;
  pageSize  = 15;
  search    = '';
  loading   = false;

  /* ── Bộ lọc ──────────────────────────────────────────────────────── */
  groups: MemberGroup[]     = [];
  units: any[] = [];
  selectedGroupId: number | null = null;
  selectedLevel: string | null   = null;
  selectedUnitId: number | null  = null;

  /** Danh sách cấp bậc cố định */
  readonly LEVELS = [
    'Trung ương', 'Tỉnh', 'Thành phố', 'Huyện', 'Xã / Phường', 'Cơ sở'
  ];

  displayedColumns = [
    'select', 'fullName', 'gender', 'birthDate',
    'ethnicity', 'religion', 'profession', 'education', 'expertise',
    'politicalTheory', 'partyProb', 'partyOff', 'position', 'cardNumber', 'actions'
  ];

  constructor(private api: ApiService, private router: Router, private snack: MatSnackBar) {}

  ngOnInit() { this.loadGroups(); this.loadUnits(); this.load(); }

  loadGroups() {
    this.api.getMemberGroups().subscribe({ next: r => this.groups = r, error: () => {} });
  }

  loadUnits() {
    this.api.getUnits({ page: 1, pageSize: 200 }).subscribe({ next: r => this.units = r.items || [], error: () => {} });
  }

  load() {
    this.loading = true;
    this.api.getMembers(
      this.page, this.pageSize, this.search,
      this.selectedGroupId ?? undefined,
      this.selectedLevel   ?? undefined,
      this.selectedUnitId  ?? undefined
    ).subscribe({
      next: (res) => { this.members = res.items; this.total = res.total; this.loading = false; },
      error: () => this.loading = false
    });
  }

  onGroupChange() { this.page = 1; this.load(); }
  onUnitChange()  { this.page = 1; this.load(); }
  onLevelChange() {
    // Khi đổi cấp bậc → reset chi đoàn nếu chi đoàn đó không thuộc cấp đã chọn
    this.selectedGroupId = null;
    this.page = 1;
    this.load();
  }

  /** Lọc danh sách chi đoàn theo cấp bậc đang chọn */
  get filteredGroups(): MemberGroup[] {
    if (!this.selectedLevel) return this.groups;
    return this.groups.filter(g => (g as any).level === this.selectedLevel);
  }

  getGroupName(id: number | null): string {
    if (!id) return 'Chi đoàn';
    return this.groups.find(g => g.id === id)?.groupName ?? 'Chi đoàn';
  }

  getUnitName(id: number | null): string {
    if (!id) return 'Đơn vị';
    return this.units.find(u => u.id === id)?.unitName ?? 'Đơn vị';
  }

  onPageChange(event: PageEvent) {
    this.page     = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.load();
  }

  searchMembers() { this.page = 1; this.load(); }

  create() { this.router.navigate(['/members/create']); }
  edit(id: number) { this.router.navigate(['/members', id, 'edit']); }

  delete(m: Member) {
    if (!confirm(`Xóa đoàn viên ${m.fullName}?`)) return;
    this.api.deleteMember(m.id).subscribe({
      next: () => { this.snack.open('Đã xóa', 'Đóng', { duration: 3000 }); this.load(); }
    });
  }

  isAllSelected() { return this.selection.selected.length === this.members.length; }
  masterToggle() {
    this.isAllSelected() ?
      this.selection.clear() :
      this.members.forEach(row => this.selection.select(row));
  }

  deleteSelected() {
    const ids = this.selection.selected.map(m => m.id!);
    if (ids.length === 0) return;
    if (confirm(`Bạn có chắc muốn xóa ${ids.length} đoàn viên đã chọn?`)) {
      this.api.deleteMembers(ids).subscribe({
        next: () => {
          this.snack.open(`Đã xóa ${ids.length} đoàn viên`, 'Đóng', { duration: 3000 });
          this.selection.clear(); this.load();
        },
        error: (err) => this.snack.open('Lỗi khi xóa: ' + (err.error?.message || err.message), 'Đóng', { duration: 5000 })
      });
    }
  }

  exportExcel() { window.location.href = this.api.exportMembersUrl(this.search); }

  deleteAll() {
    if (confirm('CẢNH BÁO: Bạn có chắc chắn muốn xóa TOÀN BỘ danh sách đoàn viên? Hành động này không thể hoàn tác.')) {
      this.api.deleteAllMembers().subscribe({
        next: () => { this.snack.open('Đã xóa toàn bộ danh sách', 'Đóng', { duration: 3000 }); this.load(); },
        error: (err) => this.snack.open('Lỗi: ' + (err.error?.message || err.message), 'Đóng', { duration: 5000 })
      });
    }
  }
}
