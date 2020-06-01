using Microsoft.Xna.Framework.Graphics;
using System.Windows;

namespace Editor
{

    

    public partial class App : Application
    {
        

        private void Application_Startup(object sender, StartupEventArgs e) {
            // TODO: Get args for command line parameters

            // Create the startup window
            MainWindow mainWindow = new MainWindow();

            mainWindow.Show();
            // StartupUri="MainWindow.xaml"
        }
    }
}
