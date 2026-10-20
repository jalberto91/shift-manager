import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly messages = inject(MessageService);

  success(detail: string): void {
    this.messages.add({ severity: 'success', summary: 'Listo', detail });
  }

  error(detail: string, traceId?: string): void {
    this.messages.add({
      severity: 'error',
      summary: 'Error',
      detail: traceId ? `${detail} (referencia: ${traceId})` : detail,
      life: 8000,
    });
  }
}
