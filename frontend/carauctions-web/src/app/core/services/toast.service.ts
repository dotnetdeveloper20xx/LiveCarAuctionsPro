import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

export interface ToastOptions {
  duration?: number;
  action?: string;
  panelClass?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private readonly snackBar = inject(MatSnackBar);

  private readonly defaultConfig: MatSnackBarConfig = {
    duration: 4000,
    horizontalPosition: 'end',
    verticalPosition: 'bottom'
  };

  show(message: string, options?: ToastOptions): void {
    this.snackBar.open(message, options?.action || 'Close', {
      ...this.defaultConfig,
      duration: options?.duration ?? this.defaultConfig.duration,
      panelClass: options?.panelClass
    });
  }

  success(message: string, options?: ToastOptions): void {
    this.show(message, {
      ...options,
      panelClass: ['toast-success', ...(options?.panelClass || [])]
    });
  }

  error(message: string, options?: ToastOptions): void {
    this.show(message, {
      ...options,
      duration: options?.duration ?? 6000,
      panelClass: ['toast-error', ...(options?.panelClass || [])]
    });
  }

  warning(message: string, options?: ToastOptions): void {
    this.show(message, {
      ...options,
      panelClass: ['toast-warning', ...(options?.panelClass || [])]
    });
  }

  info(message: string, options?: ToastOptions): void {
    this.show(message, {
      ...options,
      panelClass: ['toast-info', ...(options?.panelClass || [])]
    });
  }

  outbid(auctionTitle: string, newBid: number): void {
    this.show(`You've been outbid on "${auctionTitle}"! New bid: $${newBid.toLocaleString()}`, {
      duration: 8000,
      action: 'View',
      panelClass: ['toast-outbid']
    });
  }

  bidPlaced(amount: number): void {
    this.success(`Bid of $${amount.toLocaleString()} placed successfully!`);
  }

  auctionWon(auctionTitle: string, amount: number): void {
    this.show(`Congratulations! You won "${auctionTitle}" for $${amount.toLocaleString()}!`, {
      duration: 10000,
      panelClass: ['toast-won']
    });
  }

  auctionEndingSoon(auctionTitle: string, minutesLeft: number): void {
    this.warning(`"${auctionTitle}" ends in ${minutesLeft} minutes!`, {
      duration: 6000,
      action: 'View'
    });
  }
}
