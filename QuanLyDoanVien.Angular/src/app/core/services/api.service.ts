import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse, User, Role, Permission, MenuItem, Project, OutsideProject,
  Member, MemberGroup, FileAttachment, ExcelParseResult, PagedResult,
  AuditLog, MapProject, KTXHIndicator, KTXHReport
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = environment.apiBase;

  constructor(private http: HttpClient) {}

  private headers(): HttpHeaders {
    const token = localStorage.getItem('qldt_token') || '';
    return new HttpHeaders({ 'Authorization': token ? `Bearer ${token}` : '' });
  }

  private get<T>(url: string, params?: any): Observable<T> {
    return this.http.get<T>(`${this.base}${url}`, { headers: this.headers(), params });
  }
  private post<T>(url: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.base}${url}`, body, { headers: this.headers() });
  }
  private put<T>(url: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.base}${url}`, body, { headers: this.headers() });
  }
  private delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(`${this.base}${url}`, { headers: this.headers() });
  }

  // ─── Auth ────────────────────────────────────────────────────────────────
  login(creds: { username: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/auth/login`, creds);
  }
  logout(): Observable<any>     { return this.post('/auth/logout', {}); }
  changePassword(d: any): Observable<any> { return this.post('/auth/change-password', d); }

  // ─── Users ───────────────────────────────────────────────────────────────
  getUsers(p?: any): Observable<PagedResult<User>>  { return this.get('/users', p); }
  getUser(id: number): Observable<any>              { return this.get(`/users/${id}`); }
  createUser(d: any): Observable<any>               { return this.post('/users', d); }
  updateUser(id: number, d: any): Observable<any>   { return this.put(`/users/${id}`, d); }
  deleteUser(id: number): Observable<any>           { return this.delete(`/users/${id}`); }

  // ─── Roles / Menus / Permissions ─────────────────────────────────────────
  getRoles(): Observable<Role[]>                    { return this.get('/roles'); }
  getMenus(): Observable<MenuItem[]>                { return this.get('/roles/menus'); }
  getAllPermissions(): Observable<Permission[]>      { return this.get('/roles/permissions'); }
  getRolePerms(id: number): Observable<any>         { return this.get(`/roles/${id}/permissions`); }
  setRolePerms(id: number, d: any): Observable<any> { return this.post(`/roles/${id}/permissions`, d); }

  // ─── Categories ──────────────────────────────────────────────────────────
  getCategories(): Observable<any[]>                { return this.get('/categories'); }
  getCatItems(code: string): Observable<any[]>      { return this.get(`/categories/${code}/items`); }

  // ─── Files ───────────────────────────────────────────────────────────────
  getFiles(p?: any): Observable<PagedResult<FileAttachment>> { return this.get('/files', p); }
  parseExcel(id: number): Observable<ExcelParseResult>      { return this.get(`/files/${id}/parse`); }
  deleteFile(id: number): Observable<any>                   { return this.delete(`/files/${id}`); }
  downloadUrl(id: number): string                           { return `${this.base}/files/${id}/download`; }

  uploadFile(file: File, module?: string, description?: string): Observable<any> {
    const token = localStorage.getItem('qldt_token') || '';
    const fd = new FormData();
    fd.append('file', file);
    if (module) fd.append('module', module);
    if (description) fd.append('description', description);
    return this.http.post(`${this.base}/files/upload`, fd, {
      headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` })
    });
  }

  // ─── Module 1 – Đầu tư công ──────────────────────────────────────────────
  getProjects(p?: any): Observable<PagedResult<Project>> { return this.get('/projects', p); }
  getProject(id: number): Observable<any>               { return this.get(`/projects/${id}`); }
  createProject(d: any): Observable<any>                { return this.post('/projects', d); }
  updateProject(id: number, d: any): Observable<any>    { return this.put(`/projects/${id}`, d); }
  deleteProject(id: number): Observable<any>            { return this.delete(`/projects/${id}`); }

  addProjectFund(id: number, d: any): Observable<any>   { return this.post(`/projects/${id}/funds`, d); }
  addDisbursement(id: number, d: any): Observable<any>  { return this.post(`/projects/${id}/disbursements`, d); }

  // ─── Module 2 – Ngoài ngân sách ──────────────────────────────────────────
  getOutsideProjects(p?: any): Observable<PagedResult<OutsideProject>> { return this.get('/outside-projects', p); }
  getOutsideProject(id: number): Observable<any>        { return this.get(`/outside-projects/${id}`); }
  createOutside(d: any): Observable<any>                { return this.post('/outside-projects', d); }
  updateOutside(id: number, d: any): Observable<any>    { return this.put(`/outside-projects/${id}`, d); }

  // ─── Module 4 – Map ──────────────────────────────────────────────────────
  getMapProjects(p?: any): Observable<MapProject[]>     { return this.get('/map-projects', p); }
  updateMapProject(id: number, d: any): Observable<any> { return this.put(`/map-projects/${id}`, d); }

  // ─── Module 5 – KTXH ─────────────────────────────────────────────────────
  getIndicators(): Observable<any>                      { return this.get('/ktxh/indicators'); }
  getKTXHReports(p?: any): Observable<KTXHReport[]>    { return this.get('/ktxh/reports', p); }
  addKTXHReport(d: any): Observable<any>               { return this.post('/ktxh/reports', d); }

  // ─── Members – Đoàn viên ──────────────────────────────────────────────────
  getMembers(page: number = 1, pageSize: number = 15, search: string = ''): Observable<PagedResult<Member>> {
    return this.get<PagedResult<Member>>('/members', { page, pageSize, search });
  }
  getMember(id: number): Observable<Member>             { return this.get(`/members/${id}`); }
  createMember(d: any): Observable<any>                 { return this.post('/members', d); }
  updateMember(id: number, d: any): Observable<any>     { return this.put(`/members/${id}`, d); }
  deleteMember(id: number): Observable<any>             { return this.delete(`/members/${id}`); }
  deleteMembers(ids: number[]): Observable<any>        { return this.post('/members/delete-multiple', ids); }
  deleteAllMembers(): Observable<any>                  { return this.post('/members/delete-all', {}); }
  getMemberGroups(): Observable<MemberGroup[]>          { return this.get('/members/groups'); }
  createMemberGroup(d: any): Observable<any>            { return this.post('/members/groups', d); }
  getMemberStats(): Observable<any>                     { return this.get('/members/stats'); }
  importMembers(d: any): Observable<any>                { return this.post('/members/import', d); }
  exportMembersUrl(search: string): string {
    const token = localStorage.getItem('qldt_token') || '';
    return `${this.base}/members/export?search=${search}&token=${token}`;
  }

  // ─── Audit ────────────────────────────────────────────────────────────────
  getAuditLogs(page = 1, pageSize = 20, search = ''): Observable<PagedResult<AuditLog>> {
    return this.get<PagedResult<AuditLog>>('/audit', { page, pageSize, search });
  }
}
