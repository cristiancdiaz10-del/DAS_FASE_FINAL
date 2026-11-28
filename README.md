# Sistema de Gestión Rótulos Ranibal

Sistema web para la administración de las operaciones de Rótulos Ranibal. Desarrollado como proyecto académico utilizando ASP.NET Core MVC, C#, Entity Framework Core y SQL Server.

La aplicación permite gestionar clientes, usuarios, servicios, cotizaciones, inventario, órdenes de trabajo, pagos e informes desde una plataforma centralizada.

## 1. Funcionalidades principales

- **Clientes:** registro y administración de información de clientes.
- **Usuarios:** creación y administración de cuentas y roles.
- **Servicios:** gestión de los servicios ofrecidos por la empresa.
- **Cotizaciones:** creación y seguimiento de cotizaciones.
- **Inventario:** administración de materiales mediante operaciones asíncronas con Fetch API.
- **Órdenes de trabajo:** generación y seguimiento de órdenes a partir de cotizaciones.
- **Pagos:** registro de anticipos, abonos y consulta de saldos pendientes.
- **Informes:** consulta de información y resultados de las operaciones del sistema.
- **API REST:** servicios para consultar y gestionar los recursos del sistema.
- **Seguridad:** autenticación, autorización por roles, JWT y protección antifalsificación.

## 2. Tecnologías utilizadas

| Tecnología | Aplicación |
|---|---|
| C# | Lenguaje de programación |
| ASP.NET Core MVC | Desarrollo de la aplicación web |
| .NET 10 | Plataforma de ejecución |
| Entity Framework Core | ORM y persistencia de datos |
| SQL Server | Base de datos |
| ASP.NET Identity | Autenticación y gestión de usuarios |
| JWT | Autenticación de servicios API |
| Fetch API | Comunicación asíncrona |
| Swagger | Documentación y pruebas de API |
| Bootstrap | Diseño e interfaz de usuario |
| Git y GitHub | Control de versiones y colaboración |

## 3. Arquitectura del proyecto

El proyecto utiliza una estructura organizada por responsabilidades:

- **Controllers:** controladores MVC y API.
- **Models:** entidades del sistema.
- **Data:** contexto de base de datos y unidad de trabajo.
- **Repositories:** acceso y operaciones sobre los datos.
- **Services:** lógica de negocio.
- **DTOs:** objetos de transferencia de datos.
- **ViewModels:** modelos utilizados por las vistas.
- **Views:** interfaces de usuario.
- **Filters:** filtros de seguridad.
- **Migrations:** migraciones de Entity Framework Core.
- **wwwroot:** archivos CSS, JavaScript, imágenes y bibliotecas.

## 4. Requisitos para ejecutar el proyecto

- .NET SDK 10.0.
- Visual Studio 2022 actualizado o Visual Studio Code.
- SQL Server LocalDB o una instancia de SQL Server.
- Git, para clonar el repositorio.

## 5. Instalación y ejecución

1. Clonar el repositorio:

   ```bash
   git clone URL_DEL_REPOSITORIO
   ```

2. Ingresar a la carpeta del proyecto y abrir `DAS Proyecto.slnx` o `DAS Proyecto.csproj`.

3. Revisar la cadena de conexión en `appsettings.json`, dentro de `ConnectionStrings:DefaultConnection`.

   La configuración inicial utiliza SQL Server LocalDB y la base de datos `DAS_Proyecto_DB`.

4. Antes de ejecutar la aplicación sobre una base de datos existente de Fase 3, realizar un respaldo.

5. Ejecutar los siguientes comandos desde la carpeta raíz:

   ```bash
   dotnet restore "DAS Proyecto.slnx"
   dotnet build "DAS Proyecto.slnx"
   dotnet run --project "DAS Proyecto.csproj"
   ```

6. Abrir la aplicación en el navegador:

   `http://localhost:5049`

La aplicación aplica automáticamente las migraciones pendientes al iniciar.

## 6. Acceso al sistema y roles

El acceso se realiza mediante correo electrónico y contraseña.

El sistema contempla los siguientes roles:

