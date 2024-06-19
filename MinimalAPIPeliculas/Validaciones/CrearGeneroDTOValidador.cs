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
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("El campo {PropertyName} es requerido")
                     .MaximumLength(50).WithMessage("El campo {PropertyName} debe tener menos de {MaxLength} caracteres")
                     .Must(PrimeraLetraEnMayusculas).WithMessage("El campo {PropertyName} debe comenzar con mayusculas")
                     .MustAsync(async (nombre, _) =>
                     {
                         var existe = await repositorioGeneros.Existe(id, nombre);
                         return !existe;
                     }).WithMessage(g => $"Ya existe un genero con el nombre {g.Nombre}");
        }

        private bool PrimeraLetraEnMayusculas(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return true;
            }

            var primeraLetra = valor[0].ToString();
            return primeraLetra == primeraLetra.ToUpper();

        }
    }
}
