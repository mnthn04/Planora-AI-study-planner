import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SubjectService } from '../../services/subject.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="animate-fade-in">
      <header class="page-header">
        <div>
          <h1 class="page-title">Overview</h1>
          <p class="page-subtitle">Welcome back, {{ (authService.user$ | async)?.name }}</p>
        </div>
        <button routerLink="/add-subject" class="btn btn-primary">
          <span>+</span> New Subject
        </button>
      </header>

      <div class="stats-overview">
        <div class="card stat-card border-glow">
          <div class="stat-label">Total Subjects</div>
          <div class="stat-value">{{ subjects.length }}</div>
        </div>
        <div class="card stat-card border-glow">
          <div class="stat-label">Active Plans</div>
          <div class="stat-value">{{ activePlansCount }}</div>
        </div>
        <div class="card stat-card border-glow">
          <div class="stat-label">Overall Progress</div>
          <div class="stat-value text-primary">{{ overallProgress }}%</div>
        </div>
      </div>

      <h2 class="section-title">My Subjects</h2>

      @if (errorMessage) {
        <div class="card error-state border-error">
          <div class="error-icon">⚠️</div>
          <h3>Could not load subjects</h3>
          <p>{{ errorMessage }}</p>
          <button (click)="loadSubjects()" class="btn btn-primary">Try Again</button>
        </div>
      } @else if (loading) {
        <div class="loading-state">
          <div class="spinner"></div>
          <p>Loading your study dashboard...</p>
        </div>
      } @else if (subjects.length === 0) {
        <div class="card empty-state border-glow">
          <div class="empty-icon">📂</div>
          <h3>No subjects yet</h3>
          <p>Create your first subject and let Gemini build a study plan for you.</p>
          <button routerLink="/add-subject" class="btn btn-primary">Add Subject</button>
        </div>
      } @else {
        <div class="subject-grid">
          @for (subject of subjects; track subject.id) {
            <div class="card subject-card" [routerLink]="['/plan', subject.id]">
              <div class="subject-status">
                @if (subject.studyPlan) {
                  <span class="status-dot active"></span> Plan Generated
                } @else {
                  <span class="status-dot inactive"></span> No Plan
                }
              </div>
              <h3 class="subject-name">{{ subject.subjectName }}</h3>
              <div class="subject-meta">
                <div class="meta-item">📅 {{ subject.examDate | date:'MMM d, y' }}</div>
                <div class="meta-item">⏱️ {{ subject.dailyStudyHours }}h/day</div>
              </div>
              
              <div class="difficulty-level">
                <div class="level-pill" [class]="subject.globalDifficulty?.toLowerCase() || 'none'">
                  {{ subject.globalDifficulty || 'Pending' }}
                </div>
              </div>

              <div class="card-actions">
                <button [routerLink]="['/edit-subject', subject.id]" (click)="$event.stopPropagation()" class="btn-icon" title="Edit Subject">✏️</button>
                <button (click)="deleteSubject($event, subject.id)" class="btn-icon btn-delete" title="Delete Subject">🗑️</button>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 40px; }
    .page-title { font-size: 2rem; letter-spacing: -0.04em; margin-bottom: 4px; }
    .page-subtitle { color: var(--text-muted); font-size: 1.1rem; }
    
    .stats-overview { display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px; margin-bottom: 48px; }
    .stat-card { padding: 24px; transition: transform 0.2s; }
    .stat-card:hover { transform: translateY(-4px); }
    .stat-label { font-size: 0.875rem; color: var(--text-muted); font-weight: 500; margin-bottom: 8px; }
    .stat-value { font-size: 1.75rem; font-weight: 700; color: var(--text-main); }
    .text-primary { color: var(--primary); }
    
    .section-title { font-size: 1.25rem; font-weight: 700; margin-bottom: 24px; color: var(--text-main); }
    
    .subject-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 24px; }
    .subject-card { padding: 24px; cursor: pointer; transition: all 0.2s; position: relative; }
    .subject-card:hover { transform: scale(1.02); border-color: var(--primary); box-shadow: 0 12px 24px -10px rgba(79, 70, 229, 0.15); }
    
    .subject-status { display: flex; align-items: center; gap: 8px; font-size: 0.75rem; font-weight: 600; color: var(--text-muted); margin-bottom: 16px; text-transform: uppercase; letter-spacing: 0.05em; }
    .status-dot { width: 8px; height: 8px; border-radius: 50%; }
    .status-dot.active { background: #10B981; box-shadow: 0 0 8px rgba(16, 185, 129, 0.4); }
    .status-dot.inactive { background: #9CA3AF; }
    
    .subject-name { font-size: 1.25rem; font-weight: 700; margin-bottom: 16px; }
    .subject-meta { display: flex; gap: 16px; margin-bottom: 24px; }
    .meta-item { font-size: 0.875rem; color: var(--text-muted); font-weight: 500; }
    
    .level-pill { display: inline-block; padding: 4px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; }
    .level-pill.hard { background: #FEE2E2; color: #EF4444; }
    .level-pill.medium { background: #FEF3C7; color: #D97706; }
    .level-pill.low { background: #DCFCE7; color: #10B981; }
    .level-pill.none { background: #F3F4F6; color: #6B7280; }

    .card-actions { position: absolute; top: 16px; right: 16px; display: flex; gap: 8px; opacity: 0; transition: opacity 0.2s; }
    .subject-card:hover .card-actions { opacity: 1; }
    .btn-icon { background: white; border: 1px solid #E5E7EB; border-radius: 8px; width: 32px; height: 32px; display: flex; align-items: center; justify-content: center; cursor: pointer; font-size: 1rem; transition: all 0.2s; }
    .btn-icon:hover { border-color: var(--primary); box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
    .btn-delete:hover { border-color: #EF4444; color: #EF4444; }

    .empty-state, .error-state { text-align: center; padding: 64px; display: flex; flex-direction: column; align-items: center; gap: 16px; }
    .empty-icon, .error-icon { font-size: 3rem; margin-bottom: 8px; }
    .empty-state p, .error-state p { color: var(--text-muted); max-width: 320px; margin-bottom: 8px; }
    .border-error { border: 1px solid #FEE2E2; background: #FEF2F2; }

    .loading-state { text-align: center; padding: 100px; color: var(--text-muted); }
    .spinner { border: 3px solid #E5E7EB; border-top-color: var(--primary); border-radius: 50%; width: 24px; height: 24px; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class DashboardComponent implements OnInit {
  subjects: any[] = [];
  loading = true;
  errorMessage = '';

  private subjectService = inject(SubjectService);
  authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  get activePlansCount() {
    return this.subjects.filter(s => s.studyPlan).length;
  }

  get overallProgress() {
    if (this.subjects.length === 0) return 0;

    let totalTasks = 0;
    let completedTasks = 0;

    this.subjects.forEach(subject => {
      const plan = subject.studyPlan;
      if (plan && plan.studyTasks) {
        totalTasks += plan.studyTasks.length;
        completedTasks += plan.studyTasks.filter((t: any) => t.status === 1 || t.status === 'Completed').length;
      }
    });

    return totalTasks === 0 ? 0 : Math.round((completedTasks / totalTasks) * 100);
  }

  ngOnInit() {
    this.loadSubjects();
  }

  loadSubjects() {
    this.loading = true;
    this.errorMessage = '';

    const obs$ = this.subjectService.getSubjects();

    obs$.subscribe({
      next: (data) => {
        this.subjects = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading subjects:', err);
        this.errorMessage = 'We couldn\'t load your subjects. Error: ' + err.message;
        this.loading = false;
        this.cdr.detectChanges();
      },
      complete: () => {
      }
    });

  }

  deleteSubject(event: Event, id: string) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this subject and its study plan?')) {
      this.subjectService.deleteSubject(id).subscribe({
        next: () => {
          this.subjects = this.subjects.filter(s => s.id !== id);
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error deleting subject:', err);
          alert('Could not delete subject.');
        }
      });
    }
  }
}
