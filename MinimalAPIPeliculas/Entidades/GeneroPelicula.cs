namespace MinimalAPIPeliculas.Entidades
{
    public class GeneroPelicula
    {
        //Estas son una llave primario compuesta, entre PeliculaId y GeneroId
        public int PeliculaId { get; set; }
        public int GeneroId { get; set; }
        // Estas dos se usan para poder acceder a las propiedades de Genero y Pelicula
        public Genero Genero { get; set; } = null!;
        public Pelicula Pelicula { get; set; } = null!;
    }
}
