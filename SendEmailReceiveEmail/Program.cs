using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;

namespace SendEmailReceiveEmail
{
    class Program
    {
        private static GraphServiceClient _graphServiceClient;
        static async Task Main(string[] args)
        {

            var userid = "f51bd33a-2d64-4b09-9b85-7c4efc24b16c";
            GraphServiceClient graphClient = GetAuthenticatedGraphClient();
            var mailboxhelper = new MailboxHelper(graphClient);
            List<ResultsItem> items = mailboxhelper.ListInboxMessages(userid).Result;
            var toRec = new Recipient() { EmailAddress = new EmailAddress() { Address = "tka@cloudmission.net" } };

            Message mailbody = new Message()
            {
                Body = new ItemBody() { Content = "This is Test email from console application", ContentType = BodyType.Text },
                Subject = "Test Mail",
                ToRecipients = new List<Recipient>()
                {
                    toRec
                }
            };

             await mailboxhelper.SendDKBSMail(userid, mailbody);
        }

        private static GraphServiceClient GetAuthenticatedGraphClient()
        {
            var authenticationProvider = CreateAuthorizationProvider();
            _graphServiceClient = new GraphServiceClient(authenticationProvider);
            return _graphServiceClient;
        }

        private static IAuthenticationProvider CreateAuthorizationProvider()
        {
            var clientId = "444351bf-2e36-4fff-a938-6c94e19b0107";
            var clientSecret = "L._+aGfgo61q/kt0RjU6bU]*jG*g9NkT";
            var redirectUri = "https://localhost.com";
            var tenantId = "4341b00e-adab-4fc1-92bd-0167af58f34b";
            var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

            List<string> scopes = new List<string>();
            scopes.Add("https://graph.microsoft.com/.default");

            var cca = ConfidentialClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithRedirectUri(redirectUri)
                                                    .WithClientSecret(clientSecret)
                                                    .Build();
            return new MsalAuthenticationProvider(cca, scopes.ToArray());

        }
    }

    public class MsalAuthenticationProvider : IAuthenticationProvider
    {
        private IConfidentialClientApplication _clientApplication;
        private string[] _scopes;

        public MsalAuthenticationProvider(IConfidentialClientApplication clientApplication, string[] scopes)
        {
            _clientApplication = clientApplication;
            _scopes = scopes;
        }

        /// <summary>
        /// Update HttpRequestMessage with credentials
        /// </summary>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        /// <summary>
        /// Acquire Token 
        /// </summary>
        public async Task<string> GetTokenAsync()
        {
            AuthenticationResult authResult = null;
            authResult = await _clientApplication.AcquireTokenForClient(_scopes)
                                .ExecuteAsync();
            return authResult.AccessToken;
        }
    }

    public class MailboxHelper
    {
        private GraphServiceClient _graphClient;

        public MailboxHelper(GraphServiceClient graphClient)
        {
            if (null == graphClient) throw new ArgumentNullException(nameof(graphClient));
            _graphClient = graphClient;
        }

        public async Task<List<ResultsItem>> ListInboxMessages(string userid)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            IMailFolderMessagesCollectionPage messages = await _graphClient.Users[userid].MailFolders.Inbox.Messages.Request().Top(10).GetAsync();
            if (messages?.Count > 0)
            {
                foreach (Message message in messages)
                {
                    items.Add(new ResultsItem
                    {
                        Display = message.Subject,
                        Id = message.Id
                    });
                }
            }
            return items;
        }

        public async Task SendDKBSMail(string userid,Message mailmessage)
        {
            try { 
            await _graphClient.Users[userid].SendMail(mailmessage, true).Request().PostAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("We could not send the message: " + ex.Message);
            }
        }
    }

    public class ResultsItem
    {

        // The ID and display name for the entity's radio button.
        public string Id { get; set; }
        public string Display { get; set; }

        // The properties of an entity that display in the UI.
        public Dictionary<string, object> Properties;

        public ResultsItem()
        {
            Properties = new Dictionary<string, object>();
        }
    }
}
