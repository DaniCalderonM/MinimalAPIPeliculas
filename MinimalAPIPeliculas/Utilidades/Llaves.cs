using Microsoft.IdentityModel.Tokens;

namespace MinimalAPIPeliculas.Utilidades
{   //Clase auxiliar para obtener las llaves
    public static class Llaves
    {
        public const string IssuerPropio = "nuestra-app";
        private const string SeccionLlaves = "Authentication:Schemes:Bearer:SigningKeys";
        private const string SeccionLlaves_Emisor = "Issuer";
        private const string SeccionLlaves_Valor = "Value";

        //metodo para no tener que escribir siempre lo de nuestra app, e sun metodo auxiliar que reutiliza
        // ObtenerLlave pasandole nuestro emisor
        public static IEnumerable<SecurityKey> ObtenerLlave(IConfiguration configuration)
            => ObtenerLlave(configuration, IssuerPropio);

        public static IEnumerable<SecurityKey> ObtenerLlave(IConfiguration configuration, string issuer)
        {
            var signingKey = configuration.GetSection(SeccionLlaves)
                .GetChildren()
                .SingleOrDefault(llave => llave[SeccionLlaves_Emisor] == issuer);

            if (signingKey is not null && signingKey[SeccionLlaves_Valor] is string valorLlave)
            {
                yield return new SymmetricSecurityKey(Convert.FromBase64String(valorLlave));
            }
        }

        public static IEnumerable<SecurityKey> ObtenerTodasLasLlaves(IConfiguration configuration)
        {
            var signingKeys = configuration.GetSection(SeccionLlaves)
                .GetChildren();

            foreach (var signingKey in signingKeys)
            {
                if (signingKey[SeccionLlaves_Valor] is string valorLlave)
                {
                    yield return new SymmetricSecurityKey(Convert.FromBase64String(valorLlave));
                }
            }
        }

        

    }
}
