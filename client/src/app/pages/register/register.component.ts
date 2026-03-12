import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Password regex: at least 8 characters, one uppercase, one lowercase, one number, and one special character
  passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

  registerForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.pattern(this.passwordPattern)]]
  });

  loading = false;
  error = '';

  onSubmit() {
    if (this.registerForm.valid) {
      this.loading = true;
      this.error = '';

      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.loading = false;

          let errorMessage = 'Registration failed. Try again.';
          let isDuplicate = false;

          // Robustly parse the error response
          if (err.error) {
            const errorData = err.error;

            // Check if it's an array of Identity errors
            if (Array.isArray(errorData)) {
              const duplicate = errorData.find((e: any) =>
                e.code === 'DuplicateEmail' ||
                e.code === 'DuplicateUserName' ||
                (e.description && e.description.toLowerCase().includes('already taken'))
              );

              if (duplicate) {
                isDuplicate = true;
                errorMessage = 'This email already exists! Please use a different one.';
              } else {
                errorMessage = errorData[0]?.description || errorMessage;
              }
            }
            // Check if it's a single object error
            else if (typeof errorData === 'object') {
              errorMessage = errorData.message || errorData.description || errorMessage;
              if (errorMessage.toLowerCase().includes('already exists')) {
                isDuplicate = true;
              }
            }
            // Check if it's a string
            else if (typeof errorData === 'string') {
              errorMessage = errorData;
            }
          }

          this.error = errorMessage;

          if (isDuplicate) {
            // Use setTimeout to allow Angular to update the UI and remove the loader 
            // before the blocking alert() pops up
            setTimeout(() => {
              alert('This email already exists!');
            }, 100);
          }
        }
      });
    }
  }

  // Helper to check for specific validation errors in the UI
  get f() { return this.registerForm.controls; }
}
