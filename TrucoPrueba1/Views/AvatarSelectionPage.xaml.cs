using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace TrucoPrueba1.Views
{
    public partial class AvatarSelectionPage : Page, INotifyPropertyChanged
    {
        public event EventHandler<string> AvatarSelected;
        public List<string> Avatars { get; private set; }

        private string _currentAvatarId;
        public string CurrentAvatarId
        {
            get => _currentAvatarId;
            set
            {
                if (_currentAvatarId != value)
                {
                    _currentAvatarId = value;
                    OnPropertyChanged(nameof(CurrentAvatarId));
                }
            }
        }

        public ICommand SelectAvatarCommand { get; private set; }

        public AvatarSelectionPage(List<string> availableAvatars, string currentId)
        {
            InitializeComponent();
            Avatars = availableAvatars ?? new List<string>();
            CurrentAvatarId = currentId;
            SelectAvatarCommand = new RelayCommand(ExecuteSelectAvatar);
            DataContext = this;
            MusicInitializer.InitializeMenuMusic();
        }

        private void ExecuteSelectAvatar(object parameter)
        {
            if (parameter is string selectedId)
            {
                CurrentAvatarId = selectedId;
            }
        }

        private void ClickBack(object sender, System.Windows.RoutedEventArgs e)
        {
            AvatarSelected?.Invoke(this, CurrentAvatarId);
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
