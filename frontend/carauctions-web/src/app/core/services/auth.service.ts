import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User } from '../../shared/models/auction.model';

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: Date;
  user: User;
}

interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
  isDealer?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = environment.apiUrl;

  // State signals
  private readonly _currentUser = signal<User | null>(null);
  private readonly _token = signal<string | null>(localStorage.getItem('token'));
  private readonly _isLoading = signal(false);

  // Public readonly signals
  readonly currentUser = this._currentUser.asReadonly();
  readonly token = this._token.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly isAuthenticated = computed(() => !!this._token() && !!this._currentUser());
  readonly isAdmin = computed(() => this._currentUser()?.roles.some(r => r === 'Admin') ?? false);
  readonly isDealer = computed(() => this._currentUser()?.isDealer ?? false);

  login(credentials: LoginRequest): Observable<LoginResponse> {
    this._isLoading.set(true);
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, credentials).pipe(
      tap(response => {
        this.setSession(response);
        this._isLoading.set(false);
      }),
      catchError(error => {
        this._isLoading.set(false);
        throw error;
      })
    );
  }

  register(request: RegisterRequest): Observable<LoginResponse> {
    this._isLoading.set(true);
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/register`, request).pipe(
      tap(response => {
        this.setSession(response);
        this._isLoading.set(false);
      }),
      catchError(error => {
        this._isLoading.set(false);
        throw error;
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    this._token.set(null);
    this._currentUser.set(null);
    this.router.navigate(['/']);
  }

  refreshToken(): Observable<LoginResponse | null> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      return of(null);
    }

    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/refresh`, { refreshToken }).pipe(
      tap(response => this.setSession(response)),
      catchError(() => {
        this.logout();
        return of(null);
      })
    );
  }

  loadCurrentUser(): Observable<User | null> {
    if (!this._token()) {
      return of(null);
    }

    return this.http.get<User>(`${this.baseUrl}/users/me`).pipe(
      tap(user => this._currentUser.set(user)),
      catchError(() => {
        this.logout();
        return of(null);
      })
    );
  }

  private setSession(response: LoginResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('refreshToken', response.refreshToken);
    this._token.set(response.token);
    this._currentUser.set(response.user);
  }
}
