import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse, User, Role, Permission, MenuItem,
  Member, MemberGroup, FileAttachment, ExcelParseResult, PagedResult,
  AuditLog, Unit
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = environment.apiBase;

  constructor(private http: HttpClient) { }

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
  logout(): Observable<any> { return this.post('/auth/logout', {}); }
  changePassword(d: any): Observable<any> { return this.post('/auth/change-password', d); }

  // ─── Users ───────────────────────────────────────────────────────────────
  getUsers(p?: any): Observable<PagedResult<User>> { return this.get('/users', p); }
  getUser(id: number): Observable<any> { return this.get(`/users/${id}`); }
  createUser(d: any): Observable<any> { return this.post('/users', d); }
  updateUser(id: number, d: any): Observable<any> { return this.put(`/users/${id}`, d); }
  deleteUser(id: number): Observable<any> { return this.delete(`/users/${id}`); }

  // ─── Roles / Menus / Permissions ─────────────────────────────────────────
  getRoles(): Observable<Role[]> { return this.get('/roles'); }
  getMenus(): Observable<MenuItem[]> { return this.get('/roles/menus'); }
  getAllPermissions(): Observable<Permission[]> { return this.get('/roles/permissions'); }
  getRolePerms(id: number): Observable<any> { return this.get(`/roles/${id}/permissions`); }
  setRolePerms(id: number, d: any): Observable<any> { return this.post(`/roles/${id}/permissions`, d); }

  // ─── Categories ──────────────────────────────────────────────────────────
  getCategories(): Observable<any[]> { return this.get('/categories'); }
  getCatItems(code: string): Observable<any[]> { return this.get(`/categories/${code}/items`); }

  // ─── Files ───────────────────────────────────────────────────────────────
  getFiles(p?: any): Observable<PagedResult<FileAttachment>> { return this.get('/files', p); }
  parseExcel(id: number): Observable<ExcelParseResult> { return this.get(`/files/${id}/parse`); }
  deleteFile(id: number): Observable<any> { return this.delete(`/files/${id}`); }
  downloadUrl(id: number): string {
    const token = localStorage.getItem('qldt_token') || '';
    return `${this.base}/files/${id}/download?token=${token}`;
  }

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

  uploadMultipleFiles(files: File[], module?: string): Observable<any> {
    const token = localStorage.getItem('qldt_token') || '';
    const fd = new FormData();
    files.forEach(f => fd.append('file', f));
    if (module) fd.append('module', module);
    return this.http.post(`${this.base}/files/upload-multiple`, fd, {
      headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` }),
      reportProgress: true,
      observe: 'events'
    });
  }

  getMembers(page: number = 1, pageSize: number = 15, search: string = '', groupId?: number, level?: string, unitId?: number): Observable<PagedResult<Member>> {
    const params: any = { page, pageSize };
    if (search) params['search'] = search;
    if (groupId != null) params['groupId'] = groupId;
    if (level) params['level'] = level;
    if (unitId != null) params['unitId'] = unitId;
    return this.get<PagedResult<Member>>('/members', params);
  }
  getMember(id: number): Observable<Member> { return this.get(`/members/${id}`); }
  createMember(d: any): Observable<any> { return this.post('/members', d); }
  updateMember(id: number, d: any): Observable<any> { return this.put(`/members/${id}`, d); }
  deleteMember(id: number): Observable<any> { return this.delete(`/members/${id}`); }
  deleteMembers(ids: number[]): Observable<any> { return this.post('/members/delete-multiple', ids); }
  deleteAllMembers(): Observable<any> { return this.post('/members/delete-all', {}); }
  getMemberGroups(): Observable<MemberGroup[]> { return this.get('/members/groups'); }
  createMemberGroup(d: any): Observable<any> { return this.post('/members/groups', d); }
  updateMemberGroup(id: number, d: any): Observable<any> { return this.put(`/members/groups/${id}`, d); }
  getMemberStats(groupId?: number, level?: string, unitId?: number): Observable<any> {
    const params: any = {};
    if (groupId != null) params['groupId'] = groupId;
    if (level) params['level'] = level;
    if (unitId != null) params['unitId'] = unitId;
    return this.get('/members/stats', params);
  }
  importMembers(d: any): Observable<any> { return this.post('/members/import', d); }
  exportMembersUrl(search: string): string {
    const token = localStorage.getItem('qldt_token') || '';
    return `${this.base}/members/export?search=${search}&token=${token}`;
  }

  getUnits(p?: any): Observable<any> { return this.get('/units', p); }
  getUnit(id: number): Observable<any> { return this.get(`/units/${id}`); }
  createUnit(d: any): Observable<any> { return this.post('/units', d); }
  updateUnit(id: number, d: any): Observable<any> { return this.put(`/units/${id}`, d); }
  deleteUnit(id: number): Observable<any> { return this.delete(`/units/${id}`); }
  importUnit(id: number, d: any): Observable<any> { return this.post(`/units/${id}/import`, d); }
  importUnitMultiple(id: number, d: any): Observable<any> { return this.post(`/units/${id}/import-multiple`, d); }
  deleteUnits(ids: number[]): Observable<any> { return this.post('/units/delete-multiple', ids); }
  getUnitSummary(id: number): Observable<any> { return this.get(`/units/${id}/summary`); }
  getCombinedSummary(ids: number[]): Observable<any> { return this.post('/units/combined-summary', ids); }
  getUnitStats(unitId?: number): Observable<any> {
    const params: any = {};
    if (unitId != null) params['unitId'] = unitId;
    return this.get('/units/stats', params);
  }
  exportUnitUrl(id: number): string {
    const token = localStorage.getItem('qldt_token') || '';
    return `${this.base}/units/${id}/export?token=${token}`;
  }
  exportAllUnitsUrl(): string {
    const token = localStorage.getItem('qldt_token') || '';
    return `${this.base}/units/export-all?token=${token}`;
  }

  // Audit 
  getAuditLogs(page = 1, pageSize = 20, search = ''): Observable<PagedResult<AuditLog>> {
    return this.get<PagedResult<AuditLog>>('/audit', { page, pageSize, search });
  }
}
