/**
 * Forma que produce ExceptionHandlingMiddleware (OPT.API) para toda respuesta de error
 * (content-type application/problem+json, RFC 7807). `errores` solo viene presente
 * cuando la excepción de origen fue una ValidationException (400).
 */
export interface ApiProblemDetails {
  status: number;
  title: string;
  extensions?: {
    errores?: Record<string, string[]>;
  };
}
