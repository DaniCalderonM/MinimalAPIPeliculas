using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        // Utilizaremos Api Fluente para cambiar el tamaño del string de nombre
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar entidad Genero
            modelBuilder.Entity<Genero>().Property(p => p.Nombre).HasMaxLength(50);
            
            // Configurar entidad Actor
            modelBuilder.Entity<Actor>().Property(p => p.Nombre).HasMaxLength(150);
            modelBuilder.Entity<Actor>().Property(p => p.Foto).IsUnicode();

            // Configurar entidad Pelicula
            modelBuilder.Entity<Pelicula>().Property(p => p.Titulo).HasMaxLength(150);
            modelBuilder.Entity<Pelicula>().Property(p => p.Poster).IsUnicode();

            //Creamos la llave compuesta de la entidad GeneroPelicula
            modelBuilder.Entity<GeneroPelicula>().HasKey(g => new { g.GeneroId, g.PeliculaId });

            //Creamos la llave compuesta de la entidad ActorPelicula
            modelBuilder.Entity<ActorPelicula>().HasKey(a => new { a.PeliculaId, a.ActorId });
        }

        // Con esta configuración estamos diciendo que crearemos esta tabla en la
        // bbdd, primero se indica la entidad y luego el nombre de la tabla en plural
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Actor> Actores { get; set; } 
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<GeneroPelicula> GenerosPeliculas { get; set; }
        public DbSet<ActorPelicula> ActoresPeliculas { get; set; }
    }
}
