using BCrypt.Net;
using OPT.Migracion;
using OPT.Migracion.Destino;
using OPT.Migracion.Legacy;

var opciones = ConexionOptions.DesdeArgumentos(args);
var ejecutar = args.Contains("--execute", StringComparer.OrdinalIgnoreCase);
var forzar = args.Contains("--force", StringComparer.OrdinalIgnoreCase);

Console.WriteLine("=== OPT.Migracion — Organización (Región/Comuna, Sucursal, Empresa, Rol, Usuario) + Clínico (Cliente, Anamnesis, RecetaCristales) ===");
Console.WriteLine(ejecutar ? "Modo: EJECUCIÓN REAL (--execute)" : "Modo: DRY-RUN (agregue --execute para escribir)");
Console.WriteLine();

var legacyRepo = new LegacyRepositorio(opciones.Legacy);
var destinoRepo = new DestinoRepositorio(opciones.Destino);

// ── 1. Lectura completa del legacy (solo lectura) ──────────────────────────
var regionesLegacy = await legacyRepo.ObtenerRegionesAsync();
var comunasLegacy = await legacyRepo.ObtenerComunasAsync();
var rolesLegacy = await legacyRepo.ObtenerRolesAsync();
var sucursalesLegacy = await legacyRepo.ObtenerSucursalesAsync();
var empresasLegacy = await legacyRepo.ObtenerEmpresasAsync();
var empresaSucursalesLegacy = await legacyRepo.ObtenerEmpresaSucursalesAsync();
var usuariosLegacy = await legacyRepo.ObtenerUsuariosAsync();
var usuarioSucursalesLegacy = await legacyRepo.ObtenerUsuarioSucursalesAsync();
var clientesLegacy = await legacyRepo.ObtenerClientesAsync();
var anamnesisLegacy = await legacyRepo.ObtenerAnamnesisAsync();
var recetasLegacy = await legacyRepo.ObtenerRecetasCristalesAsync();

Console.WriteLine($"Legacy leído: {regionesLegacy.Count} regiones, {comunasLegacy.Count} comunas, {rolesLegacy.Count} roles, " +
                   $"{sucursalesLegacy.Count} sucursales, {empresasLegacy.Count} empresas, {empresaSucursalesLegacy.Count} vínculos empresa-sucursal, " +
                   $"{usuariosLegacy.Count} usuarios, {usuarioSucursalesLegacy.Count} vínculos usuario-sucursal, " +
                   $"{clientesLegacy.Count} clientes, {anamnesisLegacy.Count} anamnesis, {recetasLegacy.Count} recetas de cristales.");
Console.WriteLine();

