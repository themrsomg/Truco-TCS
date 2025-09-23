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
using TrucoPrueba1.Properties.Langs;

namespace TrucoPrueba1
{
    /// <summary>
    /// Lógica de interacción para InitialWindows.xaml
    /// </summary>
    public partial class InitialWindows : Window
    {
        public InitialWindows()
        {
            InitializeComponent();
        }
        private void Language_Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedIndex == 0)
            {
                Properties.Settings.Default.languageCode = "en-US";
            }
            else
            {
                Properties.Settings.Default.languageCode = "es-MX";
            }
            Properties.Settings.Default.Save();
        }
        private void ClickLogIn(object sender, RoutedEventArgs e)
        {
            LogIn logInWindow = new LogIn();
            logInWindow.Show();
            this.Close();
        }
        private void ClickPlayAsGuest(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(); //CREAR VENTANA DE INVITADOS
            mainWindow.Show();
            this.Close();
        }
        private void ClickSingUp(object sender, RoutedEventArgs e)
        {
            NewUser newUserWindow = new NewUser();
            newUserWindow.Show();
            this.Close();
        }
    }
}
