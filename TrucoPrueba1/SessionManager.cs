using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
    public static class SessionManager
    {
        private static readonly TrucoCallbackHandler callbackHandler = new TrucoCallbackHandler();
        private static readonly InstanceContext userContext = new InstanceContext(callbackHandler);

        private static TrucoUserServiceClient userClient;
        private static TrucoMatchServiceClient matchClient;
        private static TrucoFriendServiceClient friendClient;

        public static UserProfileData CurrentUserData { get; set; }

        public static TrucoUserServiceClient UserClient
        {
            get
            {
                if (userClient == null ||
                    userClient.State == System.ServiceModel.CommunicationState.Faulted ||
                    userClient.State == System.ServiceModel.CommunicationState.Closed)
                {
                    try
                    {
                        userClient?.Abort();
                    }
                    catch
                    {

                    }
                    userClient = new TrucoUserServiceClient(userContext, "NetTcpBinding_ITrucoUserService");
                }
                return userClient;
            }
        }

        public static TrucoMatchServiceClient MatchClient
        {
            get
            {
                if (matchClient == null ||
                    matchClient.State == System.ServiceModel.CommunicationState.Faulted ||
                    matchClient.State == System.ServiceModel.CommunicationState.Closed)
                {
                    try
                    {
                        matchClient?.Abort();
                    }
                    catch
                    {

                    }
                    matchClient = new TrucoMatchServiceClient(userContext, "NetTcpBinding_ITrucoMatchService");
                }
                return matchClient;
            }
        }

        public static TrucoFriendServiceClient FriendClient
        {
            get
            {
                if (friendClient == null ||
                    friendClient.State == System.ServiceModel.CommunicationState.Faulted ||
                    friendClient.State == System.ServiceModel.CommunicationState.Closed)
                {
                    try
                    {
                        friendClient?.Abort();
                    }
                    catch
                    {

                    }
                    friendClient = new TrucoFriendServiceClient(userContext, "NetTcpBinding_ITrucoFriendService");
                }
                return friendClient;
            }
        }
        public static string CurrentUsername { get; set; } = "UsuarioActual";
        public static async Task<string> ResolveUsernameAsync(string usernameOrEmail)
        {
            try
            {
                if (usernameOrEmail.Contains("@"))
                {
                    var profile = await UserClient.GetUserProfileByEmailAsync(usernameOrEmail);
                    if (profile != null)
                    {
                        return profile.Username;
                    }
                }
                return usernameOrEmail;
            }
            catch
            {
                return usernameOrEmail;
            }
        }
        public static void ClearSession()
        {
            try
            {
                if (userClient != null)
                {
                    if (userClient.State == System.ServiceModel.CommunicationState.Opened)
                    {
                        userClient.Close();
                    }
                    else
                    {
                        userClient.Abort();
                    }
                    userClient = null;
                }
            }
            catch
            {
                userClient = null;
            }

            CurrentUserData = null;
            CurrentUsername = "UsuarioActual";
        }
    }
}
