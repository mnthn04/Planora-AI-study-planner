import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, User } from '../../services/auth.service';

@Component({
    selector: 'app-settings',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './settings.component.html',
    styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
    authService = inject(AuthService);
    userName = '';
    userEmail = '';
    message = '';
    isError = false;
    loading = false;

    ngOnInit() {
        this.authService.user$.subscribe(user => {
            if (user) {
                this.userName = user.name;
                this.userEmail = user.email;
            }
        });
    }

    saveProfile() {
        if (!this.userName.trim()) return;

        this.loading = true;
        this.message = '';

        this.authService.updateProfile(this.userName).subscribe({
            next: () => {
                this.message = 'Profile updated successfully!';
                this.isError = false;
                this.loading = false;
                setTimeout(() => this.message = '', 3000);
            },
            error: (err) => {
                this.message = 'Failed to update profile. Please try again.';
                this.isError = true;
                this.loading = false;
            }
        });
    }
}
