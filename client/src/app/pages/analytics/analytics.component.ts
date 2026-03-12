import { Component, inject, OnInit, ViewChild, ElementRef, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService, AnalyticsData } from '../../services/analytics.service';
import { Chart, registerables } from 'chart.js';
import { Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';

Chart.register(...registerables);

@Component({
    selector: 'app-analytics',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './analytics.component.html',
    styleUrl: './analytics.component.css'
})
export class AnalyticsComponent implements OnInit, OnDestroy {
    private analyticsService = inject(AnalyticsService);
    private cdr = inject(ChangeDetectorRef);
    private router = inject(Router);

    analyticsData: AnalyticsData | null = null;
    loading = true;
    error = false;
    private routerSub: Subscription | null = null;

    @ViewChild('taskChart') taskChartCanvas!: ElementRef<HTMLCanvasElement>;
    @ViewChild('subjectChart') subjectChartCanvas!: ElementRef<HTMLCanvasElement>;

    private charts: Chart[] = [];

    ngOnInit() {
        this.loadAnalytics();

        // Listen for re-navigation to the same URL (to handle the "double click" or refresh behavior)
        this.routerSub = this.router.events.pipe(
            filter(event => event instanceof NavigationEnd && this.router.url.includes('/analytics'))
        ).subscribe(() => {
            if (!this.loading) { // Only reload if not already loading
                this.loadAnalytics();
            }
        });
    }

    ngOnDestroy() {
        if (this.routerSub) this.routerSub.unsubscribe();
        this.charts.forEach(c => c.destroy());
    }

    loadAnalytics() {
        // Use a slight delay for the loader if it's too fast, but here we want speed
        this.loading = true;
        this.error = false;
        this.cdr.detectChanges();

        console.time('AnalyticsFetch');
        this.analyticsService.getAnalytics().subscribe({
            next: (data) => {
                console.timeEnd('AnalyticsFetch');
                this.analyticsData = data;
                this.loading = false;

                // Ensure DOM is updated with canvas elements
                this.cdr.detectChanges();

                if (data.hasData) {
                    // Using requestAnimationFrame to ensure the next frame has the DOM ready
                    requestAnimationFrame(() => {
                        this.initCharts();
                    });
                }
            },
            error: (err) => {
                console.timeEnd('AnalyticsFetch');
                console.error('Error loading analytics', err);
                this.loading = false;
                this.error = true;
                this.cdr.detectChanges();
            }
        });
    }

    initCharts() {
        if (!this.analyticsData || !this.taskChartCanvas || !this.subjectChartCanvas) {
            // Re-try once if elements aren't ready
            setTimeout(() => {
                if (this.analyticsData && (this.taskChartCanvas || this.subjectChartCanvas)) {
                    this.initChartsReal();
                }
            }, 50);
            return;
        }
        this.initChartsReal();
    }

    private initChartsReal() {
        if (!this.analyticsData) return;

        // Destroy existing charts
        this.charts.forEach(c => c.destroy());
        this.charts = [];

        try {
            // Task Growth Chart
            const taskChart = new Chart(this.taskChartCanvas.nativeElement, {
                type: 'line',
                data: {
                    labels: this.analyticsData.graphs.taskGrowth.map(g => g.date),
                    datasets: [{
                        label: 'Tasks Completed',
                        data: this.analyticsData.graphs.taskGrowth.map(g => g.completedCount),
                        borderColor: '#5B7FFF',
                        backgroundColor: 'rgba(91, 127, 255, 0.1)',
                        fill: true,
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: { duration: 800 },
                    plugins: {
                        legend: { display: false }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { stepSize: 1, color: '#6B7280' },
                            grid: { color: '#F3F4F6' }
                        },
                        x: {
                            ticks: { color: '#6B7280' },
                            grid: { display: false }
                        }
                    }
                }
            });
            this.charts.push(taskChart);

            // Subject Growth Chart
            const subjectChart = new Chart(this.subjectChartCanvas.nativeElement, {
                type: 'bar',
                data: {
                    labels: this.analyticsData.graphs.subjectGrowth.map(g => g.date),
                    datasets: [{
                        label: 'Cumulative Subjects',
                        data: this.analyticsData.graphs.subjectGrowth.map(g => g.createdCount),
                        backgroundColor: '#A7F3D0',
                        borderRadius: 6
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: { duration: 800 },
                    plugins: {
                        legend: { display: false }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { stepSize: 1, color: '#6B7280' },
                            grid: { color: '#F3F4F6' }
                        },
                        x: {
                            ticks: { color: '#6B7280' },
                            grid: { display: false }
                        }
                    }
                }
            });
            this.charts.push(subjectChart);
            this.cdr.detectChanges();
        } catch (e) {
            console.error('Error initializing charts:', e);
        }
    }
}
