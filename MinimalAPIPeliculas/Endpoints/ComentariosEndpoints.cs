using AutoMapper;
using Microsoft.AspNetCore.Components.Forms.Mapping;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Repositorios;
using MinimalAPIPeliculas.Servicios;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class ComentariosEndpoints
    {
        public static RouteGroupBuilder MapComentarios(this RouteGroupBuilder group)
        {
            group.MapPost("/", Crear).AddEndpointFilter<FiltroValidaciones<CrearComentarioDTO>>()
                .RequireAuthorization();
            //Se configurara para poder acceder al cache segun a donde se encuentre
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60))
            .Tag("comentarios-get").SetVaryByRouteValue(new string[] { "peliculaId" }));
            group.MapGet("/{id:int}", ObtenerPorId);
            group.MapPut("/{id:int}", Actualizar).AddEndpointFilter<FiltroValidaciones<CrearComentarioDTO>>()
                .RequireAuthorization();
            group.MapDelete("/{id:int}", Borrar).RequireAuthorization();
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
        static async Task<Results<Created<ComentarioDTO>, NotFound, BadRequest<string>>>
            Crear(int peliculaId,
            CrearComentarioDTO crearComentarioDTO, IRepositorioComentarios repositorioComentarios,
            IRepositorioPeliculas repositorioPeliculas, IMapper mapper, IOutputCacheStore outputCacheStore,
            IServicioUsuarios servicioUsuarios)
        {
            if (!await repositorioPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            var comentario = mapper.Map<Comentario>(crearComentarioDTO);
            comentario.PeliculaId = peliculaId;

            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario is null)
            { 
                return TypedResults.BadRequest("Usuario no encontrado");
            }
            //colocando el id del usuario en el comentario
            comentario.UsuarioId = usuario.Id;

            var id = await repositorioComentarios.Crear(comentario);
            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return TypedResults.Created($"/comentario/{id}", comentarioDTO);
        }

        //Aqui utilizamos el repositorio de peliculas y el de comentarios
        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Actualizar(int peliculaId, int id,
            CrearComentarioDTO crearComentarioDTO, IOutputCacheStore outputCacheStore,
            IRepositorioComentarios repositorioComentarios, IRepositorioPeliculas repositorioPeliculas,
            IServicioUsuarios servicioUsuarios)
        {
            if (!await repositorioPeliculas.Existe(peliculaId))
            {
                return TypedResults.NotFound();
            }

            var comentarioBD = await repositorioComentarios.ObtenerPorId(id);

            if (comentarioBD is null)
            {
                return TypedResults.NotFound();
            }

            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            if (comentarioBD.UsuarioId != usuario.Id)
            {
                //Forbid me devuelve un codigo 403, que es prohibido
                return TypedResults.Forbid();
            }

            comentarioBD.Cuerpo = crearComentarioDTO.Cuerpo;

            await repositorioComentarios.Actualizar(comentarioBD);
            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            return TypedResults.NoContent();

        }

        //Aqui utilizamos el repositorio de comentarios
        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Borrar(int peliculaId, int id,
            IRepositorioComentarios repositorio, IOutputCacheStore outputCacheStore,
            IServicioUsuarios servicioUsuarios)
        {
            var comentarioBD = await repositorio.ObtenerPorId(id);

            if (comentarioBD is null)
            {
                return TypedResults.NotFound();
            }

            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            if (comentarioBD.UsuarioId != usuario.Id)
            {
                //Forbid me devuelve un codigo 403, que es prohibido
                return TypedResults.Forbid();
            }

            await repositorio.Borrar(id);
            await outputCacheStore.EvictByTagAsync("comentarios-get", default);
            return TypedResults.NoContent();
        }

    }
}
