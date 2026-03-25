import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { Role } from '../../../core/models/models';

@Component({ selector: 'app-user-form', templateUrl: './user-form.component.html' })
export class UserFormComponent implements OnInit {
  form: FormGroup;
  loading = false;
  isEdit = false;
  userId?: number;
  roles: Role[] = [];

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private router: Router,
    private route: ActivatedRoute,
    private snack: MatSnackBar
  ) {
    this.form = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: [''],
      fullName: ['', Validators.required],
      email: ['', Validators.email],
      phone: [''],
      donVi: [''],
      isAdmin: [false],
      isActive: [true],
      roleIds: [[]]
    });
  }

  ngOnInit(): void {
    this.userId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEdit = !!this.userId;

    if (this.isEdit) {
      this.form.get('username')?.disable();
      this.loadUser();
    } else {
      this.form.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    }

    this.loadRoles();
  }

  loadRoles() {
    this.api.getRoles().subscribe({ next: r => this.roles = r });
  }

  loadUser() {
    if (!this.userId) return;
    this.loading = true;
    this.api.getUser(this.userId).subscribe({
      next: (res) => {
        this.form.patchValue({
          username: res.username,
          fullName: res.fullName,
          email: res.email,
          phone: res.phone,
          donVi: res.donVi,
          isAdmin: res.isAdmin,
          isActive: res.isActive,
          roleIds: res.roles.map((r: any) => r.id)
        });
        this.loading = false;
      },
      error: () => {
        this.snack.open('Không thể tải thông tin người dùng', 'Đóng', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  save() {
    if (this.form.invalid) return;
    this.loading = true;
    const val = this.form.getRawValue();
    
    const obs = this.isEdit 
      ? this.api.updateUser(this.userId!, val)
      : this.api.createUser(val);

    obs.subscribe({
      next: () => {
        this.snack.open(this.isEdit ? 'Cập nhật thành công' : 'Tạo tài khoản thành công', 'Đóng', { duration: 3000 });
        this.router.navigate(['/users']);
      },
      error: (err) => {
        this.snack.open(err.error?.message || 'Lỗi khi lưu dữ liệu', 'Đóng', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  cancel() { this.router.navigate(['/users']); }
}
