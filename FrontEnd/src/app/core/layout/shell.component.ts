import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="bar">
      <strong>Gestor de turnos</strong>
      <nav>
        <a routerLink="/shifts" routerLinkActive="active">Turnos</a>
        <a routerLink="/collaborators" routerLinkActive="active">Colaboradores</a>
      </nav>
    </header>
    <main class="content">
      <router-outlet />
    </main>
    <p-toast />
  `,
  styles: `
    .bar {
      display: flex;
      align-items: center;
      gap: 2rem;
      padding: 0.75rem 1.5rem;
      border-bottom: 1px solid var(--p-content-border-color);
    }
    nav {
      display: flex;
      gap: 1rem;
    }
    a {
      color: inherit;
      text-decoration: none;
    }
    a.active {
      font-weight: 600;
      text-decoration: underline;
    }
    .content {
      padding: 1.5rem;
    }
  `,
})
export class ShellComponent {}
