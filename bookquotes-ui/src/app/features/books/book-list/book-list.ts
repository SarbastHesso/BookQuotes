import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterModule, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { timeout } from 'rxjs';
import { BookService } from '../../../core/services/book.service';
import { AuthService } from '../../../core/services/auth.service';
import { Book } from '../../../core/models/book.model';

@Component({
  selector: 'app-book-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './book-list.html',
  styleUrls: ['./book-list.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookList implements OnInit {
  readonly books = signal<Book[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly deleting = signal(false);
  readonly bookPendingDelete = signal<Book | null>(null);
  readonly isLoggedIn;

  constructor(
    private bookService: BookService,
    private router: Router,
    authService: AuthService,
  ) {
    this.isLoggedIn = toSignal(authService.currentUser$, { initialValue: null });
  }

  ngOnInit() {
    this.loadBooks();
  }

  // ---------------------------------------------------------
  // LOAD ALL BOOKS (Public)
  // ---------------------------------------------------------
  loadBooks() {
    this.loading.set(true);
    this.errorMessage.set('');
    this.bookService.getAll().pipe(timeout(10000)).subscribe({
      next: (res) => {
        this.books.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load books right now.');
        this.loading.set(false);
      },
    });
  }

  // ---------------------------------------------------------
  // EDIT BOOK (Protected)
  // ---------------------------------------------------------
  editBook(id: number) {
    this.router.navigate(['/books/edit', id]);
  }

  openDeleteDialog(book: Book) {
    this.bookPendingDelete.set(book);
  }

  closeDeleteDialog() {
    if (this.deleting()) {
      return;
    }

    this.bookPendingDelete.set(null);
  }

  // ---------------------------------------------------------
  // DELETE BOOK (Protected)
  // ---------------------------------------------------------
  confirmDeleteBook() {
    const book = this.bookPendingDelete();

    if (!book) {
      return;
    }

    this.deleting.set(true);
    this.bookService.delete(book.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.closeDeleteDialog();
        this.loadBooks();
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message || 'Failed to delete the book.');
        this.deleting.set(false);
      },
    });
  }
}
