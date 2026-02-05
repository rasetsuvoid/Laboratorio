import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { throwError } from 'rxjs';

export interface LoginResponse {
  expiresAt: string;
  user: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly authenticated = signal<boolean>(false);

  constructor(private http: HttpClient) {}

  login(apiKey: string) {
    const trimmed = apiKey?.trim();
    if (!trimmed) {
      return throwError(() => new Error('ApiKey requerida.'));
    }

    return this.http.post<LoginResponse>('/auth/login', { apiKey: trimmed }).pipe(
      tap(() => {
        this.authenticated.set(true);
      })
    );
  }

  logout() {
    return this.http.post('/auth/logout', {}).pipe(
      tap(() => {
        this.authenticated.set(false);
      })
    );
  }
}
