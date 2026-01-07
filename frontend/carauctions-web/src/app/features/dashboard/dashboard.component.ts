import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { WatchlistState } from '../../core/state/watchlist.state';
import { NotificationState } from '../../core/state/notification.state';
import { Bid, UserBidSummary, Auction, AuctionStatus } from '../../shared/models/auction.model';
import { CountdownTimerComponent } from '../../shared/components/countdown-timer/countdown-timer.component';

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
    MatProgressSpinnerModule,
    MatTabsModule,
    MatChipsModule,
    MatBadgeModule,
    MatDividerModule,
    MatProgressBarModule,
    CountdownTimerComponent
  ],
  template: `
    <div class="dashboard-container">
      <!-- Header -->
      <header class="dashboard-header">
        <div class="welcome-section">
          <h1>Welcome back, {{ auth.currentUser()?.firstName }}!</h1>
          <p>Manage your auctions, bids, and watchlist</p>
        </div>
        @if (auth.currentUser()?.isDealer) {
          <div class="dealer-badge">
            <mat-icon>verified</mat-icon>
            <span>Licensed Dealer</span>
          </div>
        }
      </header>

      <!-- Stats Grid -->
      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-icon class="stat-icon active">gavel</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ activeBidsCount() }}</span>
            <span class="stat-label">Active Bids</span>
          </div>
        </mat-card>

        <mat-card class="stat-card winning">
          <mat-icon class="stat-icon">emoji_events</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ winningBidsCount() }}</span>
            <span class="stat-label">Currently Winning</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon class="stat-icon won">directions_car</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ wonAuctionsCount() }}</span>
            <span class="stat-label">Won Auctions</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon class="stat-icon watching">favorite</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ watchlistState.count() }}</span>
            <span class="stat-label">Watching</span>
          </div>
        </mat-card>
      </div>

      <!-- Credit & KYC Status -->
      @if (auth.currentUser()?.creditLimit || auth.currentUser()?.kycVerified !== undefined) {
        <div class="status-cards">
          @if (auth.currentUser()?.creditLimit) {
            <mat-card class="credit-card">
              <div class="credit-header">
                <mat-icon>account_balance_wallet</mat-icon>
                <h3>Credit Status</h3>
              </div>
              <div class="credit-info">
                <div class="credit-row">
                  <span>Credit Limit</span>
                  <strong>{{ auth.currentUser()?.creditLimit?.amount | currency }}</strong>
                </div>
                <div class="credit-row">
                  <span>Available Credit</span>
                  <strong class="available">{{ auth.currentUser()?.availableCredit?.amount | currency }}</strong>
                </div>
                <mat-progress-bar
                  mode="determinate"
                  [value]="creditUsagePercent()">
                </mat-progress-bar>
                <span class="credit-percent">{{ creditUsagePercent() | number:'1.0-0' }}% Used</span>
              </div>
            </mat-card>
          }

          <mat-card class="kyc-card" [class.verified]="auth.currentUser()?.kycVerified">
            <div class="kyc-status">
              <mat-icon>{{ auth.currentUser()?.kycVerified ? 'verified_user' : 'security' }}</mat-icon>
              <div>
                <h3>KYC Status</h3>
                <span [class.verified]="auth.currentUser()?.kycVerified" [class.pending]="!auth.currentUser()?.kycVerified">
                  {{ auth.currentUser()?.kycVerified ? 'Verified' : 'Not Verified' }}
                </span>
              </div>
            </div>
            @if (!auth.currentUser()?.kycVerified) {
              <a mat-raised-button color="primary" routerLink="/profile/kyc">
                Complete Verification
              </a>
            }
          </mat-card>
        </div>
      }

      <!-- Main Content Tabs -->
      <mat-card class="main-content-card">
        <mat-tab-group>
          <!-- My Bids Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>gavel</mat-icon>
              <span>My Bids</span>
              @if (myBids().length > 0) {
                <span class="tab-badge">{{ myBids().length }}</span>
              }
            </ng-template>
            <div class="tab-content">
              @if (loading()) {
                <div class="loading">
                  <mat-spinner diameter="40"></mat-spinner>
                </div>
              } @else if (myBids().length > 0) {
                <div class="bid-list">
                  @for (bid of myBids(); track bid.id) {
                    <div class="bid-item" [routerLink]="['/auctions', bid.auctionId]">
                      <div class="bid-status" [class]="bid.status.toLowerCase()">
                        <mat-icon>{{ getBidStatusIcon(bid.status) }}</mat-icon>
                      </div>
                      <div class="bid-details">
                        <span class="bid-amount">{{ bid.amount.amount | currency }}</span>
                        <span class="bid-auction">Auction: {{ bid.auctionId.substring(0, 8) }}...</span>
                        <span class="bid-time">{{ bid.placedAt | date:'medium' }}</span>
                      </div>
                      <div class="bid-status-label" [class]="bid.status.toLowerCase()">
                        {{ bid.status }}
                        @if (bid.isProxyBid) {
                          <mat-chip class="proxy-chip">Proxy</mat-chip>
                        }
                      </div>
                    </div>
                  }
                </div>
              } @else {
                <div class="empty-state">
                  <mat-icon>gavel</mat-icon>
                  <h3>No Bids Yet</h3>
                  <p>Start bidding on vehicles to see them here</p>
                  <a mat-raised-button color="primary" routerLink="/auctions">Browse Auctions</a>
                </div>
              }
            </div>
          </mat-tab>

          <!-- Watchlist Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>favorite</mat-icon>
              <span>Watchlist</span>
              @if (watchlistState.count() > 0) {
                <span class="tab-badge">{{ watchlistState.count() }}</span>
              }
            </ng-template>
            <div class="tab-content">
              @if (watchlistState.loading()) {
                <div class="loading">
                  <mat-spinner diameter="40"></mat-spinner>
                </div>
              } @else if (watchlistState.items().length > 0) {
                <div class="watchlist-grid">
                  @for (item of watchlistState.items(); track item.id) {
                    <mat-card class="watchlist-item" [routerLink]="['/auctions', item.auctionId]">
                      @if (item.auction?.vehicle?.imageUrl) {
                        <img [src]="item.auction?.vehicle?.imageUrl" [alt]="item.auction?.title">
                      } @else {
                        <div class="no-image">
                          <mat-icon>directions_car</mat-icon>
                        </div>
                      }
                      <div class="watchlist-info">
                        <h4>{{ item.auction?.title || 'Auction ' + item.auctionId.substring(0, 8) }}</h4>
                        @if (item.auction) {
                          <span class="current-bid">
                            Current: {{ item.auction.currentHighBid.amount | currency }}
                          </span>
                          <mat-chip [class]="'status-' + item.auction.status.toLowerCase()">
                            {{ item.auction.status }}
                          </mat-chip>
                        }
                      </div>
                      <button mat-icon-button
                              class="remove-btn"
                              (click)="removeFromWatchlist($event, item.auctionId)">
                        <mat-icon>close</mat-icon>
                      </button>
                    </mat-card>
                  }
                </div>
              } @else {
                <div class="empty-state">
                  <mat-icon>favorite_border</mat-icon>
                  <h3>Watchlist Empty</h3>
                  <p>Add auctions to your watchlist to track them here</p>
                  <a mat-raised-button color="primary" routerLink="/auctions">Browse Auctions</a>
                </div>
              }
            </div>
          </mat-tab>

          <!-- Won Auctions Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>emoji_events</mat-icon>
              <span>Won Auctions</span>
              @if (wonAuctionsCount() > 0) {
                <span class="tab-badge success">{{ wonAuctionsCount() }}</span>
              }
            </ng-template>
            <div class="tab-content">
              @if (wonAuctions().length > 0) {
                <div class="won-list">
                  @for (bid of wonAuctions(); track bid.id) {
                    <mat-card class="won-item" [routerLink]="['/auctions', bid.auctionId]">
                      <mat-icon class="trophy">emoji_events</mat-icon>
                      <div class="won-details">
                        <span class="won-amount">{{ bid.amount.amount | currency }}</span>
                        <span class="won-date">Won on {{ bid.placedAt | date:'mediumDate' }}</span>
                      </div>
                      <a mat-raised-button color="primary">View Details</a>
                    </mat-card>
                  }
                </div>
              } @else {
                <div class="empty-state">
                  <mat-icon>emoji_events</mat-icon>
                  <h3>No Won Auctions</h3>
                  <p>Auctions you win will appear here</p>
                  <a mat-raised-button color="primary" routerLink="/auctions">Start Bidding</a>
                </div>
              }
            </div>
          </mat-tab>

          <!-- Notifications Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon [matBadge]="notificationState.unreadCount()"
                        [matBadgeHidden]="notificationState.unreadCount() === 0"
                        matBadgeColor="warn">
                notifications
              </mat-icon>
              <span>Notifications</span>
            </ng-template>
            <div class="tab-content">
              @if (notificationState.notifications().length > 0) {
                <div class="notifications-header">
                  <span>{{ notificationState.unreadCount() }} unread</span>
                  <button mat-button (click)="markAllRead()">Mark All Read</button>
                </div>
                <div class="notification-list">
                  @for (notification of notificationState.notifications(); track notification.id) {
                    <div class="notification-item"
                         [class.unread]="!notification.isRead"
                         (click)="handleNotificationClick(notification)">
                      <mat-icon [class]="notification.type.toLowerCase()">
                        {{ getNotificationIcon(notification.type) }}
                      </mat-icon>
                      <div class="notification-content">
                        <strong>{{ notification.title }}</strong>
                        <p>{{ notification.message }}</p>
                        <span class="notification-time">{{ notification.createdAt | date:'medium' }}</span>
                      </div>
                      @if (!notification.isRead) {
                        <div class="unread-dot"></div>
                      }
                    </div>
                  }
                </div>
              } @else {
                <div class="empty-state">
                  <mat-icon>notifications_none</mat-icon>
                  <h3>No Notifications</h3>
                  <p>You're all caught up!</p>
                </div>
              }
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card>

      <!-- Quick Actions -->
      <mat-card class="quick-actions-card">
        <h3>Quick Actions</h3>
        <div class="actions-grid">
          <a mat-stroked-button routerLink="/auctions">
            <mat-icon>search</mat-icon>
            Browse Auctions
          </a>
          @if (isSeller()) {
            <a mat-stroked-button routerLink="/seller">
              <mat-icon>storefront</mat-icon>
              Seller Dashboard
            </a>
            <a mat-stroked-button routerLink="/sell">
              <mat-icon>add_circle</mat-icon>
              List a Vehicle
            </a>
          }
          @if (auth.isAdmin()) {
            <a mat-stroked-button routerLink="/admin">
              <mat-icon>admin_panel_settings</mat-icon>
              Admin Panel
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
      </mat-card>
    </div>
  `,
  styles: [`
    .dashboard-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 24px;
    }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 32px;
    }

    .welcome-section h1 {
      margin: 0;
      font-size: 2rem;
    }

    .welcome-section p {
      margin: 8px 0 0;
      color: rgba(0, 0, 0, 0.6);
    }

    .dealer-badge {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      background: linear-gradient(135deg, #7c4dff, #536dfe);
      color: white;
      border-radius: 20px;
      font-weight: 500;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 24px;
    }

    .stat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }

    .stat-icon.active { color: #2196f3; }
    .stat-icon.winning { color: #ffc107; }
    .stat-icon.won { color: #4caf50; }
    .stat-icon.watching { color: #f44336; }

    .stat-card.winning { border-left: 4px solid #ffc107; }

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

    .status-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .credit-card, .kyc-card {
      padding: 24px;
    }

    .credit-header, .kyc-status {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }

    .credit-header mat-icon, .kyc-status mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: #1976d2;
    }

    .credit-header h3, .kyc-status h3 {
      margin: 0;
    }

    .credit-info {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .credit-row {
      display: flex;
      justify-content: space-between;
    }

    .credit-row .available {
      color: #4caf50;
    }

    .credit-percent {
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.5);
      text-align: right;
    }

    .kyc-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .kyc-status span.verified { color: #4caf50; font-weight: 500; }
    .kyc-status span.pending { color: #ff9800; }

    .kyc-card.verified {
      border-left: 4px solid #4caf50;
    }

    .kyc-card.verified .kyc-status mat-icon {
      color: #4caf50;
    }

    .main-content-card {
      margin-bottom: 24px;
    }

    .tab-content {
      padding: 24px;
      min-height: 300px;
    }

    .tab-badge {
      background: #1976d2;
      color: white;
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 0.75rem;
      margin-left: 8px;
    }

    .tab-badge.success {
      background: #4caf50;
    }

    mat-tab-group ::ng-deep .mat-mdc-tab .mdc-tab__content {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 48px;
    }

    .empty-state {
      text-align: center;
      padding: 48px;
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

    /* Bid List */
    .bid-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .bid-item {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px;
      background: #f5f5f5;
      border-radius: 8px;
      cursor: pointer;
      transition: background 0.2s;
    }

    .bid-item:hover {
      background: #eeeeee;
    }

    .bid-status {
      width: 48px;
      height: 48px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .bid-status.winning { background: #e8f5e9; color: #4caf50; }
    .bid-status.outbid { background: #ffebee; color: #f44336; }
    .bid-status.active { background: #e3f2fd; color: #2196f3; }
    .bid-status.won { background: #fff8e1; color: #ffc107; }

    .bid-details {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .bid-amount {
      font-size: 1.25rem;
      font-weight: 600;
    }

    .bid-auction, .bid-time {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }

    .bid-status-label {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 500;
    }

    .bid-status-label.winning { color: #4caf50; }
    .bid-status-label.outbid { color: #f44336; }
    .bid-status-label.active { color: #2196f3; }
    .bid-status-label.won { color: #ffc107; }

    .proxy-chip {
      font-size: 0.625rem !important;
      height: 20px !important;
    }

    /* Watchlist */
    .watchlist-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 16px;
    }

    .watchlist-item {
      display: flex;
      flex-direction: column;
      cursor: pointer;
      position: relative;
      overflow: hidden;
    }

    .watchlist-item img {
      width: 100%;
      height: 160px;
      object-fit: cover;
    }

    .watchlist-item .no-image {
      width: 100%;
      height: 160px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f5f5f5;
    }

    .watchlist-item .no-image mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: rgba(0, 0, 0, 0.2);
    }

    .watchlist-info {
      padding: 16px;
    }

    .watchlist-info h4 {
      margin: 0 0 8px;
    }

    .watchlist-info .current-bid {
      display: block;
      font-size: 1.125rem;
      font-weight: 600;
      color: #1976d2;
      margin-bottom: 8px;
    }

    .remove-btn {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.5);
      color: white;
    }

    .status-active { background: #4caf50 !important; color: white !important; }
    .status-scheduled { background: #2196f3 !important; color: white !important; }
    .status-closed, .status-completed { background: #9e9e9e !important; color: white !important; }

    /* Won Auctions */
    .won-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .won-item {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 24px;
      cursor: pointer;
    }

    .won-item .trophy {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #ffc107;
    }

    .won-details {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .won-amount {
      font-size: 1.5rem;
      font-weight: 600;
    }

    .won-date {
      color: rgba(0, 0, 0, 0.6);
    }

    /* Notifications */
    .notifications-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      padding-bottom: 16px;
      border-bottom: 1px solid rgba(0, 0, 0, 0.1);
    }

    .notification-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .notification-item {
      display: flex;
      align-items: flex-start;
      gap: 16px;
      padding: 16px;
      border-radius: 8px;
      cursor: pointer;
      transition: background 0.2s;
      position: relative;
    }

    .notification-item:hover {
      background: #f5f5f5;
    }

    .notification-item.unread {
      background: #e3f2fd;
    }

    .notification-item mat-icon {
      margin-top: 4px;
    }

    .notification-item mat-icon.outbid { color: #f44336; }
    .notification-item mat-icon.auctionwon { color: #4caf50; }
    .notification-item mat-icon.bidplaced { color: #2196f3; }

    .notification-content {
      flex: 1;
    }

    .notification-content strong {
      display: block;
      margin-bottom: 4px;
    }

    .notification-content p {
      margin: 0;
      color: rgba(0, 0, 0, 0.6);
      font-size: 0.875rem;
    }

    .notification-time {
      display: block;
      margin-top: 8px;
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.4);
    }

    .unread-dot {
      width: 8px;
      height: 8px;
      background: #1976d2;
      border-radius: 50%;
      position: absolute;
      top: 16px;
      right: 16px;
    }

    /* Quick Actions */
    .quick-actions-card {
      padding: 24px;
    }

    .quick-actions-card h3 {
      margin: 0 0 16px;
    }

    .actions-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
      gap: 12px;
    }

    .actions-grid a {
      display: flex;
      align-items: center;
      gap: 8px;
      justify-content: center;
      padding: 16px;
    }

    @media (max-width: 768px) {
      .dashboard-header {
        flex-direction: column;
        gap: 16px;
      }

      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }

      .stat-value {
        font-size: 1.5rem;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);
  readonly watchlistState = inject(WatchlistState);
  readonly notificationState = inject(NotificationState);

  readonly myBids = signal<Bid[]>([]);
  readonly loading = signal(false);

  readonly activeBidsCount = computed(() =>
    this.myBids().filter(b => b.status === 'Active' || b.status === 'Winning').length
  );

  readonly winningBidsCount = computed(() =>
    this.myBids().filter(b => b.status === 'Winning').length
  );

  readonly wonAuctions = computed(() =>
    this.myBids().filter(b => b.status === 'Won')
  );

  readonly wonAuctionsCount = computed(() => this.wonAuctions().length);

  readonly isSeller = computed(() =>
    this.auth.currentUser()?.roles?.some(r => r === 'Seller' || r === 'Dealer') ?? false
  );

  readonly creditUsagePercent = computed(() => {
    const limit = this.auth.currentUser()?.creditLimit?.amount || 0;
    const available = this.auth.currentUser()?.availableCredit?.amount || 0;
    if (limit === 0) return 0;
    return ((limit - available) / limit) * 100;
  });

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.loading.set(true);

    // Load bids
    this.api.getMyBids().subscribe({
      next: (bids) => {
        this.myBids.set(bids);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    // Load watchlist
    this.watchlistState.loadWatchlist();

    // Load notifications
    this.notificationState.loadNotifications();
  }

  getBidStatusIcon(status: string): string {
    switch (status) {
      case 'Winning': return 'emoji_events';
      case 'Outbid': return 'trending_down';
      case 'Won': return 'check_circle';
      case 'Lost': return 'cancel';
      default: return 'gavel';
    }
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'Outbid': return 'trending_down';
      case 'AuctionWon': return 'emoji_events';
      case 'BidPlaced': return 'gavel';
      case 'AuctionEndingSoon': return 'timer';
      case 'PaymentRequired': return 'payment';
      default: return 'notifications';
    }
  }

  removeFromWatchlist(event: Event, auctionId: string): void {
    event.stopPropagation();
    this.watchlistState.removeFromWatchlist(auctionId);
  }

  handleNotificationClick(notification: any): void {
    if (!notification.isRead) {
      this.notificationState.markAsRead(notification.id);
    }
    if (notification.auctionId) {
      window.location.href = `/auctions/${notification.auctionId}`;
    }
  }

  markAllRead(): void {
    this.notificationState.markAllAsRead();
  }
}
