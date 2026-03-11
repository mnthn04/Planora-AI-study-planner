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
