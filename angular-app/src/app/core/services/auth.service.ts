import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  expiresIn: number;
  tokenType: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  
  private readonly baseUrl = '/api/auth';
  
  // Reactive state using signals
  currentUser = signal<User | null>(null);
  isAuthenticated = signal(false);
  isLoading = signal(false);
  
  constructor() {
    this.loadStoredUser();
  }
  
  /**
   * Login user with email and password
   */
  login(email: string, password: string): Observable<AuthResponse> {
    this.isLoading.set(true);
    
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, { email, password })
      .pipe(
        tap(response => {
          this.handleAuthSuccess(response);
          this.toastr.success(`Welcome back, ${response.user.firstName}!`, 'Login Successful');
        }),
        catchError(error => {
          this.isLoading.set(false);
          this.toastr.error(error.error?.message || 'Login failed', 'Error');
          return throwError(() => error);
        })
      );
  }
  
  /**
   * Register new user
   */
  register(userData: any): Observable<any> {
    this.isLoading.set(true);
    
    return this.http.post(`${this.baseUrl}/register`, userData)
      .pipe(
        tap(() => {
          this.isLoading.set(false);
          this.toastr.success('Registration successful! Please check your email to confirm your account.', 'Success');
          this.router.navigate(['/login']);
        }),
        catchError(error => {
          this.isLoading.set(false);
          this.toastr.error(error.error?.message || 'Registration failed', 'Error');
          return throwError(() => error);
        })
      );
  }
  
  /**
   * Refresh access token
   */
  refreshToken(): Observable<{ accessToken: string }> {
    return this.http.post<{ accessToken: string }>(`${this.baseUrl}/refresh`, {})
      .pipe(
        tap(response => {
          localStorage.setItem('accessToken', response.accessToken);
        })
      );
  }
  
  /**
   * Logout user
   */
  logout(): Observable<any> {
    return this.http.post(`${this.baseUrl}/logout`, {})
      .pipe(
        tap(() => {
          this.clearAuth();
          this.toastr.info('You have been logged out', 'Goodbye');
          this.router.navigate(['/login']);
        })
      );
  }
  
  /**
   * Handle successful authentication
   */
  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem('accessToken', response.accessToken);
    localStorage.setItem('user', JSON.stringify(response.user));
    this.currentUser.set(response.user);
    this.isAuthenticated.set(true);
    this.isLoading.set(false);
    this.router.navigate(['/dashboard']);
  }
  
  /**
   * Load stored user from localStorage
   */
  private loadStoredUser(): void {
    const token = localStorage.getItem('accessToken');
    const userStr = localStorage.getItem('user');
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUser.set(user);
        this.isAuthenticated.set(true);
      } catch (e) {
        this.clearAuth();
      }
    }
  }
  
  /**
   * Clear authentication data
   */
  private clearAuth(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }
}
