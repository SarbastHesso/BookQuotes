import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { LoginDto, RegisterDto, AuthResponse } from '../models/auth.model';
import { User } from '../models/user.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiBaseUrl}/api/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private sessionRestored = false;
  private restoreSessionRequest$: Observable<User | null> | null = null;
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.restoreSession().subscribe();
  }

  // ---------------------------------------------------------
  // LOGIN
  // ---------------------------------------------------------
  login(dto: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, dto, { withCredentials: true }).pipe(
      tap((response) => {
        const user: User = {
          userId: response.userId,
          userName: response.userName,
        };

        this.sessionRestored = true;
        this.currentUserSubject.next(user);
      }),
    );
  }

  // ---------------------------------------------------------
  // REGISTER
  // ---------------------------------------------------------
  register(dto: RegisterDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, dto, { withCredentials: true });
  }

  // ---------------------------------------------------------
  // LOGOUT
  // ---------------------------------------------------------
  logout(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/logout`, {}, { withCredentials: true }).pipe(
      catchError(() => of(void 0)),
      tap(() => {
        this.sessionRestored = true;
        this.currentUserSubject.next(null);
      }),
    );
  }

  isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  restoreSession(): Observable<User | null> {
    if (this.sessionRestored) {
      return of(this.currentUserSubject.value);
    }

    if (this.restoreSessionRequest$) {
      return this.restoreSessionRequest$;
    }

    this.restoreSessionRequest$ = this.http.get<User>(`${this.apiUrl}/me`, { withCredentials: true }).pipe(
      tap((user) => {
        this.currentUserSubject.next(user);
      }),
      map((user) => user),
      catchError(() => {
        this.currentUserSubject.next(null);
        return of(null);
      }),
      finalize(() => {
        this.sessionRestored = true;
        this.restoreSessionRequest$ = null;
      }),
      shareReplay(1),
    );

    return this.restoreSessionRequest$;
  }
}