// ── 2. Verificación Región/Comuna (catálogo ya sembrado — solo confirma cobertura) ──
using (var conDestinoLectura = destinoRepo.Conectar())
{
    await conDestinoLectura.OpenAsync();
    var regionesDestino = await destinoRepo.ObtenerRegionesAsync(conDestinoLectura);
    var comunasDestino = await destinoRepo.ObtenerComunasAsync(conDestinoLectura);
    var rolesDestino = await destinoRepo.ObtenerRolesAsync(conDestinoLectura);

    var cobertura = CatalogoVerificador.Verificar(regionesLegacy, regionesDestino, comunasLegacy, comunasDestino);
    Console.WriteLine("--- Región/Comuna ---");
    Console.WriteLine("El catálogo nuevo ya viene sembrado (INE oficial, 16 regiones / 346 comunas) — no requiere migración de filas.");
    Console.WriteLine($"Cobertura por nombre: {cobertura.RegionesCubiertas}/{cobertura.RegionesLegacy} regiones, {cobertura.ComunasCubiertas}/{cobertura.ComunasLegacy} comunas del legacy.");
    if (cobertura.RegionesSinCobertura.Count > 0)
        Console.WriteLine($"  Regiones sin coincidencia exacta de nombre (probable variante ortográfica): {string.Join("; ", cobertura.RegionesSinCobertura)}");
    if (cobertura.ComunasSinCobertura.Count > 0)
        Console.WriteLine($"  Comunas sin coincidencia exacta de nombre: {cobertura.ComunasSinCobertura.Count} (detalle omitido).");
    Console.WriteLine();

    // ── 3. Mapeo de Rol por nombre ──────────────────────────────────────────
    var mapeoRoles = RolMapper.Construir(rolesLegacy, rolesDestino);
    Console.WriteLine("--- Rol ---");
    Console.WriteLine("Ya sembrado (8 filas: 3 genéricas + 6 heredadas del legacy) — no requiere migración de filas, solo mapeo de id legacy -> id nuevo:");
    foreach (var rol in rolesLegacy)
        Console.WriteLine($"  idRol legacy {rol.IdRol} ({rol.Rol}) -> RolId nuevo {(mapeoRoles.LegacyANuevo.TryGetValue(rol.IdRol, out var nid) ? nid : "SIN MAPEAR")}");
    if (mapeoRoles.SinMapear.Count > 0)
    {
        Console.WriteLine($"ABORTADO: roles legacy sin equivalente por nombre en destino: {string.Join("; ", mapeoRoles.SinMapear)}");
        return 1;
    }
    Console.WriteLine();

    // ── 4. Usuario "bootstrap" (autoreferencia de CreadoPor) ────────────────
    var legacyAdminRolId = rolesLegacy.FirstOrDefault(r => string.Equals(r.Rol.Trim(), "Administrador", StringComparison.OrdinalIgnoreCase))?.IdRol;
    if (legacyAdminRolId is null)
    {
        Console.WriteLine("ABORTADO: el legacy no tiene un rol 'Administrador' — no hay candidato para el usuario bootstrap (CreadoPor de auditoría).");
        return 1;
    }

    var administradores = usuariosLegacy.Where(u => u.IdRol == legacyAdminRolId).ToList();
    if (administradores.Count == 0)
    {
        Console.WriteLine("ABORTADO: no hay ningún OPT_Usuario legacy con rol Administrador — no hay candidato para el usuario bootstrap.");
        return 1;
    }

    // Algunas fechas de ingreso son centinela (1900-01-01, cuentas de sistema/prueba) — se excluyen
    // al elegir "el primer administrador real" salvo que sean la única opción disponible.
    var administradoresConFechaReal = administradores.Where(u => u.FechaIngreso.Year > 1950).ToList();
    var candidatos = administradoresConFechaReal.Count > 0 ? administradoresConFechaReal : administradores;
    var bootstrapLegacy = candidatos.MinBy(u => u.FechaIngreso)!;

    Console.WriteLine("--- Usuario bootstrap (auditoría CreadoPor) ---");
    Console.WriteLine($"Se usará como CreadoPor de todas las filas migradas (organización Y clínico): {bootstrapLegacy.RutUsuario} — {bootstrapLegacy.Nombre} (FechaIngreso legacy {bootstrapLegacy.FechaIngreso:yyyy-MM-dd}).");
    Console.WriteLine("Si Organización ya fue migrada en una sesión anterior, se reutiliza su Id ya existente en destino (búsqueda por RUT) en vez de volver a insertarlo.");
    Console.WriteLine();

    // ── 5. Resolución del conflicto de Sucursal Matriz (decisión confirmada con el usuario 2026-08-24) ──
    var sucursalesMatrizForzadasAFalse = sucursalesLegacy.Where(s => s.Matriz && s.IdSucursal != 1).ToList();
    Console.WriteLine("--- Sucursal ---");
    Console.WriteLine($"{sucursalesLegacy.Count} sucursales. Solo idSucursal=1 ('Casa Matriz') conserva EsMatriz=true.");
    foreach (var s in sucursalesMatrizForzadasAFalse)
        Console.WriteLine($"  idSucursal={s.IdSucursal} ('{s.Nombre}') tenía Matriz=1 en el legacy -> se fuerza EsMatriz=false (decisión confirmada: dato de carga erróneo, 1 vínculo empresa / 0 usuarios).");
    Console.WriteLine();

    // ── 6. Plan de Empresa/UsuarioSucursal/EmpresaSucursal (resumen) ────────
    Console.WriteLine("--- Empresa ---");
    Console.WriteLine($"{empresasLegacy.Count} empresas — RUT placeholder único 'SIN-RUT-{{idEmpresa}}' (el legacy no registra RUT de empresa).");
    Console.WriteLine();
    Console.WriteLine("--- Relaciones ---");
    Console.WriteLine($"{usuarioSucursalesLegacy.Count} vínculos Usuario-Sucursal, {empresaSucursalesLegacy.Count} vínculos Empresa-Sucursal.");
    Console.WriteLine();

    // ── 7. Mapeo de Comuna por nombre (para Cliente.ComunaId) ───────────────
    var mapeoComunasCliente = ComunaMapper.Construir(comunasLegacy, comunasDestino);
    var clientesConComunaValida = clientesLegacy.Count(c => c.IdComuna is int idc && idc != 0);
    var clientesConComunaMapeada = clientesLegacy.Count(c => c.IdComuna is int idc && idc != 0 && mapeoComunasCliente.LegacyANuevo.ContainsKey(idc));
    Console.WriteLine("--- Cliente: mapeo de Comuna ---");
    Console.WriteLine($"{clientesConComunaMapeada}/{clientesConComunaValida} clientes con idComuna válido logran mapear a un ComunaId nuevo por nombre (el resto queda con ComunaId=NULL).");
    if (mapeoComunasCliente.SinMapear.Count > 0)
        Console.WriteLine($"  Comunas legacy sin match de nombre: {mapeoComunasCliente.SinMapear.Count} (mismo hallazgo ya documentado — variantes ortográficas menores).");
    Console.WriteLine();

    // ── 8. Cliente: RUT — reporte de variantes de formato (decisión 2026-08-21: migrar tal cual) ──
    var gruposFormatoSospechoso = clientesLegacy
        .GroupBy(c => c.RutCliente.Replace("-", "").Replace(".", "").Trim().ToUpperInvariant())
        .Where(g => g.Count() > 1)
        .ToList();
    Console.WriteLine("--- Cliente: RUT con variantes de formato (posible mismo cliente duplicado) ---");
    Console.WriteLine($"{gruposFormatoSospechoso.Count} grupos de RUT que coinciden si se ignoran guiones/puntos — se migran tal cual (sin fusión automática), reporte para revisión manual posterior:");
    foreach (var g in gruposFormatoSospechoso.Take(20))
        Console.WriteLine($"  {string.Join(" / ", g.Select(c => c.RutCliente))}");
    if (gruposFormatoSospechoso.Count > 20)
        Console.WriteLine($"  ... y {gruposFormatoSospechoso.Count - 20} grupos más (omitidos).");
    Console.WriteLine();

    // Chequeo distinto y BLOQUEANTE: duplicado exacto tras la normalización real que se aplica
    // (Trim + ToUpperInvariant, igual que Usuario) — esto sí violaría UQ_Clientes_Rut si existiera.
    var gruposDuplicadosExactos = clientesLegacy
        .GroupBy(c => Normalizacion.NormalizarRut(c.RutCliente))
        .Where(g => g.Count() > 1)
        .ToList();
    if (gruposDuplicadosExactos.Count > 0)
    {
        Console.WriteLine("ABORTADO: hay RUT que colisionan exactamente tras normalizar (Trim+MAYÚSCULAS) — violarían la UNIQUE de OPT_Cliente.Rut:");
        foreach (var g in gruposDuplicadosExactos)
            Console.WriteLine($"  {string.Join(" / ", g.Select(c => c.RutCliente))}");
        return 1;
    }

    // ── 9. RecetaCristales: parseo de valores (reporte de excepciones) ──────
    var excepcionesParseo = new List<RecetaCristalesParser.Excepcion>();
    foreach (var r in recetasLegacy)
        ParsearCamposReceta(r, excepcionesParseo);

    Console.WriteLine("--- RecetaCristales: parseo de esfera/cilindro/eje ---");
    Console.WriteLine("Formato legacy inconsistente: a veces con punto decimal (\"+1.25\"), a veces dígitos puros que representan centésimas (\"+125\" = 1.25). Se maneja ambos; lo que no calza queda NULL y se reporta:");
    Console.WriteLine($"  {excepcionesParseo.Count} valores no parseables de {recetasLegacy.Count * 12} campos numéricos evaluados (12 campos esfera/cilindro/eje por receta).");
    foreach (var ex in excepcionesParseo.Take(15))
        Console.WriteLine($"  idRecetaCristales={ex.IdRecetaCristales}, campo={ex.Campo}, valor crudo=\"{ex.ValorCrudo}\"");
    if (excepcionesParseo.Count > 15)
        Console.WriteLine($"  ... y {excepcionesParseo.Count - 15} más (omitidas).");
    Console.WriteLine("DP (distancia pupilar) y ADD (adición) se preservan como texto libre (columnas nuevas DpLejos/DpCerca/AddLejos) — no se parsean como número porque el legacy las registra en formatos compuestos (ej. \"58-56\", \"+150/+200\").");
    Console.WriteLine();

    // ── 10. Cliente: FechaNacimiento fuera de rango plausible ───────────────
    var fechaNacimientoDescartada = clientesLegacy.Count(c => c.FechaNacimiento.HasValue && !Normalizacion.EsFechaNacimientoPlausible(c.FechaNacimiento.Value));
    Console.WriteLine("--- Cliente: FechaNacimiento ---");
    Console.WriteLine($"{clientesLegacy.Count(c => c.FechaNacimiento.HasValue)} clientes con FechaNacimiento en el legacy; {fechaNacimientoDescartada} fuera de un rango plausible (antes de 1900 o en el futuro, ej. centinela 0001-01-01 o error de digitación como 2049) — se migran como NULL en vez de un dato erróneo.");
    Console.WriteLine();

    // ── 11. Anamnesis: CreadoPor ─────────────────────────────────────────────
    Console.WriteLine("--- Anamnesis: CreadoPor ---");
    Console.WriteLine("Decisión confirmada: se usa siempre el Usuario bootstrap (mismo criterio uniforme que Sucursal/Empresa/Usuario), no se mapea RutUsuario real pese a estar disponible en el legacy.");
    Console.WriteLine();

    if (!ejecutar)
    {
        Console.WriteLine("Dry-run completo. Nada fue escrito en destino. Ejecute con --execute para aplicar los cambios.");
        return 0;
    }

    // ── 12. Guardia de re-ejecución, por grupo independiente ────────────────
    var tablasOrganizacion = new[] { "OPT_Usuario", "OPT_Sucursal", "OPT_Empresa", "OPT_UsuarioSucursal", "OPT_EmpresaSucursal" };
    var tablasClinico = new[] { "OPT_Cliente", "OPT_Anamnesis", "OPT_RecetaCristales" };

    var conteosOrganizacion = new Dictionary<string, int>();
    foreach (var tabla in tablasOrganizacion)
        conteosOrganizacion[tabla] = await destinoRepo.ContarFilasAsync(conDestinoLectura, tabla);

    var conteosClinico = new Dictionary<string, int>();
    foreach (var tabla in tablasClinico)
        conteosClinico[tabla] = await destinoRepo.ContarFilasAsync(conDestinoLectura, tabla);

    var organizacionYaMigrada = conteosOrganizacion.Values.Any(c => c > 0);
    var clinicoYaMigrado = conteosClinico.Values.Any(c => c > 0);
    var migrarOrganizacion = !organizacionYaMigrada || forzar;
    var migrarClinico = !clinicoYaMigrado || forzar;

    if (organizacionYaMigrada && !forzar)
    {
        Console.WriteLine("Organización (Usuario/Sucursal/Empresa/...) ya tiene datos en destino — se omite su inserción (usar --force para reintentar):");
        foreach (var (tabla, count) in conteosOrganizacion.Where(kv => kv.Value > 0))
            Console.WriteLine($"  {tabla}: {count} filas");
    }

    if (clinicoYaMigrado && !forzar)
    {
        Console.WriteLine("ABORTADO: Cliente/Anamnesis/RecetaCristales ya tienen datos en destino — evita duplicar por una re-ejecución accidental:");
        foreach (var (tabla, count) in conteosClinico.Where(kv => kv.Value > 0))
            Console.WriteLine($"  {tabla}: {count} filas");
        Console.WriteLine("Si es intencional, vuelva a ejecutar con --force.");
        return 1;
    }

    // ── 13. Ejecución real, todo en una transacción ──────────────────────────
    using var conEscritura = destinoRepo.Conectar();
    await conEscritura.OpenAsync();
    using var tx = conEscritura.BeginTransaction();
    try
    {
        int bootstrapId;
        var mapaUsuarios = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var mapaSucursales = new Dictionary<int, int>();
        var mapaEmpresas = new Dictionary<int, int>();

        if (migrarOrganizacion)
        {
            // 13.1 Usuario bootstrap primero (CreadoPor temporal, luego autoreferencia)
            bootstrapId = await InsertarUsuarioAsync(destinoRepo, conEscritura, tx, bootstrapLegacy, mapeoRoles.LegacyANuevo, creadoPor: 0);
            await destinoRepo.ActualizarCreadoPorPropioAsync(conEscritura, tx, bootstrapId);
            mapaUsuarios[bootstrapLegacy.RutUsuario] = bootstrapId;

            // 13.2 Resto de usuarios
            foreach (var u in usuariosLegacy.Where(u => u.RutUsuario != bootstrapLegacy.RutUsuario))
            {
                var nuevoId = await InsertarUsuarioAsync(destinoRepo, conEscritura, tx, u, mapeoRoles.LegacyANuevo, creadoPor: bootstrapId);
                mapaUsuarios[u.RutUsuario] = nuevoId;
            }
            Console.WriteLine($"Usuarios insertados: {mapaUsuarios.Count}");

            // 13.3 Sucursales
            foreach (var s in sucursalesLegacy)
            {
                var nueva = new NuevaSucursal(
                    s.IdSucursal, s.Nombre.Trim(), s.Direccion.Trim(), s.Telefono.Trim(),
                    EsMatriz: s.IdSucursal == 1,
                    CreadoEn: new DateTimeOffset(s.FechaRegistro, TimeSpan.Zero));
                var nuevoId = await destinoRepo.InsertarSucursalAsync(conEscritura, tx, nueva, bootstrapId);
                mapaSucursales[s.IdSucursal] = nuevoId;
            }
            Console.WriteLine($"Sucursales insertadas: {mapaSucursales.Count}");

            // 13.4 Empresas
            foreach (var e in empresasLegacy)
            {
                var nombre = e.Empresa.Trim();
                var nueva = new NuevaEmpresa(
                    e.IdEmpresa, nombre, Rut: $"SIN-RUT-{e.IdEmpresa}", RazonSocial: nombre,
                    Direccion: (e.Direccion ?? string.Empty).Trim(),
                    Telefono: (e.Telefono ?? string.Empty).Trim(),
                    Contacto: (e.Contacto ?? string.Empty).Trim(),
                    CreadoEn: DateTimeOffset.UtcNow);
                var nuevoId = await destinoRepo.InsertarEmpresaAsync(conEscritura, tx, nueva, bootstrapId);
                mapaEmpresas[e.IdEmpresa] = nuevoId;
            }
            Console.WriteLine($"Empresas insertadas: {mapaEmpresas.Count}");

            // 13.5 UsuarioSucursal
            var usuarioSucursalInsertados = 0;
            foreach (var us in usuarioSucursalesLegacy)
            {
                if (!mapaUsuarios.TryGetValue(us.RutUsuario, out var usuarioId) || !mapaSucursales.TryGetValue(us.IdSucursal, out var sucursalId))
                {
                    Console.WriteLine($"  ADVERTENCIA: se omite vínculo Usuario-Sucursal huérfano (RutUsuario={us.RutUsuario}, idSucursal={us.IdSucursal}).");
                    continue;
                }
                await destinoRepo.InsertarUsuarioSucursalAsync(conEscritura, tx, usuarioId, sucursalId);
                usuarioSucursalInsertados++;
            }
            Console.WriteLine($"Vínculos Usuario-Sucursal insertados: {usuarioSucursalInsertados}");

            // 13.6 EmpresaSucursal
            var empresaSucursalInsertados = 0;
            foreach (var es in empresaSucursalesLegacy)
            {
                if (!mapaEmpresas.TryGetValue(es.IdEmpresa, out var empresaId) || !mapaSucursales.TryGetValue(es.IdSucursal, out var sucursalId))
                {
                    Console.WriteLine($"  ADVERTENCIA: se omite vínculo Empresa-Sucursal huérfano (idEmpresa={es.IdEmpresa}, idSucursal={es.IdSucursal}).");
                    continue;
                }
                await destinoRepo.InsertarEmpresaSucursalAsync(conEscritura, tx, empresaId, sucursalId);
                empresaSucursalInsertados++;
            }
            Console.WriteLine($"Vínculos Empresa-Sucursal insertados: {empresaSucursalInsertados}");
        }
        else
        {
            var rutBootstrapNormalizado = Normalizacion.NormalizarRut(bootstrapLegacy.RutUsuario);
            var idExistente = await destinoRepo.ObtenerUsuarioIdPorRutAsync(conEscritura, tx, rutBootstrapNormalizado);
            if (idExistente is null)
            {
                Console.WriteLine($"ABORTADO: Organización ya está migrada pero no se encontró el Usuario bootstrap esperado (Rut={rutBootstrapNormalizado}) en destino.");
                await tx.RollbackAsync();
                return 1;
            }
            bootstrapId = idExistente.Value;
            Console.WriteLine($"Organización ya migrada — se reutiliza Usuario bootstrap existente (UsuarioId={bootstrapId}).");
        }

        if (migrarClinico)
        {
            // 13.7 Cliente
            var mapaClientes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in clientesLegacy)
            {
                var nombreCompleto = string.IsNullOrWhiteSpace(c.Nombre) ? "SIN NOMBRE" : c.Nombre;
                var (nombre, apellido) = Normalizacion.DividirNombreCompleto(nombreCompleto);

                var comunaId = c.IdComuna is int idc && idc != 0 && mapeoComunasCliente.LegacyANuevo.TryGetValue(idc, out var nid)
                    ? nid
                    : (int?)null;

                var fechaNacimiento = c.FechaNacimiento.HasValue && Normalizacion.EsFechaNacimientoPlausible(c.FechaNacimiento.Value)
                    ? DateOnly.FromDateTime(c.FechaNacimiento.Value)
                    : (DateOnly?)null;

                var creadoEn = c.FechaIngreso.HasValue
                    ? new DateTimeOffset(c.FechaIngreso.Value, TimeSpan.Zero)
                    : DateTimeOffset.UtcNow;

                var nuevo = new NuevoCliente(
                    RutLegacy: c.RutCliente,
                    Rut: Normalizacion.NormalizarRut(c.RutCliente),
                    Nombre: nombre,
                    Apellido: apellido,
                    Email: string.IsNullOrWhiteSpace(c.Mail) ? null : Normalizacion.NormalizarEmail(c.Mail),
                    Telefono: string.IsNullOrWhiteSpace(c.Celular) ? null : c.Celular.Trim(),
                    Direccion: string.IsNullOrWhiteSpace(c.Direccion) ? null : c.Direccion.Trim(),
                    ComunaId: comunaId,
                    FechaNacimiento: fechaNacimiento,
                    TipoPrevision: string.IsNullOrWhiteSpace(c.TipoPrevision) ? null : c.TipoPrevision.Trim(),
                    CreadoEn: creadoEn);

                var nuevoId = await destinoRepo.InsertarClienteAsync(conEscritura, tx, nuevo, bootstrapId);
                mapaClientes[c.RutCliente] = nuevoId;
            }
            Console.WriteLine($"Clientes insertados: {mapaClientes.Count}");

            // 13.8 Anamnesis
            var anamnesisInsertadas = 0;
            foreach (var a in anamnesisLegacy)
            {
                if (!mapaClientes.TryGetValue(a.RutCliente, out var clienteId))
                {
                    Console.WriteLine($"  ADVERTENCIA: se omite Anamnesis huérfana (idAnamnesis={a.IdAnamnesis}, RutCliente={a.RutCliente} sin Cliente migrado).");
                    continue;
                }

                var nueva = new NuevaAnamnesis(
                    ClienteId: clienteId,
                    Hipertension: a.Hipertension,
                    Diabetes: a.Diabetes,
                    Alergias: a.Alergias,
                    UsaLentesPrevio: a.Lentes,
                    Observaciones: RecetaCristalesParser.LimpiarTexto(a.Observacion),
                    CreadoEn: new DateTimeOffset(a.FechaIngreso, TimeSpan.Zero));

                await destinoRepo.InsertarAnamnesisAsync(conEscritura, tx, nueva, bootstrapId);
                anamnesisInsertadas++;
            }
            Console.WriteLine($"Anamnesis insertadas: {anamnesisInsertadas}");

            // 13.9 RecetaCristales
            var recetasInsertadas = 0;
            var excepcionesInsercion = new List<RecetaCristalesParser.Excepcion>();
            foreach (var r in recetasLegacy)
            {
                if (r.RutCliente is null || !mapaClientes.TryGetValue(r.RutCliente, out var clienteId))
                {
                    Console.WriteLine($"  ADVERTENCIA: se omite RecetaCristales huérfana (idRecetaCristales={r.IdRecetaCristales}, RutCliente={r.RutCliente ?? "NULL"} sin Cliente migrado).");
                    continue;
                }

                var campos = ParsearCamposReceta(r, excepcionesInsercion);
                var observaciones = RecetaCristalesParser.CombinarObservaciones(
                    ("Lejos OD", r.LejosODObservacion), ("Lejos OI", r.LejosOIObservacion),
                    ("Cerca OD", r.CercaODObservacion), ("Cerca OI", r.CercaOIObservacion),
                    ("DP Lejos", r.LejosDPObservacion), ("DP Cerca", r.CercaDPObservacion));
                if (observaciones is { Length: > 500 })
                    observaciones = observaciones[..500];

                var creadoEn = r.FechaIngreso.HasValue
                    ? new DateTimeOffset(r.FechaIngreso.Value, TimeSpan.Zero)
                    : DateTimeOffset.UtcNow;

                var nueva = new NuevaRecetaCristales(
                    ClienteId: clienteId,
                    OdEsferaLejos: campos.OdEsferaLejos, OdCilindroLejos: campos.OdCilindroLejos, OdEjeLejos: campos.OdEjeLejos,
                    OdEsferaCerca: campos.OdEsferaCerca, OdCilindroCerca: campos.OdCilindroCerca, OdEjeCerca: campos.OdEjeCerca,
                    OiEsferaLejos: campos.OiEsferaLejos, OiCilindroLejos: campos.OiCilindroLejos, OiEjeLejos: campos.OiEjeLejos,
                    OiEsferaCerca: campos.OiEsferaCerca, OiCilindroCerca: campos.OiCilindroCerca, OiEjeCerca: campos.OiEjeCerca,
                    Urgente: r.CheckUrgente, RequiereLab: r.CheckCristalesLaboratorio,
                    Observaciones: observaciones,
                    DpLejos: RecetaCristalesParser.LimpiarTexto(r.LejosDPEsferico),
                    DpCerca: RecetaCristalesParser.LimpiarTexto(r.CercaDPEsferico),
                    AddLejos: RecetaCristalesParser.LimpiarTexto(r.LejosADDEsfera),
                    CreadoEn: creadoEn);

                await destinoRepo.InsertarRecetaCristalesAsync(conEscritura, tx, nueva, bootstrapId);
                recetasInsertadas++;
            }
            Console.WriteLine($"RecetaCristales insertadas: {recetasInsertadas}");
        }

        await tx.CommitAsync();
        Console.WriteLine();
        Console.WriteLine("COMMIT exitoso. Migración aplicada.");
    }
    catch
    {
        await tx.RollbackAsync();
        Console.WriteLine();
        Console.WriteLine("ERROR — se hizo ROLLBACK completo, ningún cambio quedó aplicado.");
        throw;
    }
}

