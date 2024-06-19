using FluentValidation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas;
using MinimalAPIPeliculas.Endpoints;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Migrations;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;

var builder = WebApplication.CreateBuilder(args);
var originesPermitidos = builder.Configuration.GetValue<string>("origenesPermitidos")!;
// Inicio de área de los servicios

// Configuranto Entity Framework Core llamando a DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name= DefaultConnection"));

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

builder.Services.AddScoped<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
builder.Services.AddHttpContextAccessor();

// Constructor para AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Constructor para FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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

// Acceder a los archivos estaticos a traves de este middleware
app.UseStaticFiles();

// Antes de los endpoints va el middleware de Cors
app.UseCors();

// Configurando cache desde el servidor
app.UseOutputCache();

// Metodo get de la ruta raiz
app.MapGet("/", [EnableCors(policyName: "libre")]() => "Hello World!");

// Aplicando un Map Group
app.MapGroup("/generos").MapGeneros();
app.MapGroup("/actores").MapActores();
app.MapGroup("/peliculas").MapPeliculas();
app.MapGroup("/pelicula/{peliculaId:int}/comentarios").MapComentarios();

// Fin de área de los middleware
app.Run();

