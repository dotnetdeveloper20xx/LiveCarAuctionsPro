import { Component, Input, Output, EventEmitter, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-bid-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="bid-input-container">
      <div class="current-bid">
        <span class="label">Current Bid</span>
        <span class="amount">{{ currentBid | currency:currency }}</span>
      </div>

      <mat-form-field appearance="outline" class="bid-field">
        <mat-label>Your Bid</mat-label>
        <input
          matInput
          type="number"
          [min]="minimumBid"
          [step]="bidIncrement"
          [(ngModel)]="bidAmount"
          [disabled]="disabled || isSubmitting()"
          placeholder="Enter bid amount"
        />
        <span matTextPrefix>$&nbsp;</span>
      </mat-form-field>

      <div class="quick-bids">
        @for (increment of quickBidIncrements; track increment) {
          <button
            mat-stroked-button
            [disabled]="disabled || isSubmitting()"
            (click)="setQuickBid(increment)"
          >
            +{{ increment | currency:currency:'symbol':'1.0-0' }}
          </button>
        }
      </div>

      @if (error) {
        <div class="error-message">
          <mat-icon>error</mat-icon>
          {{ error }}
        </div>
      }

      <button
        mat-raised-button
        color="primary"
        class="submit-btn"
        [disabled]="!canSubmit() || isSubmitting()"
        (click)="submitBid()"
      >
        @if (isSubmitting()) {
          <mat-spinner diameter="20"></mat-spinner>
        } @else {
          <mat-icon>gavel</mat-icon>
          Place Bid
        }
      </button>

      <p class="bid-info">
        Minimum bid: {{ minimumBid | currency:currency }}
      </p>
    </div>
  `,
  styles: [`
    .bid-input-container {
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 16px;
      background: #fafafa;
      border-radius: 8px;
    }

    .current-bid {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 16px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .current-bid .label {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
      text-transform: uppercase;
    }

    .current-bid .amount {
      font-size: 2rem;
      font-weight: 600;
      color: #1976d2;
    }

    .bid-field {
      width: 100%;
    }

    .quick-bids {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .quick-bids button {
      flex: 1;
      min-width: 80px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background: #ffebee;
      border-radius: 4px;
      color: #c62828;
    }

    .submit-btn {
      padding: 12px 24px;
      font-size: 1.125rem;
    }

    .submit-btn mat-icon {
      margin-right: 8px;
    }

    .bid-info {
      text-align: center;
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
      margin: 0;
    }
  `]
})
export class BidInputComponent {
  @Input() currentBid = 0;
  @Input() minimumBid = 0;
  @Input() bidIncrement = 100;
  @Input() currency = 'USD';
  @Input() disabled = false;
  @Input() error: string | null = null;
  @Output() bid = new EventEmitter<number>();

  bidAmount = 0;
  private readonly _isSubmitting = signal(false);
  readonly isSubmitting = this._isSubmitting.asReadonly();

  readonly quickBidIncrements = [100, 500, 1000, 5000];

  readonly canSubmit = computed(() =>
    this.bidAmount >= this.minimumBid && !this.disabled
  );

  ngOnChanges(): void {
    if (this.bidAmount < this.minimumBid) {
      this.bidAmount = this.minimumBid;
    }
  }

  setQuickBid(increment: number): void {
    this.bidAmount = this.currentBid + increment;
  }

  submitBid(): void {
    if (!this.canSubmit()) return;

    this._isSubmitting.set(true);
    this.bid.emit(this.bidAmount);

    // Reset submitting state after a delay (parent should handle actual completion)
    setTimeout(() => this._isSubmitting.set(false), 2000);
  }
}
