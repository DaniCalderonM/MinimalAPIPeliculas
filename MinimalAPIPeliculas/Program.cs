using FluentValidation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIPeliculas;
using MinimalAPIPeliculas.Endpoints;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Migrations;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;
using MinimalAPIPeliculas.Utilidades;

var builder = WebApplication.CreateBuilder(args);
var originesPermitidos = builder.Configuration.GetValue<string>("origenesPermitidos")!;
// Inicio de área de los servicios

// Configuranto Entity Framework Core llamando a DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name= DefaultConnection"));

//Configuracion para Identity
builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//Configuracion para poder crear y registrar usuarios (va con identity)
builder.Services.AddScoped<UserManager<IdentityUser>>();
//Configuracion para inicio de sesion de usuarios (va con identity)
builder.Services.AddScoped<SignInManager<IdentityUser>>();

// Con esta configuración decimos que cualquier pagina web
// puede comunicarse con nosotros de cualquier forma.
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(configuracion =>
    {
        configuracion.WithOrigins(originesPermitidos).AllowAnyHeader().AllowAnyMethod();
    });

    opciones.AddPolicy("libre", configuracion =>
    {
        configuracion.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });

});

// Constructor para cache desde el servidor
builder.Services.AddOutputCache();

// Constructor para endpoint, que se debe utilizar
// junto con el de Swagger
builder.Services.AddEndpointsApiExplorer();
// Constructor para Swagger
builder.Services.AddSwaggerGen();

// Configurando para depender de una abstraccion (5to principio de Solid de inversion de
// dependencias)
builder.Services.AddScoped<IRepositorioGeneros, RepositorioGeneros>();
builder.Services.AddScoped<IRepositorioActores, RepositorioActores>();
builder.Services.AddScoped<IRepositorioPeliculas, RepositorioPeliculas>();
builder.Services.AddScoped<IRepositorioComentarios, RepositorioComentarios>();
builder.Services.AddScoped<IRepositorioErrores, RepositorioErrores>();
// para almacenar de forma local los archivos
builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
//añadiendo servicios usuarios
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
// añadiendo el httpcontextaccessor
builder.Services.AddHttpContextAccessor();

// Constructor para AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Constructor para FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//Me permite realizar configuraciones  respecto al manejo de errores
// o problemas -> Luego nos vamos a los middlewares para continuar
builder.Services.AddProblemDetails();

//Para configurar servicios de autenticacion y autorizacion
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{   
    //con esta linea, quitamos el mapeo de los claims
    opciones.MapInboundClaims = false;

    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //Esta es para utilizar solo una llave
        //IssuerSigningKey = Llaves.ObtenerLlave(builder.Configuration).First(),
        //Esta es para utilizar todas las llaves
        IssuerSigningKeys = Llaves.ObtenerTodasLasLlaves(builder.Configuration),
        ClockSkew = TimeSpan.Zero
    };
    
    });

builder.Services.AddAuthorization(opciones =>
{
    //Creamos una politica llamada esadmin en la que requeriremos que el usuario
    // tengo un claim llamado esadmin
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));
});

// Fin de área de los servicios

var app = builder.Build();

// Inicio de área de los middleware (deben ir en orden)

//if (builder.Environment.IsDevelopment())
//{
    //Aqui podria ir los app.UseSwagger para que se utilizara
    //solo en el entorno de desarrollo
//}

// Se pone antes de Cors, porque no necesitamos Cors para utilizar
// Swagger,ya que lo usamos en nuestro origen y no en uno ajeno
app.UseSwagger();
app.UseSwaggerUI();

// Segunda parte para poder usar el builder.Services.AddProblemDetails();
app.UseExceptionHandler(ExceptionHandlerApp => ExceptionHandlerApp.Run(async context =>
{// Todo esto es para que se guarden los errores en la base de datos
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var excepcion = exceptionHandlerFeature?.Error!;

    var error = new Error();
    error.Fecha = DateTime.UtcNow;
    error.MensajeDeError = excepcion.Message;
    error.StackTrace = excepcion.StackTrace;

    var repositorio = context.RequestServices.GetRequiredService<IRepositorioErrores>();
    await repositorio.Crear(error);
    // Hasta la linea de arriba es para que se guarden en la base de datos

    await TypedResults.BadRequest(
        new { tipo = "error", mensaje = "Ha ocurrido un mensaje de error inesperado", status = 500 })
    .ExecuteAsync(context);
}));
//Esta tambien esta ligada al paso de arriba, me permite configurar la app para
// que retorne codigos de status cuando haya error
app.UseStatusCodePages();

// Acceder a los archivos estaticos a traves de este middleware
app.UseStaticFiles();

// Antes de los endpoints va el middleware de Cors
app.UseCors();

// Configurando cache desde el servidor
app.UseOutputCache();

//Configurando autorizacion
app.UseAuthorization();


// Metodo get de la ruta raiz
app.MapGet("/", [EnableCors(policyName: "libre")]() => "Hello World!");
app.MapGet("/error", () =>
{
    throw new InvalidOperationException("error de ejemplo");
});

// Aplicando un Map Group
app.MapGroup("/generos").MapGeneros();
app.MapGroup("/actores").MapActores();
app.MapGroup("/peliculas").MapPeliculas();
app.MapGroup("/pelicula/{peliculaId:int}/comentarios").MapComentarios();
app.MapGroup("/usuarios").MapUsuarios();

// Fin de área de los middleware
app.Run();

