/**
 * Decodifica el payload de un JWT (sin verificar la firma — eso ya lo hizo el backend
 * al emitirlo). Evita agregar una dependencia externa para un caso de uso de una línea.
 */
export function decodeJwtPayload<T>(token: string): T {
  const payloadBase64Url = token.split('.')[1];
  const payloadBase64 = payloadBase64Url.replace(/-/g, '+').replace(/_/g, '/');
  const payloadJson = decodeURIComponent(
    atob(payloadBase64)
      .split('')
      .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
      .join(''),
  );
  return JSON.parse(payloadJson) as T;
}
