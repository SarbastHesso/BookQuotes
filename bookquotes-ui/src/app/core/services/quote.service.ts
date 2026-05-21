import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Quote } from '../models/quote.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class QuoteService {
  private apiUrl = `${environment.apiBaseUrl}/api/quotes`;

  constructor(private http: HttpClient) {}

  // ---------------------------------------------------------
  // GET ALL QUOTES (Public)
  // ---------------------------------------------------------
  getAll(): Observable<Quote[]> {
    return this.http.get<Quote[]>(this.apiUrl);
  }

  // ---------------------------------------------------------
  // GET QUOTE BY ID (Public)
  // ---------------------------------------------------------
  getById(id: number): Observable<Quote> {
    return this.http.get<Quote>(`${this.apiUrl}/${id}`);
  }

  // ---------------------------------------------------------
  // GET MY QUOTES (Protected)
  // ---------------------------------------------------------
  getMyQuotes(): Observable<Quote[]> {
    return this.http.get<Quote[]>(`${this.apiUrl}/my`);
  }

  // ---------------------------------------------------------
  // CREATE QUOTE (Protected)
  // ---------------------------------------------------------
  create(quote: Partial<Quote>): Observable<Quote> {
    return this.http.post<Quote>(this.apiUrl, quote);
  }

  // ---------------------------------------------------------
  // UPDATE QUOTE (Protected)
  // ---------------------------------------------------------
  update(id: number, quote: Partial<Quote>): Observable<Quote> {
    return this.http.put<Quote>(`${this.apiUrl}/${id}`, quote);
  }

  // ---------------------------------------------------------
  // DELETE QUOTE (Protected)
  // ---------------------------------------------------------
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
