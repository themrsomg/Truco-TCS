using System;
using System.Threading.Tasks;
using TrucoClient.Helpers.Services;
using TrucoClient.Helpers.Session;

namespace TrucoClient.Helpers.Exceptions
{
    public static class ClientException
    {
        public static void HandleError(Exception ex, string contextName)
        {
            Task.Run(() => SendErrorToServer(ex));
        }

        private static void SendErrorToServer(Exception ex)
        {
            try
            {
                var userClient = ClientManager.UserClient;
                string username = SessionManager.CurrentUsername ?? "Unknown";

                userClient.LogClientException(ex.Message, ex.StackTrace, username);
            }
            catch (Exception)
            {
                // Ignote
            }
        }
    }
}
