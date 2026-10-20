import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-data-state',
  imports: [MessageModule, ProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <p-progress-spinner ariaLabel="Cargando" strokeWidth="4" />
    } @else if (error(); as message) {
      <p-message severity="error">{{ message }}</p-message>
    } @else if (empty()) {
      <p>{{ emptyMessage() }}</p>
    } @else {
      <ng-content />
    }
  `,
})
export class DataStateComponent {
  readonly loading = input(false);
  readonly error = input<string | null>(null);
  readonly empty = input(false);
  readonly emptyMessage = input('No hay datos para mostrar.');
}
