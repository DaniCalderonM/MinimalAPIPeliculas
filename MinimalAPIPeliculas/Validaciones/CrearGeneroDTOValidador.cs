using FluentValidation;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.Validaciones
{
    // Heredando de la clase AbstractValidator
    public class CrearGeneroDTOValidador : AbstractValidator<CrearGeneroDTO>
    {
        public CrearGeneroDTOValidador(IRepositorioGeneros repositorioGeneros,
            //Utilizamos httpContextAccessor para ser capaces de obtener el parametro pasado en la ruta
            IHttpContextAccessor httpContextAccessor)
        {
            var valorDeRutaId = httpContextAccessor.HttpContext?.Request.RouteValues["id"];
            var id = 0;

            if (valorDeRutaId is string valorString)
            { 
                int.TryParse(valorString, out id );
            }

            //Con el placeholder {PropertyName} hacemos que tome el nombre del campo de forma automatica
            RuleFor(x => x.Nombre).NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje)
                     .MaximumLength(50).WithMessage(Utilidades.MaximumLengthMensaje)
                     .Must(Utilidades.PrimeraLetraEnMayusculas).WithMessage(Utilidades.PrimeraLetraMayusculaMensaje)
                     .MustAsync(async (nombre, _) =>
                     {
                         var existe = await repositorioGeneros.Existe(id, nombre);
                         return !existe;
                     }).WithMessage(g => $"Ya existe un genero con el nombre {g.Nombre}");
        }
    }
}
