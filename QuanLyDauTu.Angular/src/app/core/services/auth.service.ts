import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private TOKEN_KEY = 'qldt_token';
  private USER_KEY  = 'qldt_user';

  private _currentUser$ = new BehaviorSubject<AuthResponse | null>(this.getUser());
  currentUser$ = this._currentUser$.asObservable();

  saveSession(data: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, data.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(data));
    this._currentUser$.next(data);
  }

  getToken(): string | null { return localStorage.getItem(this.TOKEN_KEY); }

  getUser(): AuthResponse | null {
    try { return JSON.parse(localStorage.getItem(this.USER_KEY) || 'null'); }
    catch { return null; }
  }

  isLoggedIn(): boolean { return !!this.getToken() && !!this.getUser(); }

  hasPermission(perm: string): boolean {
    const u = this.getUser();
    if (!u) return false;
    if (u.isAdmin) return true;
    return u.permissions?.includes(perm) ?? false;
  }

  clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._currentUser$.next(null);
  }
}
