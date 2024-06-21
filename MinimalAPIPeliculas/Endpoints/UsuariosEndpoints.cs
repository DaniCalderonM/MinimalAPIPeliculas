using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Filtros;
using MinimalAPIPeliculas.Servicios;
using MinimalAPIPeliculas.Utilidades;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static RouteGroupBuilder MapUsuarios(this RouteGroupBuilder group)
        {
            group.MapPost("/registrar", Registrar)
                .AddEndpointFilter<FiltroValidaciones<CredencialesUsuarioDTO>>();
            group.MapPost("/login", Login)
                .AddEndpointFilter<FiltroValidaciones<CredencialesUsuarioDTO>>();

            group.MapPost("/haceradmin", HacerAdmin).AddEndpointFilter<FiltroValidaciones<EditarClaimDTO>>()
            .RequireAuthorization("esadmin");

            group.MapPost("/removeradmin", RemoverAdmin).AddEndpointFilter<FiltroValidaciones<EditarClaimDTO>>()
            .RequireAuthorization("esadmin");

            group.MapGet("/renovartoken", RenovarToken).RequireAuthorization();

            return group;
        }

        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<IEnumerable<IdentityError>>>>
            Registrar(CredencialesUsuarioDTO credencialesUsuarioDTO,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuarioDTO.Email,
                Email = credencialesUsuarioDTO.Email
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.Password);

            if (resultado.Succeeded)
            {
                var credencialesRespuesta = await ConstruirToken(credencialesUsuarioDTO, configuration, userManager);
                return TypedResults.Ok(credencialesRespuesta);
            }
            else
            {
                return TypedResults.BadRequest(resultado.Errors);
            }
        }

        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<string>>> Login(
            CredencialesUsuarioDTO credencialesUsuarioDTO, [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);

            if (usuario is null)
            {
                return TypedResults.BadRequest("Login incorrecto");
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario,
                // el lockoutOnFailure es para bloquear la cuenta despues de ciertos intentos
                //fallidos, en este caso se pone en false, para que no nos bloquee la cuenta
                credencialesUsuarioDTO.Password, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO, configuration, userManager);
                return TypedResults.Ok(respuestaAutenticacion);
            }
            else
            {
                return TypedResults.BadRequest("Login incorrecto");
            }

        }

        static async Task<Results<NoContent, NotFound>> HacerAdmin(EditarClaimDTO editarClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.AddClaimAsync(usuario, new Claim("esadmin", "true"));
            return TypedResults.NoContent();

        }

        static async Task<Results<NoContent, NotFound>> RemoverAdmin(EditarClaimDTO editarClaimDTO,
    [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "true"));
            return TypedResults.NoContent();

        }

        public async static Task<Results<Ok<RespuestaAutenticacionDTO>, NotFound>> RenovarToken(
            IServicioUsuarios servicioUsuarios, IConfiguration configuration,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var credencialesUsuariosDTO = new CredencialesUsuarioDTO { Email = usuario.Email! };
            var respuestaAutenticacionDTO = await ConstruirToken(credencialesUsuariosDTO, configuration,
                userManager);

            return TypedResults.Ok(respuestaAutenticacionDTO);

        }


        //Creacion y configuracion del token
        private async static Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credencialesUsuarioDTO,
            IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            var claims = new List<Claim>
            {
                new Claim("email", credencialesUsuarioDTO.Email)
            };

            var usuario = await userManager.FindByNameAsync(credencialesUsuarioDTO.Email);
            
            var claimsDB = await userManager.GetClaimsAsync(usuario!);
            //Estamos cargando los claims de la bbdd y se los estamos anexando al listado
            // que esta arriba
            claims.AddRange(claimsDB);


            var llave = Llaves.ObtenerLlave(configuration);
            var credenciales = new SigningCredentials(llave.First(), SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: credenciales);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }

    }
}
