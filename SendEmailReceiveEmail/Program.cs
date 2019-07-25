using System;
using System.Data;
using System.IdentityModel;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;

namespace SendEmailReceiveEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientId = "444351bf-2e36-4fff-a938-6c94e19b0107";

            var clientSecret = "L._+aGfgo61q/kt0RjU6bU]*jG*g9NkT";

            var redirectUri = "https://localhost.com";
            var tenantId = "4341b00e-adab-4fc1-92bd-0167af58f34b";

            var authority = "https://login.microsoftonline.com/4341b00e-adab-4fc1-92bd-0167af58f34b/v2.0";

            var cca = new ConfidentialClientApplication(clientId, authority, redirectUri, new ClientCredential(clientSecret), null, null);



            // use the default permissions assigned from within the Azure AD app registration portal

            List<string> scopes = new List<string>();

            scopes.Add("https://graph.microsoft.com/.default");



            var authenticationProvider = new MsalAuthenticationProvider(cca, scopes.ToArray());

            GraphServiceClient graphClient = new GraphServiceClient(authenticationProvider);
        }
    }
    public class MsalAuthenticationProvider : IAuthenticationProvider

    {

        private ConfidentialClientApplication _clientApplication;

        private string[] _scopes;



        public MsalAuthenticationProvider(ConfidentialClientApplication clientApplication, string[] scopes)
        {

            _clientApplication = clientApplication;

            _scopes = scopes;

        }



        public async Task AuthenticateRequestAsync(HttpRequestMessage request)

        {

            var token = await GetTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

        }



        public async Task<string> GetTokenAsync()

        {

            AuthenticationResult authResult = null;

            authResult = await _clientApplication.AcquireTokenForClientAsync(_scopes);

            return authResult.AccessToken;

        }

    }
}
