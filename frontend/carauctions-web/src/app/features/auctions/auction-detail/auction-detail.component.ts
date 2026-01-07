import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Clipboard, ClipboardModule } from '@angular/cdk/clipboard';
import { AuctionState } from '../../../core/state/auction.state';
import { SignalRService } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { WatchlistState } from '../../../core/state/watchlist.state';
import { ApiService } from '../../../core/services/api.service';
import { CountdownTimerComponent } from '../../../shared/components/countdown-timer/countdown-timer.component';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';

@Component({
  selector: 'app-auction-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatChipsModule,
    MatDividerModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    ClipboardModule,
    CountdownTimerComponent,
    TimeAgoPipe
  ],
  template: `
    <div class="auction-detail-container">
      @if (state.loading()) {
        <div class="loading">
          <mat-spinner></mat-spinner>
          <p>Loading auction details...</p>
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
            <!-- Image Gallery -->
            <mat-card class="image-card">
              <div class="image-gallery">
                <div class="main-image-container">
                  @if (auction()!.vehicle?.imageUrl) {
                    <img [src]="selectedImage() || auction()!.vehicle!.imageUrl"
                         [alt]="auction()!.title"
                         class="main-image"
                         (click)="openLightbox()">
                    <button mat-icon-button class="fullscreen-btn" (click)="openLightbox()">
                      <mat-icon>fullscreen</mat-icon>
                    </button>
                  } @else {
                    <div class="placeholder">
                      <mat-icon>directions_car</mat-icon>
                      <span>No Image Available</span>
                    </div>
                  }

                  <!-- Title Status Badge -->
                  @if (auction()!.vehicle?.titleStatus) {
                    <div class="title-badge" [class]="'title-' + auction()!.vehicle!.titleStatus.toLowerCase()">
                      {{ auction()!.vehicle!.titleStatus }}
                    </div>
                  }
                </div>

                <!-- Thumbnail Strip -->
                @if (auction()!.vehicle?.images && auction()!.vehicle!.images!.length > 1) {
                  <div class="thumbnail-strip">
                    @for (image of auction()!.vehicle!.images; track image.id) {
                      <img [src]="image.url"
                           [alt]="image.type"
                           class="thumbnail"
                           [class.active]="selectedImage() === image.url"
                           (click)="selectImage(image.url)">
                    }
                  </div>
                }
              </div>
            </mat-card>

            <!-- Info Tabs -->
            <mat-card class="info-card">
              <mat-tab-group>
                <!-- Vehicle Details Tab -->
                <mat-tab label="Vehicle Details">
                  @if (auction()!.vehicle) {
                    <div class="vehicle-details">
                      <!-- Key Specs -->
                      <div class="specs-section">
                        <h3>Key Specifications</h3>
                        <div class="detail-grid">
                          <div class="detail-item">
                            <mat-icon>directions_car</mat-icon>
                            <span class="label">Make</span>
                            <span class="value">{{ auction()!.vehicle!.make }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>category</mat-icon>
                            <span class="label">Model</span>
                            <span class="value">{{ auction()!.vehicle!.model }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>event</mat-icon>
                            <span class="label">Year</span>
                            <span class="value">{{ auction()!.vehicle!.year }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>speed</mat-icon>
                            <span class="label">Mileage</span>
                            <span class="value">{{ auction()!.vehicle!.mileage | number }} mi</span>
                          </div>
                        </div>
                      </div>

                      <!-- VIN Section -->
                      <div class="vin-section">
                        <h3>Vehicle Identification</h3>
                        <div class="vin-display">
                          <span class="vin-label">VIN:</span>
                          <code class="vin-code">{{ auction()!.vehicle!.vin }}</code>
                          <button mat-icon-button
                                  (click)="copyVin()"
                                  matTooltip="Copy VIN">
                            <mat-icon>content_copy</mat-icon>
                          </button>
                        </div>
                      </div>

                      <!-- Additional Specs -->
                      <div class="specs-section">
                        <h3>Additional Details</h3>
                        <div class="detail-grid">
                          <div class="detail-item">
                            <mat-icon>settings</mat-icon>
                            <span class="label">Transmission</span>
                            <span class="value">{{ auction()!.vehicle!.transmission || 'N/A' }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>engineering</mat-icon>
                            <span class="label">Engine</span>
                            <span class="value">{{ auction()!.vehicle!.engineType || 'N/A' }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>local_gas_station</mat-icon>
                            <span class="label">Fuel Type</span>
                            <span class="value">{{ auction()!.vehicle!.fuelType || 'N/A' }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>palette</mat-icon>
                            <span class="label">Exterior</span>
                            <span class="value">{{ auction()!.vehicle!.exteriorColor || 'N/A' }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>chair</mat-icon>
                            <span class="label">Interior</span>
                            <span class="value">{{ auction()!.vehicle!.interiorColor || 'N/A' }}</span>
                          </div>
                          <div class="detail-item">
                            <mat-icon>description</mat-icon>
                            <span class="label">Title</span>
                            <span class="value" [class]="'title-text-' + auction()!.vehicle!.titleStatus.toLowerCase()">
                              {{ auction()!.vehicle!.titleStatus }}
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>
                  }
                </mat-tab>

                <!-- Condition Report Tab -->
                <mat-tab label="Condition Report">
                  @if (auction()!.vehicle?.conditionReport) {
                    <div class="condition-report">
                      <div class="overall-grade">
                        <span class="grade-label">Overall Condition</span>
                        <span class="grade-value" [class]="'grade-' + auction()!.vehicle!.conditionReport!.overallGrade.toLowerCase()">
                          {{ auction()!.vehicle!.conditionReport!.overallGrade }}
                        </span>
                      </div>

                      <div class="grade-breakdown">
                        <div class="grade-item">
                          <mat-icon>directions_car</mat-icon>
                          <span class="label">Exterior</span>
                          <span class="grade" [class]="'grade-' + auction()!.vehicle!.conditionReport!.exteriorGrade.toLowerCase()">
                            {{ auction()!.vehicle!.conditionReport!.exteriorGrade }}
                          </span>
                        </div>
                        <div class="grade-item">
                          <mat-icon>airline_seat_recline_normal</mat-icon>
                          <span class="label">Interior</span>
                          <span class="grade" [class]="'grade-' + auction()!.vehicle!.conditionReport!.interiorGrade.toLowerCase()">
                            {{ auction()!.vehicle!.conditionReport!.interiorGrade }}
                          </span>
                        </div>
                        <div class="grade-item">
                          <mat-icon>build</mat-icon>
                          <span class="label">Mechanical</span>
                          <span class="grade" [class]="'grade-' + auction()!.vehicle!.conditionReport!.mechanicalGrade.toLowerCase()">
                            {{ auction()!.vehicle!.conditionReport!.mechanicalGrade }}
                          </span>
                        </div>
                      </div>

                      @if (auction()!.vehicle!.conditionReport!.notes) {
                        <div class="inspector-notes">
                          <h4>Inspector Notes</h4>
                          <p>{{ auction()!.vehicle!.conditionReport!.notes }}</p>
                        </div>
                      }

                      <div class="inspection-info">
                        <mat-icon>verified</mat-icon>
                        <span>Inspected {{ auction()!.vehicle!.conditionReport!.inspectedAt | timeAgo }}</span>
                      </div>
                    </div>
                  } @else {
                    <div class="no-report">
                      <mat-icon>assignment_late</mat-icon>
                      <p>Condition report not available for this vehicle</p>
                    </div>
                  }
                </mat-tab>

                <!-- Bid History Tab -->
                <mat-tab label="Bid History ({{ state.bidHistory().length }})">
                  <div class="bid-history">
                    @if (state.bidHistory().length > 0) {
                      <mat-list>
                        @for (bid of state.bidHistory(); track bid.id; let i = $index) {
                          <mat-list-item [class.winning-bid]="i === 0">
                            <mat-icon matListItemIcon [class.winning]="i === 0">
                              {{ i === 0 ? 'emoji_events' : 'gavel' }}
                            </mat-icon>
                            <div matListItemTitle class="bid-amount">
                              {{ bid.amount.amount | currency:bid.amount.currency }}
                              @if (bid.isProxyBid) {
                                <mat-chip class="proxy-chip">Auto-Bid</mat-chip>
                              }
                              @if (i === 0) {
                                <mat-chip class="leading-chip">Leading</mat-chip>
                              }
                            </div>
                            <div matListItemLine class="bid-info">
                              {{ bid.bidderName || 'Bidder ' + bid.bidderId.substring(0, 8) }}
                              <span class="bid-time">{{ bid.placedAt | timeAgo }}</span>
                            </div>
                          </mat-list-item>
                          @if (i < state.bidHistory().length - 1) {
                            <mat-divider></mat-divider>
                          }
                        }
                      </mat-list>
                    } @else {
                      <div class="no-bids">
                        <mat-icon>gavel</mat-icon>
                        <p>No bids yet. Be the first to bid!</p>
                      </div>
                    }
                  </div>
                </mat-tab>

                <!-- Description Tab -->
                <mat-tab label="Description">
                  <div class="description-content">
                    @if (auction()!.description) {
                      <p>{{ auction()!.description }}</p>
                    } @else {
                      <p class="no-description">No additional description provided.</p>
                    }
                  </div>
                </mat-tab>
              </mat-tab-group>
            </mat-card>
          </div>

          <!-- Sidebar -->
          <aside class="sidebar">
            <!-- Auction Info Card -->
            <mat-card class="auction-info-card">
              <mat-card-header>
                <mat-card-title>{{ auction()!.title }}</mat-card-title>
                <mat-chip-set class="status-chips">
                  <mat-chip [class]="'status-' + auction()!.status.toLowerCase()">
                    {{ auction()!.status }}
                  </mat-chip>
                  <mat-chip [class]="'type-' + auction()!.type.toLowerCase()">
                    {{ auction()!.type }}
                  </mat-chip>
                  @if (auction()!.isDealerOnly) {
                    <mat-chip class="dealer-chip">Dealers Only</mat-chip>
                  }
                </mat-chip-set>
              </mat-card-header>

              <mat-card-content>
                <!-- Countdown -->
                @if (auction()!.status === 'Active') {
                  <div class="countdown-section">
                    <span class="label">Time Remaining</span>
                    <app-countdown-timer [endTime]="auction()!.endTime"></app-countdown-timer>
                    @if (auction()!.antiSnipingEnabled) {
                      <div class="anti-sniping-notice">
                        <mat-icon>schedule</mat-icon>
                        <span>Anti-sniping: Extends {{ auction()!.antiSnipingExtensionMinutes || 5 }} min if bid placed in final minutes</span>
                      </div>
                    }
                  </div>
                }

                <mat-divider></mat-divider>

                <!-- Price Information -->
                <div class="price-info">
                  <div class="current-bid-display">
                    <span class="label">Current Bid</span>
                    <span class="amount">{{ auction()!.currentHighBid.amount | currency:auction()!.currentHighBid.currency }}</span>
                    <span class="bid-count">{{ auction()!.bidCount }} bid{{ auction()!.bidCount !== 1 ? 's' : '' }}</span>
                  </div>

                  <div class="price-row">
                    <span class="label">Starting Price</span>
                    <span class="value">{{ auction()!.startingPrice.amount | currency:auction()!.startingPrice.currency }}</span>
                  </div>

                  @if (auction()!.reservePrice) {
                    <div class="price-row reserve-row">
                      <span class="label">Reserve</span>
                      <span class="value" [class.met]="reserveMet()" [class.not-met]="!reserveMet()">
                        <mat-icon>{{ reserveMet() ? 'check_circle' : 'cancel' }}</mat-icon>
                        {{ reserveMet() ? 'Reserve Met' : 'Reserve Not Met' }}
                      </span>
                    </div>
                  }

                  @if (auction()!.buyNowPrice && auction()!.status === 'Active') {
                    <div class="buy-now-section">
                      <span class="label">Buy Now Price</span>
                      <span class="buy-now-price">{{ auction()!.buyNowPrice!.amount | currency:auction()!.buyNowPrice!.currency }}</span>
                      @if (auth.isAuthenticated()) {
                        <button mat-raised-button color="accent" class="buy-now-btn" (click)="buyNow()">
                          <mat-icon>shopping_cart</mat-icon>
                          Buy Now
                        </button>
                      }
                    </div>
                  }
                </div>

                <mat-divider></mat-divider>

                <!-- Bidding Section -->
                @if (auction()!.status === 'Active') {
                  @if (auth.isAuthenticated()) {
                    <div class="bidding-section">
                      <!-- Regular Bid -->
                      <div class="bid-input-group">
                        <mat-form-field appearance="outline" class="bid-field">
                          <mat-label>Your Bid</mat-label>
                          <input matInput type="number" [(ngModel)]="bidAmount" [min]="minimumBid()">
                          <span matTextPrefix>$&nbsp;</span>
                        </mat-form-field>
                        <button mat-raised-button color="primary"
                                (click)="placeBid()"
                                [disabled]="!bidAmount || bidAmount < minimumBid()">
                          <mat-icon>gavel</mat-icon>
                          Place Bid
                        </button>
                      </div>

                      <!-- Quick Bid Buttons -->
                      <div class="quick-bids">
                        @for (increment of [100, 500, 1000, 5000]; track increment) {
                          <button mat-stroked-button (click)="quickBid(increment)">
                            +{{ increment | currency:'USD':'symbol':'1.0-0' }}
                          </button>
                        }
                      </div>

                      <!-- Proxy Bidding -->
                      <div class="proxy-bidding">
                        <mat-checkbox [(ngModel)]="useProxyBid">Enable Proxy Bidding</mat-checkbox>
                        @if (useProxyBid) {
                          <mat-form-field appearance="outline" class="proxy-field">
                            <mat-label>Maximum Bid</mat-label>
                            <input matInput type="number" [(ngModel)]="maxProxyAmount">
                            <span matTextPrefix>$&nbsp;</span>
                            <mat-hint>System will auto-bid up to this amount</mat-hint>
                          </mat-form-field>
                        }
                      </div>

                      @if (auction()!.isDealerOnly && !auth.isDealer()) {
                        <div class="dealer-only-notice">
                          <mat-icon>info</mat-icon>
                          <span>This auction is for licensed dealers only</span>
                        </div>
                      }
                    </div>
                  } @else {
                    <div class="login-prompt">
                      <p>Sign in to place a bid</p>
                      <a mat-raised-button color="primary" routerLink="/login">Login</a>
                      <a mat-button routerLink="/register">Create Account</a>
                    </div>
                  }
                } @else if (auction()!.status === 'Closed' || auction()!.status === 'Completed') {
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

                <mat-divider></mat-divider>

                <!-- Action Buttons -->
                <div class="action-buttons">
                  <button mat-stroked-button (click)="toggleWatchlist()" [class.watching]="isWatching()">
                    <mat-icon>{{ isWatching() ? 'favorite' : 'favorite_border' }}</mat-icon>
                    {{ isWatching() ? 'Watching' : 'Watch' }}
                  </button>
                  <button mat-stroked-button (click)="shareAuction()">
                    <mat-icon>share</mat-icon>
                    Share
                  </button>
                </div>
              </mat-card-content>
            </mat-card>

            <!-- Connection Status -->
            <div class="connection-status" [class.connected]="signalR.connectionState() === 'Connected'">
              <mat-icon>{{ signalR.connectionState() === 'Connected' ? 'wifi' : 'wifi_off' }}</mat-icon>
              {{ signalR.connectionState() === 'Connected' ? 'Live Updates Active' : 'Reconnecting...' }}
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
      gap: 16px;
    }

    .error mat-icon { font-size: 64px; width: 64px; height: 64px; color: #f44336; }

    .auction-layout {
      display: grid;
      grid-template-columns: 1fr 420px;
      gap: 24px;
    }

    @media (max-width: 1024px) {
      .auction-layout { grid-template-columns: 1fr; }
      .sidebar { order: -1; }
    }

    .main-content { display: flex; flex-direction: column; gap: 24px; }

    /* Image Gallery */
    .image-card { overflow: hidden; }
    .main-image-container { position: relative; }
    .main-image { width: 100%; max-height: 500px; object-fit: cover; cursor: zoom-in; }
    .fullscreen-btn { position: absolute; top: 16px; right: 16px; background: rgba(0,0,0,0.5); color: white; }
    .placeholder { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 400px; background: #f5f5f5; color: rgba(0,0,0,0.3); }
    .placeholder mat-icon { font-size: 128px; width: 128px; height: 128px; }

    .title-badge {
      position: absolute; top: 16px; left: 16px;
      padding: 6px 12px; border-radius: 4px;
      font-weight: 600; font-size: 0.875rem;
    }
    .title-clean { background: #4caf50; color: white; }
    .title-rebuilt { background: #ff9800; color: white; }
    .title-salvage { background: #f44336; color: white; }

    .thumbnail-strip { display: flex; gap: 8px; padding: 16px; overflow-x: auto; }
    .thumbnail { width: 80px; height: 60px; object-fit: cover; border-radius: 4px; cursor: pointer; opacity: 0.7; transition: opacity 0.2s; }
    .thumbnail:hover, .thumbnail.active { opacity: 1; outline: 2px solid #1976d2; }

    /* Vehicle Details */
    .vehicle-details { padding: 24px; }
    .specs-section { margin-bottom: 24px; }
    .specs-section h3 { margin: 0 0 16px; font-size: 1.125rem; color: rgba(0,0,0,0.87); }

    .detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 16px; }
    .detail-item { display: flex; flex-direction: column; padding: 16px; background: #f5f5f5; border-radius: 8px; }
    .detail-item mat-icon { color: #1976d2; margin-bottom: 8px; }
    .detail-item .label { font-size: 0.75rem; color: rgba(0,0,0,0.6); margin-bottom: 4px; }
    .detail-item .value { font-weight: 500; font-size: 1rem; }

    .vin-section { margin: 24px 0; padding: 16px; background: #e3f2fd; border-radius: 8px; }
    .vin-section h3 { margin: 0 0 12px; }
    .vin-display { display: flex; align-items: center; gap: 12px; }
    .vin-label { font-weight: 500; }
    .vin-code { font-family: monospace; font-size: 1.125rem; letter-spacing: 1px; background: white; padding: 8px 16px; border-radius: 4px; }

    .title-text-clean { color: #4caf50; }
    .title-text-rebuilt { color: #ff9800; }
    .title-text-salvage { color: #f44336; }

    /* Condition Report */
    .condition-report { padding: 24px; }
    .overall-grade { text-align: center; margin-bottom: 32px; }
    .grade-label { display: block; font-size: 0.875rem; color: rgba(0,0,0,0.6); margin-bottom: 8px; }
    .grade-value { font-size: 3rem; font-weight: 700; }
    .grade-grade5 { color: #4caf50; }
    .grade-grade4 { color: #8bc34a; }
    .grade-grade3 { color: #ff9800; }
    .grade-grade2 { color: #ff5722; }
    .grade-grade1 { color: #f44336; }

    .grade-breakdown { display: flex; justify-content: space-around; margin-bottom: 24px; }
    .grade-item { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .grade-item mat-icon { color: #1976d2; }
    .grade-item .grade { font-size: 1.5rem; font-weight: 600; }

    .inspector-notes { background: #f5f5f5; padding: 16px; border-radius: 8px; margin-bottom: 16px; }
    .inspector-notes h4 { margin: 0 0 8px; }
    .inspection-info { display: flex; align-items: center; gap: 8px; color: rgba(0,0,0,0.6); }
    .no-report { text-align: center; padding: 48px; color: rgba(0,0,0,0.5); }
    .no-report mat-icon { font-size: 64px; width: 64px; height: 64px; }

    /* Bid History */
    .bid-history { padding: 16px; max-height: 400px; overflow-y: auto; }
    .winning-bid { background: #e8f5e9; }
    .bid-amount { display: flex; align-items: center; gap: 8px; }
    .proxy-chip { font-size: 0.625rem; height: 20px; }
    .leading-chip { background: #4caf50 !important; color: white !important; font-size: 0.625rem; height: 20px; }
    .bid-info { font-size: 0.875rem; }
    .bid-time { color: rgba(0,0,0,0.5); margin-left: 8px; }
    mat-icon.winning { color: #ffc107; }
    .no-bids { text-align: center; padding: 48px; color: rgba(0,0,0,0.5); }
    .no-bids mat-icon { font-size: 64px; width: 64px; height: 64px; }

    .description-content { padding: 24px; line-height: 1.6; }
    .no-description { color: rgba(0,0,0,0.5); font-style: italic; }

    /* Sidebar */
    .sidebar { display: flex; flex-direction: column; gap: 16px; }
    .auction-info-card mat-card-header { flex-direction: column; align-items: flex-start; padding-bottom: 16px; }
    .status-chips { margin-top: 12px; }

    .status-active { background: #4caf50 !important; color: white !important; }
    .status-scheduled { background: #2196f3 !important; color: white !important; }
    .status-closed, .status-completed { background: #9e9e9e !important; color: white !important; }
    .type-live { background: #f44336 !important; color: white !important; }
    .type-timed { background: #ff9800 !important; color: white !important; }
    .dealer-chip { background: #7c4dff !important; color: white !important; }

    .countdown-section { text-align: center; padding: 16px; }
    .countdown-section .label { display: block; font-size: 0.875rem; color: rgba(0,0,0,0.6); margin-bottom: 8px; }

    .anti-sniping-notice { display: flex; align-items: center; gap: 8px; margin-top: 12px; padding: 8px 12px; background: #fff3e0; border-radius: 4px; font-size: 0.75rem; color: #e65100; }

    .price-info { padding: 16px 0; }
    .current-bid-display { text-align: center; margin-bottom: 16px; }
    .current-bid-display .label { display: block; font-size: 0.875rem; color: rgba(0,0,0,0.6); }
    .current-bid-display .amount { display: block; font-size: 2rem; font-weight: 700; color: #1976d2; }
    .current-bid-display .bid-count { font-size: 0.875rem; color: rgba(0,0,0,0.5); }

    .price-row { display: flex; justify-content: space-between; padding: 8px 0; }
    .reserve-row .value { display: flex; align-items: center; gap: 4px; }
    .reserve-row .value.met { color: #4caf50; }
    .reserve-row .value.not-met { color: #f44336; }
    .reserve-row .value mat-icon { font-size: 18px; width: 18px; height: 18px; }

    .buy-now-section { text-align: center; padding: 16px; background: #fff8e1; border-radius: 8px; margin-top: 16px; }
    .buy-now-price { display: block; font-size: 1.5rem; font-weight: 700; color: #ff6f00; margin: 8px 0; }
    .buy-now-btn { width: 100%; margin-top: 8px; }

    .bidding-section { padding: 16px 0; }
    .bid-input-group { display: flex; gap: 12px; align-items: flex-start; }
    .bid-field { flex: 1; }
    .quick-bids { display: flex; gap: 8px; margin: 12px 0; flex-wrap: wrap; }
    .quick-bids button { flex: 1; min-width: 70px; }

    .proxy-bidding { margin-top: 16px; padding: 16px; background: #f5f5f5; border-radius: 8px; }
    .proxy-field { width: 100%; margin-top: 12px; }

    .dealer-only-notice { display: flex; align-items: center; gap: 8px; padding: 12px; background: #fff3e0; border-radius: 8px; color: #e65100; margin-top: 16px; }

    .login-prompt { text-align: center; padding: 24px; }
    .login-prompt p { margin-bottom: 16px; color: rgba(0,0,0,0.6); }

    .auction-ended, .auction-scheduled { text-align: center; padding: 24px; }
    .auction-ended mat-icon, .auction-scheduled mat-icon { font-size: 48px; width: 48px; height: 48px; }
    .auction-ended mat-icon { color: #4caf50; }
    .auction-ended .final-price { font-size: 1.5rem; font-weight: 600; color: #1976d2; }

    .action-buttons { display: flex; gap: 12px; padding-top: 16px; }
    .action-buttons button { flex: 1; }
    .action-buttons button.watching { color: #f44336; }
    .action-buttons button.watching mat-icon { color: #f44336; }

    .connection-status { display: flex; align-items: center; justify-content: center; gap: 8px; padding: 12px; background: #ffebee; border-radius: 8px; color: #c62828; font-size: 0.875rem; }
    .connection-status.connected { background: #e8f5e9; color: #2e7d32; }
  `]
})
export class AuctionDetailComponent implements OnInit, OnDestroy {
  readonly state = inject(AuctionState);
  readonly signalR = inject(SignalRService);
  readonly auth = inject(AuthService);
  readonly watchlistState = inject(WatchlistState);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);
  private readonly clipboard = inject(Clipboard);
  private readonly api = inject(ApiService);

  readonly auction = this.state.selectedAuction;
  readonly selectedImage = signal<string | null>(null);

  bidAmount: number = 0;
  useProxyBid = false;
  maxProxyAmount: number = 0;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.signalR.startConnection();
      this.state.selectAuction(id);
      if (this.auth.isAuthenticated()) {
        this.watchlistState.loadWatchlist();
      }
    }
  }

  ngOnDestroy(): void {
    this.state.clearSelectedAuction();
  }

  minimumBid(): number {
    const current = this.auction()?.currentHighBid.amount || 0;
    return current + 100;
  }

  reserveMet(): boolean {
    const auction = this.auction();
    if (!auction?.reservePrice) return true;
    return auction.currentHighBid.amount >= auction.reservePrice.amount;
  }

  isWatching(): boolean {
    const auction = this.auction();
    return auction ? this.watchlistState.isWatching(auction.id) : false;
  }

  selectImage(url: string): void {
    this.selectedImage.set(url);
  }

  openLightbox(): void {
    // Could implement a lightbox dialog here
    const imageUrl = this.selectedImage() || this.auction()?.vehicle?.imageUrl;
    if (imageUrl) {
      window.open(imageUrl, '_blank');
    }
  }

  copyVin(): void {
    const vin = this.auction()?.vehicle?.vin;
    if (vin) {
      this.clipboard.copy(vin);
      this.snackBar.open('VIN copied to clipboard', 'Close', { duration: 2000 });
    }
  }

  placeBid(): void {
    if (!this.bidAmount || this.bidAmount < this.minimumBid()) return;

    if (this.useProxyBid && this.maxProxyAmount > 0) {
      this.api.placeBid(
        this.auction()!.id,
        this.bidAmount,
        'USD',
        true,
        this.maxProxyAmount
      ).subscribe({
        next: () => {
          this.snackBar.open('Proxy bid placed successfully!', 'Close', { duration: 3000 });
          this.bidAmount = 0;
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Failed to place bid', 'Close', { duration: 3000 });
        }
      });
    } else {
      this.state.placeBid(this.bidAmount);
      this.snackBar.open('Bid placed successfully!', 'Close', { duration: 3000 });
      this.bidAmount = 0;
    }
  }

  quickBid(increment: number): void {
    this.bidAmount = this.minimumBid() + increment - 100;
    this.placeBid();
  }

  buyNow(): void {
    if (!this.auction()?.buyNowPrice) return;

    if (confirm(`Buy this vehicle now for ${this.auction()!.buyNowPrice!.amount.toLocaleString('en-US', { style: 'currency', currency: 'USD' })}?`)) {
      this.api.buyNow(this.auction()!.id).subscribe({
        next: () => {
          this.snackBar.open('Congratulations! You have purchased this vehicle!', 'Close', { duration: 5000 });
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Failed to complete purchase', 'Close', { duration: 3000 });
        }
      });
    }
  }

  toggleWatchlist(): void {
    const auction = this.auction();
    if (auction) {
      this.watchlistState.toggleWatch(auction.id);
      const message = this.isWatching() ? 'Added to watchlist' : 'Removed from watchlist';
      this.snackBar.open(message, 'Close', { duration: 2000 });
    }
  }

  shareAuction(): void {
    const url = window.location.href;
    if (navigator.share) {
      navigator.share({
        title: this.auction()?.title,
        text: `Check out this auction: ${this.auction()?.title}`,
        url: url
      });
    } else {
      this.clipboard.copy(url);
      this.snackBar.open('Link copied to clipboard', 'Close', { duration: 2000 });
    }
  }
}
