# Integración de Fase 4 — lectura antes de probar

## Base y aportaciones utilizadas

| Fuente | Aportaciones integradas | Tratamiento |
|---|---|---|
| `Integracion_fase4` | Estructura MVC, entidades y persistencia EF Core, servicios/repositorios, módulos operativos, pagos/informes y suite `PruebasPago` | Se conserva como base. |
| `Branch_Crystel` | ASP.NET Identity, adaptación de Usuario y DbContext, migración Identity, controladores de autenticación/usuarios, DTOs, API protegida, Swagger y JWT | Integrado y adaptado para compartir el servicio de emisión JWT y mapear los controladores API. |
| `Samuel` | Implementación de API REST y documentación de Swagger | Se utilizó para contrastar la API. Sus endpoints estaban marcados `[AllowAnonymous]`; se prefirió la versión protegida por JWT de Crystel. |
| `Branch_Rodrigo` | Interacción asíncrona de Inventario mediante Fetch API y formularios conectados a `/api/materiales` | Se integró en las tres vistas de Inventario y en `wwwroot/js/inventario.js`, añadiendo antifalsificación y renovación de JWT. |

## Cambios de adaptación importantes

1. Se integró `IdentityDbContext<Usuario, IdentityRole<int>, int>` y la migración `20261126034518_AgregarIdentity` de Crystel. La propiedad de dominio `Correo` queda como alias `[NotMapped]` de `Email`, por lo que las consultas LINQ usan `Email`; `CotizacionRepository` también compara el usuario por `Id`.
2. Se mantuvieron las reglas y relaciones originales de Fase 3, incluyendo el índice filtrado único `IX_Pagos_AnticipoPorOrden` (`[TipoPago] = 0`), el índice de pagos por orden/fecha y el índice único de `CodigoPago`.
3. Se centralizó la emisión JWT en `JwtTokenService`. El inicio de sesión web conserva la cookie Identity y añade una cookie JWT `HttpOnly` para la API. El token también puede obtenerse con `POST /api/auth/token` para clientes externos.
4. Se añadió `app.MapControllers()`; sin ese mapeo los endpoints con atributos de ruta no quedarían publicados.
5. Se añadió `ApiAntiforgeryFilter`: las operaciones API realizadas desde el navegador con JWT en cookie exigen `RequestVerificationToken`; los clientes externos que envían `Authorization: Bearer` no dependen de cookies y no pasan por esa validación CSRF.
6. La clave de desarrollo se genera y guarda localmente en `.local/jwt-development.key`, no en `appsettings.json`. El archivo se ignora en Git. En producción la clave debe configurarse como secreto.
7. Se conservó la configuración de conexión SQL Server y la localización `en-US` de la base.
8. Se mantuvieron las pantallas dinámicas de la base. No se copiaron controladores/vistas de Rodrigo que reemplazaban persistencia real por colecciones de demostración o usaban el namespace alternativo `RotulosRanibal.Controllers`. Tampoco se mezclaron los DTOs planos de Materiales de Rodrigo con los DTOs agrupados en `DTOs/Materiales`, porque declaraban los mismos tipos y generarían duplicados.

## Antes de la primera ejecución

- Haz respaldo de `DAS_Proyecto_DB`.
- Confirma que el usuario existente tenga correo y `PasswordHash`; la migración valida esos datos antes de convertir la tabla de usuarios a Identity.
- Ejecuta restore/build y luego inicia la aplicación. El inicio aplica automáticamente las migraciones pendientes. Si la migración falla, revisa el error de SQL y no borres manualmente tablas de Identity.
- Si la base está vacía y no tiene usuarios, configura `InitialAdmin:Email` y `InitialAdmin:Password` con User Secrets antes del inicio. Las cuentas existentes de Fase 3 no se reemplazan.
- Después de la primera autenticación, revisa API/Swagger y sigue la matriz de pruebas. Las 32 pruebas de pagos previas deben repetirse para esta versión.

## Limitaciones que requieren prueba en el equipo

- No se pudo ejecutar compilación ni pruebas en este entorno de preparación porque no está instalado el SDK `dotnet`. El proyecto se preparó mediante análisis estático de las cuatro ramas, comparación de archivos y validación estructural; no debe presentarse como compilado o probado aquí.
- La migración de Identity debe ejecutarse contra una copia de respaldo de la base local para comprobar que los datos y los hashes actuales se conservan y permiten iniciar sesión.
- El despliegue en IIS o Azure, las pruebas de API conectadas a SQL Server y la grabación de video necesitan evidencias obtenidas en el equipo/entorno del grupo.
