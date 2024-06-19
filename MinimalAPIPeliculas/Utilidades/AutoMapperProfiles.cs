using AutoMapper;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            // Mapeamos desde CreaGeneroDTO hacia Genero
            CreateMap<CrearGeneroDTO, Genero>();
            // Mapeamos desde Genero hacia GeneroDTO
            CreateMap<Genero, GeneroDTO>();

            // Mapeamos desde CrearActorDTO hacia Actor
            CreateMap<CrearActorDTO, Actor>()
                //Estamos ignorando la propiedad foto para cuando se haga el mappeo,
                // porque en un lado es string y en el otro IFormFile
                .ForMember(x => x.Foto, opciones => opciones.Ignore());
            // Mapeamos desde Actor hacia ActorDTO
            CreateMap<Actor, ActorDTO>();

            // Mapeamos desde CrearPeliculaDTO hacia Pelicula
            CreateMap<CrearPeliculaDTO, Pelicula>()
                .ForMember(x => x.Poster, opciones => opciones.Ignore());
            // Mapeamos desde Pelicula hacia PeliculaDTO
            CreateMap<Pelicula, PeliculaDTO>()
                .ForMember(p => p.Generos,
                entidad => entidad.MapFrom(p =>
                p.GenerosPeliculas.Select(gp =>
                new GeneroDTO { Id = gp.GeneroId, Nombre = gp.Genero.Nombre })))
                .ForMember(p => p.Actores, entidad => entidad.MapFrom(p => 
                p.ActoresPeliculas.Select(ap =>
                new ActorPeliculaDTO { Id = ap.Actor.Id,
                Nombre = ap.Actor.Nombre,
                Personaje = ap.Personaje })));

            // Mapeamos desde CrearComentarioDTO hacia Comentario
            CreateMap<CrearComentarioDTO, Comentario>();
            // Mapeamos desde Comentario hacia ComentarioDTO
            CreateMap<Comentario, ComentarioDTO>();

            //Mapeamos desde AsignarActorPeliculaDTO hacia ActorPelicula
            CreateMap<AsignarActorPeliculaDTO, ActorPelicula>();

        }
    }
}
