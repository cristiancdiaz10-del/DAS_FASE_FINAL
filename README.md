# Sistema de gestión Rótulos Ranibal — Fase 4

Proyecto ASP.NET Core MVC, C#, .NET 10, Entity Framework Core y SQL Server. Esta versión integra la base funcional `Integracion_fase4` con la autenticación Identity/JWT y la API REST de Crystel, además de la interacción asíncrona del inventario aportada por Rodrigo.

## Requisitos

- .NET SDK 10.0.
- Visual Studio 2022 actualizado o Visual Studio Code.
- SQL Server LocalDB o una instancia de SQL Server accesible.
- La base `DAS_Proyecto_DB` configurada en `appsettings.json` (por defecto usa LocalDB).

## Inicio rápido

1. Descomprime el proyecto y abre `DAS Proyecto.slnx` o `DAS Proyecto.csproj`.
2. Revisa `ConnectionStrings:DefaultConnection` en `appsettings.json`.
3. Antes del primer inicio de esta versión, realiza un respaldo de la base de datos. El arranque ejecuta `Database.MigrateAsync()` y aplicará la nueva migración `AgregarIdentity` junto con cualquier migración pendiente.
4. En una terminal ubicada en la carpeta del proyecto, ejecuta:

   ```bash
   dotnet restore "DAS Proyecto.slnx"
   dotnet build "DAS Proyecto.slnx"
   dotnet run --project "DAS Proyecto.csproj"
   ```

5. Abre `http://localhost:5049` o la URL indicada por Visual Studio.
6. Si usas una base existente de Fase 3, las cuentas y hashes de contraseña se conservan; la migración asigna los roles de acuerdo con `Usuarios.Rol`. Inicia sesión con la cuenta existente.

### Base nueva sin usuarios

Antes de iniciar la aplicación por primera vez, define credenciales de administrador usando User Secrets:

```bash
dotnet user-secrets set "InitialAdmin:Email" "admin@ranibal.com" --project "DAS Proyecto.csproj"
dotnet user-secrets set "InitialAdmin:Password" "UnaClaveSegura2026" --project "DAS Proyecto.csproj"
```

La contraseña debe cumplir la política de Identity: al menos 8 caracteres, mayúscula, minúscula y número. No guardes la contraseña en `appsettings.json` ni en Git.

## API REST y Swagger

En entorno `Development`, Swagger queda disponible en:

- `http://localhost:5049/swagger`

Para obtener un token independiente de la interfaz web:

1. Ejecuta `POST /api/auth/token` con `correo` y `contrasena`.
2. Copia `access_token` de la respuesta.
3. En Swagger, selecciona **Authorize** y pega el valor como `Bearer <token>`.
4. Prueba `/api/prueba-seguridad/privado`, `/administrador` y `/operativo` para verificar autenticación y roles.

Rutas principales:

| Recurso | Métodos | Ruta |
|---|---|---|
| Autenticación | POST | `/api/auth/token` |
| Cotizaciones | GET, POST, GET por ID, PUT, DELETE | `/api/cotizaciones` |
| Materiales | GET, POST, GET por ID, PUT, DELETE | `/api/materiales` |
| Órdenes | GET, POST, GET por ID, PUT de estado, POST para agregar material | `/api/ordenes` |
| Pagos | GET, POST, GET por ID y GET por orden | `/api/pagos` |
| Seguridad | GET | `/api/prueba-seguridad/*` |

Las operaciones de eliminación de órdenes y de modificación/eliminación de pagos responden `409 Conflict` intencionalmente para preservar la trazabilidad de órdenes y la auditoría contable. Los verbos PUT/DELETE de pagos se publican para comunicar esa regla de negocio, no para permitir alteraciones financieras.

## Pruebas existentes

La suite `PruebasPago` cubre reglas de anticipos, abonos, saldo y cálculos de informes usando repositorios de prueba. Para ejecutarla:

```bash
dotnet run --project "PruebasPago/PruebasPago.csproj"
```

El resultado histórico de la rama base fue 32 pruebas correctas y 0 fallidas. Debe repetirse en el entorno local después de integrar esta versión. Las pruebas unitarias no sustituyen las pruebas de integración de la API con SQL Server; consulta `Documentacion/PRUEBAS_FASE4.md`.

## Documentación del proyecto

- `Documentacion/INTEGRACION_LEEME.md`: alcance de la unificación, decisiones y primer inicio.
- `Documentacion/RUBRICA_FASE4_ESTADO.md`: relación de requisitos de la rúbrica con código y evidencia pendiente.
- `Documentacion/MANUAL_TECNICO.md`: arquitectura, configuración, seguridad, API y despliegue.
- `Documentacion/MANUAL_USUARIO.md`: guía de uso del sistema.
- `Documentacion/PRUEBAS_FASE4.md`: matriz de pruebas y evidencias que debe completar el equipo.
- `PruebasAPI/Ranibal-Fase4.http`: solicitudes de referencia para probar la API.

## Secretos y despliegue

En desarrollo, la aplicación genera una clave JWT aleatoria persistente en `.local/jwt-development.key`; esa carpeta está excluida de Git. No compartas esa clave. En producción configura por variables de entorno o un almacén seguro:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key` (mínimo 32 bytes)
- `Jwt__Issuer`
- `Jwt__Audience`

Usa HTTPS, configura el entorno `Production`, aplica las migraciones de forma controlada y publica con `dotnet publish -c Release`. El despliegue real en IIS/Azure y la validación desde un entorno accesible deben documentarse con evidencia del equipo; no se consideran realizados por el mero hecho de incluir la configuración.
