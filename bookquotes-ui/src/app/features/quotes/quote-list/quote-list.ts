import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { QuoteService } from '../../../core/services/quote.service';
import { AuthService } from '../../../core/services/auth.service';
import { Quote } from '../../../core/models/quote.model';

@Component({
  selector: 'app-quote-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './quote-list.html',
  styleUrl: './quote-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuoteList implements OnInit {
  readonly quotes = signal<Quote[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly currentUser;

  constructor(
    private readonly quoteService: QuoteService,
    authService: AuthService,
  ) {
    this.currentUser = toSignal(authService.currentUser$, { initialValue: null });
  }

  ngOnInit(): void {
    this.loadQuotes();
  }

  loadQuotes(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.quoteService.getAll().subscribe({
      next: (quotes) => {
        this.quotes.set(quotes);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load quotes right now.');
        this.loading.set(false);
      },
    });
  }
}
