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
using System.Windows.Shapes;
using System.Xml.Linq;
using TrucoPrueba1.Properties.Langs;
using TrucoPrueba1.TrucoServer; 
using System.Windows.Navigation;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para LogIn.xaml
    /// </summary>
    public partial class LogInPage : Page
    {
        public LogInPage()
        {
            InitializeComponent();
            string trackPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Songs",
                "music_in_menus.mp3"
            );
            MusicManager.Play(trackPath);
            MusicManager.Volume = 0.3;
        }

        private async void ClickLogIn(object sender, RoutedEventArgs e)
        {
            string usernameOrEmail = txtEmailUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(usernameOrEmail) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show(Lang.DialogTextFillFields, Lang.GlobalTextFillFields, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = await SessionManager.UserClient.LoginAsync(usernameOrEmail, password);

                if (success)
                {
                    SessionManager.CurrentUsername = usernameOrEmail;

                    MessageBox.Show(Lang.GlobalTextWelcome + " " + SessionManager.CurrentUsername + "!", Lang.GlobalTextWelcome + "!", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.NavigationService.Navigate(new MainPage());
                }
                else
                {
                    MessageBox.Show(Lang.DialogTextInvalidUserPass, Lang.DialogTextWrongCredentials, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.DialogTextError + ex.Message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickForgotPassword(object sender, RoutedEventArgs e)
        {
            // this.NavigationService.Navigate(new ForgotPasswordWindow()); 
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartPage());
        }
    }
}
