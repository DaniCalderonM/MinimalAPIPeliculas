namespace MinimalAPIPeliculas.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Cuerpo { get; set; } = null!;
        // Este es para hacer la relacion de uno es a muchos
        // una pelicula puede tener muchos comentarios
        public int PeliculaId { get; set; }
    }
}
