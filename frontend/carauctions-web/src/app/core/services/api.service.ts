import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Auction,
  Bid,
  PaginatedResult,
  Vehicle,
  User
} from '../../shared/models/auction.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // Auctions
  getAuctions(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    type?: string;
    searchTerm?: string;
  }): Observable<PaginatedResult<Auction>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
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

  // Bids
  placeBid(auctionId: string, amount: number, currency: string = 'USD', isProxy: boolean = false, maxProxyAmount?: number): Observable<{ bidId: string }> {
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

  // Vehicles
  getVehicle(id: string): Observable<Vehicle> {
    return this.http.get<Vehicle>(`${this.baseUrl}/vehicles/${id}`);
  }

  searchVehicles(params?: {
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

  // Users
  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/users/me`);
  }

  getUserBidHistory(): Observable<Bid[]> {
    return this.http.get<Bid[]>(`${this.baseUrl}/users/me/bids`);
  }
}
