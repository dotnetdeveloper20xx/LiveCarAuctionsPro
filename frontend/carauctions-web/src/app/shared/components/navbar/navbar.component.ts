import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationState } from '../../../core/state/notification.state';
import { WatchlistState } from '../../../core/state/watchlist.state';

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
    MatDividerModule,
    MatBadgeModule
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
        @if (auth.isAuthenticated() && isSeller()) {
          <a mat-button routerLink="/seller" routerLinkActive="active">
            <mat-icon>storefront</mat-icon>
            Sell
          </a>
        }
      </nav>

      <span class="spacer"></span>

      @if (auth.isAuthenticated()) {
        <!-- Notifications -->
        <button mat-icon-button [matMenuTriggerFor]="notifMenu" class="icon-btn">
          <mat-icon [matBadge]="notificationState.unreadCount()"
                    [matBadgeHidden]="notificationState.unreadCount() === 0"
                    matBadgeColor="warn"
                    matBadgeSize="small">
            notifications
          </mat-icon>
        </button>
        <mat-menu #notifMenu="matMenu" class="notification-menu">
          <div class="menu-header" (click)="$event.stopPropagation()">
            <strong>Notifications</strong>
            @if (notificationState.unreadCount() > 0) {
              <button mat-button (click)="markAllRead()">Mark all read</button>
            }
          </div>
          <mat-divider></mat-divider>
          @if (notificationState.notifications().length > 0) {
            @for (notif of notificationState.notifications().slice(0, 5); track notif.id) {
              <button mat-menu-item class="notification-item" [class.unread]="!notif.isRead"
                      (click)="handleNotificationClick(notif)">
                <mat-icon [class]="notif.type.toLowerCase()">
                  {{ getNotificationIcon(notif.type) }}
                </mat-icon>
                <div class="notif-content">
                  <strong>{{ notif.title }}</strong>
                  <span>{{ notif.message | slice:0:50 }}{{ notif.message.length > 50 ? '...' : '' }}</span>
                </div>
              </button>
            }
            <mat-divider></mat-divider>
            <a mat-menu-item routerLink="/dashboard" class="view-all">
              View all notifications
            </a>
          } @else {
            <div class="empty-notif">
              <mat-icon>notifications_none</mat-icon>
              <span>No notifications</span>
            </div>
          }
        </mat-menu>

        <!-- Watchlist -->
        <button mat-icon-button [matMenuTriggerFor]="watchMenu" class="icon-btn">
          <mat-icon [matBadge]="watchlistState.count()"
                    [matBadgeHidden]="watchlistState.count() === 0"
                    matBadgeColor="accent"
                    matBadgeSize="small">
            favorite
          </mat-icon>
        </button>
        <mat-menu #watchMenu="matMenu" class="watchlist-menu">
          <div class="menu-header" (click)="$event.stopPropagation()">
            <strong>Watchlist</strong>
            <span class="count">{{ watchlistState.count() }} items</span>
          </div>
          <mat-divider></mat-divider>
          @if (watchlistState.items().length > 0) {
            @for (item of watchlistState.items().slice(0, 5); track item.id) {
              <a mat-menu-item [routerLink]="['/auctions', item.auctionId]" class="watchlist-item">
                <mat-icon>directions_car</mat-icon>
                <div class="watch-content">
                  <strong>{{ item.auction?.title || 'Auction' }}</strong>
                  @if (item.auction) {
                    <span>{{ item.auction.currentHighBid.amount | currency }}</span>
                  }
                </div>
              </a>
            }
            <mat-divider></mat-divider>
            <a mat-menu-item routerLink="/dashboard" class="view-all">
              View full watchlist
            </a>
          } @else {
            <div class="empty-notif">
              <mat-icon>favorite_border</mat-icon>
              <span>No watched auctions</span>
            </div>
          }
        </mat-menu>

        <!-- User Menu -->
        <button mat-icon-button [matMenuTriggerFor]="userMenu">
          <mat-icon>account_circle</mat-icon>
        </button>
        <mat-menu #userMenu="matMenu">
          <div class="user-info">
            <strong>{{ auth.currentUser()?.firstName }} {{ auth.currentUser()?.lastName }}</strong>
            <small>{{ auth.currentUser()?.email }}</small>
            @if (auth.currentUser()?.isDealer) {
              <span class="dealer-tag">
                <mat-icon>verified</mat-icon>
                Dealer
              </span>
            }
          </div>
          <mat-divider></mat-divider>
          <a mat-menu-item routerLink="/dashboard">
            <mat-icon>dashboard</mat-icon>
            Dashboard
          </a>
          <a mat-menu-item routerLink="/dashboard">
            <mat-icon>gavel</mat-icon>
            My Bids
          </a>
          @if (isSeller()) {
            <a mat-menu-item routerLink="/seller">
              <mat-icon>storefront</mat-icon>
              Seller Dashboard
            </a>
          }
          @if (auth.isAdmin()) {
            <mat-divider></mat-divider>
            <a mat-menu-item routerLink="/admin">
              <mat-icon>admin_panel_settings</mat-icon>
              Admin Panel
            </a>
          }
          <mat-divider></mat-divider>
          <a mat-menu-item routerLink="/profile">
            <mat-icon>person</mat-icon>
            Profile
          </a>
          <a mat-menu-item routerLink="/settings">
            <mat-icon>settings</mat-icon>
            Settings
          </a>
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

    .icon-btn {
      margin-right: 8px;
    }

    .user-info {
      padding: 16px;
      display: flex;
      flex-direction: column;
    }

    .user-info small {
      color: rgba(0, 0, 0, 0.6);
    }

    .dealer-tag {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      margin-top: 8px;
      padding: 4px 8px;
      background: linear-gradient(135deg, #7c4dff, #536dfe);
      color: white;
      border-radius: 12px;
      font-size: 0.75rem;
    }

    .dealer-tag mat-icon {
      font-size: 14px;
      width: 14px;
      height: 14px;
    }

    /* Notification Menu */
    .menu-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
    }

    .menu-header .count {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }

    .notification-item, .watchlist-item {
      display: flex !important;
      align-items: flex-start !important;
      gap: 12px;
      padding: 12px 16px !important;
      height: auto !important;
      line-height: 1.4 !important;
    }

    .notification-item.unread {
      background: #e3f2fd;
    }

    .notification-item mat-icon.outbid { color: #f44336; }
    .notification-item mat-icon.auctionwon { color: #4caf50; }
    .notification-item mat-icon.bidplaced { color: #2196f3; }

    .notif-content, .watch-content {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-width: 0;
    }

    .notif-content strong, .watch-content strong {
      font-size: 0.875rem;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .notif-content span, .watch-content span {
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.6);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .empty-notif {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 24px;
      color: rgba(0, 0, 0, 0.5);
    }

    .empty-notif mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      margin-bottom: 8px;
    }

    .view-all {
      text-align: center;
      color: #1976d2;
    }

    @media (max-width: 768px) {
      .nav-links {
        display: none;
      }
    }
  `]
})
export class NavbarComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly notificationState = inject(NotificationState);
  readonly watchlistState = inject(WatchlistState);

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.notificationState.loadNotifications();
      this.watchlistState.loadWatchlist();
    }
  }

  isSeller(): boolean {
    return this.auth.currentUser()?.roles?.some(r => r === 'Seller' || r === 'Dealer') ?? false;
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'Outbid': return 'trending_down';
      case 'AuctionWon': return 'emoji_events';
      case 'BidPlaced': return 'gavel';
      case 'AuctionEndingSoon': return 'timer';
      default: return 'notifications';
    }
  }

  markAllRead(): void {
    this.notificationState.markAllAsRead();
  }

  handleNotificationClick(notification: any): void {
    if (!notification.isRead) {
      this.notificationState.markAsRead(notification.id);
    }
  }
}
