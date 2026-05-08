using System;
using System.ServiceModel;
using System.Windows;
using TrucoClient.Helpers.Exceptions;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;
using TrucoClient.Views;

namespace TrucoClient.Helpers.Services
{
    public static class ClientManager
    {
        private const string MESSAGE_ERROR = "Error";

        private static TrucoCallbackHandler callbackHandler = new TrucoCallbackHandler();
        private static InstanceContext context = new InstanceContext(callbackHandler);

        private static TrucoUserServiceClient userClient;
        private static TrucoMatchServiceClient matchClient;
        private static TrucoFriendServiceClient friendClient;
        private static TrucoTournamentServiceClient tournamentClient;

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

        public static TrucoTournamentServiceClient TournamentClient
        {
            get
            {
                if (tournamentClient == null || tournamentClient.State == CommunicationState.Faulted || tournamentClient.State == CommunicationState.Closed)
                {
                    SafeAbort(tournamentClient);
                    tournamentClient = new TrucoTournamentServiceClient(context, "NetTcpBinding_ITrucoTournamentService");
                }

                return tournamentClient;
            }
        }

        public static void CloseAllClients()
        {
            SafeClose(userClient);
            SafeClose(matchClient);
            SafeClose(friendClient);
            SafeClose(tournamentClient);

            userClient = null;
            matchClient = null;
            friendClient = null;
            tournamentClient = null;
        }

        public static void ResetConnections()
        {
            try
            {
                CloseAllClients();
                callbackHandler = new TrucoCallbackHandler();
                context = new InstanceContext(callbackHandler);
            }
            catch (TimeoutException ex)
            {
                ClientException.HandleError(ex, nameof(ResetConnections));
                CustomMessageBox.Show(Lang.ExceptionTextTimeout,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (EndpointNotFoundException ex)
            {
                ClientException.HandleError(ex, nameof(ResetConnections));
                CustomMessageBox.Show(Lang.ExceptionTextConnectionError,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                ClientException.HandleError(ex, nameof(ResetConnections));
                CustomMessageBox.Show(Lang.ExceptionTextCommunication,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ClientException.HandleError(ex, nameof(ResetConnections));
                CustomMessageBox.Show(Lang.ExceptionTextErrorRestartingConnections,
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            catch (TimeoutException)
            {
                client.Abort();
            }
            catch (CommunicationException)
            {
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
            }
        }

        private static void SafeAbort(ICommunicationObject client)
        {
            try
            {
                if (client != null)
                {
                    client.Abort();
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void SetCallbackHandler(GameBasePage gameBasePage)
        {
        }
    }
}