import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { QuoteService } from '../../../core/services/quote.service';
import { Quote } from '../../../core/models/quote.model';

@Component({
  selector: 'app-quote-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './quote-form.html',
  styleUrl: './quote-form.scss',
})
export class QuoteForm implements OnInit {
  id: number | null = null;
  isSaving = false;
  errorMessage = '';
  private existingQuotes: Quote[] = [];
  readonly form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly quoteService: QuoteService,
  ) {
    this.form = this.fb.group({
      text: ['', [Validators.required, Validators.maxLength(1000)]],
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.id = idParam ? Number(idParam) : null;

    this.loadExistingQuotes();
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = '';

    const payload = {
      text: (this.form.getRawValue().text ?? '').trim(),
    } as Partial<Quote>;

    if (this.isDuplicateQuote(payload.text ?? '')) {
      this.errorMessage = this.id
        ? 'You already have a saved quote with this text. Update the existing one or change the text.'
        : 'You already saved this quote. Try a different one or edit the existing entry.';
      return;
    }

    this.isSaving = true;

    if (this.id) {
      this.quoteService.update(this.id, payload).pipe(finalize(() => (this.isSaving = false))).subscribe({
        next: () => this.router.navigate(['/quotes/my']),
        error: (err: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(err, 'Failed to update the quote.');
        },
      });
      return;
    }

    this.quoteService.create(payload).pipe(finalize(() => (this.isSaving = false))).subscribe({
      next: () => this.router.navigate(['/quotes/my']),
      error: (err: HttpErrorResponse) => {
        this.errorMessage = this.getErrorMessage(err, 'Failed to create the quote.');
      },
    });
  }

  private loadExistingQuotes(): void {
    this.quoteService.getMyQuotes().subscribe({
      next: (quotes) => {
        this.existingQuotes = quotes;

        if (!this.id) {
          return;
        }

        const quote = quotes.find((item) => item.id === this.id);

        if (!quote) {
          this.errorMessage = 'Quote not found in your saved list.';
          return;
        }

        this.form.patchValue({ text: quote.text });
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = this.getErrorMessage(err, 'Failed to load the quote.');
      },
    });
  }

  private isDuplicateQuote(text: string): boolean {
    const normalizedText = text.trim().toLowerCase();

    if (!normalizedText) {
      return false;
    }

    return this.existingQuotes.some((quote) => quote.id !== this.id && quote.text.trim().toLowerCase() === normalizedText);
  }

  private getErrorMessage(err: HttpErrorResponse, fallbackMessage: string): string {
    try {
      if (err.error && typeof err.error === 'object' && 'errors' in err.error) {
        const validationErrors = this.collectValidationErrors((err.error as { errors: unknown }).errors);
        if (validationErrors) {
          return validationErrors;
        }
      }

      if (err.error && typeof err.error === 'object' && 'message' in err.error) {
        const message = (err.error as { message?: unknown }).message;
        if (typeof message === 'string' && message.trim()) {
          return this.mapQuoteErrorMessage(message);
        }
      }

      if (typeof err.error === 'string' && err.error.trim()) {
        return this.mapQuoteErrorMessage(err.error);
      }

      if (typeof err.message === 'string' && err.message.trim()) {
        return err.message;
      }
    } catch {
      return fallbackMessage;
    }

    return fallbackMessage;
  }

  private collectValidationErrors(errors: unknown): string {
    if (!errors || typeof errors !== 'object') {
      return '';
    }

    const messages = Object.values(errors).flatMap((value) => {
      if (Array.isArray(value)) {
        return value.filter((item): item is string => typeof item === 'string' && item.trim().length > 0);
      }

      if (typeof value === 'string' && value.trim()) {
        return [value];
      }

      return [];
    });

    return messages.join(' ');
  }

  private mapQuoteErrorMessage(message: string): string {
    switch (message) {
      case 'You already added this quote.':
        return 'You already saved this quote. Try a different one or edit the existing entry.';
      case 'You already have a quote with this text.':
        return 'You already have a saved quote with this text. Update the existing one or change the text.';
      case 'You can only save up to 5 quotes.':
        return 'You have reached the 5-quote limit. Delete one of your saved quotes before adding another.';
      default:
        return message;
    }
  }
}
