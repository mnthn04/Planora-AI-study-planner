import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class SubjectService {
    private http = inject(HttpClient);
    private baseUrl = 'http://localhost:5178/api';
    private subjectsUrl = `${this.baseUrl}/subjects`;
    private planUrl = `${this.baseUrl}/studyplan`;

    getSubjects(): Observable<any[]> {
        return this.http.get<any[]>(this.subjectsUrl);
    }

    getSubject(id: string): Observable<any> {
        return this.http.get<any>(`${this.subjectsUrl}/${id}`);
    }

    createSubject(data: any): Observable<any> {
        return this.http.post(this.subjectsUrl, data);
    }

    addTopic(data: any): Observable<any> {
        return this.http.post(`${this.baseUrl}/topics`, data);
    }

    generatePlan(subjectId: string): Observable<any> {
        // Global difficulty defaults to Medium for now
        return this.http.post(`${this.planUrl}/generate/${subjectId}`, { globalDifficulty: 'Medium' });
    }

    updateSubject(id: string, data: any): Observable<any> {
        return this.http.put(`${this.subjectsUrl}/${id}`, data);
    }

    deleteSubject(id: string): Observable<any> {
        return this.http.delete(`${this.subjectsUrl}/${id}`);
    }

    updateTaskStatus(taskId: string, status: string): Observable<any> {
        // We'll implement this endpoint in the backend if not present
        return this.http.patch(`${this.baseUrl}/tasks/${taskId}/status`, { status });
    }
}
