import { Component, Input, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-countdown-timer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="countdown" [class.urgent]="isUrgent()">
      @if (isExpired()) {
        <span class="expired">Ended</span>
      } @else {
        <div class="time-unit" *ngIf="days() > 0">
          <span class="value">{{ days() }}</span>
          <span class="label">days</span>
        </div>
        <div class="time-unit">
          <span class="value">{{ hours() | number:'2.0' }}</span>
          <span class="label">hrs</span>
        </div>
        <div class="separator">:</div>
        <div class="time-unit">
          <span class="value">{{ minutes() | number:'2.0' }}</span>
          <span class="label">min</span>
        </div>
        <div class="separator">:</div>
        <div class="time-unit">
          <span class="value">{{ seconds() | number:'2.0' }}</span>
          <span class="label">sec</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .countdown {
      display: flex;
      align-items: center;
      gap: 4px;
      font-family: 'Roboto Mono', monospace;
    }

    .countdown.urgent {
      color: #f44336;
      animation: pulse 1s infinite;
    }

    .time-unit {
      display: flex;
      flex-direction: column;
      align-items: center;
      min-width: 40px;
    }

    .value {
      font-size: 1.5rem;
      font-weight: 600;
      line-height: 1;
    }

    .label {
      font-size: 0.625rem;
      text-transform: uppercase;
      color: rgba(0, 0, 0, 0.6);
    }

    .separator {
      font-size: 1.5rem;
      font-weight: 600;
      margin-bottom: 12px;
    }

    .expired {
      font-size: 1.25rem;
      font-weight: 600;
      color: #f44336;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.7; }
    }
  `]
})
export class CountdownTimerComponent implements OnInit, OnDestroy {
  @Input({ required: true }) endTime!: Date;
  @Input() urgentThreshold = 300; // 5 minutes in seconds

  private intervalId: any;
  private readonly _remaining = signal(0);

  readonly days = computed(() => Math.floor(this._remaining() / 86400));
  readonly hours = computed(() => Math.floor((this._remaining() % 86400) / 3600));
  readonly minutes = computed(() => Math.floor((this._remaining() % 3600) / 60));
  readonly seconds = computed(() => Math.floor(this._remaining() % 60));
  readonly isExpired = computed(() => this._remaining() <= 0);
  readonly isUrgent = computed(() => this._remaining() > 0 && this._remaining() <= this.urgentThreshold);

  ngOnInit(): void {
    this.updateRemaining();
    this.intervalId = setInterval(() => this.updateRemaining(), 1000);
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  private updateRemaining(): void {
    const end = new Date(this.endTime).getTime();
    const now = Date.now();
    const remaining = Math.max(0, Math.floor((end - now) / 1000));
    this._remaining.set(remaining);
  }
}
