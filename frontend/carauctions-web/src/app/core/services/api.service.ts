import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  Auction,
  Bid,
  PaginatedResult,
  Vehicle,
  User,
  WatchlistItem,
  Notification,
  UserBidSummary,
  SellerStats,
  SearchFilters
} from '../../shared/models/auction.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // ==================== AUCTIONS ====================

  getAuctions(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    type?: string;
    searchTerm?: string;
    make?: string;
    model?: string;
    yearFrom?: number;
    yearTo?: number;
    priceFrom?: number;
    priceTo?: number;
    dealerOnly?: boolean;
  }): Observable<PaginatedResult<Auction>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }
    return this.http.get<PaginatedResult<Auction>>(`${this.baseUrl}/auctions`, { params: httpParams });
  }

  getAuction(id: string): Observable<Auction> {
    return this.http.get<Auction>(`${this.baseUrl}/auctions/${id}`);
  }

  getActiveAuctions(): Observable<Auction[]> {
    return this.http.get<Auction[]>(`${this.baseUrl}/auctions/active`);
  }

  createAuction(auction: Partial<Auction>): Observable<Auction> {
    return this.http.post<Auction>(`${this.baseUrl}/auctions`, auction);
  }

  scheduleAuction(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auctions/${id}/schedule`, {});
  }

  startAuction(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auctions/${id}/start`, {});
  }

  cancelAuction(id: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auctions/${id}/cancel`, { reason });
  }

  // Buy Now
  buyNow(auctionId: string): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(`${this.baseUrl}/auctions/${auctionId}/buy-now`, {});
  }

  // ==================== BIDS ====================

  placeBid(
    auctionId: string,
    amount: number,
    currency: string = 'USD',
    isProxy: boolean = false,
    maxProxyAmount?: number
  ): Observable<{ bidId: string }> {
    return this.http.post<{ bidId: string }>(`${this.baseUrl}/bids`, {
      auctionId,
      amount,
      currency,
      isProxy,
      maxProxyAmount
    });
  }

  getBidHistory(auctionId: string): Observable<Bid[]> {
    return this.http.get<Bid[]>(`${this.baseUrl}/bids/auction/${auctionId}`);
  }

  getMyBids(): Observable<Bid[]> {
    return this.http.get<Bid[]>(`${this.baseUrl}/bids/my-bids`);
  }

  getUserBidSummaries(): Observable<UserBidSummary[]> {
    // This would be a new endpoint - for now return mock data based on my bids
    return this.getMyBids().pipe(
      map(bids => bids.map(bid => ({
        auctionId: bid.auctionId,
        auctionTitle: 'Auction',
        vehicleInfo: '',
        myHighestBid: bid.amount,
        currentHighBid: bid.amount,
        status: bid.status === 'Winning' ? 'winning' as const : 'outbid' as const,
        endTime: new Date(),
        auctionStatus: 'Active' as any
      })))
    );
  }

  // ==================== VEHICLES ====================

  getVehicle(id: string): Observable<Vehicle> {
    return this.http.get<Vehicle>(`${this.baseUrl}/vehicles/${id}`);
  }

  getVehicles(params?: {
    pageNumber?: number;
    pageSize?: number;
    make?: string;
    model?: string;
    yearFrom?: number;
    yearTo?: number;
  }): Observable<PaginatedResult<Vehicle>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }
    return this.http.get<PaginatedResult<Vehicle>>(`${this.baseUrl}/vehicles`, { params: httpParams });
  }

  searchVehicles(params?: {
    make?: string;
    model?: string;
    yearFrom?: number;
    yearTo?: number;
  }): Observable<PaginatedResult<Vehicle>> {
    return this.getVehicles(params);
  }

  createVehicle(vehicle: Partial<Vehicle>): Observable<Vehicle> {
    return this.http.post<Vehicle>(`${this.baseUrl}/vehicles`, vehicle);
  }

  // ==================== USERS ====================

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/auth/me`);
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/users/${id}`);
  }

  updateUser(id: string, data: Partial<User>): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/users/${id}`, data);
  }

  getUserBidHistory(): Observable<Bid[]> {
    return this.http.get<Bid[]>(`${this.baseUrl}/users/me/bids`);
  }

  submitKyc(documents: FormData): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/me/kyc`, documents);
  }

  // ==================== WATCHLIST ====================

  getWatchlist(): Observable<WatchlistItem[]> {
    return this.http.get<WatchlistItem[]>(`${this.baseUrl}/watchlist`).pipe(
      catchError(() => of([]))
    );
  }

  addToWatchlist(auctionId: string): Observable<WatchlistItem> {
    return this.http.post<WatchlistItem>(`${this.baseUrl}/watchlist`, { auctionId });
  }

  removeFromWatchlist(auctionId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/watchlist/${auctionId}`);
  }

  isWatching(auctionId: string): Observable<boolean> {
    return this.getWatchlist().pipe(
      map(items => items.some(item => item.auctionId === auctionId)),
      catchError(() => of(false))
    );
  }

  // ==================== NOTIFICATIONS ====================

  getNotifications(): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.baseUrl}/notifications`).pipe(
      catchError(() => of([]))
    );
  }

  getUnreadNotificationCount(): Observable<number> {
    return this.getNotifications().pipe(
      map(notifications => notifications.filter(n => !n.isRead).length),
      catchError(() => of(0))
    );
  }

  markNotificationAsRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/notifications/${id}/read`, {});
  }

  markAllNotificationsAsRead(): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/notifications/read-all`, {});
  }

  // ==================== SELLER ====================

  getSellerStats(): Observable<SellerStats> {
    return this.http.get<SellerStats>(`${this.baseUrl}/seller/stats`).pipe(
      catchError(() => of({
        totalListings: 0,
        activeAuctions: 0,
        completedAuctions: 0,
        totalSales: { amount: 0, currency: 'USD' },
        averagePrice: { amount: 0, currency: 'USD' }
      }))
    );
  }

  getSellerAuctions(): Observable<Auction[]> {
    return this.http.get<Auction[]>(`${this.baseUrl}/seller/auctions`).pipe(
      catchError(() => of([]))
    );
  }

  // ==================== ADMIN ====================

  getAdminUsers(params?: { pageNumber?: number; pageSize?: number }): Observable<PaginatedResult<User>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined) {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }
    return this.http.get<PaginatedResult<User>>(`${this.baseUrl}/admin/users`, { params: httpParams });
  }

  getAdminAuctions(params?: { pageNumber?: number; pageSize?: number; status?: string }): Observable<PaginatedResult<Auction>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined) {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }
    return this.http.get<PaginatedResult<Auction>>(`${this.baseUrl}/admin/auctions`, { params: httpParams });
  }

  suspendUser(userId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/users/${userId}/suspend`, { reason });
  }

  activateUser(userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/users/${userId}/activate`, {});
  }

  verifyUserKyc(userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/admin/users/${userId}/verify-kyc`, {});
  }

  // ==================== SEARCH ====================

  getAvailableMakes(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/vehicles/makes`).pipe(
      catchError(() => of([
        'Audi', 'BMW', 'Chevrolet', 'Dodge', 'Ford', 'GMC', 'Honda', 'Hyundai',
        'Jeep', 'Kia', 'Lexus', 'Mazda', 'Mercedes-Benz', 'Nissan', 'Porsche',
        'Range Rover', 'Subaru', 'Tesla', 'Toyota', 'Volkswagen'
      ]))
    );
  }

  getModelsForMake(make: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/vehicles/makes/${make}/models`).pipe(
      catchError(() => of([]))
    );
  }
}
