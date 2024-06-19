namespace MinimalAPIPeliculas.Entidades
{
    public class ActorPelicula
    {
        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!;
        public int PeliculaId { get; set; }
        public Pelicula Pelicula { get; set; } = null!;
        //Para el orden en que quiero salgal los actores, primero el protagonista
        // y luego los secundarios
        public int Orden {  get; set; }
        // Para que aparezca el nombre del personaje correspondiente al actor
        public string Personaje { get; set; } = null!;
    }
}
