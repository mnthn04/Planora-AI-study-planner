import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { SubjectService } from '../../services/subject.service';

@Component({
  selector: 'app-plan-view',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="animate-fade-in">
      @if (loading) {
        <div class="loading-state">
          <div class="spinner"></div>
          <p>Scanning Syllabus...</p>
        </div>
      } @else if (subject) {
        <header class="plan-header">
          <div class="header-content">
            <button routerLink="/dashboard" class="btn-back">← Back</button>
            <h1 class="subject-title">{{ subject.subjectName }}</h1>
            <div class="subject-badges">
              @if (isCompleted) {
                <span class="badge completed">✅ Completed</span>
              }
              <span class="badge">📅 {{ subject.examDate | date:'mediumDate' }}</span>
              <span class="badge">📖 {{ subject.topics.length }} Topics</span>
            </div>
          </div>
          
          <div class="header-actions">
            <button class="btn btn-ghost" [routerLink]="['/edit-subject', subject.id]">
              ✏️ Edit
            </button>
            <button class="btn btn-ghost text-error" (click)="deleteSubject()">
              🗑️ Delete
            </button>
            @if (subject.studyPlan) {
      
              <button class="btn btn-ghost" (click)="generatePlan()">
                Regenerate AI Plan
              </button>
            } @else {
              <button class="btn btn-primary" (click)="generatePlan()" [disabled]="generating">
                {{ generating ? 'Analyzing...' : 'Generate Plan' }}
              </button>
            }
          </div>
        </header>

        @if (generating) {
          <div class="card generating-card animate-fade-in">
            <div class="sparkle-icon">✨</div>
            <h2>Generating Study Plan...</h2>
            <p>We're prioritizing your topics and creating a timeline.</p>
            <div class="progress-pulse"></div>
          </div>
        } @else if (subject.studyPlan) {
          <div class="plan-layout">
            <div class="plan-main">
              <header class="section-header">
                <h2 class="column-title">Study Schedule</h2>
                <div class="progress-container card">
                  <div class="progress-details">
                    <span class="progress-label">Overall Completion</span>
                    <span class="progress-percent">{{ progressPercent | number:'1.0-0' }}%</span>
                  </div>
                  <div class="linear-progress-bg">
                    <div class="linear-progress-fill" [style.width.%]="progressPercent"></div>
                  </div>
                </div>
              </header>

              <div class="timeline-v2">
                @for (task of sortedTasks; track task.id) {
                  <article class="timeline-slot" [class.is-done]="task.status === 'Completed' || task.status === 1">
                    <div class="slot-marker">
                      <div class="marker-dot"></div>
                      <div class="marker-line"></div>
                    </div>
                    
                    <div class="card timeline-card">
                      <div class="card-content">
                        <div class="task-meta">
                          <span class="day-anchor">Day {{ task.dayNumber }}</span>
                        </div>
                        <div class="task-body">
                          <div class="topic-info">
                            <span class="topic-label">Topic:</span>
                            <h4 class="topic-name">{{ task.topicName }}</h4>
                          </div>
                          
                          @if (task.activities) {
                             <ul class="activities-list">
                               @for (activity of task.activities.split('\n'); track activity) {
                                 <li>{{ activity }}</li>
                               }
                             </ul>
                           }
                           
                           <div class="task-footer">
                             <button class="btn btn-flashcard" (click)="openFlashcards(task)">
                               🗂️ Flashcards
                             </button>
                           </div>
                         </div>
                       </div>
                      
                      <div class="card-action">
                        <label class="custom-checkbox">
                          <input type="checkbox" 
                                 [checked]="task.status === 'Completed' || task.status === 1" 
                                 (change)="toggleTask(task)">
                          <span class="checkmark"></span>
                          <span class="checkbox-text">Mark done</span>
                        </label>
                      </div>
                    </div>
                  </article>
                }
              </div>
            </div>

            <div class="sidebar-column">
              <div class="card s-topic-card">
                <h3>Syllabus Priority</h3>
                <div class="priority-list">
                  @for (topic of subject.topics; track topic.id) {
                    <div class="priority-item">
                      <span class="p-dot" [class]="topic.difficultyLevel.toLowerCase()"></span>
                      <span class="p-name">{{ topic.topicName }}</span>
                      <span class="p-lvl">{{ topic.difficultyLevel }}</span>
                    </div>
                  }
                </div>
              </div>
            </div>
          </div>
        } @else {
            <div class="card empty-plan">
             <div class="empty-icon">🧠</div>
             <h3>Ready to level up?</h3>
             <p>Your syllabus is ready. Click the button above to generate a custom day-wise study schedule.</p>
           </div>
         }
       }
     </div>

     <!-- Flashcard Modal -->
     @if (showFlashcardModal) {
       <div class="modal-overlay" (click)="closeFlashcards()">
         <div class="modal-content glass" (click)="$event.stopPropagation()">
           <header class="modal-header">
             <h2>Flashcards: {{ activeTask?.topicName }}</h2>
             <button class="btn-close" (click)="closeFlashcards()">✕</button>
           </header>
           
           <div class="modal-body">
             @if (loadingFlashcards) {
               <div class="loading-cards">
                 <div class="spinner"></div>
                 <p>AI is crafting your flashcards...</p>
               </div>
             } @else if (activeFlashcards.length > 0) {
               <div class="flashcards-grid">
                 @for (card of activeFlashcards; track card.id) {
                   <div class="flashcard" [class.flipped]="card.isFlipped" (click)="card.isFlipped = !card.isFlipped">
                     <div class="flashcard-inner">
                       <div class="flashcard-front">
                         <span class="card-label">Question</span>
                         <p>{{ card.question }}</p>
                         <span class="hint">Click to flip</span>
                       </div>
                       <div class="flashcard-back">
                         <span class="card-label">Answer</span>
                         <p>{{ card.answer }}</p>
                         <span class="hint">Click to flip back</span>
                       </div>
                     </div>
                   </div>
                 }
               </div>
             } @else {
               <p class="empty-state">No flashcards found for this topic.</p>
             }
           </div>
         </div>
       </div>
     }
   `,
  styles: [`
    .plan-header { 
      display: flex; 
      justify-content: space-between; 
      align-items: flex-end; 
      margin-bottom: 40px; 
      border-bottom: 1px solid #E5E7EB; 
      padding-bottom: 32px; 
    }
    .subject-title { font-size: 2.25rem; font-weight: 800; letter-spacing: -0.05em; margin: 8px 0; }
    .btn-back { background: none; border: none; color: var(--primary); font-weight: 700; font-size: 0.875rem; cursor: pointer; padding: 0; }
    .subject-badges { display: flex; gap: 12px; }
    .badge { padding: 4px 12px; background: #F3F4F6; border-radius: 999px; font-size: 0.8rem; font-weight: 600; color: var(--text-muted); }
    .badge.completed { background: #DCFCE7; color: #10B981; border: 1px solid #10B981; }

    .plan-layout { display: grid; grid-template-columns: 1fr 300px; gap: 48px; }
    
    .section-header { margin-bottom: 40px; }
    .column-title { font-size: 1.25rem; font-weight: 700; margin-bottom: 20px; }
    
    .progress-container { padding: 20px; border-radius: var(--radius-md); }
    .progress-details { display: flex; justify-content: space-between; margin-bottom: 12px; }
    .progress-label { font-size: 0.875rem; font-weight: 600; color: var(--text-muted); }
    .progress-percent { font-size: 1rem; font-weight: 800; color: var(--primary); }
    
    .linear-progress-bg { height: 8px; background: #F3F4F6; border-radius: 4px; overflow: hidden; }
    .linear-progress-fill { height: 100%; background: var(--grad-primary); transition: width 0.4s cubic-bezier(0.4, 0, 0.2, 1); }

    .timeline-v2 { padding-top: 10px; }
    .timeline-slot { display: flex; gap: 24px; min-height: 120px; position: relative; }
    
    .slot-marker { 
      display: flex; flex-direction: column; align-items: center; }
    .marker-dot { 
      width: 14px; height: 14px; border-radius: 50%; 
      background: white; border: 3px solid #E5E7EB; 
      z-index: 2; transition: all 0.2s;
    }
    .marker-line { flex: 1; width: 2px; background: #EEE; transition: background 0.2s; }
    
    .timeline-slot.is-done .marker-dot { border-color: #10B981; background: #10B981; }
    .timeline-slot.is-done .marker-line { background: #10B981; }
    .timeline-slot:last-child .marker-line { display: none; }

    .timeline-card { flex: 1; padding: 20px 24px; display: flex; align-items: center; justify-content: space-between; margin-bottom: 24px; }
    .timeline-slot.is-done .timeline-card { opacity: 0.7; background: #F9FAFB; }
    
    .task-meta { margin-bottom: 4px; }
    .day-anchor { font-size: 0.75rem; font-weight: 700; color: var(--primary); text-transform: uppercase; letter-spacing: 0.05em; }
    
    .task-body { display: flex; flex-direction: column; gap: 8px; flex: 1; }
    .topic-info { display: flex; align-items: baseline; gap: 8px; }
    .topic-label { font-size: 0.9rem; color: var(--text-muted); font-weight: 500; }
    .topic-name { font-size: 1.15rem; font-weight: 700; }
    
    .activities-list { 
      margin: 4px 0 0 0; padding: 0; list-style: none; 
      display: flex; flex-direction: column; gap: 6px;
    }
    .activities-list li { 
      font-size: 0.9rem; color: var(--text-muted); 
      padding-left: 18px; position: relative; line-height: 1.4;
    }
    .activities-list li::before { 
      content: "•"; position: absolute; left: 4px; color: var(--primary); font-weight: bold; 
    }

    /* Custom Checkbox */
    .custom-checkbox { display: flex; align-items: center; gap: 10px; cursor: pointer; user-select: none; }
    .custom-checkbox input { position: absolute; opacity: 0; cursor: pointer; height: 0; width: 0; }
    .checkmark { 
      height: 22px; width: 22px; background-color: white; 
      border: 2px solid #E5E7EB; border-radius: 6px; 
      display: inline-block; position: relative; transition: all 0.2s;
    }
    .custom-checkbox:hover input ~ .checkmark { border-color: var(--primary); }
    .custom-checkbox input:checked ~ .checkmark { background-color: var(--primary); border-color: var(--primary); }
    .checkmark:after { 
      content: ""; position: absolute; display: none;
      left: 7px; top: 3px; width: 5px; height: 10px;
      border: solid white; border-width: 0 2.5px 2.5px 0;
      transform: rotate(45deg);
    }
    .custom-checkbox input:checked ~ .checkmark:after { display: block; }
    .checkbox-text { font-size: 0.875rem; font-weight: 600; color: var(--text-muted); }
    .custom-checkbox input:checked ~ .checkbox-text { color: var(--primary); }

    .priority-list { display: flex; flex-direction: column; gap: 12px; }
    .priority-item { display: flex; align-items: center; gap: 12px; font-size: 0.9rem; }
    .p-dot { width: 8px; height: 8px; border-radius: 50%; }
    .p-dot.hard { background: #EF4444; }
    .p-dot.medium { background: #D97706; }
    .p-dot.easy { background: #10B981; }
    .p-name { flex: 1; font-weight: 500; }
    .p-lvl { font-size: 0.7rem; font-weight: 700; color: var(--text-muted); opacity: 0.6; }

    .generating-card { text-align: center; padding: 80px; display: flex; flex-direction: column; align-items: center; gap: 16px; margin-top: 40px; }
    .sparkle-icon { font-size: 3rem; animation: bounce 1s infinite alternate; }
    @keyframes bounce { from { transform: translateY(0); } to { transform: translateY(-10px); } }
    .progress-pulse { width: 200px; height: 4px; background: #EEE; border-radius: 2px; position: relative; overflow: hidden; }
    .progress-pulse::after { content: ''; position: absolute; left: -50%; width: 50%; height: 100%; background: var(--primary); animation: loading-pulse 1.5s infinite; }
    @keyframes loading-pulse { to { left: 100%; } }

    .loading-state { text-align: center; padding: 100px; color: var(--text-muted); }
    .spinner { border: 3px solid #E5E7EB; border-top-color: var(--primary); border-radius: 50%; width: 32px; height: 32px; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    
    .header-actions { display: flex; gap: 12px; align-items: center; }
    .text-error { color: #EF4444 !important; }
    .btn-ghost.text-error:hover { background: #FEF2F2; }

    .task-footer { margin-top: 16px; display: flex; gap: 8px; }
    .btn-flashcard { 
      background: white; border: 1px solid #E5E7EB; color: var(--text-main); 
      font-size: 0.8rem; font-weight: 600; padding: 6px 12px; border-radius: 8px;
      transition: all 0.2s; cursor: pointer;
    }
    .btn-flashcard:hover { border-color: var(--primary); color: var(--primary); background: #EEF2FF; }

    /* Modal Styles */
    .modal-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.4); 
      display: flex; align-items: center; justify-content: center; z-index: 1000;
      backdrop-filter: blur(4px);
    }
    .modal-content {
      width: 90%; max-width: 800px; max-height: 85vh; background: white;
      border-radius: 24px; padding: 32px; overflow-y: auto; position: relative;
    }
    .modal-header { 
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px;
    }
    .modal-header h2 { font-size: 1.5rem; font-weight: 800; margin: 0; }
    .btn-close { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: var(--text-muted); }

    .flashcards-grid {
      display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px;
    }
    .flashcard {
      height: 280px; perspective: 1000px; cursor: pointer;
    }
    .flashcard-inner {
      position: relative; width: 100%; height: 100%; text-align: center;
      transition: transform 0.6s; transform-style: preserve-3d;
      box-shadow: 0 4px 12px rgba(0,0,0,0.05); border-radius: 16px;
    }
    .flashcard.flipped .flashcard-inner { transform: rotateY(180deg); }
    .flashcard-front, .flashcard-back {
      position: absolute; width: 100%; height: 100%; backface-visibility: hidden;
      display: flex; flex-direction: column; align-items: center; 
      padding: 20px; border-radius: 16px; border: 1px solid #f0f0f0;
      overflow-y: auto;
    }
    .flashcard-front { background: white; color: black; justify-content: center; }
    .flashcard-back { background: var(--grad-primary); color: white; transform: rotateY(180deg); justify-content: flex-start; }
    
    .flashcard-front p, .flashcard-back p { 
      font-size: 0.95rem; line-height: 1.5; margin: 0; 
      width: 100%; word-break: break-word; 
    }

    .card-label { font-size: 0.7rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.05em; opacity: 0.6; margin-bottom: 8px; flex-shrink: 0; }
    .hint { font-size: 0.7rem; margin-top: 12px; opacity: 0.5; font-style: italic; flex-shrink: 0; }
    
    .loading-cards { text-align: center; padding: 40px; }
    .empty-state { text-align: center; color: var(--text-muted); padding: 40px; }

    /* Custom scrollbar for cards */
    .flashcard-front::-webkit-scrollbar, .flashcard-back::-webkit-scrollbar { width: 4px; }
    .flashcard-front::-webkit-scrollbar-track, .flashcard-back::-webkit-scrollbar-track { background: transparent; }
    .flashcard-front::-webkit-scrollbar-thumb { background: #E5E7EB; border-radius: 10px; }
    .flashcard-back::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.3); border-radius: 10px; }
  `]
})
export class PlanViewComponent implements OnInit {
  subject: any;
  loading = true;
  generating = false;

  activeTask: any = null;
  activeFlashcards: any[] = [];
  showFlashcardModal = false;
  loadingFlashcards = false;

  private route = inject(ActivatedRoute);
  private subjectService = inject(SubjectService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  get sortedTasks() {
    return this.subject?.studyPlan?.studyTasks?.sort((a: any, b: any) => a.dayNumber - b.dayNumber) || [];
  }

  get completedCount() {
    return this.subject?.studyPlan?.studyTasks.filter((t: any) => t.status === 'Completed' || t.status === 1).length || 0;
  }

  get progressPercent() {
    if (!this.subject?.studyPlan?.studyTasks.length) return 0;
    return (this.completedCount / this.subject.studyPlan.studyTasks.length) * 100;
  }

  get isCompleted() {
    if (!this.subject?.studyPlan?.studyTasks.length) return false;
    return this.completedCount === this.subject.studyPlan.studyTasks.length;
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadSubject(id);
    }
  }

  loadSubject(id: string) {
    this.subjectService.getSubject(id).subscribe({
      next: (data) => {
        this.subject = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  generatePlan() {
    this.generating = true;
    this.cdr.detectChanges();
    this.subjectService.generatePlan(this.subject.id).subscribe({
      next: (plan: any) => {
        this.subject.studyPlan = plan;
        this.generating = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.generating = false;
        this.cdr.detectChanges();
      }
    });
  }

  toggleTask(task: any) {
    const isCompleted = task.status === 'Completed' || task.status === 1;
    const newStatus = isCompleted ? 'Pending' : 'Completed';
    const originalStatus = task.status;
    task.status = newStatus;

    this.subjectService.updateTaskStatus(task.id, newStatus).subscribe({
      error: () => task.status = originalStatus
    });
  }

  deleteSubject() {
    if (confirm('Are you sure you want to delete this subject and its study plan?')) {
      const id = this.subject.id;
      this.subjectService.deleteSubject(id).subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.error('Error deleting subject:', err);
          alert('Could not delete subject.');
        }
      });
    }
  }

  openFlashcards(task: any) {
    this.activeTask = task;
    this.showFlashcardModal = true;

    // Check if we already have flashcards loaded for this task
    if (task.flashcards && task.flashcards.length > 0) {
      this.activeFlashcards = task.flashcards.map((c: any) => ({ ...c, isFlipped: false }));
      this.loadingFlashcards = false;
      this.cdr.detectChanges();
      return;
    }

    this.loadingFlashcards = true;
    this.activeFlashcards = [];

    this.subjectService.generateFlashcards(task.id).subscribe({
      next: (cards) => {
        // Save them to the local task object so they persist during this session view
        task.flashcards = cards;
        this.activeFlashcards = cards.map(c => ({ ...c, isFlipped: false }));
        this.loadingFlashcards = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingFlashcards = false;
        this.cdr.detectChanges();
      }
    });
  }

  closeFlashcards() {
    this.showFlashcardModal = false;
    this.activeTask = null;
    this.activeFlashcards = [];
  }
}
