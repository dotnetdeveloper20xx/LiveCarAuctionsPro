import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { Bid, Auction } from '../../shared/models/auction.model';

export interface BidUpdate {
  auctionId: string;
  bidId: string;
  bidderId: string;
  bidderName: string;
  amount: number;
  currency: string;
  timestamp: Date;
}

export interface AuctionUpdate {
  auctionId: string;
  status: string;
  currentHighBid: number;
  endTime: Date;
  message?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;

  // Signals for reactive state
  readonly connectionState = signal<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  readonly lastBidUpdate = signal<BidUpdate | null>(null);
  readonly lastAuctionUpdate = signal<AuctionUpdate | null>(null);

  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/auction`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerHandlers();

    try {
      await this.hubConnection.start();
      this.connectionState.set(signalR.HubConnectionState.Connected);
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    }
  }

  async joinAuction(auctionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinAuction', auctionId);
    }
  }

  async leaveAuction(auctionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveAuction', auctionId);
    }
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('BidPlaced', (update: BidUpdate) => {
      this.lastBidUpdate.set({
        ...update,
        timestamp: new Date(update.timestamp)
      });
    });

    this.hubConnection.on('AuctionUpdated', (update: AuctionUpdate) => {
      this.lastAuctionUpdate.set({
        ...update,
        endTime: new Date(update.endTime)
      });
    });

    this.hubConnection.on('AuctionEnded', (update: AuctionUpdate) => {
      this.lastAuctionUpdate.set({
        ...update,
        endTime: new Date(update.endTime),
        message: 'Auction has ended'
      });
    });

    this.hubConnection.onreconnecting(() => {
      this.connectionState.set(signalR.HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected(() => {
      this.connectionState.set(signalR.HubConnectionState.Connected);
    });

    this.hubConnection.onclose(() => {
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    });
  }
}
