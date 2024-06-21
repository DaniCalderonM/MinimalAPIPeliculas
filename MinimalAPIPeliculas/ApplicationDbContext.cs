using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas
{
    //Al comienzo heredaba Dbcontext
    //public class ApplicationDbContext : DbContext
    //Ahora heredo de IdentityDbContext para que se añadan las tablas de identity en la bbdd
    public class ApplicationDbContext : IdentityDbContext
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

            //Asignarles nombres personalizados a las tablas de Identity
            modelBuilder.Entity<IdentityUser>().ToTable("Usuarios");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            //Informacion sobre un rol, asignar permisos especiales a roles
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RolesClaims");
            //Informacion sobre un usuario, asignar permisos especiales a usuarios
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UsuariosClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UsuariosLogins");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UsuariosRoles");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UsuariosTokens");
        }

        // Con esta configuración estamos diciendo que crearemos esta tabla en la
        // bbdd, primero se indica la entidad y luego el nombre de la tabla en plural
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Actor> Actores { get; set; } 
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<GeneroPelicula> GenerosPeliculas { get; set; }
        public DbSet<ActorPelicula> ActoresPeliculas { get; set; }
        public DbSet<Error> Errores { get; set; }
    }
}
