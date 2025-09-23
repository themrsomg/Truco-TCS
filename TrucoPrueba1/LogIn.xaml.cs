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

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {
        public LogIn()
        {
            InitializeComponent();
        }
        private void ClickLogIn(object sender, RoutedEventArgs e)
        {
            string usernameOrEmail = txtEmailUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(usernameOrEmail) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show(Lang.DialogTextFillFields, Lang.GlobalTextFillFields, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            usernameOrEmail = txtEmailUsername.Text;
            password = txtPassword.Password;

            try
            {
                using (var context = new baseDatosPruebaEntities())
                {
                    var user = context.User
                        .FirstOrDefault(u =>
                            (u.email == usernameOrEmail || u.nickname == usernameOrEmail) &&
                            u.passwordHash == password
                        );

                    if (user != null)
                    {
                        MessageBox.Show(Lang.GlobalTextWelcome + " " + user.nickname + "!", Lang.GlobalTextWelcome + "!", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(Lang.DialogTextInvalidUserPass, Lang.DialogTextWrongCredentials, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.DialogTextError + ex.Message);
            }
        }

        private void ClickForgotPassword(object sender, RoutedEventArgs e)
        {
            //FORGOT PASSWORD WINDOW
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            InitialWindows initialWindows = new InitialWindows();
            initialWindows.Show();
            this.Close();
        }
    }
}
