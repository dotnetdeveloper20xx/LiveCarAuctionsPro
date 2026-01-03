import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuctionState } from '../../../core/state/auction.state';
import { CountdownTimerComponent } from '../../../shared/components/countdown-timer/countdown-timer.component';
import { AuctionStatus, AuctionType } from '../../../shared/models/auction.model';

@Component({
  selector: 'app-auction-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    CountdownTimerComponent
  ],
  template: `
    <div class="auction-list-container">
      <header class="page-header">
        <h1>Live Auctions</h1>
        <p>Find your next vehicle from our selection of auctions</p>
      </header>

      <div class="filters">
        <mat-form-field appearance="outline">
          <mat-label>Search</mat-label>
          <input matInput [(ngModel)]="searchTerm" (keyup.enter)="search()" placeholder="Search auctions...">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Status</mat-label>
          <mat-select [(ngModel)]="statusFilter" (selectionChange)="search()">
            <mat-option value="">All</mat-option>
            <mat-option value="Active">Active</mat-option>
            <mat-option value="Scheduled">Upcoming</mat-option>
            <mat-option value="Closed">Closed</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Type</mat-label>
          <mat-select [(ngModel)]="typeFilter" (selectionChange)="search()">
            <mat-option value="">All</mat-option>
            <mat-option value="Timed">Timed</mat-option>
            <mat-option value="Live">Live</mat-option>
            <mat-option value="BuyNow">Buy Now</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      @if (state.loading()) {
        <div class="loading">
          <mat-spinner></mat-spinner>
        </div>
      } @else if (state.error()) {
        <div class="error">
          <mat-icon>error</mat-icon>
          <p>{{ state.error() }}</p>
          <button mat-button (click)="loadAuctions()">Retry</button>
        </div>
      } @else {
        <div class="auctions-grid">
          @for (auction of state.auctions(); track auction.id) {
            <mat-card class="auction-card" [routerLink]="['/auctions', auction.id]">
              <div class="auction-image">
                @if (auction.vehicle?.imageUrl) {
                  <img [src]="auction.vehicle!.imageUrl" [alt]="auction.title">
                } @else {
                  <div class="placeholder">
                    <mat-icon>directions_car</mat-icon>
                  </div>
                }
                <mat-chip-set class="status-chips">
                  <mat-chip [class]="'status-' + auction.status.toLowerCase()">
                    {{ auction.status }}
                  </mat-chip>
                  @if (auction.isDealerOnly) {
                    <mat-chip color="accent">Dealers Only</mat-chip>
                  }
                </mat-chip-set>
              </div>

              <mat-card-content>
                <h3 class="auction-title">{{ auction.title }}</h3>

                @if (auction.vehicle) {
                  <p class="vehicle-info">
                    {{ auction.vehicle.year }} {{ auction.vehicle.make }} {{ auction.vehicle.model }}
                  </p>
                  <p class="vehicle-details">
                    <span>{{ auction.vehicle.mileage | number }} mi</span>
                    <span>{{ auction.vehicle.transmission }}</span>
                  </p>
                }

                <div class="bid-info">
                  <div class="current-bid">
                    <span class="label">Current Bid</span>
                    <span class="amount">{{ auction.currentHighBid.amount | currency:auction.currentHighBid.currency }}</span>
                  </div>
                  <div class="bid-count">
                    <mat-icon>gavel</mat-icon>
                    {{ auction.bidCount }} bids
                  </div>
                </div>

                @if (auction.status === 'Active') {
                  <div class="time-remaining">
                    <span class="label">Ends in</span>
                    <app-countdown-timer [endTime]="auction.endTime"></app-countdown-timer>
                  </div>
                }
              </mat-card-content>

              <mat-card-actions>
                <button mat-button color="primary">
                  <mat-icon>visibility</mat-icon>
                  View Details
                </button>
                @if (auction.status === 'Active') {
                  <button mat-raised-button color="primary">
                    <mat-icon>gavel</mat-icon>
                    Place Bid
                  </button>
                }
              </mat-card-actions>
            </mat-card>
          } @empty {
            <div class="no-auctions">
              <mat-icon>search_off</mat-icon>
              <p>No auctions found</p>
            </div>
          }
        </div>

        <mat-paginator
          [length]="state.pagination().totalCount"
          [pageSize]="state.pagination().pageSize"
          [pageIndex]="state.pagination().pageNumber - 1"
          [pageSizeOptions]="[12, 24, 48]"
          (page)="onPageChange($event)"
          showFirstLastButtons>
        </mat-paginator>
      }
    </div>
  `,
  styles: [`
    .auction-list-container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 24px;
    }

    .page-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .page-header h1 {
      margin: 0;
      font-size: 2rem;
    }

    .page-header p {
      color: rgba(0, 0, 0, 0.6);
      margin: 8px 0 0;
    }

    .filters {
      display: flex;
      gap: 16px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }

    .filters mat-form-field {
      flex: 1;
      min-width: 200px;
    }

    .loading, .error {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 64px;
    }

    .error mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #f44336;
    }

    .auctions-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 24px;
      margin-bottom: 24px;
    }

    .auction-card {
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
    }

    .auction-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
    }

    .auction-image {
      position: relative;
      height: 200px;
      background: #f5f5f5;
      overflow: hidden;
    }

    .auction-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .auction-image .placeholder {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100%;
      color: rgba(0, 0, 0, 0.3);
    }

    .auction-image .placeholder mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
    }

    .status-chips {
      position: absolute;
      top: 8px;
      left: 8px;
    }

    .status-active { background: #4caf50 !important; color: white !important; }
    .status-scheduled { background: #2196f3 !important; color: white !important; }
    .status-closed { background: #9e9e9e !important; color: white !important; }

    .auction-title {
      margin: 0 0 8px;
      font-size: 1.125rem;
      font-weight: 500;
    }

    .vehicle-info {
      margin: 0;
      color: rgba(0, 0, 0, 0.8);
    }

    .vehicle-details {
      margin: 4px 0 16px;
      color: rgba(0, 0, 0, 0.6);
      font-size: 0.875rem;
    }

    .vehicle-details span:not(:last-child)::after {
      content: ' • ';
    }

    .bid-info {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px;
      background: #f5f5f5;
      border-radius: 8px;
      margin-bottom: 12px;
    }

    .current-bid .label {
      display: block;
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.6);
    }

    .current-bid .amount {
      font-size: 1.25rem;
      font-weight: 600;
      color: #1976d2;
    }

    .bid-count {
      display: flex;
      align-items: center;
      gap: 4px;
      color: rgba(0, 0, 0, 0.6);
    }

    .time-remaining {
      text-align: center;
    }

    .time-remaining .label {
      display: block;
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.6);
      margin-bottom: 4px;
    }

    mat-card-actions {
      display: flex;
      justify-content: space-between;
      padding: 8px 16px 16px;
    }

    .no-auctions {
      grid-column: 1 / -1;
      text-align: center;
      padding: 64px;
      color: rgba(0, 0, 0, 0.6);
    }

    .no-auctions mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
    }
  `]
})
export class AuctionListComponent implements OnInit {
  readonly state = inject(AuctionState);

  searchTerm = '';
  statusFilter = '';
  typeFilter = '';

  ngOnInit(): void {
    this.loadAuctions();
  }

  loadAuctions(): void {
    this.state.loadAuctions({
      pageNumber: 1,
      pageSize: 12,
      status: this.statusFilter || undefined,
      type: this.typeFilter || undefined,
      searchTerm: this.searchTerm || undefined
    });
  }

  search(): void {
    this.loadAuctions();
  }

  onPageChange(event: PageEvent): void {
    this.state.loadAuctions({
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
      status: this.statusFilter || undefined,
      type: this.typeFilter || undefined,
      searchTerm: this.searchTerm || undefined
    });
  }
}
