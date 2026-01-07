import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { Auction, SellerStats, AuctionStatus } from '../../shared/models/auction.model';

@Component({
  selector: 'app-seller-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatDividerModule
  ],
  template: `
    <div class="seller-container">
      <header class="seller-header">
        <div>
          <h1>Seller Dashboard</h1>
          <p>Manage your vehicle listings and auctions</p>
        </div>
        <a mat-raised-button color="primary" routerLink="/sell/new">
          <mat-icon>add</mat-icon>
          List New Vehicle
        </a>
      </header>

      <!-- Stats Overview -->
      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-icon>inventory</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ stats()?.totalListings || 0 }}</span>
            <span class="stat-label">Total Listings</span>
          </div>
        </mat-card>

        <mat-card class="stat-card active">
          <mat-icon>gavel</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ stats()?.activeAuctions || 0 }}</span>
            <span class="stat-label">Active Auctions</span>
          </div>
        </mat-card>

        <mat-card class="stat-card completed">
          <mat-icon>check_circle</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ stats()?.completedAuctions || 0 }}</span>
            <span class="stat-label">Completed Sales</span>
          </div>
        </mat-card>

        <mat-card class="stat-card revenue">
          <mat-icon>attach_money</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ stats()?.totalSales?.amount | currency }}</span>
            <span class="stat-label">Total Revenue</span>
          </div>
        </mat-card>
      </div>

      <!-- Performance Card -->
      <mat-card class="performance-card">
        <mat-card-header>
          <mat-card-title>Performance Overview</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="performance-grid">
            <div class="performance-item">
              <span class="label">Average Sale Price</span>
              <span class="value">{{ stats()?.averagePrice?.amount | currency }}</span>
            </div>
            <div class="performance-item">
              <span class="label">Success Rate</span>
              <span class="value">{{ successRate() | number:'1.0-0' }}%</span>
            </div>
            <div class="performance-item">
              <span class="label">Active Now</span>
              <span class="value highlight">{{ activeAuctions().length }}</span>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- My Auctions -->
      <mat-card class="auctions-card">
        <mat-card-header>
          <mat-card-title>My Auctions</mat-card-title>
          <div class="header-actions">
            <button mat-button [class.active]="filter() === 'all'" (click)="setFilter('all')">All</button>
            <button mat-button [class.active]="filter() === 'active'" (click)="setFilter('active')">Active</button>
            <button mat-button [class.active]="filter() === 'scheduled'" (click)="setFilter('scheduled')">Scheduled</button>
            <button mat-button [class.active]="filter() === 'completed'" (click)="setFilter('completed')">Completed</button>
          </div>
        </mat-card-header>
        <mat-card-content>
          @if (loading()) {
            <div class="loading">
              <mat-spinner diameter="40"></mat-spinner>
            </div>
          } @else if (filteredAuctions().length > 0) {
            <div class="auctions-list">
              @for (auction of filteredAuctions(); track auction.id) {
                <div class="auction-row">
                  <div class="auction-image">
                    @if (auction.vehicle?.imageUrl) {
                      <img [src]="auction.vehicle?.imageUrl" [alt]="auction.title">
                    } @else {
                      <mat-icon>directions_car</mat-icon>
                    }
                  </div>
                  <div class="auction-info">
                    <h3>{{ auction.title }}</h3>
                    <p>{{ auction.vehicle?.year }} {{ auction.vehicle?.make }} {{ auction.vehicle?.model }}</p>
                    <div class="auction-meta">
                      <span>
                        <mat-icon>gavel</mat-icon>
                        {{ auction.bidCount }} bids
                      </span>
                      <span>
                        <mat-icon>visibility</mat-icon>
                        {{ auction.watcherCount || 0 }} watchers
                      </span>
                    </div>
                  </div>
                  <div class="auction-price">
                    <span class="current-bid">{{ auction.currentHighBid.amount | currency }}</span>
                    <span class="starting">Starting: {{ auction.startingPrice.amount | currency }}</span>
                  </div>
                  <div class="auction-status">
                    <mat-chip [class]="'status-' + auction.status.toLowerCase()">
                      {{ auction.status }}
                    </mat-chip>
                  </div>
                  <div class="auction-actions">
                    <button mat-icon-button [matMenuTriggerFor]="auctionMenu">
                      <mat-icon>more_vert</mat-icon>
                    </button>
                    <mat-menu #auctionMenu="matMenu">
                      <a mat-menu-item [routerLink]="['/auctions', auction.id]">
                        <mat-icon>visibility</mat-icon>
                        View Auction
                      </a>
                      @if (auction.status === 'Draft') {
                        <button mat-menu-item (click)="scheduleAuction(auction.id)">
                          <mat-icon>schedule</mat-icon>
                          Schedule
                        </button>
                      }
                      @if (auction.status === 'Scheduled') {
                        <button mat-menu-item (click)="startAuction(auction.id)">
                          <mat-icon>play_arrow</mat-icon>
                          Start Now
                        </button>
                      }
                      @if (auction.status === 'Active' || auction.status === 'Scheduled') {
                        <button mat-menu-item class="danger" (click)="cancelAuction(auction.id)">
                          <mat-icon>cancel</mat-icon>
                          Cancel
                        </button>
                      }
                    </mat-menu>
                  </div>
                </div>
              }
            </div>
          } @else {
            <div class="empty-state">
              <mat-icon>inventory_2</mat-icon>
              <h3>No Auctions Found</h3>
              <p>{{ filter() === 'all' ? 'Start listing vehicles to see them here' : 'No ' + filter() + ' auctions' }}</p>
              @if (filter() === 'all') {
                <a mat-raised-button color="primary" routerLink="/sell/new">
                  <mat-icon>add</mat-icon>
                  List Your First Vehicle
                </a>
              }
            </div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .seller-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 24px;
    }

    .seller-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 32px;
    }

    .seller-header h1 {
      margin: 0;
      font-size: 2rem;
    }

    .seller-header p {
      margin: 8px 0 0;
      color: rgba(0, 0, 0, 0.6);
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 24px;
    }

    .stat-card mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #1976d2;
    }

    .stat-card.active mat-icon { color: #4caf50; }
    .stat-card.completed mat-icon { color: #ff9800; }
    .stat-card.revenue mat-icon { color: #2e7d32; }

    .stat-content {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 2rem;
      font-weight: 600;
      line-height: 1;
    }

    .stat-label {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
      margin-top: 4px;
    }

    .performance-card {
      margin-bottom: 24px;
    }

    .performance-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 24px;
      padding: 16px 0;
    }

    .performance-item {
      text-align: center;
    }

    .performance-item .label {
      display: block;
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
      margin-bottom: 8px;
    }

    .performance-item .value {
      font-size: 1.5rem;
      font-weight: 600;
    }

    .performance-item .value.highlight {
      color: #4caf50;
    }

    .auctions-card mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .header-actions {
      display: flex;
      gap: 8px;
    }

    .header-actions button.active {
      background: rgba(25, 118, 210, 0.1);
      color: #1976d2;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 48px;
    }

    .auctions-list {
      display: flex;
      flex-direction: column;
    }

    .auction-row {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px;
      border-bottom: 1px solid rgba(0, 0, 0, 0.1);
    }

    .auction-row:last-child {
      border-bottom: none;
    }

    .auction-image {
      width: 80px;
      height: 60px;
      border-radius: 8px;
      overflow: hidden;
      background: #f5f5f5;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .auction-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .auction-image mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: rgba(0, 0, 0, 0.3);
    }

    .auction-info {
      flex: 1;
    }

    .auction-info h3 {
      margin: 0 0 4px;
      font-size: 1rem;
    }

    .auction-info p {
      margin: 0;
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }

    .auction-meta {
      display: flex;
      gap: 16px;
      margin-top: 8px;
    }

    .auction-meta span {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.5);
    }

    .auction-meta mat-icon {
      font-size: 14px;
      width: 14px;
      height: 14px;
    }

    .auction-price {
      text-align: right;
    }

    .current-bid {
      display: block;
      font-size: 1.25rem;
      font-weight: 600;
      color: #1976d2;
    }

    .starting {
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.5);
    }

    .status-draft { background: #9e9e9e !important; color: white !important; }
    .status-scheduled { background: #2196f3 !important; color: white !important; }
    .status-active { background: #4caf50 !important; color: white !important; }
    .status-closed, .status-completed { background: #ff9800 !important; color: white !important; }
    .status-cancelled { background: #f44336 !important; color: white !important; }

    .auction-actions {
      margin-left: 16px;
    }

    .danger {
      color: #f44336;
    }

    .empty-state {
      text-align: center;
      padding: 64px 24px;
      color: rgba(0, 0, 0, 0.6);
    }

    .empty-state mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
    }

    .empty-state h3 {
      margin: 0 0 8px;
      color: rgba(0, 0, 0, 0.87);
    }

    .empty-state p {
      margin: 0 0 24px;
    }

    @media (max-width: 768px) {
      .seller-header {
        flex-direction: column;
        gap: 16px;
      }

      .auction-row {
        flex-wrap: wrap;
      }

      .auction-price {
        order: 5;
        width: 100%;
        text-align: left;
        margin-top: 8px;
      }
    }
  `]
})
export class SellerDashboardComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly auth = inject(AuthService);

  readonly stats = signal<SellerStats | null>(null);
  readonly auctions = signal<Auction[]>([]);
  readonly loading = signal(false);
  readonly filter = signal<'all' | 'active' | 'scheduled' | 'completed'>('all');

  readonly activeAuctions = computed(() =>
    this.auctions().filter(a => a.status === AuctionStatus.Active)
  );

  readonly filteredAuctions = computed(() => {
    const f = this.filter();
    const all = this.auctions();
    if (f === 'all') return all;
    if (f === 'active') return all.filter(a => a.status === AuctionStatus.Active);
    if (f === 'scheduled') return all.filter(a => a.status === AuctionStatus.Scheduled);
    if (f === 'completed') return all.filter(a =>
      a.status === AuctionStatus.Completed || a.status === AuctionStatus.Closed
    );
    return all;
  });

  readonly successRate = computed(() => {
    const stats = this.stats();
    if (!stats || stats.totalListings === 0) return 0;
    return (stats.completedAuctions / stats.totalListings) * 100;
  });

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.loading.set(true);

    this.api.getSellerStats().subscribe({
      next: (stats) => this.stats.set(stats),
      error: () => {}
    });

    this.api.getSellerAuctions().subscribe({
      next: (auctions) => {
        this.auctions.set(auctions);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  setFilter(filter: 'all' | 'active' | 'scheduled' | 'completed'): void {
    this.filter.set(filter);
  }

  scheduleAuction(id: string): void {
    this.api.scheduleAuction(id).subscribe({
      next: () => this.loadData(),
      error: (err) => console.error('Failed to schedule auction', err)
    });
  }

  startAuction(id: string): void {
    this.api.startAuction(id).subscribe({
      next: () => this.loadData(),
      error: (err) => console.error('Failed to start auction', err)
    });
  }

  cancelAuction(id: string): void {
    if (confirm('Are you sure you want to cancel this auction?')) {
      this.api.cancelAuction(id, 'Cancelled by seller').subscribe({
        next: () => this.loadData(),
        error: (err) => console.error('Failed to cancel auction', err)
      });
    }
  }
}
