namespace MinimalAPIPeliculas.DTOs
{
    public class CrearActorDTO
    {
        public string Nombre { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        // IFormFile se utiliza para recibir archivos de cualquier tipo
        public IFormFile? Foto { get; set; }
    }
}
