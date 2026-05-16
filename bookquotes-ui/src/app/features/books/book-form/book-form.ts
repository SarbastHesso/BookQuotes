import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { BookService } from '../../../core/services/book.service';
import { Book } from '../../../core/models/book.model';

@Component({
  selector: 'app-book-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './book-form.html',
  styleUrls: ['./book-form.scss'],
})
export class BookForm implements OnInit {
  id: number | null = null;
  errorMessage = '';
  isSaving = false;
  private existingBooks: Book[] = [];

  // ---------------------------------------------------------
  // FORM (initialized in ngOnInit)
  // ---------------------------------------------------------
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private bookService: BookService,
  ) {}

  ngOnInit() {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      author: ['', [Validators.required, Validators.maxLength(200)]],
      publishedYear: [2024, [Validators.required, Validators.min(1450), Validators.max(2100)]],
    });

    this.id = Number(this.route.snapshot.paramMap.get('id'));

    this.loadExistingBooks();
  }

  // ---------------------------------------------------------
  // SAVE BOOK (Protected)
  // ---------------------------------------------------------
  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    const book = {
      title: (this.form.getRawValue().title ?? '').trim(),
      author: (this.form.getRawValue().author ?? '').trim(),
      publishedYear: this.form.getRawValue().publishedYear,
    } as Partial<Book>;

    if (this.isDuplicateBook(book.title ?? '', book.author ?? '')) {
      this.errorMessage = this.id
        ? 'Another book with the same title and author already exists. Update that entry or change the details.'
        : 'This book is already in the library. Try a different title and author combination.';
      return;
    }

    this.isSaving = true;

    if (this.id) {
      this.bookService.update(this.id, book).pipe(finalize(() => (this.isSaving = false))).subscribe({
        next: () => {
          this.router.navigate(['/books']);
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(err, 'Failed to update the book.');
        },
      });
    } else {
      this.bookService.create(book).pipe(finalize(() => (this.isSaving = false))).subscribe({
        next: () => {
          this.router.navigate(['/books']);
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(err, 'Failed to create the book.');
        },
      });
    }
  }

  private loadExistingBooks(): void {
    this.bookService.getAll().subscribe({
      next: (books) => {
        this.existingBooks = books;

        if (!this.id) {
          return;
        }

        const book = books.find((item) => item.id === this.id);
        if (!book) {
          this.errorMessage = 'Book not found.';
          return;
        }

        this.form.patchValue(book);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = this.getErrorMessage(err, 'Failed to load the book details.');
      },
    });
  }

  private isDuplicateBook(title: string, author: string): boolean {
    const normalizedTitle = title.trim().toLowerCase();
    const normalizedAuthor = author.trim().toLowerCase();

    if (!normalizedTitle || !normalizedAuthor) {
      return false;
    }

    return this.existingBooks.some(
      (book) => book.id !== this.id && book.title.trim().toLowerCase() === normalizedTitle && book.author.trim().toLowerCase() === normalizedAuthor,
    );
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
          return this.mapBookErrorMessage(message);
        }
      }

      if (typeof err.error === 'string' && err.error.trim()) {
        return this.mapBookErrorMessage(err.error);
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

  private mapBookErrorMessage(message: string): string {
    switch (message) {
      case 'A book with the same title and author already exists.':
        return 'This book is already in the library. Try a different title and author combination.';
      case 'Another book with the same title and author already exists.':
        return 'Another book with the same title and author already exists. Update that entry or change the details.';
      default:
        return message;
    }
  }
}
