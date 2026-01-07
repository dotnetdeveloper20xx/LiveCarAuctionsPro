import { Component, OnInit, inject, signal } from '@angular/core';
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
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSliderModule } from '@angular/material/slider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { AuctionState } from '../../../core/state/auction.state';
import { WatchlistState } from '../../../core/state/watchlist.state';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';
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
    MatExpansionModule,
    MatSliderModule,
    MatCheckboxModule,
    MatDividerModule,
    CountdownTimerComponent
  ],
  template: `
    <div class="auction-list-container">
      <header class="page-header">
        <h1>Live Auctions</h1>
        <p>Find your next vehicle from our selection of auctions</p>
      </header>

      <!-- Quick Filters -->
      <div class="quick-filters">
        <mat-form-field appearance="outline" class="search-field">
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

        <button mat-stroked-button (click)="showAdvanced = !showAdvanced">
          <mat-icon>{{ showAdvanced ? 'expand_less' : 'tune' }}</mat-icon>
          {{ showAdvanced ? 'Hide Filters' : 'More Filters' }}
        </button>
      </div>

      <!-- Advanced Filters -->
      @if (showAdvanced) {
        <mat-card class="advanced-filters">
          <h3>Advanced Filters</h3>
          <div class="filter-grid">
            <mat-form-field appearance="outline">
              <mat-label>Make</mat-label>
              <mat-select [(ngModel)]="makeFilter" (selectionChange)="onMakeChange()">
                <mat-option value="">Any Make</mat-option>
                @for (make of availableMakes(); track make) {
                  <mat-option [value]="make">{{ make }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Model</mat-label>
              <mat-select [(ngModel)]="modelFilter" [disabled]="!makeFilter">
                <mat-option value="">Any Model</mat-option>
                @for (model of availableModels(); track model) {
                  <mat-option [value]="model">{{ model }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Year From</mat-label>
              <mat-select [(ngModel)]="yearFrom">
                <mat-option value="">Any</mat-option>
                @for (year of yearOptions; track year) {
                  <mat-option [value]="year">{{ year }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Year To</mat-label>
              <mat-select [(ngModel)]="yearTo">
                <mat-option value="">Any</mat-option>
                @for (year of yearOptions; track year) {
                  <mat-option [value]="year">{{ year }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Min Price</mat-label>
              <input matInput type="number" [(ngModel)]="priceFrom" placeholder="0">
              <span matTextPrefix>$&nbsp;</span>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Max Price</mat-label>
              <input matInput type="number" [(ngModel)]="priceTo" placeholder="No limit">
              <span matTextPrefix>$&nbsp;</span>
            </mat-form-field>
          </div>

          <div class="filter-options">
            <mat-checkbox [(ngModel)]="dealerOnly" color="primary">
              Dealer-Only Auctions
            </mat-checkbox>
          </div>

          <div class="filter-actions">
            <button mat-button (click)="clearFilters()">Clear All</button>
            <button mat-raised-button color="primary" (click)="search()">
              <mat-icon>search</mat-icon>
              Apply Filters
            </button>
          </div>
        </mat-card>
      }

      <!-- Active Filters Summary -->
      @if (hasActiveFilters()) {
        <div class="active-filters">
          <span class="label">Active Filters:</span>
          @if (makeFilter) {
            <mat-chip (removed)="makeFilter = ''; modelFilter = ''; search()">
              {{ makeFilter }}
              <mat-icon matChipRemove>cancel</mat-icon>
            </mat-chip>
          }
          @if (modelFilter) {
            <mat-chip (removed)="modelFilter = ''; search()">
              {{ modelFilter }}
              <mat-icon matChipRemove>cancel</mat-icon>
            </mat-chip>
          }
          @if (yearFrom || yearTo) {
            <mat-chip (removed)="yearFrom = ''; yearTo = ''; search()">
              {{ yearFrom || 'Any' }} - {{ yearTo || 'Any' }}
              <mat-icon matChipRemove>cancel</mat-icon>
            </mat-chip>
          }
          @if (priceFrom || priceTo) {
            <mat-chip (removed)="priceFrom = 0; priceTo = 0; search()">
              {{ (priceFrom || 0) | currency }} - {{ priceTo ? (priceTo | currency) : '∞' }}
              <mat-icon matChipRemove>cancel</mat-icon>
            </mat-chip>
          }
          @if (dealerOnly) {
            <mat-chip (removed)="dealerOnly = false; search()">
              Dealer Only
              <mat-icon matChipRemove>cancel</mat-icon>
            </mat-chip>
          }
          <button mat-button color="warn" (click)="clearFilters()">Clear All</button>
        </div>
      }

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
        <div class="results-header">
          <span>{{ state.pagination().totalCount }} auctions found</span>
          <mat-form-field appearance="outline" class="sort-field">
            <mat-label>Sort By</mat-label>
            <mat-select [(ngModel)]="sortBy" (selectionChange)="search()">
              <mat-option value="">Default</mat-option>
              <mat-option value="ending-soon">Ending Soon</mat-option>
              <mat-option value="price-low">Price: Low to High</mat-option>
              <mat-option value="price-high">Price: High to Low</mat-option>
              <mat-option value="most-bids">Most Bids</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="auctions-grid">
          @for (auction of state.auctions(); track auction.id) {
            <mat-card class="auction-card">
              <div class="auction-image" [routerLink]="['/auctions', auction.id]">
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
                    <mat-chip class="dealer-chip">Dealers Only</mat-chip>
                  }
                </mat-chip-set>

                <!-- Watchlist Button -->
                @if (auth.isAuthenticated()) {
                  <button mat-icon-button
                          class="watchlist-btn"
                          [class.watching]="isWatching(auction.id)"
                          (click)="toggleWatchlist($event, auction.id)">
                    <mat-icon>{{ isWatching(auction.id) ? 'favorite' : 'favorite_border' }}</mat-icon>
                  </button>
                }

                <!-- Buy Now Badge -->
                @if (auction.buyNowPrice) {
                  <div class="buy-now-badge">
                    <mat-icon>shopping_cart</mat-icon>
                    Buy Now: {{ auction.buyNowPrice.amount | currency }}
                  </div>
                }
              </div>

              <mat-card-content [routerLink]="['/auctions', auction.id]">
                <h3 class="auction-title">{{ auction.title }}</h3>

                @if (auction.vehicle) {
                  <p class="vehicle-info">
                    {{ auction.vehicle.year }} {{ auction.vehicle.make }} {{ auction.vehicle.model }}
                  </p>
                  <p class="vehicle-details">
                    <span>{{ auction.vehicle.mileage | number }} mi</span>
                    @if (auction.vehicle.transmission) {
                      <span>{{ auction.vehicle.transmission }}</span>
                    }
                    @if (auction.vehicle.titleStatus) {
                      <span [class]="'title-' + auction.vehicle.titleStatus.toLowerCase()">
                        {{ auction.vehicle.titleStatus }}
                      </span>
                    }
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
                } @else if (auction.status === 'Scheduled') {
                  <div class="time-remaining scheduled">
                    <span class="label">Starts</span>
                    <span class="start-date">{{ auction.startTime | date:'medium' }}</span>
                  </div>
                }

                @if (auction.reservePrice) {
                  <div class="reserve-indicator" [class.met]="auction.currentHighBid.amount >= auction.reservePrice.amount">
                    <mat-icon>{{ auction.currentHighBid.amount >= auction.reservePrice.amount ? 'check_circle' : 'info' }}</mat-icon>
                    {{ auction.currentHighBid.amount >= auction.reservePrice.amount ? 'Reserve Met' : 'Reserve Not Met' }}
                  </div>
                }
              </mat-card-content>

              <mat-card-actions>
                <button mat-button color="primary" [routerLink]="['/auctions', auction.id]">
                  <mat-icon>visibility</mat-icon>
                  View Details
                </button>
                @if (auction.status === 'Active') {
                  <button mat-raised-button color="primary" [routerLink]="['/auctions', auction.id]">
                    <mat-icon>gavel</mat-icon>
                    Place Bid
                  </button>
                }
              </mat-card-actions>
            </mat-card>
          } @empty {
            <div class="no-auctions">
              <mat-icon>search_off</mat-icon>
              <h3>No auctions found</h3>
              <p>Try adjusting your filters or search terms</p>
              <button mat-raised-button color="primary" (click)="clearFilters()">
                Clear Filters
              </button>
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

    .quick-filters {
      display: flex;
      gap: 16px;
      margin-bottom: 24px;
      flex-wrap: wrap;
      align-items: center;
    }

    .search-field {
      flex: 2;
      min-width: 250px;
    }

    .quick-filters mat-form-field:not(.search-field) {
      flex: 1;
      min-width: 150px;
    }

    .advanced-filters {
      padding: 24px;
      margin-bottom: 24px;
    }

    .advanced-filters h3 {
      margin: 0 0 16px;
    }

    .filter-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
      gap: 16px;
      margin-bottom: 16px;
    }

    .filter-options {
      margin-bottom: 16px;
    }

    .filter-actions {
      display: flex;
      justify-content: flex-end;
      gap: 16px;
    }

    .active-filters {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }

    .active-filters .label {
      font-weight: 500;
      color: rgba(0, 0, 0, 0.6);
    }

    .results-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    .sort-field {
      width: 200px;
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
      display: flex;
      flex-direction: column;
    }

    .auction-image {
      position: relative;
      height: 200px;
      background: #f5f5f5;
      overflow: hidden;
      cursor: pointer;
    }

    .auction-image img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform 0.3s;
    }

    .auction-card:hover .auction-image img {
      transform: scale(1.05);
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
    .dealer-chip { background: #7c4dff !important; color: white !important; }

    .watchlist-btn {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(255, 255, 255, 0.9);
    }

    .watchlist-btn.watching {
      color: #f44336;
    }

    .buy-now-badge {
      position: absolute;
      bottom: 8px;
      right: 8px;
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 4px 8px;
      background: #ff6f00;
      color: white;
      border-radius: 4px;
      font-size: 0.75rem;
      font-weight: 500;
    }

    .buy-now-badge mat-icon {
      font-size: 14px;
      width: 14px;
      height: 14px;
    }

    mat-card-content {
      flex: 1;
      cursor: pointer;
    }

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

    .title-clean { color: #4caf50; }
    .title-rebuilt { color: #ff9800; }
    .title-salvage { color: #f44336; }

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
      margin-bottom: 12px;
    }

    .time-remaining .label {
      display: block;
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.6);
      margin-bottom: 4px;
    }

    .time-remaining.scheduled .start-date {
      font-size: 0.875rem;
      color: #2196f3;
    }

    .reserve-indicator {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
      padding: 8px;
      background: #ffebee;
      color: #c62828;
      border-radius: 4px;
      font-size: 0.875rem;
    }

    .reserve-indicator.met {
      background: #e8f5e9;
      color: #2e7d32;
    }

    .reserve-indicator mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
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

    .no-auctions h3 {
      margin: 16px 0 8px;
      color: rgba(0, 0, 0, 0.87);
    }

    @media (max-width: 768px) {
      .quick-filters {
        flex-direction: column;
      }

      .quick-filters > * {
        width: 100%;
      }

      .results-header {
        flex-direction: column;
        gap: 16px;
        align-items: flex-start;
      }

      .sort-field {
        width: 100%;
      }
    }
  `]
})
export class AuctionListComponent implements OnInit {
  readonly state = inject(AuctionState);
  readonly watchlistState = inject(WatchlistState);
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);

  // Quick filters
  searchTerm = '';
  statusFilter = '';
  typeFilter = '';
  sortBy = '';

  // Advanced filters
  showAdvanced = false;
  makeFilter = '';
  modelFilter = '';
  yearFrom: string | number = '';
  yearTo: string | number = '';
  priceFrom = 0;
  priceTo = 0;
  dealerOnly = false;

  // Options
  readonly availableMakes = signal<string[]>([]);
  readonly availableModels = signal<string[]>([]);
  readonly yearOptions: number[] = [];

  constructor() {
    const currentYear = new Date().getFullYear();
    for (let year = currentYear + 1; year >= 1990; year--) {
      this.yearOptions.push(year);
    }
  }

  ngOnInit(): void {
    this.loadAuctions();
    this.loadMakes();

    if (this.auth.isAuthenticated()) {
      this.watchlistState.loadWatchlist();
    }
  }

  private loadMakes(): void {
    this.api.getAvailableMakes().subscribe({
      next: (makes) => this.availableMakes.set(makes)
    });
  }

  onMakeChange(): void {
    this.modelFilter = '';
    if (this.makeFilter) {
      this.api.getModelsForMake(this.makeFilter).subscribe({
        next: (models) => this.availableModels.set(models)
      });
    } else {
      this.availableModels.set([]);
    }
  }

  loadAuctions(): void {
    this.state.loadAuctions({
      pageNumber: 1,
      pageSize: 12,
      status: this.statusFilter || undefined,
      type: this.typeFilter || undefined,
      searchTerm: this.searchTerm || undefined,
      make: this.makeFilter || undefined,
      model: this.modelFilter || undefined,
      yearFrom: this.yearFrom ? Number(this.yearFrom) : undefined,
      yearTo: this.yearTo ? Number(this.yearTo) : undefined,
      priceFrom: this.priceFrom || undefined,
      priceTo: this.priceTo || undefined,
      dealerOnly: this.dealerOnly || undefined
    });
  }

  search(): void {
    this.loadAuctions();
  }

  hasActiveFilters(): boolean {
    return !!(this.makeFilter || this.modelFilter || this.yearFrom || this.yearTo ||
              this.priceFrom || this.priceTo || this.dealerOnly);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.typeFilter = '';
    this.makeFilter = '';
    this.modelFilter = '';
    this.yearFrom = '';
    this.yearTo = '';
    this.priceFrom = 0;
    this.priceTo = 0;
    this.dealerOnly = false;
    this.sortBy = '';
    this.search();
  }

  onPageChange(event: PageEvent): void {
    this.state.loadAuctions({
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
      status: this.statusFilter || undefined,
      type: this.typeFilter || undefined,
      searchTerm: this.searchTerm || undefined,
      make: this.makeFilter || undefined,
      model: this.modelFilter || undefined,
      yearFrom: this.yearFrom ? Number(this.yearFrom) : undefined,
      yearTo: this.yearTo ? Number(this.yearTo) : undefined,
      priceFrom: this.priceFrom || undefined,
      priceTo: this.priceTo || undefined,
      dealerOnly: this.dealerOnly || undefined
    });
  }

  isWatching(auctionId: string): boolean {
    return this.watchlistState.isWatching(auctionId);
  }

  toggleWatchlist(event: Event, auctionId: string): void {
    event.stopPropagation();
    event.preventDefault();
    this.watchlistState.toggleWatch(auctionId);
  }
}