return 0;

static async Task<int> InsertarUsuarioAsync(
    DestinoRepositorio repo, Microsoft.Data.SqlClient.SqlConnection con, Microsoft.Data.SqlClient.SqlTransaction tx,
    LegacyUsuario u, IReadOnlyDictionary<int, int> mapeoRoles, int creadoPor)
{
    var (nombre, apellido) = Normalizacion.DividirNombreCompleto(u.Nombre);
    var rolId = u.IdRol.HasValue && mapeoRoles.TryGetValue(u.IdRol.Value, out var rid)
        ? rid
        : mapeoRoles.Values.Max(); // fallback defensivo — no se observó ningún caso en los datos reales (13/13 usuarios tienen idRol asignado y mapeable)

    var nuevo = new NuevoUsuario(
        RutLegacy: u.RutUsuario,
        Rut: Normalizacion.NormalizarRut(u.RutUsuario),
        Nombre: nombre,
        Apellido: apellido,
        Email: Normalizacion.NormalizarEmail(u.Mail),
        ClaveHash: BCrypt.Net.BCrypt.HashPassword(u.Clave, workFactor: 12),
        RolId: rolId,
        CreadoEn: new DateTimeOffset(u.FechaIngreso, TimeSpan.Zero));

    return await repo.InsertarUsuarioAsync(con, tx, nuevo, creadoPor);
}

