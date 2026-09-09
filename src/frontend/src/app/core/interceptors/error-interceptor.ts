import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { Auth } from '../services/auth';
import { ApiProblemDetails } from '../../shared/models/api-problem-details';
import { Toast } from '../../shared/services/toast';

/**
 * Traduce las respuestas de error de la API (ProblemDetails, ver ExceptionHandlingMiddleware)
 * en notificaciones al usuario y maneja el caso 401 cerrando la sesión.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const router = inject(Router);
  const toast = inject(Toast);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          auth.logout();
          router.navigateByUrl('/login');
        } else {
          const problema = error.error as ApiProblemDetails | undefined;
          toast.error(problema?.title ?? 'Ocurrió un error inesperado. Intente nuevamente.');
        }
      }

      return throwError(() => error);
    }),
  );
};
