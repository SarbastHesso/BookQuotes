import { Component, signal } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Navbar } from './shared/components/navbar/navbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
})
export class App {
  protected readonly title = signal('bookquotes-ui');
  constructor(private router: Router) {
    // Scroll to top (account for sticky header) on each successful navigation.
    this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd)).subscribe(() => {
      try {
        // small timeout to let mobile browsers settle after navigation
        setTimeout(() => window.scrollTo({ top: 0, left: 0, behavior: 'auto' }), 50);
      } catch {}
    });
  }
}
