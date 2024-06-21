using AutoMapper;
using Microsoft.AspNetCore.Components.Forms.Mapping;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class ComentariosEndpoints
    {
        public static RouteGroupBuilder MapComentarios(this RouteGroupBuilder group)
        {
            group.MapPost("/", Crear).AddEndpointFilter<FiltroValidaciones<CrearComentarioDTO>>();
            //Se configurara para poder acceder al cache segun a donde se encuentre
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60))
            .Tag("comentarios-get").SetVaryByRouteValue(new string[] { "peliculaId" }));
            group.MapGet("/{id:int}", ObtenerPorId);
            group.MapPut("/{id:int}", Actualizar).AddEndpointFilter<FiltroValidaciones<CrearComentarioDTO>>(); ;
            group.MapDelete("/{id:int}", Borrar);
            return group;
        }

        //Aqui utilizamos el repositorio de peliculas y el de comentarios
        static async Task<Results<Ok<List<ComentarioDTO>>, NotFound>> ObtenerTodos(int peliculaId,
            IRepositorioComentarios repositorioComentarios, IRepositorioPeliculas repositorioPeliculas,
            IMapper mapper)
        {
            if (!await repositorioPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            var comentarios = await repositorioComentarios.ObtenerTodos(peliculaId);
            var comentariosDTO = mapper.Map<List<ComentarioDTO>>(comentarios);
            return TypedResults.Ok(comentariosDTO);
        }

        //Aqui solo utilizamos el repositorio de comentarios
        static async Task<Results<Ok<ComentarioDTO>, NotFound>> ObtenerPorId(int peliculaId, int id,
            IRepositorioComentarios repositorio, IMapper mapper)
        {
            var comentario = await repositorio.ObtenerPorId(id);

            if (comentario is null)
            {
                return TypedResults.NotFound();
            }

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return TypedResults.Ok(comentarioDTO);
        }



        //Aqui utilizamos el repositorio de peliculas y el de comentarios
        static async Task<Results<Created<ComentarioDTO>, NotFound>> Crear(int peliculaId,
            CrearComentarioDTO crearComentarioDTO, IRepositorioComentarios repositorioComentarios,
            IRepositorioPeliculas repositorioPeliculas, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            if (!await repositorioPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            var comentario = mapper.Map<Comentario>(crearComentarioDTO);
            comentario.PeliculaId = peliculaId;
            var id = await repositorioComentarios.Crear(comentario);
            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return TypedResults.Created($"/comentario/{id}", comentarioDTO);
        }

        //Aqui utilizamos el repositorio de peliculas y el de comentarios
        static async Task<Results<NoContent, NotFound>> Actualizar(int peliculaId, int id,
            CrearComentarioDTO crearComentarioDTO, IOutputCacheStore outputCacheStore,
            IRepositorioComentarios repositorioComentarios, IRepositorioPeliculas repositorioPeliculas, 
            IMapper mapper)
        {
            if (!await repositorioPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            if (!await repositorioComentarios.Existe(id))
            {
                return TypedResults.NotFound();
            }

            var comentario = mapper.Map<Comentario>(crearComentarioDTO);
            comentario.Id = id;
            comentario.PeliculaId = peliculaId;

            await repositorioComentarios.Actualizar(comentario);
            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            return TypedResults.NoContent();

        }

        //Aqui utilizamos el repositorio de comentarios
        static async Task<Results<NoContent, NotFound>> Borrar(int peliculaId, int id,
            IRepositorioComentarios repositorio, IOutputCacheStore outputCacheStore)
        {
            if (!await repositorio.Existe(id))
            {
                return TypedResults.NotFound();
            }

            await repositorio.Borrar(id);
            return TypedResults.NoContent();
        }

    }
}
