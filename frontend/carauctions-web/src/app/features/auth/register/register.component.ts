import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="register-container">
      <mat-card class="register-card">
        <mat-card-header>
          <mat-card-title>Create Account</mat-card-title>
          <mat-card-subtitle>Join CarAuctions today</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <form (ngSubmit)="register()" #registerForm="ngForm">
            <div class="name-row">
              <mat-form-field appearance="outline">
                <mat-label>First Name</mat-label>
                <input
                  matInput
                  name="firstName"
                  [(ngModel)]="firstName"
                  required
                />
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Last Name</mat-label>
                <input
                  matInput
                  name="lastName"
                  [(ngModel)]="lastName"
                  required
                />
              </mat-form-field>
            </div>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input
                matInput
                type="email"
                name="email"
                [(ngModel)]="email"
                required
                email
              />
              <mat-icon matSuffix>email</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Phone (optional)</mat-label>
              <input
                matInput
                type="tel"
                name="phone"
                [(ngModel)]="phone"
              />
              <mat-icon matSuffix>phone</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input
                matInput
                [type]="hidePassword ? 'password' : 'text'"
                name="password"
                [(ngModel)]="password"
                required
                minlength="8"
              />
              <button
                mat-icon-button
                matSuffix
                type="button"
                (click)="hidePassword = !hidePassword"
              >
                <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
              <mat-hint>Minimum 8 characters</mat-hint>
            </mat-form-field>

            <mat-checkbox name="isDealer" [(ngModel)]="isDealer" class="dealer-checkbox">
              I am a licensed dealer
            </mat-checkbox>

            @if (error) {
              <div class="error-message">
                <mat-icon>error</mat-icon>
                {{ error }}
              </div>
            }

            <button
              mat-raised-button
              color="primary"
              type="submit"
              class="full-width submit-btn"
              [disabled]="!registerForm.valid || auth.isLoading()"
            >
              @if (auth.isLoading()) {
                <mat-spinner diameter="20"></mat-spinner>
              } @else {
                Create Account
              }
            </button>
          </form>
        </mat-card-content>

        <mat-card-actions>
          <p>Already have an account? <a routerLink="/login">Sign In</a></p>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .register-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: calc(100vh - 120px);
      padding: 24px;
    }

    .register-card {
      width: 100%;
      max-width: 500px;
    }

    mat-card-header {
      justify-content: center;
      text-align: center;
      margin-bottom: 24px;
    }

    .name-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .full-width {
      width: 100%;
    }

    mat-form-field {
      margin-bottom: 16px;
    }

    .dealer-checkbox {
      margin-bottom: 24px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background: #ffebee;
      border-radius: 4px;
      color: #c62828;
      margin-bottom: 16px;
    }

    .submit-btn {
      padding: 12px;
      font-size: 1rem;
    }

    mat-card-actions {
      padding: 16px;
    }

    mat-card-actions a {
      color: #1976d2;
      text-decoration: none;
    }
  `]
})
export class RegisterComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  password = '';
  isDealer = false;
  hidePassword = true;
  error: string | null = null;

  register(): void {
    this.error = null;
    this.auth.register({
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      password: this.password,
      phone: this.phone || undefined,
      isDealer: this.isDealer
    }).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        this.error = err.error?.message || 'Registration failed. Please try again.';
      }
    });
  }
}
