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
  /** Ids de todas las sucursales asignadas al usuario (UsuarioSucursal), separados por coma. */
  sucursales: string;
  exp: number;
}

/** Sesión derivada del LoginResponse + claims del JWT, mantenida en memoria por AuthService */
export interface UsuarioActual {
  usuarioId: number;
  nombreCompleto: string;
  rut: string;
  rolId: number;
  /** Sucursal activa al momento de iniciar sesión (la primera asignada, regla del legacy). */
  sucursalActivaId: number;
  /** Todas las sucursales asignadas al usuario — la base del selector del menú. */
  sucursalesAsignadas: number[];
}
