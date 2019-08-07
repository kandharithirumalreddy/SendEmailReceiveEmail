using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using System.Linq;

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
            List<Message> inboxitems = mailboxhelper.ListInboxMessages(userid,"test").Result;
            List<Message> sentitems = mailboxhelper.ListSentMessages(userid, "string").Result;
            var toRec = new Recipient() { EmailAddress = new EmailAddress() { Address = "tka@cloudmission.net" } };

            Message mailbody = new Message()
            {
                Body = new ItemBody() { Content = Constants.getTemplate1("bkd1"), ContentType = BodyType.Html },
                Subject = Constants.template1Subject,
                ToRecipients = new List<Recipient>()
                {
                    toRec
                }
            };

             //await mailboxhelper.SendDKBSMail(userid, mailbody);
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

        /// <summary>
        /// MEthod for fetching the Inbox messages
        /// </summary>
        /// <param name="userid">Messages from which user</param>
        /// <param name="searchparam1">filter parameter for the subject</param>
        /// <returns></returns>
        public async Task<List<Message>> ListInboxMessages(string userid,string searchparam)
        {
            //List<ResultsItem> items = new List<ResultsItem>();

            IMailFolderMessagesCollectionPage messages = await _graphClient.Users[userid].MailFolders.Inbox.Messages.Request().Top(100).GetAsync();
            //List<Message> fmsgs = messages.ToList<Message>().Where(i => i.Subject.ToLower().Contains(searchparam)).ToList<Message>();
            //if (messages?.Count > 0)
            //{
            //    foreach (Message message in messages)
            //    {
            //        items.Add(new ResultsItem
            //        {
            //            Subject = message.Subject,
            //            Id = message.Id
            //        });
            //    }
            //}

            return messages.ToList().Where(i => i.Subject.ToLower().Contains(searchparam)).ToList<Message>();
        }

        public async Task<List<Message>> ListSentMessages(string userid,string fparam1)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            List<QueryOption> options = new List<QueryOption>()
            {
                new QueryOption("filter","folder ne null"),
                new QueryOption("select","id,name,webUrl")
             };

            IMailFolderMessagesCollectionPage messages = await _graphClient.Users[userid].MailFolders.SentItems.Messages.Request().Top(100).GetAsync();
            return messages.ToList().Where(i => i.Subject.ToLower().Contains(fparam1)).ToList<Message>();
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
        public string Subject { get; set; }

        // The properties of an entity that display in the UI.
        public Dictionary<string, object> Properties;

        public ResultsItem()
        {
            Properties = new Dictionary<string, object>();
        }
    }
}
