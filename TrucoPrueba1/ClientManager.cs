using System.ServiceModel;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
    public static class ClientManager
    {
        private static readonly TrucoCallbackHandler callbackHandler = new TrucoCallbackHandler();
        private static readonly InstanceContext context = new InstanceContext(callbackHandler);

        private static TrucoUserServiceClient userClient;
        private static TrucoMatchServiceClient matchClient;
        private static TrucoFriendServiceClient friendClient;

        public static TrucoUserServiceClient UserClient
        {
            get
            {
                if (userClient == null || userClient.State == CommunicationState.Faulted || userClient.State == CommunicationState.Closed)
                {
                    SafeAbort(userClient);
                    userClient = new TrucoUserServiceClient(context, "NetTcpBinding_ITrucoUserService");
                }
                return userClient;
            }
        }

        public static TrucoMatchServiceClient MatchClient
        {
            get
            {
                if (matchClient == null || matchClient.State == CommunicationState.Faulted || matchClient.State == CommunicationState.Closed)
                {
                    SafeAbort(matchClient);
                    matchClient = new TrucoMatchServiceClient(context, "NetTcpBinding_ITrucoMatchService");
                }
                return matchClient;
            }
        }

        public static TrucoFriendServiceClient FriendClient
        {
            get
            {
                if (friendClient == null || friendClient.State == CommunicationState.Faulted || friendClient.State == CommunicationState.Closed)
                {
                    SafeAbort(friendClient);
                    friendClient = new TrucoFriendServiceClient(context, "NetTcpBinding_ITrucoFriendService");
                }
                return friendClient;
            }
        }

        public static void CloseAllClients()
        {
            SafeClose(userClient);
            SafeClose(matchClient);
            SafeClose(friendClient);

            userClient = null;
            matchClient = null;
            friendClient = null;
        }

        private static void SafeClose(ICommunicationObject client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                if (client.State == CommunicationState.Opened)
                {
                    client.Close();
                }
                else
                {
                    client.Abort();
                }
            }
            catch
            {
                client.Abort();
            }
        }

        private static void SafeAbort(ICommunicationObject client)
        {
            try
            {
                client?.Abort();
            }
            catch { }
        }
    }
}
