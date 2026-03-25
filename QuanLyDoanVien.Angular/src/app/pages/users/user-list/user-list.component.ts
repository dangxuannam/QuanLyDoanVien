import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { User, Role } from '../../../core/models/models';

@Component({ selector: 'app-user-list', templateUrl: './user-list.component.html' })
export class UserListComponent implements OnInit {
  users: User[] = [];
  total = 0;
  page = 1;
  pageSize = 15;
  search = '';
  loading = false;
  displayedColumns = ['username','fullName','email','phone','donVi','active','actions'];

  constructor(private api: ApiService, private router: Router, private snack: MatSnackBar) {}
  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getUsers({ page: this.page, pageSize: this.pageSize, search: this.search })
      .subscribe({ 
        next: r => { this.users = r.items; this.total = r.total; this.loading = false; }, 
        error: () => { this.loading = false; } 
      });
  }

  create() { this.router.navigate(['/users/create']); }
  edit(id: number) { this.router.navigate(['/users', id, 'edit']); }

  toggleActive(u: User) {
    this.api.updateUser(u.id, { ...u, isActive: !u.isActive }).subscribe({
      next: () => { this.snack.open('Cập nhật thành công', 'Đóng', { duration: 3000 }); this.load(); }
    });
  }

  delete(u: User) {
    if (!confirm(`Xóa tài khoản ${u.username}?`)) return;
    this.api.deleteUser(u.id).subscribe({
      next: () => { this.snack.open('Đã xóa', 'Đóng', { duration: 3000 }); this.load(); },
      error: (e) => this.snack.open(e.error?.message || 'Lỗi xóa', 'Đóng', { duration: 3000 })
    });
  }
}
