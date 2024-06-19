using System.ComponentModel.DataAnnotations;

namespace MinimalAPIPeliculas.Entidades
{
    public class Genero
    {
        public int Id { get; set; }
        // Esta era la forma Anotaciones de datos
        //[StringLength(50)]
        public string Nombre { get; set; } = null!;

        //Hacemos esto por la relacion muchos a muchos
        public List<GeneroPelicula> GenerosPeliculas { get; set; } = new List<GeneroPelicula>();
    }
}
