import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({ selector: 'app-role-permissions', templateUrl: './role-permissions.component.html' })
export class RolePermissionsComponent implements OnInit {
  roleId!: number;
  role: any = {};
  permissions: any[] = [];
  loading = false;
  modules: string[] = [];
  groupedPermissions: { [key: string]: any[] } = {};

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.roleId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getRolePerms(this.roleId).subscribe({
      next: (res) => {
        this.role = res.role;
        this.permissions = res.permissions;
        this.groupPermissions();
        this.loading = false;
      },
      error: () => {
        this.snack.open('Không thể tải quyền truy cập', 'Đóng', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  groupPermissions() {
    this.modules = Array.from(new Set(this.permissions.map(p => p.module || 'Khác')));
    this.groupedPermissions = {};
    this.modules.forEach(m => {
      this.groupedPermissions[m] = this.permissions.filter(p => (p.module || 'Khác') === m);
    });
  }

  save() {
    this.loading = true;
    const selectedIds = this.permissions.filter(p => p.assigned).map(p => p.id);
    this.api.setRolePerms(this.roleId, { permissionIds: selectedIds }).subscribe({
      next: () => {
        this.snack.open('Cập nhật quyền thành công', 'Đóng', { duration: 3000 });
        this.router.navigate(['/roles']);
      },
      error: () => {
        this.snack.open('Lỗi khi lưu quyền truy cập', 'Đóng', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  cancel() { this.router.navigate(['/roles']); }

  toggleAll(module: string, event: any) {
    const checked = event.checked;
    this.groupedPermissions[module].forEach(p => p.assigned = checked);
  }

  isAllSelected(module: string) {
    return this.groupedPermissions[module].every(p => p.assigned);
  }
}
