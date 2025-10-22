using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrucoPrueba1.Properties.Langs;
using TrucoPrueba1.TrucoServer;

namespace TrucoPrueba1.Views
{
    /// <summary>
    /// Lógica de interacción para ForgotPasswordPage.xaml
    /// </summary>
    public partial class ForgotPasswordStepOnePage : Page
    {
        public ForgotPasswordStepOnePage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickButtonSendCode(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (!FieldsValidation())
            {
                return;
            }

            if (EmailVerification())
            {
                return;
            }

            try
            {
                var callback = new TrucoUserCallback();
                var context = new System.ServiceModel.InstanceContext(callback);
                var client = new TrucoUserServiceClient(context, "NetTcpBinding_ITrucoUserService");

                string languageCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                bool sent = client.RequestEmailVerification(email, languageCode);

                if (sent)
                {
                    MessageBox.Show(Lang.ForgotPasswordTextSent2, Lang.ForgotPasswordRecovery, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new ForgotPasswordStepTwoPage(email));
                }
                else
                {
                    MessageBox.Show(Lang.ForgotPasswordTextError2, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartPage());
        }

        private bool FieldsValidation()
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show(Lang.DialogTextFillFields, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (email.Length < 5)
            {
                MessageBox.Show(Lang.DialogTextShortEmail, Lang.DialogTextShortEmail, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (email.Length > 250)
            {
                MessageBox.Show(Lang.DialogTextLongEmail, Lang.DialogTextLongEmail, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool EmailVerification()
        {
            string email = txtEmail.Text.Trim();

            try
            {
                var callback = new TrucoUserCallback();
                var context = new System.ServiceModel.InstanceContext(callback);
                var client = new TrucoUserServiceClient(context, "NetTcpBinding_ITrucoUserService");

                bool emailExists = client.EmailExists(email);

                if (!emailExists)
                {
                    MessageBox.Show(Lang.GlobalTextEmailDoesntExist, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error WCF", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }
    }
}
