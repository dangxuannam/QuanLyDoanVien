import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Angular Material
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';

// Layout
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { SidebarComponent } from './layout/sidebar/sidebar.component';
import { TopbarComponent } from './layout/topbar/topbar.component';

// Pages
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

const MATERIAL_MODULES = [
  MatToolbarModule, MatSidenavModule, MatListModule, MatIconModule,
  MatButtonModule, MatCardModule, MatTableModule, MatPaginatorModule,
  MatInputModule, MatFormFieldModule, MatSelectModule, MatCheckboxModule,
  MatDialogModule, MatSnackBarModule, MatProgressSpinnerModule, MatProgressBarModule,
  MatMenuModule, MatChipsModule, MatBadgeModule, MatTabsModule, MatExpansionModule,
  MatDatepickerModule, MatNativeDateModule, MatTooltipModule, MatDividerModule
];

@NgModule({
  declarations: [
    AppComponent,
    MainLayoutComponent, SidebarComponent, TopbarComponent,
    LoginComponent, DashboardComponent,
    UserListComponent, UserFormComponent,
    RoleListComponent, RolePermissionsComponent,

    MemberListComponent, MemberFormComponent, MemberGroupsComponent,
    FileUploadComponent, FileListComponent,
    AuditListComponent,
    ProfileComponent, ChangePasswordComponent
  ],
  imports: [
    BrowserModule, BrowserAnimationsModule, HttpClientModule,
    FormsModule, ReactiveFormsModule, AppRoutingModule,
    ...MATERIAL_MODULES
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
