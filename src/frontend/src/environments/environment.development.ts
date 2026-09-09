export const environment = {
  production: false,
  // Puerto de OPT.API al correr `dotnet run` en local (ver la URL que imprime la consola
  // o https://localhost:{puerto}/swagger). Cors:Origins en appsettings.Development.json
  // ya incluye http://localhost:4200 (puerto por defecto de `ng serve`).
  apiUrl: 'https://localhost:63595/api',
};
