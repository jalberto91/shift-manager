import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { toApiError } from './api-error';
import { messageForError } from './error-messages';
import { NotificationService } from './notification.service';

// Los 4xx de negocio los resuelve cada pantalla por código; aquí solo avisan los fallos globales.
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);
  return next(req).pipe(
    catchError((response: HttpErrorResponse) => {
      const error = toApiError(response);
      if (error.status === 0 || error.status >= 500) {
        notifications.error(messageForError(error), error.traceId);
      }
      return throwError(() => error);
    }),
  );
};
