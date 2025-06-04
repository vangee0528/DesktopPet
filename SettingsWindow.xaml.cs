using System.Windows;

namespace DesktopPet
{
    public partial class SettingsWindow : Window
    {
        private readonly PetSettings settings;

        public SettingsWindow(PetSettings settings)
        {
            InitializeComponent();
            this.settings = settings;
            DataContext = settings;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            settings.Save();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
