import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1>Página no encontrada</h1>
    <a routerLink="/">Volver al inicio</a>
  `,
})
export class NotFoundComponent {}
