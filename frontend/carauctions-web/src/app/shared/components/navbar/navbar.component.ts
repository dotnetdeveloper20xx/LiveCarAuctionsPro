import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule
  ],
  template: `
    <mat-toolbar color="primary" class="navbar">
      <a routerLink="/" class="logo">
        <mat-icon>directions_car</mat-icon>
        <span>CarAuctions</span>
      </a>

      <nav class="nav-links">
        <a mat-button routerLink="/auctions" routerLinkActive="active">
          <mat-icon>gavel</mat-icon>
          Auctions
        </a>
        <a mat-button routerLink="/vehicles" routerLinkActive="active">
          <mat-icon>garage</mat-icon>
          Vehicles
        </a>
      </nav>

      <span class="spacer"></span>

      @if (auth.isAuthenticated()) {
        <button mat-icon-button [matMenuTriggerFor]="userMenu">
          <mat-icon>account_circle</mat-icon>
        </button>
        <mat-menu #userMenu="matMenu">
          <div class="user-info">
            <strong>{{ auth.currentUser()?.firstName }} {{ auth.currentUser()?.lastName }}</strong>
            <small>{{ auth.currentUser()?.email }}</small>
          </div>
          <mat-divider></mat-divider>
          <a mat-menu-item routerLink="/dashboard">
            <mat-icon>dashboard</mat-icon>
            Dashboard
          </a>
          <a mat-menu-item routerLink="/my-bids">
            <mat-icon>history</mat-icon>
            My Bids
          </a>
          @if (auth.isAdmin()) {
            <a mat-menu-item routerLink="/admin">
              <mat-icon>admin_panel_settings</mat-icon>
              Admin
            </a>
          }
          <mat-divider></mat-divider>
          <button mat-menu-item (click)="auth.logout()">
            <mat-icon>logout</mat-icon>
            Logout
          </button>
        </mat-menu>
      } @else {
        <a mat-button routerLink="/login">Login</a>
        <a mat-raised-button color="accent" routerLink="/register">Register</a>
      }
    </mat-toolbar>
  `,
  styles: [`
    .navbar {
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 8px;
      text-decoration: none;
      color: inherit;
      font-size: 1.25rem;
      font-weight: 500;
    }

    .nav-links {
      margin-left: 32px;
      display: flex;
      gap: 8px;
    }

    .nav-links a {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .nav-links a.active {
      background: rgba(255, 255, 255, 0.1);
    }

    .spacer {
      flex: 1;
    }

    .user-info {
      padding: 16px;
      display: flex;
      flex-direction: column;
    }

    .user-info small {
      color: rgba(0, 0, 0, 0.6);
    }
  `]
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
}
