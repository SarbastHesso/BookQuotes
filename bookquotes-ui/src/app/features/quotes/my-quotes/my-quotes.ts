import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { QuoteService } from '../../../core/services/quote.service';
import { Quote } from '../../../core/models/quote.model';

@Component({
  selector: 'app-my-quotes',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-quotes.html',
  styleUrl: './my-quotes.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyQuotes implements OnInit {
  readonly quotes = signal<Quote[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly deleting = signal(false);
  readonly quotePendingDelete = signal<Quote | null>(null);
  readonly maxQuotes = 5;

  constructor(
    private readonly quoteService: QuoteService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadQuotes();
  }

  loadQuotes(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.quoteService.getMyQuotes().subscribe({
      next: (quotes) => {
        this.quotes.set(quotes);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Unable to load your quotes.');
        this.loading.set(false);
      },
    });
  }

  editQuote(id: number): void {
    this.router.navigate(['/quotes/edit', id]);
  }

  openDeleteDialog(quote: Quote): void {
    this.quotePendingDelete.set(quote);
  }

  closeDeleteDialog(): void {
    if (this.deleting()) {
      return;
    }

    this.quotePendingDelete.set(null);
  }

  confirmDeleteQuote(): void {
    const quote = this.quotePendingDelete();

    if (!quote) {
      return;
    }

    this.deleting.set(true);
    this.quoteService.delete(quote.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.closeDeleteDialog();
        this.loadQuotes();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to delete the quote.');
        this.deleting.set(false);
      },
    });
  }
}
