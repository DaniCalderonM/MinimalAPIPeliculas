using FluentValidation;
using MinimalAPIPeliculas.DTOs;

namespace MinimalAPIPeliculas.Validaciones
{
    // Estamos heredando de AbstractValidator
    public class CrearActorDTOValidador: AbstractValidator<CrearActorDTO>
    {
        public CrearActorDTOValidador()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("El campo {PropertyName} es requerido")
                .MaximumLength(150).WithMessage("El campo {PropertyName} debe tener menos de {MaxLength} caracteres");

            var fechaMinima = new DateTime(1900, 1, 1);

            RuleFor(x => x.FechaNacimiento).GreaterThanOrEqualTo(fechaMinima)
                .WithMessage("El campo {PropertyName} debe ser posterior a " + fechaMinima.ToString("yyyy-MM-dd"));

        }
    }
}
