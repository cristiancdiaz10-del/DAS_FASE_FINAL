# Plan de pruebas y evidencias — Fase 4

## 1. Preparación

- [ ] Crear respaldo de `DAS_Proyecto_DB`.
- [ ] Ejecutar `dotnet restore "DAS Proyecto.slnx"`.
- [ ] Ejecutar `dotnet build "DAS Proyecto.slnx"` y guardar captura de resultado.
- [ ] Ejecutar `dotnet run --project "PruebasPago/PruebasPago.csproj"`; guardar conteo real de aprobadas/fallidas.
- [ ] Iniciar la aplicación y conservar el log donde se confirma entorno y migraciones.

## 2. Identity y seguridad

- [ ] Iniciar sesión con el administrador existente de Fase 3.
- [ ] Confirmar que conserva sus datos y contraseña después de la migración Identity.
- [ ] Ingresar cinco veces una contraseña incorrecta y comprobar bloqueo temporal (15 minutos).
- [ ] Iniciar sesión con un usuario Operativo y comprobar que no accede a Usuarios.
- [ ] Revisar en DevTools que `Ranibal.Auth` y `Ranibal.ApiToken` sean cookies HttpOnly.
- [ ] Obtener un JWT por `POST /api/auth/token` y comprobar que vence en 30 minutos.
- [ ] Consultar `/api/prueba-seguridad/privado` con token válido (200) y sin token (401).
- [ ] Consultar `/api/prueba-seguridad/administrador` con token de Administrador (200) y Operativo (403).
- [ ] Consultar `/api/prueba-seguridad/operativo` con token Operativo (200) y Administrador (403).
- [ ] Enviar una operación API con JWT cookie, pero sin `RequestVerificationToken`; debe responder 400.
- [ ] Repetir la operación con el token antifalsificación; debe pasar la validación CSRF.

## 3. API REST — Swagger

- [ ] Abrir `/swagger` en Development.
- [ ] Obtener token desde `POST /api/auth/token`.
- [ ] Autorizar Swagger con `Bearer <token>`.
- [ ] Probar GET de materiales/cotizaciones/órdenes/pagos.
- [ ] Probar POST de material y luego GET por ID.
- [ ] Probar PUT de material y comprobar los cambios persistidos.
- [ ] Probar DELETE de material sin detalles asociados.
- [ ] Probar errores de validación (400), IDs inexistentes (404) y conflictos por relaciones (409).
- [ ] Probar creación de cotización y orden con IDs existentes, sin reutilizar datos de producción.
- [ ] Probar registro de pago respetando anticipo/saldo; comprobar que PUT/DELETE devuelven 409 por inmutabilidad contable.

## 4. Fetch API / Inventario

- [ ] Iniciar sesión y abrir Inventario.
- [ ] Verificar carga asíncrona y estado de carga/error.
- [ ] Buscar un material y comprobar que la tabla se actualiza sin recargar la página.
- [ ] Crear material desde formulario; confirmar guardado en SQL Server.
- [ ] Editar material desde formulario; confirmar persistencia.
- [ ] Eliminar material sin detalles asociados; confirmar actualización parcial de la tabla.
- [ ] Esperar o simular expiración de JWT y comprobar renovación mientras Identity siga activa.
- [ ] Repetir un POST/PUT/DELETE sin token antifalsificación y verificar rechazo.
- [ ] Introducir texto similar a HTML/script en el nombre o descripción y comprobar que aparece como texto, no como código ejecutable.

## 5. Logging y errores

- [ ] Revisar los mensajes de inicio y migración.
- [ ] Provocar una excepción controlada en Development y confirmar registro con ILogger y respuesta JSON para `/api`.
- [ ] Confirmar que Production no envía detalle de excepción al cliente.
- [ ] Registrar evidencia de validación 400/401/403/404/409/500 según corresponda.

## 6. Prueba integral del producto

- [ ] Recorrer Login, Dashboard, Clientes, Usuarios, Servicios, Cotizaciones, Inventario, Órdenes, Pagos e Informes.
- [ ] Probar flujo de cotización aceptada → orden → materiales → anticipo/abonos/saldo → informe.
- [ ] Confirmar relaciones e integridad en SQL Server.
- [ ] Repetir suite `PruebasPago` tras integrar.

## 7. Despliegue y entregables

- [ ] Ejecutar `dotnet publish "DAS Proyecto.csproj" -c Release -o .\publish`.
- [ ] Desplegar en IIS o Azure (uno de los entornos solicitados).
- [ ] Configurar conexión SQL, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` y HTTPS sin guardar secretos en el repositorio.
- [ ] Probar login, API y módulos desde la URL publicada.
- [ ] Preparar capturas de pantalla, manuales revisados y video de demostración.

## Guion de video sugerido

1. Presentar propósito del sistema y arquitectura ASP.NET Core MVC / EF Core / SQL Server.
2. Iniciar sesión con Administrador y explicar roles y permisos.
3. Mostrar un recorrido real de módulos y persistencia.
4. Mostrar Swagger, token JWT, una consulta GET y una operación POST/PUT.
5. Mostrar la carga asíncrona de Inventario mediante Fetch API.
6. Mostrar una validación/error y el registro correspondiente en logs.
7. Mostrar la aplicación publicada y cerrar con requisitos cumplidos y mejoras futuras.
