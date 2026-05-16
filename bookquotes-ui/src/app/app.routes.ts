import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // ---------------------------------------------------------
  // HOME → redirect to /books
  // ---------------------------------------------------------
  {
    path: '',
    redirectTo: 'books',
    pathMatch: 'full',
  },

  // ---------------------------------------------------------
  // BOOKS
  // ---------------------------------------------------------
  {
    path: 'books',
    loadComponent: () => import('./features/books/book-list/book-list').then((m) => m.BookList),
  },
  {
    path: 'books/add',
    canActivate: [authGuard],
    loadComponent: () => import('./features/books/book-form/book-form').then((m) => m.BookForm),
  },
  {
    path: 'books/edit/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/books/book-form/book-form').then((m) => m.BookForm),
  },

  // ---------------------------------------------------------
  // QUOTES
  // ---------------------------------------------------------
  {
    path: 'quotes',
    loadComponent: () => import('./features/quotes/quote-list/quote-list').then((m) => m.QuoteList),
  },
  {
    path: 'quotes/add',
    canActivate: [authGuard],
    loadComponent: () => import('./features/quotes/quote-form/quote-form').then((m) => m.QuoteForm),
  },
  {
    path: 'quotes/edit/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/quotes/quote-form/quote-form').then((m) => m.QuoteForm),
  },
  {
    path: 'quotes/my',
    canActivate: [authGuard],
    loadComponent: () => import('./features/quotes/my-quotes/my-quotes').then((m) => m.MyQuotes),
  },

  // ---------------------------------------------------------
  // AUTH
  // ---------------------------------------------------------
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },

  // ---------------------------------------------------------
  // FALLBACK
  // ---------------------------------------------------------
  {
    path: '**',
    redirectTo: 'books',
  },
];
