/** Contrato exacto de OPT.Application/Features/Auth/Commands/Login/LoginCommand.cs */
export interface LoginRequest {
  rut: string;
  clave: string;
}

/** Contrato exacto de LoginResult (record) devuelto por POST /api/auth/login */
export interface LoginResponse {
  token: string;
  usuarioId: number;
  nombreCompleto: string;
  sucursalActivaId: number;
}

/** Claims propios incluidos en el JWT (ver TokenService.GenerarToken) */
export interface JwtClaims {
  sub: string;
  rut: string;
  nombre: string;
  rolId: string;
  sucursalId: string;
  exp: number;
}

/** Sesión derivada del LoginResponse + claims del JWT, mantenida en memoria por AuthService */
export interface UsuarioActual {
  usuarioId: number;
  nombreCompleto: string;
  rut: string;
  rolId: number;
  sucursalActivaId: number;
}
