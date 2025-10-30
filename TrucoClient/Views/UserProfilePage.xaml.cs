using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TrucoClient.Properties.Langs;
using TrucoClient.TrucoServer;
using TrucoClient.Views;

namespace TrucoClient
{
    public partial class UserProfilePage : Page
    {
        private UserProfileData currentUserData;
        private const int MAX_CHANGES = 2;
        private const String URL_AVATAR_DEFAULT = "pack://application:,,,/TrucoClient;component/Resources/Avatars/avatar_aaa_default.png";
        private const int MIN_USERNAME_LENGTH = 4;
        private const int MAX_USERNAME_LENGTH = 20;
        private const int MIN_TEXT_LENGTH = 4;
        private const int MAX_TEXT_LENGTH = 20;
        private string originalUsername;

        private readonly List<string> availableAvatars = new List<string>
        {
            "avatar_aaa_default", "avatar_c_hr_rallycarback",
            "avatar_c_hr_rallycarfront", "avatar_c_hr_redcarback", "avatar_c_hr_redcarfront",
            "avatar_c_nr_jetski", "avatar_fc_boxer", "avatar_fc_boxing", "avatar_fc_bw_bigman",
            "avatar_fc_bw_hippie", "avatar_fc_bw_skull", "avatar_fc_cod_soldierone",
            "avatar_fc_cod_soldierthree", "avatar_fc_cod_soldiertwo", "avatar_fc_gv_chop",
            "avatar_fc_gv_franklin", "avatar_fc_gv_michael", "avatar_fc_gv_trevor",
            "avatar_fc_h_warriorblue", "avatar_fc_h_warriorgreen", "avatar_fc_hr_a_husky",
            "avatar_fc_hr_a_marathonpig", "avatar_fc_hr_a_monkey", "avatar_fc_hr_a_teddybear", 
            "avatar_fc_hr_ap_robberbandana", "avatar_fc_hr_ap_robbergasmask", "avatar_fc_hr_ap_vampire",
            "avatar_fc_hr_ap_zombie", "avatar_fc_hr_bigchin", "avatar_fc_hr_p_blondegirl",
            "avatar_fc_hr_p_bullskull", "avatar_fc_hr_p_demonicninja", "avatar_fc_hr_p_emorocker", 
            "avatar_fc_hr_p_hobbit", "avatar_fc_hr_p_winterqueen", "avatar_fc_twod_c_angryman", 
            "avatar_fc_twod_c_aviator", "avatar_fc_twod_c_biker", "avatar_fc_twod_c_cowboyone", 
            "avatar_fc_twod_c_demongirl", "avatar_fc_twod_c_gamerskull", "avatar_fc_twod_c_hitman", 
            "avatar_fc_twod_c_jesus", "avatar_fc_twod_c_pirate", "avatar_fc_twod_c_plug", 
            "avatar_fc_twod_t_alien", "avatar_fc_twod_t_banjo", "avatar_fc_twod_t_bubblegum", 
            "avatar_fc_twod_t_cowboytwo", "avatar_fc_twod_t_cyberpanda", "avatar_fc_twod_t_dog", 
            "avatar_fc_twod_t_girl", "avatar_fc_twod_t_ninja", "avatar_fc_twod_t_singingman", 
            "avatar_fl_argentina", "avatar_fl_bolivia", "avatar_fl_brasil", "avatar_fl_canada", 
            "avatar_fl_chile", "avatar_fl_colombia", "avatar_fl_costarica", "avatar_fl_cuba", 
            "avatar_fl_dominican", "avatar_fl_ecuador", "avatar_fl_elsalvador", "avatar_fl_guatemala", 
            "avatar_fl_honduras", "avatar_fl_mexico", "avatar_fl_nicaragua", "avatar_fl_panama", 
            "avatar_fl_paraguay", "avatar_fl_peru", "avatar_fl_puertorico", "avatar_fl_spain", 
            "avatar_fl_uk", "avatar_fl_uruguay", "avatar_fl_usa", "avatar_fl_venezuela", 
            "avatar_ft_america", "avatar_ft_atlas", "avatar_ft_atleticosanluis", "avatar_ft_chivas", 
            "avatar_ft_cruzazul", "avatar_ft_fcjuarez", "avatar_ft_leon", "avatar_ft_mazatlan", 
            "avatar_ft_monterrey", "avatar_ft_necaxa", "avatar_ft_pachuca", "avatar_ft_pueblafc", 
            "avatar_ft_pumas", "avatar_ft_queretaro", "avatar_ft_santos", "avatar_ft_tiburones", 
            "avatar_ft_tigres", "avatar_ft_tijuana", "avatar_ft_toluca", "avatar_t_hr_bigdaddy", 
            "avatar_t_hr_eagle", "avatar_t_hr_football", "avatar_t_hr_goldensun", "avatar_t_hr_golf", 
            "avatar_t_hr_lime", "avatar_t_hr_smiley", "avatar_t_hr_wheel", "avatar_t_logo_skate", 
            "avatar_t_mc_cookie", "avatar_t_mc_goldenapple", "avatar_t_mc_porkchop", 
            "avatar_t_txt_bigbomb", "avatar_t_txt_bigreds", "avatar_t_txt_ggheart", "avatar_t_txt_hammer", 
            "avatar_t_txt_lisgreen", "avatar_t_txt_lisred", "avatar_t_txt_moneybag", "avatar_t_txt_pumpkin", 
            "avatar_t_txt_saucy", "avatar_t_txt_soap", "avatar_t_txt_softwaresquad", "avatar_t_txt_sq", 
            "avatar_tt_dragon", "avatar_tt_lilbird", "avatar_tt_revolver", "avatar_tt_twentydollars",
        };

