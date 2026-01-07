import { Injectable, inject, signal, computed } from '@angular/core';
import { ApiService } from '../services/api.service';
import { WatchlistItem } from '../../shared/models/auction.model';

@Injectable({
  providedIn: 'root'
})
export class WatchlistState {
  private readonly api = inject(ApiService);

  private readonly _items = signal<WatchlistItem[]>([]);
  private readonly _loading = signal(false);
  private readonly _watchedIds = signal<Set<string>>(new Set());

  readonly items = this._items.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly count = computed(() => this._items().length);

  isWatching(auctionId: string): boolean {
    return this._watchedIds().has(auctionId);
  }

  loadWatchlist(): void {
    this._loading.set(true);
    this.api.getWatchlist().subscribe({
      next: (items) => {
        this._items.set(items);
        this._watchedIds.set(new Set(items.map(i => i.auctionId)));
        this._loading.set(false);
      },
      error: () => {
        this._loading.set(false);
      }
    });
  }

  toggleWatch(auctionId: string): void {
    if (this.isWatching(auctionId)) {
      this.removeFromWatchlist(auctionId);
    } else {
      this.addToWatchlist(auctionId);
    }
  }

  addToWatchlist(auctionId: string): void {
    // Optimistic update
    this._watchedIds.update(ids => new Set([...ids, auctionId]));

    this.api.addToWatchlist(auctionId).subscribe({
      next: (item) => {
        this._items.update(items => [...items, item]);
      },
      error: () => {
        // Rollback on error
        this._watchedIds.update(ids => {
          const newIds = new Set(ids);
          newIds.delete(auctionId);
          return newIds;
        });
      }
    });
  }

  removeFromWatchlist(auctionId: string): void {
    // Optimistic update
    this._watchedIds.update(ids => {
      const newIds = new Set(ids);
      newIds.delete(auctionId);
      return newIds;
    });
    this._items.update(items => items.filter(i => i.auctionId !== auctionId));

    this.api.removeFromWatchlist(auctionId).subscribe({
      error: () => {
        // Rollback on error - reload watchlist
        this.loadWatchlist();
      }
    });
  }
}
