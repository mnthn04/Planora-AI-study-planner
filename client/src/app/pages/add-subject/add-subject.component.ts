import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { SubjectService } from '../../services/subject.service';

@Component({
  selector: 'app-add-subject',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="animate-fade-in container-narrow">
      <header class="page-header">
        <div>
          <h1 class="page-title">{{ isEditMode ? 'Edit Subject' : 'New Subject' }}</h1>
          <p class="page-subtitle">{{ isEditMode ? 'Update your subject configuration' : 'Configure your syllabus for AI generation' }}</p>
        </div>
      </header>

      <form [formGroup]="subjectForm" (ngSubmit)="onSubmit()">
        @if (errorMessage) {
          <div class="alert alert-error animate-fade-in">
            {{ errorMessage }}
          </div>
        }
        <div class="card form-section">
          <div class="input-group">
            <label class="input-label">Subject Name</label>
            <input type="text" formControlName="subjectName" class="input-field" placeholder="e.g. Advanced Mathematics">
          </div>

          <div class="form-row">
            <div class="input-group flex-1">
              <label class="input-label">Exam Date</label>
              <input type="date" formControlName="examDate" class="input-field">
            </div>
            <div class="input-group flex-1">
              <label class="input-label">Daily Study Hours</label>
              <input type="number" formControlName="dailyStudyHours" class="input-field" placeholder="3">
            </div>
          </div>
        </div>

        <div class="topics-container" [class.disabled]="isEditMode">
          <div class="topics-header">
            <h3>Syllabus Topics</h3>
            @if (!isEditMode) {
              <button type="button" class="btn btn-ghost" (click)="addTopic()">
                <span>+</span> Add Topic
              </button>
            }
          </div>

          @if (isEditMode) {
            <p class="info-text">Note: Topics cannot be edited here. Use the plan view to manage topics.</p>
          }

          <div formArrayName="topics" class="topics-list">
            @for (topic of topics.controls; track $index) {
              <div [formGroupName]="$index" class="card topic-item animate-fade-in">
                <input type="text" formControlName="topicName" class="input-field no-border" [readonly]="isEditMode" placeholder="Chapter Name">
                <div class="topic-actions">
                  <select formControlName="difficultyLevel" class="difficulty-dropdown" [disabled]="isEditMode">
                    <option value="Easy">Easy</option>
                    <option value="Medium">Medium</option>
                    <option value="Hard">Hard</option>
                  </select>
                  @if (!isEditMode) {
                    <button type="button" class="remove-btn" (click)="removeTopic($index)">🗑️</button>
                  }
                </div>
              </div>
            }
          </div>
        </div>

        <div class="form-footer">
          <button type="button" routerLink="/dashboard" class="btn btn-ghost">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="subjectForm.invalid || loading">
            {{ loading ? 'Saving...' : (isEditMode ? 'Update Subject' : 'Finish Setup') }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container-narrow { max-width: 720px; margin: 0 auto; }
    .page-header { margin-bottom: 32px; }
    .page-title { font-size: 1.75rem; letter-spacing: -0.04em; }
    
    .form-section { padding: 32px; margin-bottom: 32px; }
    .form-row { display: flex; gap: 20px; }
    .flex-1 { flex: 1; }
    
    .topics-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .topics-header h3 { font-size: 1.1rem; font-weight: 700; }
    
    .topics-list { display: flex; flex-direction: column; gap: 12px; }
    .topic-item { display: flex; align-items: center; padding: 12px 20px; gap: 16px; border-radius: var(--radius-md); }
    .no-border { border: none !important; padding-left: 0; font-weight: 500; font-size: 1rem; flex: 1; }
    .topic-actions { display: flex; align-items: center; gap: 12px; }
    
    .difficulty-dropdown { 
      padding: 6px 12px; 
      border-radius: 8px; 
      border: 1px solid #E5E7EB; 
      font-size: 0.85rem; 
      font-weight: 600;
      color: var(--text-muted);
      cursor: pointer;
    }
    
    .remove-btn { 
      background: none; 
      border: none; 
      cursor: pointer; 
      font-size: 1.1rem; 
      padding: 4px; 
      opacity: 0.5;
      transition: opacity 0.2s;
    }
    .remove-btn:hover { opacity: 1; }

    .form-footer { display: flex; justify-content: flex-end; gap: 16px; margin-top: 48px; border-top: 1px solid #E5E7EB; padding-top: 32px; }
    
    .alert {
      padding: 12px 20px;
      border-radius: var(--radius-md);
      margin-bottom: 24px;
      font-weight: 500;
      font-size: 0.95rem;
    }
    .alert-error {
      background: #FEF2F2;
      color: #DC2626;
      border: 1px solid #FEE2E2;
    }
    .info-text { font-size: 0.875rem; color: var(--text-muted); margin-bottom: 16px; font-style: italic; }
  `]
})
export class AddSubjectComponent implements OnInit {
  private fb = inject(FormBuilder);
  private subjectService = inject(SubjectService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loading = false;
  isEditMode = false;
  subjectId: string | null = null;
  errorMessage = '';

  subjectForm = this.fb.group({
    subjectName: ['', Validators.required],
    examDate: ['', Validators.required],
    dailyStudyHours: [2, [Validators.required, Validators.min(1)]],
    topics: this.fb.array([])
  });

  get topics() {
    return this.subjectForm.get('topics') as FormArray;
  }

  ngOnInit() {
    this.subjectId = this.route.snapshot.paramMap.get('id');
    if (this.subjectId) {
      this.isEditMode = true;
      this.loadSubjectData(this.subjectId);
    } else {
      this.addTopic();
    }
  }

  loadSubjectData(id: string) {
    this.loading = true;
    this.subjectService.getSubject(id).subscribe({
      next: (subject: any) => {
        this.subjectForm.patchValue({
          subjectName: subject.subjectName,
          examDate: new Date(subject.examDate).toISOString().split('T')[0],
          dailyStudyHours: subject.dailyStudyHours
        });

        // Populate topics
        if (subject.topics && subject.topics.length > 0) {
          subject.topics.forEach((t: any) => {
            const topicGroup = this.fb.group({
              topicName: [t.topicName, Validators.required],
              difficultyLevel: [t.difficultyLevel, Validators.required]
            });
            this.topics.push(topicGroup);
          });
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = 'Could not load subject data.';
        console.error('Error loading subject:', err);
      }
    });
  }

  addTopic() {
    const topicGroup = this.fb.group({
      topicName: ['', Validators.required],
      difficultyLevel: ['Medium', Validators.required]
    });
    this.topics.push(topicGroup);
  }

  removeTopic(index: number) {
    if (this.topics.length > 1) {
      this.topics.removeAt(index);
    }
  }

  onSubmit() {
    if (this.subjectForm.valid && !this.loading) {
      this.loading = true;
      this.errorMessage = '';
      const formValue = this.subjectForm.value;

      const subjectData: any = {
        subjectName: formValue.subjectName,
        examDate: formValue.examDate,
        dailyStudyHours: formValue.dailyStudyHours
      };

      if (this.isEditMode && this.subjectId) {
        this.subjectService.updateSubject(this.subjectId, subjectData).subscribe({
          next: () => {
            this.router.navigate(['/dashboard']);
          },
          error: (err) => {
            this.loading = false;
            this.errorMessage = 'Could not update subject.';
            console.error('Error updating subject:', err);
          }
        });
      } else {
        this.subjectService.createSubject(subjectData).subscribe({
          next: (subject: any) => {
            this.saveTopics(subject.id, formValue.topics as any[]);
          },
          error: (err) => {
            this.loading = false;
            this.errorMessage = err.status === 401
              ? 'Please login to save your subject.'
              : 'Could not save subject. Please check your connection.';
            console.error('Error creating subject:', err);
          }
        });
      }
    }
  }

  private saveTopics(subjectId: string, topics: any[]) {
    let completed = 0;
    topics.forEach(t => {
      this.subjectService.addTopic({ ...t, subjectId }).subscribe({
        next: () => {
          completed++;
          if (completed === topics.length) {
            this.subjectService.generatePlan(subjectId).subscribe({
              next: () => {
                this.router.navigate(['/dashboard']);
              },
              error: (err) => {
                console.error('Error generating plan:', err);
                this.router.navigate(['/dashboard']);
              }
            });
          }
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = 'Some topics could not be saved.';
          console.error('Error adding topic:', err);
        }
      });
    });
  }
}
