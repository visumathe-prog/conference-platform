import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  currentUser = signal<any>(null);
  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<any> {
    return this.http.post('/api/auth/login', { email, password }).pipe(
      tap((res: any) => {
        localStorage.setItem('token', res.accessToken);
        this.currentUser.set(res.user);
      })
    );
  }

  register(data: any): Observable<any> {
    return this.http.post('/api/auth/register', data);
  }

  logout(): Observable<any> {
    return this.http.post('/api/auth/logout', {});
  }
}
