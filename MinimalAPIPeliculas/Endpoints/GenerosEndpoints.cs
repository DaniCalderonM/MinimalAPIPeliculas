using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class GenerosEndpoints
    {
        public static RouteGroupBuilder MapGeneros(this RouteGroupBuilder group)
        {
            // Metodo get para obtener todos los generos
            group.MapGet("/", ObtenerGeneros)
                // Para activar el cache del servidor por 15 segundos
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("generos-get"))
                //Para añadir autorizacion a la ruta
                .RequireAuthorization();

            // Metodo get para obtener generos por id - Aplicando filtro en el endpoint
            group.MapGet("/{id:int}", ObtenerGeneroPorId);

            // Metodo Post para crear un genero, de manera asincrona para obtener de la BBDD
            group.MapPost("/", CrearGenero).AddEndpointFilter<FiltroValidaciones<CrearGeneroDTO>>();

            // Metodo Put para actualizar un genero por su id
            group.MapPut("/{id:int}", ActualizarGenero).AddEndpointFilter<FiltroValidaciones<CrearGeneroDTO>>();

            // Metodo para borrar un genero por id
            group.MapDelete("/{id:int}", EliminarGeneroPorId);
            return group;
        }

        // Función nombrada llamada ObtenerGeneros
        static async Task<Ok<List<GeneroDTO>>> ObtenerGeneros(IRepositorioGeneros repositorio, IMapper mapper)
        {
            var generos = await repositorio.ObtenerTodos();
            var generosDTO = mapper.Map<List<GeneroDTO>>(generos);
            return TypedResults.Ok(generosDTO);
        }

        // Función nombrada llamada ObtenerGeneroPorId
        static async Task<Results<Ok<GeneroDTO>, NotFound>> ObtenerGeneroPorId(IRepositorioGeneros repositorio,
            int id, IMapper mapper)
        {
            var genero = await repositorio.ObtenerPorId(id);

            if (genero is null)
            {
                return TypedResults.NotFound();
            }

            var generoDTO = mapper.Map<GeneroDTO>(genero);

            return TypedResults.Ok(generoDTO);
        }

        // Función nombrada llamada CrearGenero
        static async Task<Results<Created<GeneroDTO>, ValidationProblem>> CrearGenero(CrearGeneroDTO crearGeneroDTO, IRepositorioGeneros repositorio,
            IOutputCacheStore outputCacheStore, IMapper mapper)
        { 
            // Queremos mappear hacia Genero y queremos mappear el crearGeneroDTO
            var genero = mapper.Map<Genero>(crearGeneroDTO);
            var id = await repositorio.Crear(genero);
            await outputCacheStore.EvictByTagAsync("generos-get", default);
            // Queremos mappear hacia GeneroDTO y queremos mappear el genero
            var generoDTO = mapper.Map<GeneroDTO>(genero);

            // Con el Created devolvemos un codigo 201 de que algo fue creado
            return TypedResults.Created($"/generos/{id}", generoDTO);
        }

        // Función nombrada llamada ActualiuzarGenero
        static async Task<Results<NoContent, NotFound, ValidationProblem>> ActualizarGenero(int id, CrearGeneroDTO crearGeneroDTO,
            IRepositorioGeneros repositorio, IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var genero = mapper.Map<Genero>(crearGeneroDTO);
            genero.Id = id;

            await repositorio.Actualizar(genero);
            await outputCacheStore.EvictByTagAsync("generos-get", default);
            // Con NoContent retornamos un codigo 204
            return TypedResults.NoContent();
        }

        // Función nombrada llamada EliminarGeneroPorId
        static async Task<Results<NoContent, NotFound>> EliminarGeneroPorId(int id, IRepositorioGeneros repositorio,
            IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Borrar(id);
            await outputCacheStore.EvictByTagAsync("generos-get", default);
            return TypedResults.NoContent();
        }

    }
}
