import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';
import { User } from '../../../core/models/user.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Navbar {
  private readonly authService = inject(AuthService);
  private readonly themeService = inject(ThemeService);
  private readonly router = inject(Router);
  private readonly elementRef = inject(ElementRef<HTMLElement>);

  readonly user = toSignal<User | null>(this.authService.currentUser$, { initialValue: null });
  readonly theme = this.themeService.theme;

  @HostListener('document:click', ['$event'])
  handleDocumentClick(event: MouseEvent) {
    const menu = document.getElementById('navbarSupportedContent');
    const clickTarget = event.target;

    if (!(clickTarget instanceof Node) || !menu?.classList.contains('show')) {
      return;
    }

    if (!this.elementRef.nativeElement.contains(clickTarget)) {
      this.collapseMenu();
    }
  }

  logout() {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
        this.collapseMenu();
      },
    });
  }

  collapseMenu() {
    const menu = document.getElementById('navbarSupportedContent');
    if (menu) {
      menu.classList.remove('show');
    }
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}
