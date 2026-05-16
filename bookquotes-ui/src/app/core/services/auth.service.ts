import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginDto, RegisterDto, AuthResponse } from '../models/auth.model';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = '/api/auth';
  private storageKey = 'user';
  private legacyTokenKey = 'token';
  private legacyUserIdKey = 'userId';
  private legacyUserNameKey = 'userName';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  // ---------------------------------------------------------
  // LOGIN
  // ---------------------------------------------------------
  login(dto: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, dto).pipe(
      tap((response) => {
        const user: User = {
          userId: response.userId,
          userName: response.userName,
          token: response.token,
        };

        this.saveUser(user);
        this.currentUserSubject.next(user);
      }),
    );
  }

  // ---------------------------------------------------------
  // REGISTER
  // ---------------------------------------------------------
  register(dto: RegisterDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, dto);
  }

  // ---------------------------------------------------------
  // LOGOUT
  // ---------------------------------------------------------
  logout(): void {
    localStorage.removeItem(this.storageKey);
    localStorage.removeItem(this.legacyTokenKey);
    localStorage.removeItem(this.legacyUserIdKey);
    localStorage.removeItem(this.legacyUserNameKey);
    this.currentUserSubject.next(null);
  }

  // ---------------------------------------------------------
  // TOKEN + USER HELPERS
  // ---------------------------------------------------------
  getToken(): string | null {
    return this.currentUserSubject.value?.token ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private saveUser(user: User): void {
    localStorage.setItem(this.storageKey, JSON.stringify(user));
    localStorage.removeItem(this.legacyTokenKey);
    localStorage.removeItem(this.legacyUserIdKey);
    localStorage.removeItem(this.legacyUserNameKey);
  }

  private getUserFromStorage(): User | null {
    const data = localStorage.getItem(this.storageKey);

    if (!data) {
      return this.getLegacyUserFromStorage();
    }

    try {
      return JSON.parse(data) as User;
    } catch {
      localStorage.removeItem(this.storageKey);
      return this.getLegacyUserFromStorage();
    }
  }

  private getLegacyUserFromStorage(): User | null {
    const token = localStorage.getItem(this.legacyTokenKey);
    const userId = localStorage.getItem(this.legacyUserIdKey);
    const userName = localStorage.getItem(this.legacyUserNameKey);

    if (!token || !userId || !userName) {
      return null;
    }

    const parsedUserId = Number(userId);
    if (Number.isNaN(parsedUserId)) {
      localStorage.removeItem(this.legacyUserIdKey);
      return null;
    }

    const user: User = {
      token,
      userId: parsedUserId,
      userName,
    };

    this.saveUser(user);

    return user;
  }
}
