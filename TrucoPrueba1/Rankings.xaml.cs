using System.Linq;
using System.Windows;

namespace TrucoPrueba1
{
    public partial class Rankings : Window
    {
        public Rankings()
        {
            InitializeComponent();
            LoadRankings();
        }
        private void LoadRankings()
        {
            using (var context = new baseDatosPruebaEntities())
            {
                var users = context.User
                                   .OrderByDescending(u => u.wins)
                                   .Take(10)
                                   .ToList();

                dgRankings.ItemsSource = users;
            }
        }
        private void ClickButtonExit(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
