import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  get<T>(url: string): Observable<T> {
    return this.http.get<T>(`/api/${url}`);
  }

  post<T>(url: string, data: any): Observable<T> {
    return this.http.post<T>(`/api/${url}`, data);
  }

  put<T>(url: string, data: any): Observable<T> {
    return this.http.put<T>(`/api/${url}`, data);
  }

  delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(`/api/${url}`);
  }
}
