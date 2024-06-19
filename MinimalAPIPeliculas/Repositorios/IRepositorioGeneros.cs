using MinimalAPIPeliculas.Entidades;

namespace MinimalAPIPeliculas.Repositorios
{
    public interface IRepositorioGeneros
    {
        // Metodo para obtener un listado de todos los generos
        Task<List<Genero>> ObtenerTodos();
        // Metodo para obtener un genero por su id
        Task<Genero?> ObtenerPorId(int id);
        // Metodo para crear un genero
        Task<int> Crear(Genero genero);

        //Metodo para preguntar si existe el id con un boolean
        Task<bool> Existe(int id);

        // Metodo para actualizar el genero
        Task Actualizar(Genero genero);

        // Metodo para borrar un genero
        Task Borrar(int id);
        Task<List<int>> Existen(List<int> ids);
        Task<bool> Existe(int id, string nombre);
    }
}