static RecetaCamposParseados ParsearCamposReceta(LegacyRecetaCristales r, List<RecetaCristalesParser.Excepcion> excepciones) => new(
    OdEsferaLejos: RecetaCristalesParser.ParsearGraduacion(r.LejosODEsferico, r.IdRecetaCristales, "LejosODEsferico", excepciones),
    OdCilindroLejos: RecetaCristalesParser.ParsearGraduacion(r.LejosODCilindro, r.IdRecetaCristales, "LejosODCilindro", excepciones),
    OdEjeLejos: RecetaCristalesParser.ParsearEje(r.LejosODEje, r.IdRecetaCristales, "LejosODEje", excepciones),
    OdEsferaCerca: RecetaCristalesParser.ParsearGraduacion(r.CercaODEsferico, r.IdRecetaCristales, "CercaODEsferico", excepciones),
    OdCilindroCerca: RecetaCristalesParser.ParsearGraduacion(r.CercaODCilindro, r.IdRecetaCristales, "CercaODCilindro", excepciones),
    OdEjeCerca: RecetaCristalesParser.ParsearEje(r.CercaODEje, r.IdRecetaCristales, "CercaODEje", excepciones),
    OiEsferaLejos: RecetaCristalesParser.ParsearGraduacion(r.LejosOIEsferico, r.IdRecetaCristales, "LejosOIEsferico", excepciones),
    OiCilindroLejos: RecetaCristalesParser.ParsearGraduacion(r.LejosOICilindro, r.IdRecetaCristales, "LejosOICilindro", excepciones),
    OiEjeLejos: RecetaCristalesParser.ParsearEje(r.LejosOIEje, r.IdRecetaCristales, "LejosOIEje", excepciones),
    OiEsferaCerca: RecetaCristalesParser.ParsearGraduacion(r.CercaOIEsferico, r.IdRecetaCristales, "CercaOIEsferico", excepciones),
    OiCilindroCerca: RecetaCristalesParser.ParsearGraduacion(r.CercaOICilindro, r.IdRecetaCristales, "CercaOICilindro", excepciones),
    OiEjeCerca: RecetaCristalesParser.ParsearEje(r.CercaOIEje, r.IdRecetaCristales, "CercaOIEje", excepciones));

internal sealed record RecetaCamposParseados(
    decimal? OdEsferaLejos, decimal? OdCilindroLejos, int? OdEjeLejos,
    decimal? OdEsferaCerca, decimal? OdCilindroCerca, int? OdEjeCerca,
    decimal? OiEsferaLejos, decimal? OiCilindroLejos, int? OiEjeLejos,
    decimal? OiEsferaCerca, decimal? OiCilindroCerca, int? OiEjeCerca);
