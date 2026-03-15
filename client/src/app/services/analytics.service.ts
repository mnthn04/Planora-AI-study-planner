import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface AnalyticsData {
    hasData: boolean;
    summary: {
        totalSubjects: number;
        activePlans: number;
        completedTasks: number;
        totalTasks: number;
        overallProgress: number;
    };
    graphs: {
        taskGrowth: Array<{ date: string; completedCount: number }>;
        subjectGrowth: Array<{ date: string; createdCount: number }>;
    };
    testStats?: {
        totalTests: number;
        averageScore: number;
        highestScore: number;
        recentScores: Array<{
            subjectName: string;
            score: number;
            totalQuestions: number;
            createdAt: string;
        }>;
    };
}

@Injectable({
    providedIn: 'root'
})
export class AnalyticsService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5178/api/analytics';

    getAnalytics(): Observable<AnalyticsData> {
        return this.http.get<AnalyticsData>(this.apiUrl);
    }
}
