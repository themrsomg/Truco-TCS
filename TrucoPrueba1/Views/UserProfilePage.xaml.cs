using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1;
using TrucoPrueba1.Properties.Langs;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1
{
    public partial class UserProfilePage : Page
    {
        private UserProfileData currentUserData;

        private readonly List<string> AvailableAvatars = new List<string>
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

        private const int MAX_CHANGES = 2;
        private string originalUsername;

        public UserProfilePage()
        {
            InitializeComponent();
            originalUsername = string.Empty;
            LoadUserProfile();
            MusicInitializer.InitializeMenuMusic();
        }

        private async void LoadUserProfile()
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
                else if (!string.IsNullOrWhiteSpace(SessionManager.CurrentUsername) &&
                         SessionManager.CurrentUsername != "UsuarioActual")
                {
                    var client = SessionManager.UserClient;
                    currentUserData = await client.GetUserProfileAsync(SessionManager.CurrentUsername);

                    if (currentUserData == null)
                    {
                        return;
                    }

                    this.DataContext = currentUserData;
                    LoadAvatarImage(currentUserData.AvatarId);

                    originalUsername = currentUserData.Username ?? string.Empty;

                    txtUsername.Text = currentUserData.Username;
                    txtEmail.Text = currentUserData.Email;
                    txtFacebookLink.Text = currentUserData.FacebookHandle;
                    txtXLink.Text = currentUserData.XHandle;
                    txtInstagramLink.Text = currentUserData.InstagramHandle;

                    SessionManager.CurrentUserData = currentUserData;
                    UpdateUsernameWarning(currentUserData.NameChangeCount);
                    UpdateSocialMediaLinks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
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

            bool usernameChanged = !string.IsNullOrWhiteSpace(originalUsername) && !string.Equals(newUsername, originalUsername, StringComparison.Ordinal);
            bool facebookChanged = !string.Equals(newFacebook, currentUserData.FacebookHandle, StringComparison.Ordinal);
            bool xChanged = !string.Equals(newX, currentUserData.XHandle, StringComparison.Ordinal);
            bool instagramChanged = !string.Equals(newInstagram, currentUserData.InstagramHandle, StringComparison.Ordinal);

            if (!usernameChanged && !facebookChanged && !xChanged && !instagramChanged)
            {
                MessageBox.Show(Lang.UserProfileTextSaveNoChanges, Lang.UserProfileTextNoChanges, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (usernameChanged && newUsername.Length < 4)
            {
                MessageBox.Show(Lang.DialogTextShortUsername, Lang.DialogTextShortUsername, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usernameChanged && currentUserData.NameChangeCount >= MAX_CHANGES)
            {
                MessageBox.Show(Lang.UserProfileTextTwoChanges, Lang.UserProfileTextError, MessageBoxButton.OK, MessageBoxImage.Stop);
                txtUsername.Text = originalUsername;
                return;
            }

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

                var client = SessionManager.UserClient;

                if (usernameChanged)
                {
                    bool usernameExists = await Task.Run(() => client.UsernameExists(newUsername));
                    if (usernameExists)
                    {
                        MessageBox.Show(Lang.GlobalTextUsernameAlreadyInUse, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtUsername.Text = originalUsername;
                        return;
                    }
                }

                bool success = await client.SaveUserProfileAsync(currentUserData);

                if (success)
                {
                    if (usernameChanged)
                    {
                        currentUserData.NameChangeCount++;
                        originalUsername = newUsername;
                        SessionManager.CurrentUsername = newUsername;
                    }

                    UpdateUsernameWarning(currentUserData.NameChangeCount);
                    UpdateSocialMediaLinks();

                    MessageBox.Show(Lang.UserProfileTextSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
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
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Text = originalUsername;
            }
            finally
            {
                saveButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
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
                var client = SessionManager.UserClient;

                bool success = await client.UpdateUserAvatarAsync(SessionManager.CurrentUsername, newAvatarId);

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
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
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

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void ClickChangePassword(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentUserData == null || string.IsNullOrWhiteSpace(currentUserData.Email))
                {
                    MessageBox.Show(Lang.DialogTextPasswordChangeErrorTwo, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string email = currentUserData.Email;
                string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var client = SessionManager.UserClient;

                bool sent = client.RequestEmailVerification(email, languageCode);
                if (!sent)
                {
                    MessageBox.Show(Lang.StartTextRegisterCodeSended, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string code = Microsoft.VisualBasic.Interaction.InputBox(
                    Lang.StartTextRegisterIntroduceCode,
                    Lang.StartTextRegisterEmailVerification,
                    ""
                );

                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show(Lang.StartTextRegisterMustEnterCode, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string newPassword = Microsoft.VisualBasic.Interaction.InputBox(
                    Lang.DialogTextEnterNewPassword,
                    Lang.DialogTextNewPassword,
                    ""
                );

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show(Lang.DialogTextEmptyPassword, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string confirmPassword = Microsoft.VisualBasic.Interaction.InputBox(
                    Lang.DialogTextConfirmPassword,
                    Lang.DialogTextNewPassword,
                    ""
                );

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show(Lang.DialogTextPasswordsDontMatch, Lang.DialogTextPasswordsDontMatch, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPassword.Length < 8)
                {
                    MessageBox.Show(Lang.DialogTextShortPassword, Lang.DialogTextShortPassword, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool changed = client.PasswordReset(email, code, newPassword);

                if (changed)
                {
                    MessageBox.Show(Lang.DialogTextPasswordChangedSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    MessageBox.Show(Lang.DialogTextPasswordChangeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickChangeAvatar(object sender, RoutedEventArgs e)
        {
            if (currentUserData == null)
            {
                return;
            }

            var avatarPage = new Views.AvatarSelectionPage(AvailableAvatars, currentUserData.AvatarId);
            avatarPage.AvatarSelected += AvatarPage_AvatarSelected;
            NavigationService.Navigate(avatarPage);
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

        private void LoadAvatarImage(string avatarId)
        {
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = "avatar_aaa_default";
            }

            string packUri = $"pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/{avatarId}.png";

            try
            {
                imgAvatar.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch
            {
                imgAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/avatar_aaa_default.png", UriKind.Absolute));
            }
        }
    }
}