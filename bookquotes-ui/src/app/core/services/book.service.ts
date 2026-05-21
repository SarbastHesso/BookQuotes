import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book } from '../models/book.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  private apiUrl = `${environment.apiBaseUrl}/api/books`;

  constructor(private http: HttpClient) {}

  // ---------------------------------------------------------
  // GET ALL BOOKS (Public)
  // ---------------------------------------------------------
  getAll(): Observable<Book[]> {
    return this.http.get<Book[]>(this.apiUrl);
  }

  // ---------------------------------------------------------
  // GET BOOK BY ID (Public)
  // ---------------------------------------------------------
  getById(id: number): Observable<Book> {
    return this.http.get<Book>(`${this.apiUrl}/${id}`);
  }

  // ---------------------------------------------------------
  // CREATE BOOK (Protected)
  // ---------------------------------------------------------
  create(book: Partial<Book>): Observable<Book> {
    return this.http.post<Book>(this.apiUrl, book);
  }

  // ---------------------------------------------------------
  // UPDATE BOOK (Protected)
  // ---------------------------------------------------------
  update(id: number, book: Partial<Book>): Observable<Book> {
    return this.http.put<Book>(`${this.apiUrl}/${id}`, book);
  }

  // ---------------------------------------------------------
  // DELETE BOOK (Protected)
  // ---------------------------------------------------------
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
