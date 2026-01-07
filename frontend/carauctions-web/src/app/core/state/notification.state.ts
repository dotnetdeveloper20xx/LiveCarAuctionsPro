import { Injectable, inject, signal, computed } from '@angular/core';
import { ApiService } from '../services/api.service';
import { SignalRService } from '../services/signalr.service';
import { Notification, NotificationType } from '../../shared/models/auction.model';
import { effect } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NotificationState {
  private readonly api = inject(ApiService);
  private readonly signalR = inject(SignalRService);

  private readonly _notifications = signal<Notification[]>([]);
  private readonly _loading = signal(false);

  readonly notifications = this._notifications.asReadonly();
  readonly loading = this._loading.asReadonly();

  readonly unreadCount = computed(() =>
    this._notifications().filter(n => !n.isRead).length
  );

  readonly unreadNotifications = computed(() =>
    this._notifications().filter(n => !n.isRead)
  );

  constructor() {
    // Listen for real-time bid updates to create notifications
    effect(() => {
      const bidUpdate = this.signalR.lastBidUpdate();
      if (bidUpdate) {
        this.addLocalNotification({
          id: crypto.randomUUID(),
          type: NotificationType.BidPlaced,
          title: 'New Bid Placed',
          message: `A bid of $${bidUpdate.amount.toLocaleString()} was placed`,
          auctionId: bidUpdate.auctionId,
          isRead: false,
          createdAt: new Date()
        });
      }
    });
  }

  loadNotifications(): void {
    this._loading.set(true);
    this.api.getNotifications().subscribe({
      next: (notifications) => {
        this._notifications.set(notifications);
        this._loading.set(false);
      },
      error: () => {
        this._loading.set(false);
      }
    });
  }

  addLocalNotification(notification: Notification): void {
    this._notifications.update(notifications => [notification, ...notifications]);
  }

  addOutbidNotification(auctionId: string, auctionTitle: string, amount: number): void {
    this.addLocalNotification({
      id: crypto.randomUUID(),
      type: NotificationType.Outbid,
      title: 'You\'ve been outbid!',
      message: `Someone placed a higher bid of $${amount.toLocaleString()} on "${auctionTitle}"`,
      auctionId,
      isRead: false,
      createdAt: new Date()
    });
  }

  addAuctionWonNotification(auctionId: string, auctionTitle: string, amount: number): void {
    this.addLocalNotification({
      id: crypto.randomUUID(),
      type: NotificationType.AuctionWon,
      title: 'Congratulations! You won!',
      message: `You won "${auctionTitle}" with a bid of $${amount.toLocaleString()}`,
      auctionId,
      isRead: false,
      createdAt: new Date()
    });
  }

  markAsRead(id: string): void {
    this._notifications.update(notifications =>
      notifications.map(n => n.id === id ? { ...n, isRead: true } : n)
    );
    this.api.markNotificationAsRead(id).subscribe();
  }

  markAllAsRead(): void {
    this._notifications.update(notifications =>
      notifications.map(n => ({ ...n, isRead: true }))
    );
    this.api.markAllNotificationsAsRead().subscribe();
  }

  clearNotification(id: string): void {
    this._notifications.update(notifications =>
      notifications.filter(n => n.id !== id)
    );
  }
}
