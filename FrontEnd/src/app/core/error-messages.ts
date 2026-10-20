import { ApiError } from './api-error';

const MESSAGES: Record<string, string> = {
  'collaborator.name': 'El nombre es obligatorio y no puede superar los 200 caracteres.',
  'collaborator.email': 'El email no es válido.',
  'collaborator.country': 'El país debe ser un código de dos letras, por ejemplo ES.',
  'collaborator.country-unknown': 'El país no está soportado.',
  'collaborator.email-taken': 'Ya existe un colaborador con ese email.',
  'collaborator.not-found': 'El colaborador no existe.',
  'shift.date': 'La fecha es obligatoria.',
  'shift.time-range': 'La hora de fin debe ser posterior a la de inicio.',
  'shift.collaborator-unavailable': 'El colaborador no existe o está dado de baja.',
  'shift.overlap': 'El turno se solapa con otro del mismo colaborador.',
  'shift.date-range': 'La fecha "desde" no puede ser posterior a "hasta".',
  'shift.not-found': 'El turno no existe.',
  'shift.import.file': 'El archivo no es un .xlsx válido.',
  'shift.import.file-too-large': 'El archivo supera el máximo de 5 MB.',
  'shift.import.columns': 'Faltan columnas obligatorias: Email, Fecha, HoraInicio y HoraFin.',
};

const GENERIC = 'La solicitud no es válida.';
const SERVER = 'Ocurrió un error inesperado. Intentá de nuevo más tarde.';
const OFFLINE = 'No se pudo conectar con el servidor.';

export function messageFor(code: string): string {
  return MESSAGES[code] ?? GENERIC;
}

export function messageForError(error: ApiError): string {
  if (error.status === 0) return OFFLINE;
  if (error.status === 413) return MESSAGES['shift.import.file-too-large'];
  if (error.status >= 500) return SERVER;
  return error.codes.length > 0 ? messageFor(error.codes[0]) : GENERIC;
}