- **Administrador:** permite gestionar usuarios y acceder a las funciones administrativas.
- **Operativo:** permite utilizar las funcionalidades autorizadas para las operaciones del sistema, sin acceso a la administración de usuarios.

### Configuración del primer administrador

Cuando se utiliza una base de datos nueva que todavía no contiene usuarios, se pueden configurar las credenciales iniciales mediante User Secrets:

```bash
dotnet user-secrets set "InitialAdmin:Email" "TU_CORREO_ADMIN" --project "DAS Proyecto.csproj"
dotnet user-secrets set "InitialAdmin:Password" "TU_CLAVE_SEGURA" --project "DAS Proyecto.csproj"
```

Después de iniciar la aplicación, se crea el primer usuario con el rol Administrador.

La contraseña debe tener como mínimo 8 caracteres, una letra mayúscula, una minúscula y un número.

**Nota:** si se utiliza una base de datos existente que ya tiene usuarios, se deben utilizar las credenciales de esas cuentas. La configuración del administrador inicial no reemplaza usuarios existentes.

Las credenciales de demostración para la evaluación deben compartirse por un medio privado, no mediante este repositorio público.

## 7. API REST y Swagger

La aplicación dispone de una API REST para trabajar con los recursos del sistema.

| Recurso | Ruta |
|---|---|
| Autenticación | `/api/auth/token` |
| Materiales | `/api/materiales` |
| Cotizaciones | `/api/cotizaciones` |
| Órdenes de trabajo | `/api/ordenes` |
| Pagos | `/api/pagos` |
| Pruebas de seguridad | `/api/prueba-seguridad/` |

Para obtener un token JWT se utiliza `POST /api/auth/token`, enviando el correo y la contraseña de una cuenta registrada.

Swagger está disponible durante el entorno de desarrollo:

`http://localhost:5049/swagger`

También se incluye el archivo `PruebasAPI/Ranibal-Fase4.http` con solicitudes de referencia para probar los servicios.

Algunas operaciones tienen restricciones de integridad: por ejemplo, la modificación o eliminación de pagos y la eliminación de órdenes pueden responder `409 Conflict` para proteger la trazabilidad y los registros contables.

## 8. Pruebas

El proyecto incluye una suite de pruebas de consola en `PruebasPago`.

Para ejecutarla:

```bash
dotnet run --project "PruebasPago/PruebasPago.csproj"
```

En la última ejecución verificada por el equipo se obtuvieron:

- 47 pruebas correctas.
- 0 pruebas fallidas.

Estas pruebas incluyen validaciones de pagos, anticipos, abonos, saldos, informes y validaciones de datos. No sustituyen las pruebas de integración de todos los endpoints de la API.

## 9. Estado del despliegue

Se realizó una publicación y validación de la aplicación en IIS en el equipo local.

Dirección utilizada durante las pruebas:

`http://localhost:8085`

El endpoint `/health` respondió correctamente e informó disponibilidad de la base de datos.

**Importante:** esta dirección corresponde al entorno local de pruebas y no representa un alojamiento público accesible desde Internet.

## 10. Seguridad y configuración

La aplicación utiliza ASP.NET Identity, JWT, roles y políticas de autorización.

Para configurar un entorno de producción se deben establecer las siguientes variables:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`

La clave JWT debe tener al menos 32 bytes y almacenarse como secreto del entorno.

No se deben publicar contraseñas, claves JWT, archivos de configuración privados ni credenciales de bases de datos en GitHub.

## 11. Documentación y entregables

El manual técnico y el manual de usuario se entregan como documentos independientes.

El repositorio contiene el código fuente, las migraciones, las pruebas y los archivos necesarios para ejecutar y revisar el proyecto.

### Manual técnico

Se incorpora el Manual Técnico del sistema Rótulos Ranibal, correspondiente a la documentación de la Fase 4.

[Consultar Manual Técnico (PDF)](./Manual_Tecnico_Rotulos_Ranibal.pdf)

**Nota:** Debido a que el aula virtual no permitió cargar archivos adicionales, el manual técnico se agregó directamente a este repositorio para su consulta.
