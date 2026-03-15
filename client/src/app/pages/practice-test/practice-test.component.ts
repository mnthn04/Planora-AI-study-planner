import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PracticeTestService, PracticeQuestion } from '../../services/practice-test.service';

@Component({
    selector: 'app-practice-test',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './practice-test.component.html',
    styleUrl: './practice-test.component.css'
})
export class PracticeTestComponent implements OnInit {
    subjectId: string = '';
    subjectName: string = '';
    questions: PracticeQuestion[] = [];
    currentIndex: number = 0;
    userAnswers: string[] = [];
    isFinished: boolean = false;
    score: number = 0;
    loading: boolean = true;
    submitting: boolean = false;
    showExplanation: boolean = false;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private testService: PracticeTestService
    ) { }

    ngOnInit(): void {
        this.subjectId = this.route.snapshot.paramMap.get('id') || '';
        if (!this.subjectId) {
            this.router.navigate(['/dashboard']);
            return;
        }
        this.loadQuestions();
    }

    loadQuestions(): void {
        this.loading = true;
        this.testService.generateTest(this.subjectId).subscribe({
            next: (data) => {
                this.questions = data;
                this.userAnswers = new Array(data.length).fill('');
                this.loading = false;
            },
            error: (err) => {
                console.error('Failed to load questions', err);
                this.loading = false;
                alert('Failed to generate practice test. Please try again.');
                this.router.navigate(['/dashboard']);
            }
        });
    }

    selectOption(option: string): void {
        if (this.isFinished) return;
        const optionLetter = option.charAt(0); // Assuming options are like "A. content" or if they are just content, we need another way
        // Actually the API returns options as a list of strings.
        // Let's assume the user selects by index.
        const index = this.questions[this.currentIndex].options.indexOf(option);
        const letter = ['A', 'B', 'C', 'D'][index];
        this.userAnswers[this.currentIndex] = letter;
    }

    nextQuestion(): void {
        if (this.currentIndex < this.questions.length - 1) {
            this.currentIndex++;
        } else {
            this.finishTest();
        }
    }

    prevQuestion(): void {
        if (this.currentIndex > 0) {
            this.currentIndex--;
        }
    }

    finishTest(): void {
        this.isFinished = true;
        this.calculateScore();
        this.submitResults();
    }

    calculateScore(): void {
        this.score = 0;
        this.questions.forEach((q, i) => {
            if (this.userAnswers[i] === q.correctAnswer) {
                this.score++;
            }
        });
    }

    submitResults(): void {
        this.submitting = true;
        const submission = {
            subjectId: this.subjectId,
            score: this.score,
            totalQuestions: this.questions.length,
            questions: this.questions.map((q, i) => ({
                questionText: q.question,
                options: q.options,
                correctAnswer: q.correctAnswer,
                userAnswer: this.userAnswers[i],
                explanation: q.explanation
            }))
        };

        this.testService.submitTest(submission).subscribe({
            next: () => {
                this.submitting = false;
            },
            error: (err) => {
                console.error('Failed to submit results', err);
                this.submitting = false;
            }
        });
    }
}
