import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { UserListComponent } from './pages/users/user-list/user-list.component';
import { UserFormComponent } from './pages/users/user-form/user-form.component';
import { RoleListComponent } from './pages/roles/role-list/role-list.component';
import { RolePermissionsComponent } from './pages/roles/role-permissions/role-permissions.component';

import { MemberListComponent } from './pages/members/member-list/member-list.component';
import { MemberFormComponent } from './pages/members/member-form/member-form.component';
import { MemberGroupsComponent } from './pages/members/member-groups/member-groups.component';
import { FileUploadComponent } from './pages/files/file-upload/file-upload.component';
import { FileListComponent } from './pages/files/file-list/file-list.component';
import { AuditListComponent } from './pages/audit/audit-list/audit-list.component';

import { ProfileComponent } from './pages/profile/profile.component';
import { ChangePasswordComponent } from './pages/change-password/change-password.component';
import { UnitListComponent } from './pages/units/unit-list/unit-list.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      // He thong
      { path: 'users', component: UserListComponent },
      { path: 'users/create', component: UserFormComponent },
      { path: 'users/:id/edit', component: UserFormComponent },
      { path: 'roles', component: RoleListComponent },
      { path: 'roles/:id/permissions', component: RolePermissionsComponent },
      { path: 'files', component: FileListComponent },
      { path: 'files/upload', component: FileUploadComponent },
      { path: 'audit', component: AuditListComponent },
      // Doan vien
      { path: 'members', component: MemberListComponent },
      { path: 'members/create', component: MemberFormComponent },
      { path: 'members/import', component: FileUploadComponent, data: { module: 'DOAN_VIEN' } },
      { path: 'members/:id/edit', component: MemberFormComponent },
      { path: 'member-groups', component: MemberGroupsComponent },
      // Don vi
      { path: 'units', component: UnitListComponent },
      // Profile
      { path: 'profile', component: ProfileComponent },
      { path: 'change-password', component: ChangePasswordComponent }
    ]
  },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
