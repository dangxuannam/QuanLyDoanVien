import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Role } from '../../../core/models/models';

@Component({ selector: 'app-role-list', templateUrl: './role-list.component.html' })
export class RoleListComponent implements OnInit {
  roles: Role[] = [];
  loading = false;
  displayedColumns = ['roleName', 'roleCode', 'description', 'actions'];

  constructor(private api: ApiService, private router: Router, private snack: MatSnackBar) {}

  ngOnInit(): void { this.load(); }

  load() {
    this.loading = true;
    this.api.getRoles().subscribe({
      next: r => { this.roles = r; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  managePermissions(id: number) {
    this.router.navigate(['/roles', id, 'permissions']);
  }
}
