import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { Auth } from '../services/auth';

/** Agrega el Bearer token a toda request dirigida a la API del backend. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const token = auth.obtenerToken();

  if (!token) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    }),
  );
};
