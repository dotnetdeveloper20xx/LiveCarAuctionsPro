import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ApiService } from '../../core/services/api.service';
import { User, Auction, AuctionStatus, PaginatedResult } from '../../shared/models/auction.model';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatTabsModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  template: `
    <div class="admin-container">
      <header class="admin-header">
        <h1>
          <mat-icon>admin_panel_settings</mat-icon>
          Admin Panel
        </h1>
        <p>Manage users, auctions, and platform settings</p>
      </header>

      <!-- Stats Overview -->
      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-icon>people</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ userStats().total }}</span>
            <span class="stat-label">Total Users</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon>gavel</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ auctionStats().active }}</span>
            <span class="stat-label">Active Auctions</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon>verified_user</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ userStats().verified }}</span>
            <span class="stat-label">Verified Users</span>
          </div>
        </mat-card>

        <mat-card class="stat-card">
          <mat-icon>store</mat-icon>
          <div class="stat-content">
            <span class="stat-value">{{ userStats().dealers }}</span>
            <span class="stat-label">Dealers</span>
          </div>
        </mat-card>
      </div>

      <!-- Main Content Tabs -->
      <mat-card class="main-card">
        <mat-tab-group>
          <!-- Users Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>people</mat-icon>
              <span>Users</span>
            </ng-template>
            <div class="tab-content">
              <div class="toolbar">
                <mat-form-field appearance="outline" class="search-field">
                  <mat-label>Search users</mat-label>
                  <input matInput [(ngModel)]="userSearch" placeholder="Email or name...">
                  <mat-icon matSuffix>search</mat-icon>
                </mat-form-field>
              </div>

              @if (usersLoading()) {
                <div class="loading">
                  <mat-spinner diameter="40"></mat-spinner>
                </div>
              } @else {
                <div class="table-container">
                  <table mat-table [dataSource]="filteredUsers()">
                    <ng-container matColumnDef="user">
                      <th mat-header-cell *matHeaderCellDef>User</th>
                      <td mat-cell *matCellDef="let user">
                        <div class="user-cell">
                          <mat-icon class="avatar">account_circle</mat-icon>
                          <div>
                            <strong>{{ user.firstName }} {{ user.lastName }}</strong>
                            <span>{{ user.email }}</span>
                          </div>
                        </div>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="roles">
                      <th mat-header-cell *matHeaderCellDef>Roles</th>
                      <td mat-cell *matCellDef="let user">
                        <mat-chip-set>
                          @for (role of user.roles; track role) {
                            <mat-chip [class]="'role-' + role.toLowerCase()">{{ role }}</mat-chip>
                          }
                        </mat-chip-set>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="status">
                      <th mat-header-cell *matHeaderCellDef>Status</th>
                      <td mat-cell *matCellDef="let user">
                        <div class="status-cell">
                          @if (user.kycVerified) {
                            <mat-chip class="verified">
                              <mat-icon>verified</mat-icon> KYC Verified
                            </mat-chip>
                          } @else {
                            <mat-chip class="pending">Pending KYC</mat-chip>
                          }
                          @if (user.status === 'Suspended') {
                            <mat-chip class="suspended">Suspended</mat-chip>
                          }
                        </div>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="actions">
                      <th mat-header-cell *matHeaderCellDef>Actions</th>
                      <td mat-cell *matCellDef="let user">
                        <button mat-icon-button [matMenuTriggerFor]="userMenu">
                          <mat-icon>more_vert</mat-icon>
                        </button>
                        <mat-menu #userMenu="matMenu">
                          <button mat-menu-item (click)="viewUser(user)">
                            <mat-icon>visibility</mat-icon>
                            View Details
                          </button>
                          @if (!user.kycVerified) {
                            <button mat-menu-item (click)="verifyKyc(user)">
                              <mat-icon>verified_user</mat-icon>
                              Verify KYC
                            </button>
                          }
                          @if (user.status !== 'Suspended') {
                            <button mat-menu-item class="danger" (click)="suspendUser(user)">
                              <mat-icon>block</mat-icon>
                              Suspend User
                            </button>
                          } @else {
                            <button mat-menu-item (click)="activateUser(user)">
                              <mat-icon>check_circle</mat-icon>
                              Activate User
                            </button>
                          }
                        </mat-menu>
                      </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="userColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: userColumns;"></tr>
                  </table>
                </div>

                <mat-paginator
                  [length]="usersTotal()"
                  [pageSize]="10"
                  [pageSizeOptions]="[10, 25, 50]"
                  (page)="onUsersPageChange($event)">
                </mat-paginator>
              }
            </div>
          </mat-tab>

          <!-- Auctions Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>gavel</mat-icon>
              <span>Auctions</span>
            </ng-template>
            <div class="tab-content">
              <div class="toolbar">
                <div class="filter-buttons">
                  <button mat-button [class.active]="auctionFilter() === 'all'" (click)="setAuctionFilter('all')">All</button>
                  <button mat-button [class.active]="auctionFilter() === 'active'" (click)="setAuctionFilter('active')">Active</button>
                  <button mat-button [class.active]="auctionFilter() === 'scheduled'" (click)="setAuctionFilter('scheduled')">Scheduled</button>
                  <button mat-button [class.active]="auctionFilter() === 'completed'" (click)="setAuctionFilter('completed')">Completed</button>
                </div>
              </div>

              @if (auctionsLoading()) {
                <div class="loading">
                  <mat-spinner diameter="40"></mat-spinner>
                </div>
              } @else {
                <div class="table-container">
                  <table mat-table [dataSource]="auctions()">
                    <ng-container matColumnDef="auction">
                      <th mat-header-cell *matHeaderCellDef>Auction</th>
                      <td mat-cell *matCellDef="let auction">
                        <div class="auction-cell">
                          @if (auction.vehicle?.imageUrl) {
                            <img [src]="auction.vehicle.imageUrl" [alt]="auction.title">
                          } @else {
                            <div class="no-image">
                              <mat-icon>directions_car</mat-icon>
                            </div>
                          }
                          <div>
                            <strong>{{ auction.title }}</strong>
                            <span>{{ auction.vehicle?.year }} {{ auction.vehicle?.make }} {{ auction.vehicle?.model }}</span>
                          </div>
                        </div>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="seller">
                      <th mat-header-cell *matHeaderCellDef>Seller</th>
                      <td mat-cell *matCellDef="let auction">{{ auction.sellerName || auction.sellerId?.substring(0, 8) }}</td>
                    </ng-container>

                    <ng-container matColumnDef="currentBid">
                      <th mat-header-cell *matHeaderCellDef>Current Bid</th>
                      <td mat-cell *matCellDef="let auction">
                        <strong>{{ auction.currentHighBid.amount | currency }}</strong>
                        <span class="bid-count">({{ auction.bidCount }} bids)</span>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="status">
                      <th mat-header-cell *matHeaderCellDef>Status</th>
                      <td mat-cell *matCellDef="let auction">
                        <mat-chip [class]="'status-' + auction.status.toLowerCase()">
                          {{ auction.status }}
                        </mat-chip>
                      </td>
                    </ng-container>

                    <ng-container matColumnDef="actions">
                      <th mat-header-cell *matHeaderCellDef>Actions</th>
                      <td mat-cell *matCellDef="let auction">
                        <button mat-icon-button [matMenuTriggerFor]="auctionMenu">
                          <mat-icon>more_vert</mat-icon>
                        </button>
                        <mat-menu #auctionMenu="matMenu">
                          <a mat-menu-item [routerLink]="['/auctions', auction.id]">
                            <mat-icon>visibility</mat-icon>
                            View Auction
                          </a>
                          @if (auction.status === 'Active') {
                            <button mat-menu-item class="danger" (click)="cancelAuction(auction)">
                              <mat-icon>cancel</mat-icon>
                              Cancel Auction
                            </button>
                          }
                        </mat-menu>
                      </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="auctionColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: auctionColumns;"></tr>
                  </table>
                </div>

                <mat-paginator
                  [length]="auctionsTotal()"
                  [pageSize]="10"
                  [pageSizeOptions]="[10, 25, 50]"
                  (page)="onAuctionsPageChange($event)">
                </mat-paginator>
              }
            </div>
          </mat-tab>

          <!-- Settings Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>settings</mat-icon>
              <span>Settings</span>
            </ng-template>
            <div class="tab-content settings-content">
              <h3>Platform Settings</h3>
              <mat-card class="settings-card">
                <h4>Auction Settings</h4>
                <div class="settings-row">
                  <span>Default Anti-Sniping Extension</span>
                  <span>5 minutes</span>
                </div>
                <div class="settings-row">
                  <span>Minimum Bid Increment</span>
                  <span>$100</span>
                </div>
                <div class="settings-row">
                  <span>Buyer's Premium</span>
                  <span>5%</span>
                </div>
              </mat-card>

              <mat-card class="settings-card">
                <h4>User Settings</h4>
                <div class="settings-row">
                  <span>Default Credit Limit</span>
                  <span>$50,000</span>
                </div>
                <div class="settings-row">
                  <span>Dealer Credit Limit</span>
                  <span>$500,000</span>
                </div>
                <div class="settings-row">
                  <span>KYC Required for Bidding</span>
                  <span>Yes</span>
                </div>
              </mat-card>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card>
    </div>
  `,
  styles: [`
    .admin-container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 24px;
    }

    .admin-header {
      margin-bottom: 32px;
    }

    .admin-header h1 {
      margin: 0;
      font-size: 2rem;
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .admin-header h1 mat-icon {
      font-size: 2rem;
      width: 2rem;
      height: 2rem;
      color: #1976d2;
    }

    .admin-header p {
      margin: 8px 0 0;
      color: rgba(0, 0, 0, 0.6);
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

    .stat-card mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #1976d2;
    }

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

    .main-card {
      overflow: hidden;
    }

    .tab-content {
      padding: 24px;
    }

    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }

    .search-field {
      width: 300px;
    }

    .filter-buttons {
      display: flex;
      gap: 8px;
    }

    .filter-buttons button.active {
      background: rgba(25, 118, 210, 0.1);
      color: #1976d2;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 48px;
    }

    .table-container {
      overflow-x: auto;
    }

    table {
      width: 100%;
    }

    .user-cell {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .user-cell .avatar {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: rgba(0, 0, 0, 0.5);
    }

    .user-cell div {
      display: flex;
      flex-direction: column;
    }

    .user-cell span {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }

    .status-cell {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .verified { background: #e8f5e9 !important; color: #2e7d32 !important; }
    .pending { background: #fff3e0 !important; color: #e65100 !important; }
    .suspended { background: #ffebee !important; color: #c62828 !important; }

    .role-admin { background: #e3f2fd !important; color: #1565c0 !important; }
    .role-seller { background: #f3e5f5 !important; color: #7b1fa2 !important; }
    .role-dealer { background: #ede7f6 !important; color: #512da8 !important; }
    .role-buyer { background: #e8f5e9 !important; color: #2e7d32 !important; }

    .auction-cell {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .auction-cell img {
      width: 60px;
      height: 45px;
      object-fit: cover;
      border-radius: 4px;
    }

    .auction-cell .no-image {
      width: 60px;
      height: 45px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f5f5f5;
      border-radius: 4px;
    }

    .auction-cell .no-image mat-icon {
      color: rgba(0, 0, 0, 0.3);
    }

    .auction-cell div {
      display: flex;
      flex-direction: column;
    }

    .auction-cell span {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }

    .bid-count {
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.5);
      margin-left: 4px;
    }

    .status-draft { background: #9e9e9e !important; color: white !important; }
    .status-scheduled { background: #2196f3 !important; color: white !important; }
    .status-active { background: #4caf50 !important; color: white !important; }
    .status-closed, .status-completed { background: #ff9800 !important; color: white !important; }
    .status-cancelled { background: #f44336 !important; color: white !important; }

    .danger {
      color: #f44336;
    }

    .settings-content h3 {
      margin: 0 0 24px;
    }

    .settings-card {
      padding: 24px;
      margin-bottom: 16px;
    }

    .settings-card h4 {
      margin: 0 0 16px;
      color: #1976d2;
    }

    .settings-row {
      display: flex;
      justify-content: space-between;
      padding: 12px 0;
      border-bottom: 1px solid rgba(0, 0, 0, 0.1);
    }

    .settings-row:last-child {
      border-bottom: none;
    }

    mat-tab-group ::ng-deep .mat-mdc-tab .mdc-tab__content {
      display: flex;
      align-items: center;
      gap: 8px;
    }
  `]
})
export class AdminPanelComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly users = signal<User[]>([]);
  readonly auctions = signal<Auction[]>([]);
  readonly usersLoading = signal(false);
  readonly auctionsLoading = signal(false);
  readonly usersTotal = signal(0);
  readonly auctionsTotal = signal(0);
  readonly auctionFilter = signal<'all' | 'active' | 'scheduled' | 'completed'>('all');

  userSearch = '';
  userColumns = ['user', 'roles', 'status', 'actions'];
  auctionColumns = ['auction', 'seller', 'currentBid', 'status', 'actions'];

  readonly userStats = computed(() => {
    const users = this.users();
    return {
      total: this.usersTotal(),
      verified: users.filter(u => u.kycVerified).length,
      dealers: users.filter(u => u.isDealer).length
    };
  });

  readonly auctionStats = computed(() => {
    return {
      active: this.auctions().filter(a => a.status === AuctionStatus.Active).length
    };
  });

  readonly filteredUsers = computed(() => {
    const search = this.userSearch.toLowerCase();
    if (!search) return this.users();
    return this.users().filter(u =>
      u.email.toLowerCase().includes(search) ||
      u.firstName.toLowerCase().includes(search) ||
      u.lastName.toLowerCase().includes(search)
    );
  });

  ngOnInit(): void {
    this.loadUsers();
    this.loadAuctions();
  }

  private loadUsers(page = 1): void {
    this.usersLoading.set(true);
    this.api.getAdminUsers({ pageNumber: page, pageSize: 10 }).subscribe({
      next: (result) => {
        this.users.set(result.items);
        this.usersTotal.set(result.totalCount);
        this.usersLoading.set(false);
      },
      error: () => this.usersLoading.set(false)
    });
  }

  private loadAuctions(page = 1): void {
    this.auctionsLoading.set(true);
    const filter = this.auctionFilter();
    const status = filter === 'all' ? undefined : filter.charAt(0).toUpperCase() + filter.slice(1);

    this.api.getAdminAuctions({ pageNumber: page, pageSize: 10, status }).subscribe({
      next: (result) => {
        this.auctions.set(result.items);
        this.auctionsTotal.set(result.totalCount);
        this.auctionsLoading.set(false);
      },
      error: () => this.auctionsLoading.set(false)
    });
  }

  setAuctionFilter(filter: 'all' | 'active' | 'scheduled' | 'completed'): void {
    this.auctionFilter.set(filter);
    this.loadAuctions();
  }

  onUsersPageChange(event: PageEvent): void {
    this.loadUsers(event.pageIndex + 1);
  }

  onAuctionsPageChange(event: PageEvent): void {
    this.loadAuctions(event.pageIndex + 1);
  }

  viewUser(user: User): void {
    // Could open a dialog with user details
    console.log('View user:', user);
  }

  verifyKyc(user: User): void {
    this.api.verifyUserKyc(user.id).subscribe({
      next: () => {
        this.snackBar.open('User KYC verified successfully', 'Close', { duration: 3000 });
        this.loadUsers();
      },
      error: () => this.snackBar.open('Failed to verify KYC', 'Close', { duration: 3000 })
    });
  }

  suspendUser(user: User): void {
    const reason = prompt('Enter suspension reason:');
    if (reason) {
      this.api.suspendUser(user.id, reason).subscribe({
        next: () => {
          this.snackBar.open('User suspended', 'Close', { duration: 3000 });
          this.loadUsers();
        },
        error: () => this.snackBar.open('Failed to suspend user', 'Close', { duration: 3000 })
      });
    }
  }

  activateUser(user: User): void {
    this.api.activateUser(user.id).subscribe({
      next: () => {
        this.snackBar.open('User activated', 'Close', { duration: 3000 });
        this.loadUsers();
      },
      error: () => this.snackBar.open('Failed to activate user', 'Close', { duration: 3000 })
    });
  }

  cancelAuction(auction: Auction): void {
    if (confirm('Are you sure you want to cancel this auction?')) {
      this.api.cancelAuction(auction.id, 'Cancelled by admin').subscribe({
        next: () => {
          this.snackBar.open('Auction cancelled', 'Close', { duration: 3000 });
          this.loadAuctions();
        },
        error: () => this.snackBar.open('Failed to cancel auction', 'Close', { duration: 3000 })
      });
    }
  }
}
