import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule],
  template: `
    <div class="app-container">
      @if (authService.user$ | async; as user) {
        <aside class="sidebar">
          <div class="logo-container">
            <div class="logo-icon">P</div>
            <span class="logo-text">Planora</span>
          </div>

          <nav class="nav-menu">
            <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">
              <span class="icon">🏠</span> Dashboard
            </a>
            <a routerLink="/add-subject" routerLinkActive="active" class="nav-item">
              <span class="icon">➕</span> New Subject
            </a>
            <a routerLink="/analytics" routerLinkActive="active" class="nav-item">
              <span class="icon">📈</span> Analytics
            </a>
            <a routerLink="/settings" routerLinkActive="active" class="nav-item">
              <span class="icon">⚙️</span> Settings
            </a>
          </nav>

          <div class="user-profile">
            <div class="avatar">{{ user.name.charAt(0) }}</div>
            <div class="user-info">
              <div class="user-name">{{ user.name }}</div>
              <button (click)="logout()" class="logout-btn">Sign out</button>
            </div>
          </div>
        </aside>

        <main class="main-content">
          <router-outlet></router-outlet>
        </main>
      } @else {
        <main style="flex: 1;">
          <router-outlet></router-outlet>
        </main>
      }
    </div>
  `,
  styles: [`
    .icon { font-size: 1.1rem; }
    .user-info { display: flex; flex-direction: column; gap: 2px; }
    .user-name { font-size: 0.9rem; font-weight: 600; color: var(--text-main); }
    .logout-btn { 
      background: none; 
      border: none; 
      color: var(--text-muted); 
      font-size: 0.75rem; 
      font-weight: 500; 
      cursor: pointer; 
      text-align: left;
      padding: 0;
    }
    .logout-btn:hover { color: #EF4444; }
  `]
})
export class App {
  authService = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.authService.logout();
  }
}
