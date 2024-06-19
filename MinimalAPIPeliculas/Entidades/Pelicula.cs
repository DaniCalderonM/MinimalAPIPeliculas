namespace MinimalAPIPeliculas.Entidades
{
    public class Pelicula
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public bool EnCines { get; set; }
        public DateTime FechaLanzamiento { get; set; }
        public string? Poster { get; set; }
        // Hacemos esto para indicarle a pelicula que le corresponde una lista
        // de comentarios (relacion uno es a muchos)
        public List<Comentario> Comentarios { get; set; } = new List<Comentario>();
        
        //Hacemos esto por la relacion muchos a muchos
        public List<GeneroPelicula> GenerosPeliculas { get; set; } = new List<GeneroPelicula>();
        public List<ActorPelicula> ActoresPeliculas { get; set; } = new List<ActorPelicula>();
    }
}
