using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    public partial class NewUser : Window
    {
        public NewUser()
        {
            InitializeComponent();
        }
        private void ClickButtonRegister(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();
            string password2 = txtPassword2.Password.Trim();
            string username = txtUsername.Text.Trim();

            if (!FieldsValidation())
            {
                return;
            }

            try
            {
            using (var context = new baseDatosPruebaEntities())
                {
                    if (!EmailAndUsernameVerification())
                    {
                        return;
                    }

                    password = txtPassword.Password;
                    var newUser = new User
                    {
                        email = email,
                        passwordHash = password, //Falta aplicar el hash
                        nickname = username,
                        wins = 0
                    };

                    context.User.Add(newUser);
                    context.SaveChanges();
                }
                MessageBox.Show(Lang.DialogTextNewUserSuccess, Lang.GlobalTextSuccess, MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.DialogTextNewUserException + $" {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FieldsValidation()
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();
            string password2 = txtPassword2.Password.Trim();
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(password2) ||
                string.IsNullOrEmpty(username))
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

            if (username.Length < 4)
            {
                MessageBox.Show(Lang.DialogTextShortUsername, Lang.DialogTextShortUsername, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (username.Length > 20)
            {
                MessageBox.Show(Lang.DialogTextLongUsername, Lang.DialogTextLongUsername, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool EmailAndUsernameVerification()
        {
            string email = txtEmail.Text.Trim();
            string username = txtUsername.Text.Trim();

            using (var context = new baseDatosPruebaEntities())
            {
                bool existsEmail = context.User.Any(u => u.email == email);
                if (existsEmail)
                {
                    MessageBox.Show(Lang.DialogTextEmailAlreadyExists, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                bool existsUsername = context.User.Any(u => u.nickname == username);
                if (existsUsername)
                {
                    MessageBox.Show(Lang.DialogTextUserAlreadyExists, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                return true;
            }
        }
        private void ClickButtonExit(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();
            string password2 = txtPassword2.Password.Trim();
            string username = txtUsername.Text.Trim();

            if (!string.IsNullOrEmpty(email) ||
                !string.IsNullOrEmpty(password) ||
                !string.IsNullOrEmpty(password2) ||
                !string.IsNullOrEmpty(username))
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(Lang.DialogTextConfirmationNewUser, Lang.GlobalTextConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    LogIn logInWindow = new LogIn();
                    logInWindow.Show();
                    this.Close();
                } 
                else
                {
                    return;
                }
            }
            else
            {
                LogIn logInWindow = new LogIn();
                logInWindow.Show();
                this.Close();
            }
        }
    }
}
