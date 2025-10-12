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
    /// Lógica de interacción para ForgotPasswordStepTwoPage.xaml
    /// </summary>
    public partial class ForgotPasswordStepTwoPage : Page
    {
        private string email;
        public ForgotPasswordStepTwoPage(string email)
        {
            InitializeComponent();
            this.email = email;
            MusicInitializer.InitializeMenuMusic();
            blckVerificationCodeText.Text = string.Format($"Escribe el código de verificación enviado a {email}");
        }

        private void ClickButtonSave(object sender, RoutedEventArgs e)
        {
            if (!FieldsValidation())
            {
                return;
            }

            string code = txtVerificationCode.Text.Trim();
            string password = txtPassword.Password.Trim();

            try
            {
                var callback = new TrucoUserCallback();
                var context = new System.ServiceModel.InstanceContext(callback);
                var client = new TrucoUserServiceClient(context, "NetTcpBinding_ITrucoUserService");

                bool success = client.PasswordReset(email, code, password);

                if (success)
                {
                    MessageBox.Show("Tu contraseña ha sido restablecida correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.NavigationService.Navigate(new LogInPage());
                }
                else
                {
                    MessageBox.Show("Código incorrecto o expirado, o el correo no existe.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartPage());
        }

        private bool FieldsValidation()
        {
            string password = txtPassword.Password.Trim();
            string password2 = txtPassword2.Password.Trim();

            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(password2))
            {
                MessageBox.Show(Lang.DialogTextFillFields, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!string.Equals(password, password2))
            {
                MessageBox.Show(Lang.DialogTextPasswordsDontMatch, Lang.DialogTextPasswordsDontMatch, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (password.Length < 8)
            {
                MessageBox.Show(Lang.DialogTextShortPassword, Lang.DialogTextShortPassword, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (password.Length > 50)
            {
                MessageBox.Show(Lang.DialogTextLongPassword, Lang.DialogTextLongPassword, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
