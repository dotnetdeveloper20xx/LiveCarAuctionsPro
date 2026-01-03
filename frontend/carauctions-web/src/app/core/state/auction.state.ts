import { Injectable, inject, signal, computed } from '@angular/core';
import { ApiService } from '../services/api.service';
import { SignalRService, BidUpdate, AuctionUpdate } from '../services/signalr.service';
import { Auction, Bid, AuctionStatus, PaginatedResult } from '../../shared/models/auction.model';
import { effect } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuctionState {
  private readonly api = inject(ApiService);
  private readonly signalR = inject(SignalRService);

  // Private signals
  private readonly _auctions = signal<Auction[]>([]);
  private readonly _selectedAuction = signal<Auction | null>(null);
  private readonly _bidHistory = signal<Bid[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _pagination = signal<{
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  }>({ pageNumber: 1, pageSize: 12, totalCount: 0, totalPages: 0 });

  // Public readonly signals
  readonly auctions = this._auctions.asReadonly();
  readonly selectedAuction = this._selectedAuction.asReadonly();
  readonly bidHistory = this._bidHistory.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly pagination = this._pagination.asReadonly();

  // Computed signals
  readonly activeAuctions = computed(() =>
    this._auctions().filter(a => a.status === AuctionStatus.Active)
  );

  readonly endingSoonAuctions = computed(() => {
    const now = new Date();
    const oneHour = 60 * 60 * 1000;
    return this._auctions()
      .filter(a => a.status === AuctionStatus.Active)
      .filter(a => new Date(a.endTime).getTime() - now.getTime() < oneHour)
      .sort((a, b) => new Date(a.endTime).getTime() - new Date(b.endTime).getTime());
  });

  readonly highestBid = computed(() => {
    const history = this._bidHistory();
    if (history.length === 0) return null;
    return history.reduce((max, bid) =>
      bid.amount.amount > max.amount.amount ? bid : max
    );
  });

  constructor() {
    // React to SignalR bid updates
    effect(() => {
      const update = this.signalR.lastBidUpdate();
      if (update) {
        this.handleBidUpdate(update);
      }
    });

    // React to SignalR auction updates
    effect(() => {
      const update = this.signalR.lastAuctionUpdate();
      if (update) {
        this.handleAuctionUpdate(update);
      }
    });
  }

  loadAuctions(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    type?: string;
    searchTerm?: string;
  }): void {
    this._loading.set(true);
    this._error.set(null);

    this.api.getAuctions(params).subscribe({
      next: (result) => {
        this._auctions.set(result.items);
        this._pagination.set({
          pageNumber: result.pageNumber,
          pageSize: result.pageSize,
          totalCount: result.totalCount,
          totalPages: result.totalPages
        });
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set(err.message || 'Failed to load auctions');
        this._loading.set(false);
      }
    });
  }

  loadActiveAuctions(): void {
    this._loading.set(true);
    this._error.set(null);

    this.api.getActiveAuctions().subscribe({
      next: (auctions) => {
        this._auctions.set(auctions);
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set(err.message || 'Failed to load active auctions');
        this._loading.set(false);
      }
    });
  }

  selectAuction(id: string): void {
    this._loading.set(true);
    this._error.set(null);

    this.api.getAuction(id).subscribe({
      next: (auction) => {
        this._selectedAuction.set(auction);
        this.loadBidHistory(id);
        this.signalR.joinAuction(id);
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set(err.message || 'Failed to load auction');
        this._loading.set(false);
      }
    });
  }

  clearSelectedAuction(): void {
    const current = this._selectedAuction();
    if (current) {
      this.signalR.leaveAuction(current.id);
    }
    this._selectedAuction.set(null);
    this._bidHistory.set([]);
  }

  loadBidHistory(auctionId: string): void {
    this.api.getBidHistory(auctionId).subscribe({
      next: (bids) => this._bidHistory.set(bids),
      error: (err) => console.error('Failed to load bid history:', err)
    });
  }

  placeBid(amount: number, currency: string = 'USD'): void {
    const auction = this._selectedAuction();
    if (!auction) return;

    this._loading.set(true);
    this._error.set(null);

    this.api.placeBid(auction.id, amount, currency).subscribe({
      next: () => {
        this._loading.set(false);
        // Bid update will come through SignalR
      },
      error: (err) => {
        this._error.set(err.error?.errors?.[0]?.description || 'Failed to place bid');
        this._loading.set(false);
      }
    });
  }

  private handleBidUpdate(update: BidUpdate): void {
    // Update selected auction if it matches
    const selected = this._selectedAuction();
    if (selected && selected.id === update.auctionId) {
      this._selectedAuction.set({
        ...selected,
        currentHighBid: { amount: update.amount, currency: update.currency },
        bidCount: selected.bidCount + 1
      });

      // Add bid to history
      const newBid: Bid = {
        id: update.bidId,
        auctionId: update.auctionId,
        bidderId: update.bidderId,
        bidderName: update.bidderName,
        amount: { amount: update.amount, currency: update.currency },
        status: 'Winning' as any,
        placedAt: update.timestamp,
        isProxyBid: false
      };
      this._bidHistory.update(history => [newBid, ...history]);
    }

    // Update auctions list
    this._auctions.update(auctions =>
      auctions.map(a =>
        a.id === update.auctionId
          ? { ...a, currentHighBid: { amount: update.amount, currency: update.currency }, bidCount: a.bidCount + 1 }
          : a
      )
    );
  }

  private handleAuctionUpdate(update: AuctionUpdate): void {
    const selected = this._selectedAuction();
    if (selected && selected.id === update.auctionId) {
      this._selectedAuction.set({
        ...selected,
        status: update.status as AuctionStatus,
        currentHighBid: { ...selected.currentHighBid, amount: update.currentHighBid },
        endTime: update.endTime
      });
    }

    this._auctions.update(auctions =>
      auctions.map(a =>
        a.id === update.auctionId
          ? { ...a, status: update.status as AuctionStatus, endTime: update.endTime }
          : a
      )
    );
  }
}
