using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using DAS_Proyecto.Data;
using DAS_Proyecto.Filters;
using DAS_Proyecto.Models;
using DAS_Proyecto.Repositories.Implementations;
using DAS_Proyecto.Repositories.Interfaces;
using DAS_Proyecto.Services;
using DAS_Proyecto.Services.Implementations;
using DAS_Proyecto.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Claims;

namespace DAS_Proyecto;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var bootstrapLogger = loggerFactory.CreateLogger<Program>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se configuró ConnectionStrings:DefaultConnection.");
        }

        ConfigurarJwt(builder, bootstrapLogger);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services
            .AddIdentity<Usuario, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Ranibal.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        var jwtKey = builder.Configuration["Jwt:Key"]!;
        var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
        var jwtAudience = builder.Configuration["Jwt:Audience"]!;

        builder.Services
            .AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                // Permite que la interfaz Razor consuma la API con un JWT HttpOnly.
                // Los clientes externos pueden seguir enviando Authorization: Bearer.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrWhiteSpace(context.Token) &&
                            context.Request.Cookies.TryGetValue(
                                JwtTokenService.CookieName,
                                out var tokenCookie))
                        {
                            context.Token = tokenCookie;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("Administrador", policy =>
                policy.RequireRole(nameof(RolUsuario.Administrador)));

            options.AddPolicy("Operativo", policy =>
                policy.RequireRole(nameof(RolUsuario.Operativo)));

            options.AddPolicy("Api", policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        });

        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "RequestVerificationToken";
        });

        builder.Services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Rótulos Ranibal - API REST",
                Version = "v1",
                Description = "Servicios REST para cotizaciones, materiales, órdenes de trabajo y pagos."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Introduce el token JWT obtenido desde POST /api/auth/token."
            });

            options.OperationFilter<AuthorizeOperationFilter>();

            var xmlPath = Path.Combine(
                AppContext.BaseDirectory,
                $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
        builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
        builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        builder.Services.AddScoped<IPagoRepository, PagoRepository>();
        builder.Services.AddScoped<ICotizacionRepository, CotizacionRepository>();
        builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
        builder.Services.AddScoped<IOrdenTrabajoRepository, OrdenTrabajoRepository>();
        builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

        builder.Services.AddScoped<PagoValidacionService>();
        builder.Services.AddScoped<InformeMensualService>();
        builder.Services.AddScoped<IPagoService, PagoService>();
        builder.Services.AddScoped<ICotizacionService, CotizacionService>();
        builder.Services.AddScoped<IMaterialService, MaterialService>();
        builder.Services.AddScoped<IOrdenTrabajoService, OrdenTrabajoService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<JwtTokenService>();
        builder.Services.AddScoped<ApiAntiforgeryFilter>();

        var app = builder.Build();

        // Conserva la localización definida en la versión funcional de Fase 3.
        var culturas = new[] { new CultureInfo("en-US") };
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-US"),
            SupportedCultures = culturas,
            SupportedUICultures = culturas
        });

        // Aplica las migraciones versionadas y comprueba que existan los roles.
        await InicializarBaseEIdentityAsync(app.Services, app.Configuration, app.Logger);

        // Logging centralizado de solicitudes HTTP.
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices
                .GetRequiredService<ILogger<Program>>();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next();
            }
            finally
            {
                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;
                var metodo = context.Request.Method;
                var ruta = context.Request.Path;
                var duracion = stopwatch.ElapsedMilliseconds;
                var traceId = context.TraceIdentifier;

                if (statusCode >= 500)
                {
                    logger.LogError(
                        "HTTP {Method} {Path} respondió {StatusCode} en {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                        metodo, ruta, statusCode, duracion, traceId);
                }
                else if (statusCode >= 400)
                {
                    logger.LogWarning(
                        "HTTP {Method} {Path} respondió {StatusCode} en {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                        metodo, ruta, statusCode, duracion, traceId);
                }
                else
                {
                    logger.LogInformation(
                        "HTTP {Method} {Path} respondió {StatusCode} en {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                        metodo, ruta, statusCode, duracion, traceId);
                }
            }
        });

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features
                    .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;
                var log = context.RequestServices.GetRequiredService<ILogger<Program>>();
                log.LogError(exception, "Error no controlado al procesar {Path}.", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Error interno del servidor",
                        Detail = app.Environment.IsDevelopment()
                            ? exception?.Message
                            : "Ocurrió un error al procesar la solicitud."
                    });
                    return;
                }

                context.Response.Redirect("/Home/Error");
            });
        });

        if (!app.Environment.IsDevelopment())
            app.UseHsts();
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rótulos Ranibal API v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // Endpoint mínimo para supervisar la disponibilidad de la base de datos.
        app.MapGet("/health", async (ApplicationDbContext db, ILogger<Program> log) =>
        {
            try
            {
                var disponible = await db.Database.CanConnectAsync();
                if (!disponible)
                {
                    log.LogWarning("La comprobación de salud no pudo conectar con SQL Server.");
                    return Results.Problem(
                        title: "Servicio no disponible",
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                return Results.Ok(new
                {
                    status = "Healthy",
                    database = "Available",
                    timestampUtc = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Falló la comprobación de salud de la aplicación.");
                return Results.Problem(
                    title: "Servicio no disponible",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }).AllowAnonymous();

        // Las acciones POST de los controladores MVC ya validan AntiForgeryToken.
        // Los controladores API validan CSRF cuando el JWT viaja en cookie.
        app.MapControllers();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Account}/{action=Login}/{id?}");

        app.Logger.LogInformation(
            "Rótulos Ranibal iniciado en entorno {EnvironmentName}.",
            app.Environment.EnvironmentName);

        await app.RunAsync();
    }

    private static void ConfigurarJwt(WebApplicationBuilder builder, ILogger logger)
    {
        var esDesarrollo = builder.Environment.IsDevelopment();
        var key = builder.Configuration["Jwt:Key"];
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];

        if (esDesarrollo)
        {
            // Clave local persistente, ignorada por Git y nunca incluida en el ZIP.
            if (string.IsNullOrWhiteSpace(key))
            {
                var carpetaLocal = Path.Combine(builder.Environment.ContentRootPath, ".local");
                Directory.CreateDirectory(carpetaLocal);
                var rutaClave = Path.Combine(carpetaLocal, "jwt-development.key");

                if (File.Exists(rutaClave))
                    key = File.ReadAllText(rutaClave).Trim();
                else
                {
                    key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                    File.WriteAllText(rutaClave, key);
                }

                builder.Configuration["Jwt:Key"] = key;
                logger.LogWarning(
                    "Se utiliza una clave JWT local de desarrollo almacenada en .local. No compartas ese archivo.");
            }

            if (string.IsNullOrWhiteSpace(issuer))
                builder.Configuration["Jwt:Issuer"] = issuer = "DAS_Proyecto_Development";

            if (string.IsNullOrWhiteSpace(audience))
                builder.Configuration["Jwt:Audience"] = audience = "DAS_Proyecto_Development_API";
        }

        key = builder.Configuration["Jwt:Key"];
        issuer = builder.Configuration["Jwt:Issuer"];
        audience = builder.Configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(key) ||
            Encoding.UTF8.GetByteCount(key) < 32 ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "Configura Jwt:Key (mínimo 32 bytes), Jwt:Issuer y Jwt:Audience mediante secretos o variables de entorno.");
        }
    }

    private static async Task InicializarBaseEIdentityAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        using var scope = services.CreateScope();
        var scoped = scope.ServiceProvider;
        var db = scoped.GetRequiredService<ApplicationDbContext>();

        try
        {
            await db.Database.MigrateAsync();

            var roleManager = scoped.GetRequiredService<RoleManager<IdentityRole<int>>>();
            foreach (var nombreRol in new[]
            {
                nameof(RolUsuario.Administrador),
                nameof(RolUsuario.Operativo)
            })
            {
                if (!await roleManager.RoleExistsAsync(nombreRol))
                {
                    var creado = await roleManager.CreateAsync(
                        new IdentityRole<int> { Name = nombreRol });
                    if (!creado.Succeeded)
                    {
                        var detalle = string.Join("; ", creado.Errors.Select(e => e.Description));
                        throw new InvalidOperationException(
                            $"No se pudo crear el rol {nombreRol}: {detalle}");
                    }
                }
            }

            var userManager = scoped.GetRequiredService<UserManager<Usuario>>();
            if (!await userManager.Users.AnyAsync())
            {
                var correo = configuration["InitialAdmin:Email"];
                var password = configuration["InitialAdmin:Password"];

                if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning(
                        "La base de datos no contiene usuarios. Configura InitialAdmin:Email y InitialAdmin:Password para crear el primer administrador.");
                    return;
                }

                var admin = new Usuario
                {
                    Nombre = "Administrador",
                    Correo = correo.Trim(),
                    Rol = RolUsuario.Administrador,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(admin, password);
                if (!resultado.Succeeded)
                {
                    var detalle = string.Join("; ", resultado.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(
                        $"No se pudo crear el administrador inicial: {detalle}");
                }

                var rolResultado = await userManager.AddToRoleAsync(
                    admin,
                    nameof(RolUsuario.Administrador));
                if (!rolResultado.Succeeded)
                {
                    var detalle = string.Join("; ", rolResultado.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(
                        $"El administrador se creó, pero no se pudo asignar el rol: {detalle}");
                }

                logger.LogInformation(
                    "Se creó el usuario administrador inicial {Email}.",
                    admin.Email);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "No se pudo preparar la base de datos o ASP.NET Identity.");
            throw;
        }
    }
}

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var permiteAccesoAnonimo = context.ApiDescription.ActionDescriptor
            .EndpointMetadata.Any(metadata => metadata is IAllowAnonymous);

        if (permiteAccesoAnonimo)
            return;

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            }
        ];
    }
}
