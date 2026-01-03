import { Component, OnInit, OnDestroy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuctionState } from '../../../core/state/auction.state';
import { SignalRService } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { CountdownTimerComponent } from '../../../shared/components/countdown-timer/countdown-timer.component';
import { BidInputComponent } from '../../../shared/components/bid-input/bid-input.component';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';

@Component({
  selector: 'app-auction-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatChipsModule,
    MatDividerModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    CountdownTimerComponent,
    BidInputComponent,
    TimeAgoPipe
  ],
  template: `
    <div class="auction-detail-container">
      @if (state.loading()) {
        <div class="loading">
          <mat-spinner></mat-spinner>
        </div>
      } @else if (state.error() && !auction()) {
        <div class="error">
          <mat-icon>error</mat-icon>
          <p>{{ state.error() }}</p>
          <button mat-button routerLink="/auctions">Back to Auctions</button>
        </div>
      } @else if (auction()) {
        <div class="auction-layout">
          <!-- Main Content -->
          <div class="main-content">
            <mat-card class="image-card">
              @if (auction()!.vehicle?.imageUrl) {
                <img [src]="auction()!.vehicle!.imageUrl" [alt]="auction()!.title" class="main-image">
              } @else {
                <div class="placeholder">
                  <mat-icon>directions_car</mat-icon>
                </div>
              }
            </mat-card>

            <mat-card class="info-card">
              <mat-tab-group>
                <mat-tab label="Vehicle Details">
                  @if (auction()!.vehicle) {
                    <div class="vehicle-details">
                      <div class="detail-grid">
                        <div class="detail-item">
                          <span class="label">Make</span>
                          <span class="value">{{ auction()!.vehicle!.make }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Model</span>
                          <span class="value">{{ auction()!.vehicle!.model }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Year</span>
                          <span class="value">{{ auction()!.vehicle!.year }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Mileage</span>
                          <span class="value">{{ auction()!.vehicle!.mileage | number }} mi</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">VIN</span>
                          <span class="value">{{ auction()!.vehicle!.vin }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Transmission</span>
                          <span class="value">{{ auction()!.vehicle!.transmission || 'N/A' }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Engine</span>
                          <span class="value">{{ auction()!.vehicle!.engineType || 'N/A' }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Fuel Type</span>
                          <span class="value">{{ auction()!.vehicle!.fuelType || 'N/A' }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Exterior Color</span>
                          <span class="value">{{ auction()!.vehicle!.exteriorColor || 'N/A' }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Interior Color</span>
                          <span class="value">{{ auction()!.vehicle!.interiorColor || 'N/A' }}</span>
                        </div>
                        <div class="detail-item">
                          <span class="label">Title Status</span>
                          <span class="value">{{ auction()!.vehicle!.titleStatus }}</span>
                        </div>
                      </div>
                    </div>
                  }
                </mat-tab>

                <mat-tab label="Bid History">
                  <div class="bid-history">
                    @if (state.bidHistory().length > 0) {
                      <mat-list>
                        @for (bid of state.bidHistory(); track bid.id) {
                          <mat-list-item>
                            <mat-icon matListItemIcon>gavel</mat-icon>
                            <div matListItemTitle>
                              {{ bid.amount.amount | currency:bid.amount.currency }}
                              @if (bid.isProxyBid) {
                                <mat-chip>Proxy</mat-chip>
                              }
                            </div>
                            <div matListItemLine>
                              {{ bid.bidderName || 'Anonymous' }} - {{ bid.placedAt | timeAgo }}
                            </div>
                          </mat-list-item>
                        }
                      </mat-list>
                    } @else {
                      <p class="no-bids">No bids yet. Be the first to bid!</p>
                    }
                  </div>
                </mat-tab>
              </mat-tab-group>
            </mat-card>
          </div>

          <!-- Sidebar -->
          <aside class="sidebar">
            <mat-card class="auction-info-card">
              <mat-card-header>
                <mat-card-title>{{ auction()!.title }}</mat-card-title>
                <mat-chip-set>
                  <mat-chip [class]="'status-' + auction()!.status.toLowerCase()">
                    {{ auction()!.status }}
                  </mat-chip>
                  @if (auction()!.isDealerOnly) {
                    <mat-chip color="accent">Dealers Only</mat-chip>
                  }
                </mat-chip-set>
              </mat-card-header>

              <mat-card-content>
                @if (auction()!.status === 'Active') {
                  <div class="countdown-section">
                    <span class="label">Time Remaining</span>
                    <app-countdown-timer [endTime]="auction()!.endTime"></app-countdown-timer>
                  </div>
                }

                <mat-divider></mat-divider>

                <div class="price-info">
                  <div class="price-row">
                    <span class="label">Starting Price</span>
                    <span class="value">{{ auction()!.startingPrice.amount | currency:auction()!.startingPrice.currency }}</span>
                  </div>
                  @if (auction()!.reservePrice) {
                    <div class="price-row">
                      <span class="label">Reserve</span>
                      <span class="value" [class.met]="auction()!.currentHighBid.amount >= auction()!.reservePrice!.amount">
                        {{ auction()!.currentHighBid.amount >= auction()!.reservePrice!.amount ? 'Met' : 'Not Met' }}
                      </span>
                    </div>
                  }
                  @if (auction()!.buyNowPrice) {
                    <div class="price-row">
                      <span class="label">Buy Now</span>
                      <span class="value buy-now">{{ auction()!.buyNowPrice!.amount | currency:auction()!.buyNowPrice!.currency }}</span>
                    </div>
                  }
                </div>

                <mat-divider></mat-divider>

                @if (auction()!.status === 'Active') {
                  @if (auth.isAuthenticated()) {
                    <app-bid-input
                      [currentBid]="auction()!.currentHighBid.amount"
                      [minimumBid]="auction()!.currentHighBid.amount + 100"
                      [currency]="auction()!.currentHighBid.currency"
                      [error]="state.error()"
                      [disabled]="auction()!.isDealerOnly && !auth.isDealer()"
                      (bid)="placeBid($event)"
                    ></app-bid-input>

                    @if (auction()!.isDealerOnly && !auth.isDealer()) {
                      <p class="dealer-only-notice">
                        <mat-icon>info</mat-icon>
                        This auction is for licensed dealers only
                      </p>
                    }
                  } @else {
                    <div class="login-prompt">
                      <p>Sign in to place a bid</p>
                      <a mat-raised-button color="primary" routerLink="/login">Login</a>
                      <a mat-button routerLink="/register">Create Account</a>
                    </div>
                  }
                } @else if (auction()!.status === 'Closed') {
                  <div class="auction-ended">
                    <mat-icon>check_circle</mat-icon>
                    <p>This auction has ended</p>
                    <p class="final-price">
                      Final Price: {{ auction()!.currentHighBid.amount | currency:auction()!.currentHighBid.currency }}
                    </p>
                  </div>
                } @else if (auction()!.status === 'Scheduled') {
                  <div class="auction-scheduled">
                    <mat-icon>schedule</mat-icon>
                    <p>Auction starts on</p>
                    <p class="start-time">{{ auction()!.startTime | date:'medium' }}</p>
                  </div>
                }
              </mat-card-content>
            </mat-card>

            <!-- Connection Status -->
            <div class="connection-status" [class.connected]="signalR.connectionState() === 'Connected'">
              <mat-icon>{{ signalR.connectionState() === 'Connected' ? 'wifi' : 'wifi_off' }}</mat-icon>
              {{ signalR.connectionState() === 'Connected' ? 'Live Updates' : 'Reconnecting...' }}
            </div>
          </aside>
        </div>
      }
    </div>
  `,
  styles: [`
    .auction-detail-container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 24px;
    }

    .loading, .error {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 64px;
    }

    .auction-layout {
      display: grid;
      grid-template-columns: 1fr 400px;
      gap: 24px;
    }

    @media (max-width: 1024px) {
      .auction-layout {
        grid-template-columns: 1fr;
      }
    }

    .main-content {
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .image-card {
      overflow: hidden;
    }

    .main-image {
      width: 100%;
      max-height: 500px;
      object-fit: cover;
    }

    .placeholder {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 400px;
      background: #f5f5f5;
    }

    .placeholder mat-icon {
      font-size: 128px;
      width: 128px;
      height: 128px;
      color: rgba(0, 0, 0, 0.2);
    }

    .vehicle-details {
      padding: 16px;
    }

    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 16px;
    }

    .detail-item {
      padding: 12px;
      background: #f5f5f5;
      border-radius: 8px;
    }

    .detail-item .label {
      display: block;
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.6);
      margin-bottom: 4px;
    }

    .detail-item .value {
      font-weight: 500;
    }

    .bid-history {
      padding: 16px;
    }

    .no-bids {
      text-align: center;
      color: rgba(0, 0, 0, 0.6);
      padding: 32px;
    }

    .sidebar {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .auction-info-card mat-card-header {
      flex-direction: column;
      align-items: flex-start;
    }

    .countdown-section {
      text-align: center;
      padding: 16px;
    }

    .countdown-section .label {
      display: block;
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
      margin-bottom: 8px;
    }

    .price-info {
      padding: 16px 0;
    }

    .price-row {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
    }

    .price-row .value.met {
      color: #4caf50;
      font-weight: 500;
    }

    .price-row .value.buy-now {
      color: #1976d2;
      font-weight: 600;
    }

    .status-active { background: #4caf50 !important; color: white !important; }
    .status-scheduled { background: #2196f3 !important; color: white !important; }
    .status-closed { background: #9e9e9e !important; color: white !important; }

    .login-prompt {
      text-align: center;
      padding: 24px;
    }

    .login-prompt p {
      margin-bottom: 16px;
      color: rgba(0, 0, 0, 0.6);
    }

    .auction-ended, .auction-scheduled {
      text-align: center;
      padding: 24px;
    }

    .auction-ended mat-icon, .auction-scheduled mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }

    .auction-ended mat-icon {
      color: #4caf50;
    }

    .auction-ended .final-price {
      font-size: 1.5rem;
      font-weight: 600;
      color: #1976d2;
    }

    .dealer-only-notice {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background: #fff3e0;
      border-radius: 8px;
      color: #e65100;
      margin-top: 16px;
    }

    .connection-status {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 8px;
      background: #ffebee;
      border-radius: 4px;
      color: #c62828;
      font-size: 0.875rem;
    }

    .connection-status.connected {
      background: #e8f5e9;
      color: #2e7d32;
    }
  `]
})
export class AuctionDetailComponent implements OnInit, OnDestroy {
  readonly state = inject(AuctionState);
  readonly signalR = inject(SignalRService);
  readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  readonly auction = this.state.selectedAuction;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.signalR.startConnection();
      this.state.selectAuction(id);
    }
  }

  ngOnDestroy(): void {
    this.state.clearSelectedAuction();
  }

  placeBid(amount: number): void {
    this.state.placeBid(amount);
    this.snackBar.open('Bid placed successfully!', 'Close', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top'
    });
  }
}
