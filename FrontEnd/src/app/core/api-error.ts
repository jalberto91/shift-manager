import { HttpErrorResponse } from '@angular/common/http';

export interface ApiError {
  status: number;
  codes: string[];
  traceId?: string;
  conflictingShiftId?: string;
}

interface ProblemDetails {
  errors?: Record<string, string[]>;
  traceId?: string;
  conflictingShiftId?: string;
}

export function toApiError(response: HttpErrorResponse): ApiError {
  const body = (response.error ?? {}) as ProblemDetails;
  return {
    status: response.status,
    codes: Object.keys(body.errors ?? {}),
    traceId: body.traceId,
    conflictingShiftId: body.conflictingShiftId,
  };
}