        public UserProfilePage()
        {
            InitializeComponent();
            originalUsername = string.Empty;
            LoadUserProfile();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void ClickSave(object sender, RoutedEventArgs e)
        {
            if (currentUserData == null)
            {
                return;
            }

            string newUsername = txtUsername.Text.Trim();
            string newFacebook = txtFacebookLink.Text.Trim();
            string newX = txtXLink.Text.Trim();
            string newInstagram = txtInstagramLink.Text.Trim();

            if (HasUnsavedChanges(newUsername, newFacebook, newX, newInstagram))
            {
                var confirm = MessageBox.Show(
                    Lang.UserProfileTextConfirmChanges,
                    Lang.GlobalTextConfirmation,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            Button saveButton = sender as Button;
            saveButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                string oldUsername = currentUserData.Username;
                int oldChangeCount = currentUserData.NameChangeCount;
                string oldFacebook = currentUserData.FacebookHandle;
                string oldX = currentUserData.XHandle;
                string oldInstagram = currentUserData.InstagramHandle;

                var userClient = ClientManager.UserClient;

                UpdateCurrentUserDataFromFields(newUsername, newFacebook, newX, newInstagram);

                if (!NewUserValidation(newUsername))
                {
                    return;
                }

                if (await UsernameExistsAsync(newUsername, userClient))
                {
                    currentUserData.Username = oldUsername;
                    txtUsername.Text = oldUsername;
                    return;
                }

                bool success = await userClient.SaveUserProfileAsync(currentUserData);

                if (success)
                {
                    if (!string.Equals(newUsername, originalUsername, StringComparison.Ordinal))
                    {
                        currentUserData.NameChangeCount++;
                    }

                    LoadUserProfile();

                    MessageBox.Show(Lang.UserProfileTextSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);

                    SessionManager.CurrentUserData = currentUserData;
                    SessionManager.CurrentUsername = currentUserData.Username;
                    originalUsername = currentUserData.Username;

                }
                else
                {
                    currentUserData.Username = oldUsername;
                    currentUserData.NameChangeCount = oldChangeCount;
                    currentUserData.FacebookHandle = oldFacebook;
                    currentUserData.XHandle = oldX;
                    currentUserData.InstagramHandle = oldInstagram;
                    txtUsername.Text = oldUsername;

                    MessageBox.Show(Lang.UserProfileTextErrorSaving, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                MessageBox.Show($"No se pudo conectar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                saveButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private void ClickChangeAvatar(object sender, RoutedEventArgs e)
        {
            if (currentUserData == null)
            {
                return;
            }

            var avatarPage = new Views.AvatarSelectionPage(availableAvatars, currentUserData.AvatarId);
            avatarPage.AvatarSelected += AvatarPage_AvatarSelected;
            NavigationService.Navigate(avatarPage);
        }

        private async void AvatarPage_AvatarSelected(object sender, string newAvatarId)
        {
            if (sender is Views.AvatarSelectionPage avatarPage)
            {
                avatarPage.AvatarSelected -= AvatarPage_AvatarSelected;
            }

            if (newAvatarId == currentUserData.AvatarId)
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                var userClient = ClientManager.UserClient;

                bool success = await userClient.UpdateUserAvatarAsync(SessionManager.CurrentUsername, newAvatarId);

                if (success)
                {
                    currentUserData.AvatarId = newAvatarId;

                    this.DataContext = null;
                    this.DataContext = currentUserData;
                    LoadAvatarImage(newAvatarId);

                    MessageBox.Show(Lang.UserProfileTextAvatarSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(Lang.UserProfileTextAvatarError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                MessageBox.Show($"No se pudo conectar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private void ClickChangePassword(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ChangePasswordPage());
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void UsernamePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$");
        }

        private void LoadUserProfile()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                if (SessionManager.CurrentUserData != null)
                {
                    currentUserData = SessionManager.CurrentUserData;
                    originalUsername = currentUserData.Username ?? string.Empty;

                    this.DataContext = currentUserData;
                    LoadAvatarImage(currentUserData.AvatarId);

                    txtUsername.Text = currentUserData.Username;
                    txtEmail.Text = currentUserData.Email;
                    txtFacebookLink.Text = currentUserData.FacebookHandle;
                    txtXLink.Text = currentUserData.XHandle;
                    txtInstagramLink.Text = currentUserData.InstagramHandle;

                    UpdateUsernameWarning(currentUserData.NameChangeCount);
                    UpdateSocialMediaLinks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void LoadAvatarImage(string avatarId)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = "avatar_aaa_default";
            }

            string packUri = $"pack://application:,,,/TrucoClient;component/Resources/Avatars/{avatarId}.png";

            try
            {
                imgAvatar.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch
            {
                imgAvatar.Source = new BitmapImage(new Uri(URL_AVATAR_DEFAULT, UriKind.Absolute));
            }
        }

        private async Task<bool> UsernameExistsAsync(string username, TrucoUserServiceClient userClient)
        {
            bool usernameExists = await Task.Run(() => userClient.UsernameExists(username));

            if (usernameExists)
            {
                ShowError(txtUsername, Lang.GlobalTextUsernameAlreadyInUse);
                txtUsername.Text = originalUsername;
            }

            return usernameExists;
            
        }

        private void UpdateUsernameWarning(int count)
        {
            int remaining = MAX_CHANGES - count;
            txtUsernameWarning.Text = string.Format(Lang.UserProfileTextUsernameChangesWarning, remaining, MAX_CHANGES);
            txtUsernameWarning.Visibility = Visibility.Visible;

            if (remaining <= 0)
            {
                txtUsernameWarning.Text = Lang.UserProfileTextChangesError;
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Red);
                txtUsername.IsReadOnly = true;
            }
            else if (remaining == 1)
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                txtUsernameWarning.Foreground = new SolidColorBrush(Colors.White); 
            }
        }

        private void UpdateSocialMediaLinks()
        {
            linkFacebookContainer.Visibility = string.IsNullOrWhiteSpace(txtFacebookLink.Text.Trim()) ? Visibility.Collapsed : Visibility.Visible;
            linkXContainer.Visibility = string.IsNullOrWhiteSpace(txtXLink.Text.Trim()) ? Visibility.Collapsed : Visibility.Visible;
            linkInstagramContainer.Visibility = string.IsNullOrWhiteSpace(txtInstagramLink.Text.Trim()) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateCurrentUserDataFromFields(string newUsername, string newFacebook, string newX, string newInstagram)
        {
            bool usernameChanged = !string.IsNullOrWhiteSpace(originalUsername) && !string.Equals(newUsername, originalUsername, StringComparison.Ordinal);
            bool facebookChanged = !string.Equals(newFacebook, currentUserData.FacebookHandle, StringComparison.Ordinal);
            bool xChanged = !string.Equals(newX, currentUserData.XHandle, StringComparison.Ordinal);
            bool instagramChanged = !string.Equals(newInstagram, currentUserData.InstagramHandle, StringComparison.Ordinal);

            if (usernameChanged)
            {
                currentUserData.Username = newUsername;
            }
            if (facebookChanged)
            {
                currentUserData.FacebookHandle = newFacebook;
            }
            if (xChanged)
            {
                currentUserData.XHandle = newX;
            }
            if (instagramChanged)
            {
                currentUserData.InstagramHandle = newInstagram;
            }
        }

        private bool HasUnsavedChanges(string newUsername, string newFacebook, string newX, string newInstagram)
        {
            bool hasUnsavedChanges = true;

            bool usernameChanged = !string.IsNullOrWhiteSpace(originalUsername) && !string.Equals(newUsername, originalUsername, StringComparison.Ordinal);
            bool facebookChanged = !string.Equals(newFacebook, currentUserData.FacebookHandle, StringComparison.Ordinal);
            bool xChanged = !string.Equals(newX, currentUserData.XHandle, StringComparison.Ordinal);
            bool instagramChanged = !string.Equals(newInstagram, currentUserData.InstagramHandle, StringComparison.Ordinal);

            if (!usernameChanged && !facebookChanged && !xChanged && !instagramChanged)
            {
                MessageBox.Show(Lang.UserProfileTextSaveNoChanges, Lang.UserProfileTextNoChanges, MessageBoxButton.OK, MessageBoxImage.Information);

                hasUnsavedChanges = false;
            }

            return hasUnsavedChanges;
        }

        private bool NewUserValidation (string newUsername)
        {
            ClearAllErrors();

            bool isValid = true;

            bool usernameChanged = !string.IsNullOrWhiteSpace(originalUsername) && !string.Equals(newUsername, originalUsername, StringComparison.Ordinal);

            if (usernameChanged && newUsername.Length < MIN_USERNAME_LENGTH)
            {
                ShowError(txtUsername, Lang.DialogTextShortUsername);
                isValid = false;
            }

            if (usernameChanged && newUsername.Length > MAX_USERNAME_LENGTH)
            {
                ShowError(txtUsername, Lang.DialogTextLongUsername);
                isValid = false;
            }
            else if (!IsValidUsername(newUsername))
            {
                ShowError(txtUsername, Lang.GlobalTextInvalidUsername);
                isValid = false;
            }

            if (usernameChanged && currentUserData.NameChangeCount >= MAX_CHANGES)
            {
                ShowError(txtUsername, Lang.UserProfileTextChangesError);
                txtUsername.Text = originalUsername;
                isValid = false;
            }

            return isValid;
        }

        private bool IsValidUsername(string username)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string handle = "";
            string baseUrl = "";

            if (sender == linkFacebook)
            {
                handle = txtFacebookLink.Text.Trim();
                baseUrl = "https://www.facebook.com/";
            }
            else if (sender == linkX)
            {
                handle = txtXLink.Text.Trim();
                baseUrl = "https://x.com/";
            }
            else if (sender == linkInstagram)
            {
                handle = txtInstagramLink.Text.Trim();
                baseUrl = "https://www.instagram.com/";
            }

            if (!string.IsNullOrWhiteSpace(handle))
            {
                string finalUrl = baseUrl + handle;
                try
                {
                    if (Uri.IsWellFormedUriString(finalUrl, UriKind.Absolute))
                    {
                        Process.Start(new ProcessStartInfo(finalUrl) { UseShellExecute = true });
                    }
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private TextBlock GetErrorTextBlock(Control field)
        {
            TextBlock errorBlock = null;

            if (field == txtUsername)
            {
                errorBlock = blckUsernameError;
            }
            /*if (field == txtFacebookLink)
            {
                errorBlock = blckFacebookError;
            }
            if (field == txtXLink)
            {
                errorBlock = blckXError;
            }
            if (field == txtInstagramLink)
            {
                return blckInstagramError;
            }*/

            return errorBlock;
        }

        private void ShowError(Control field, string errorMessage)
        {
            TextBlock errorBlock = GetErrorTextBlock(field);

            if (errorBlock != null)
            {
                errorBlock.Text = errorMessage;
            }

            field.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void ClearSpecificError(Control field)
        {
            TextBlock errorBlock = GetErrorTextBlock(field);

            if (errorBlock != null)
            {
                errorBlock.Text = string.Empty;
            }

            field.ClearValue(Border.BorderBrushProperty);
        }

        private void ClearAllErrors()
        {
            ClearSpecificError(txtUsername);
            /*ClearSpecificError(txtFacebookLink);
            ClearSpecificError(txtXLink);
            ClearSpecificError(txtInstagramLink);*/
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            ClearSpecificError(textBox);
            string text = textBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (textBox == txtUsername)
            {
                if (text.Length < MIN_TEXT_LENGTH)
                {
                    ShowError(txtUsername, Lang.DialogTextShortUsername);
                }
                else if (text.Length > MAX_TEXT_LENGTH)
                {
                    ShowError(txtUsername, Lang.DialogTextLongUsername);
                }
            }
        }
        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                ShowError(textBox, Lang.GlobalTextRequieredField);
            }
        }
    }
}