namespace MinimalAPIPeliculas.Servicios
{
    public interface IAlmacenadorArchivos
    {
        Task Borrar(string? ruta, string contenedor);
        Task<string> Almacenar(string contenedor, IFormFile archivo);
        // Con este metodo al editar, se borra el archivo y luego se vuelve a subir 
        // uno nuevo, y esto hace que se borre tanto de azure como de nuestro local
        async Task<string> Editar(string? ruta, string contenedor, IFormFile archivo)
        {
            await Borrar(ruta, contenedor);
            return await Almacenar (contenedor, archivo);
        }
    }
}
