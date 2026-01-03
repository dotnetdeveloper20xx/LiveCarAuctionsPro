import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { Bid } from '../../shared/models/auction.model';
import { signal, computed } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="dashboard-container">
      <header class="dashboard-header">
        <h1>Welcome, {{ auth.currentUser()?.firstName }}!</h1>
        <p>Manage your auctions and bids</p>
      </header>

      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-icon>gavel</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ myBids().length }}</span>
            <span class="stat-label">Active Bids</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon>emoji_events</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ winningBids().length }}</span>
            <span class="stat-label">Winning</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon>directions_car</mat-icon>
          <div class="stat-content">
            <span class="stat-value">0</span>
            <span class="stat-label">Won Auctions</span>
          </div>
        </mat-card>

        @if (auth.currentUser()?.creditLimit) {
          <mat-card class="stat-card">
            <mat-icon>account_balance_wallet</mat-icon>
            <div class="stat-content">
              <span class="stat-value">{{ auth.currentUser()?.creditLimit?.amount | currency }}</span>
              <span class="stat-label">Credit Limit</span>
            </div>
          </mat-card>
        }
      </div>

      <div class="dashboard-grid">
        <mat-card class="bids-card">
          <mat-card-header>
            <mat-card-title>My Recent Bids</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            @if (loading()) {
              <div class="loading">
                <mat-spinner diameter="40"></mat-spinner>
              </div>
            } @else if (myBids().length > 0) {
              <mat-list>
                @for (bid of myBids().slice(0, 5); track bid.id) {
                  <mat-list-item [routerLink]="['/auctions', bid.auctionId]">
                    <mat-icon matListItemIcon [class]="bid.status.toLowerCase()">
                      {{ bid.status === 'Winning' ? 'emoji_events' : 'gavel' }}
                    </mat-icon>
                    <div matListItemTitle>{{ bid.amount.amount | currency:bid.amount.currency }}</div>
                    <div matListItemLine>{{ bid.placedAt | date:'medium' }}</div>
                    <span matListItemMeta [class]="'status-' + bid.status.toLowerCase()">
                      {{ bid.status }}
                    </span>
                  </mat-list-item>
                }
              </mat-list>
            } @else {
              <div class="empty-state">
                <mat-icon>gavel</mat-icon>
                <p>No bids yet</p>
                <a mat-button color="primary" routerLink="/auctions">Browse Auctions</a>
              </div>
            }
          </mat-card-content>
          @if (myBids().length > 5) {
            <mat-card-actions>
              <a mat-button routerLink="/my-bids">View All Bids</a>
            </mat-card-actions>
          }
        </mat-card>

        <mat-card class="quick-actions-card">
          <mat-card-header>
            <mat-card-title>Quick Actions</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="actions-grid">
              <a mat-stroked-button routerLink="/auctions">
                <mat-icon>search</mat-icon>
                Browse Auctions
              </a>
              @if (isSeller()) {
                <a mat-stroked-button routerLink="/sell">
                  <mat-icon>add_circle</mat-icon>
                  List a Vehicle
                </a>
              }
              <a mat-stroked-button routerLink="/profile">
                <mat-icon>person</mat-icon>
                Edit Profile
              </a>
              <a mat-stroked-button routerLink="/settings">
                <mat-icon>settings</mat-icon>
                Settings
              </a>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 24px;
    }

    .dashboard-header {
      margin-bottom: 32px;
    }

    .dashboard-header h1 {
      margin: 0;
      font-size: 2rem;
    }

    .dashboard-header p {
      margin: 8px 0 0;
      color: rgba(0, 0, 0, 0.6);
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 32px;
    }

    .stat-card {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 24px;
    }

    .stat-card mat-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #1976d2;
    }

    .stat-content {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 1.5rem;
      font-weight: 600;
    }

    .stat-label {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 24px;
    }

    @media (max-width: 768px) {
      .dashboard-grid {
        grid-template-columns: 1fr;
      }
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 32px;
    }

    .empty-state {
      text-align: center;
      padding: 32px;
      color: rgba(0, 0, 0, 0.6);
    }

    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }

    mat-list-item {
      cursor: pointer;
    }

    .status-winning { color: #4caf50; font-weight: 500; }
    .status-outbid { color: #f44336; }
    .status-active { color: #2196f3; }

    .winning { color: #4caf50; }

    .actions-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 12px;
    }

    .actions-grid a {
      display: flex;
      align-items: center;
      gap: 8px;
      justify-content: center;
      padding: 16px;
    }
  `]
})
export class DashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);

  readonly myBids = signal<Bid[]>([]);
  readonly loading = signal(false);

  readonly winningBids = signal<Bid[]>([]);

  readonly isSeller = computed(() =>
    this.auth.currentUser()?.roles?.some(r => r === 'Seller') ?? false
  );

  ngOnInit(): void {
    this.loadMyBids();
  }

  private loadMyBids(): void {
    this.loading.set(true);
    this.api.getMyBids().subscribe({
      next: (bids) => {
        this.myBids.set(bids);
        this.winningBids.set(bids.filter(b => b.status === 'Winning'));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
