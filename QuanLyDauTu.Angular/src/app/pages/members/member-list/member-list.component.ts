import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PageEvent } from '@angular/material/paginator';
import { ApiService } from '../../../core/services/api.service';
import { Member } from '../../../core/models/models';
import { SelectionModel } from '@angular/cdk/collections';

@Component({ selector: 'app-member-list', templateUrl: './member-list.component.html' })
export class MemberListComponent implements OnInit {
  members: Member[] = [];
  selection = new SelectionModel<Member>(true, []);
  total = 0;
  page = 1;
  pageSize = 15;
  search = '';
  loading = false;
  displayedColumns = [
    'select', 'fullName', 'gender', 'birthDate',
    'ethnicity', 'religion', 'profession', 'education', 'expertise',
    'politicalTheory', 'partyProb', 'partyOff', 'position', 'cardNumber', 'actions'
  ];

  constructor(private api: ApiService, private router: Router, private snack: MatSnackBar) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getMembers(this.page, this.pageSize, this.search).subscribe({
      next: (res) => {
        this.members = res.items;
        this.total = res.total;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.load();
  }

  searchMembers() {
    this.page = 1;
    this.load();
  }

  create() { this.router.navigate(['/members/create']); }
  edit(id: number) { this.router.navigate(['/members', id, 'edit']); }
  
  delete(m: Member) {
    if (!confirm(`Xóa đoàn viên ${m.fullName}?`)) return;
    this.api.deleteMember(m.id).subscribe({
      next: () => { this.snack.open('Đã xóa', 'Đóng', { duration: 3000 }); this.load(); }
    });
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.members.length;
    return numSelected === numRows;
  }

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
          this.selection.clear();
          this.load();
        },
        error: (err) => {
          console.error('Lỗi khi xóa:', err);
          this.snack.open('Lỗi khi xóa: ' + (err.error?.message || err.message || 'Lỗi không xác định'), 'Đóng', { duration: 5000 });
        }
      });
    }
  }

  exportExcel() {
    window.location.href = this.api.exportMembersUrl(this.search);
  }

  deleteAll() {
    if (confirm('CẢNH BÁO: Bạn có chắc chắn muốn xóa TOÀN BỘ danh sách đoàn viên? Hành động này không thể hoàn tác.')) {
      this.api.deleteAllMembers().subscribe({
        next: () => {
          this.snack.open('Đã xóa toàn bộ danh sách', 'Đóng', { duration: 3000 });
          this.load();
        },
        error: (err) => {
          console.error('Lỗi khi xóa tất cả:', err);
          this.snack.open('Lỗi khi xóa tất cả: ' + (err.error?.message || err.message || 'Lỗi không xác định'), 'Đóng', { duration: 5000 });
        }
      });
    }
  }
}
