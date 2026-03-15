import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PracticeQuestion {
    question: string;
    options: string[];
    correctAnswer: string;
    explanation: string;
}

export interface TestSubmission {
    subjectId: string;
    score: number;
    totalQuestions: number;
    questions: any[];
}

@Injectable({
    providedIn: 'root'
})
export class PracticeTestService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5178/api/PracticeTest';

    generateTest(subjectId: string): Observable<PracticeQuestion[]> {
        return this.http.get<PracticeQuestion[]>(`${this.apiUrl}/generate/${subjectId}`);
    }

    submitTest(submission: TestSubmission): Observable<any> {
        return this.http.post(`${this.apiUrl}/submit`, submission);
    }

    getResults(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/results`);
    }
}
