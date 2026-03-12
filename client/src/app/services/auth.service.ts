import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { Router } from '@angular/router';

export interface User {
  token: string;
  name: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = 'http://localhost:5178/api/auth';

  private userSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  user$ = this.userSubject.asObservable();

  register(data: any) {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  login(data: any) {
    return this.http.post<User>(`${this.apiUrl}/login`, data).pipe(
      tap(user => {
        localStorage.setItem('user', JSON.stringify(user));
        this.userSubject.next(user);
      })
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.userSubject.next(null);
    this.router.navigate(['/login']);
  }

  updateProfile(name: string) {
    return this.http.put<any>(`${this.apiUrl}/profile`, { name }).pipe(
      tap(res => {
        const user = this.userSubject.value;
        if (user) {
          const updatedUser = { ...user, name: res.name };
          localStorage.setItem('user', JSON.stringify(updatedUser));
          this.userSubject.next(updatedUser);
        }
      })
    );
  }

  getToken() {
    return this.userSubject.value?.token;
  }

  isLoggedIn(): boolean {
    return !!this.userSubject.value;
  }

  private getUserFromStorage(): User | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }
}
