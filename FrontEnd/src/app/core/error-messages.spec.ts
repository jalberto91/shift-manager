import { HttpErrorResponse } from '@angular/common/http';
import { toApiError } from './api-error';
import { messageFor, messageForError } from './error-messages';

describe('toApiError', () => {
  it('extrae códigos, traceId y conflictingShiftId del Problem Details', () => {
    const response = new HttpErrorResponse({
      status: 409,
      error: {
        errors: { 'shift.overlap': ['The shift overlaps another one'] },
        traceId: 'abc',
        conflictingShiftId: 'id-1',
      },
    });

    expect(toApiError(response)).toEqual({
      status: 409,
      codes: ['shift.overlap'],
      traceId: 'abc',
      conflictingShiftId: 'id-1',
    });
  });

  it('tolera una respuesta sin cuerpo', () => {
    const error = toApiError(new HttpErrorResponse({ status: 0 }));

    expect(error.codes).toEqual([]);
  });
});

describe('messageForError', () => {
  it('traduce el primer código de negocio a un texto en español', () => {
    const message = messageForError({ status: 409, codes: ['shift.overlap'] });

    expect(message).toBe(messageFor('shift.overlap'));
  });

  it('usa un texto genérico para un código desconocido', () => {
    const message = messageForError({ status: 400, codes: ['$.startTime'] });

    expect(message).toBe('La solicitud no es válida.');
  });

  it('no filtra el detalle de un 500', () => {
    const message = messageForError({ status: 500, codes: [] });

    expect(message).toContain('error inesperado');
  });
});
