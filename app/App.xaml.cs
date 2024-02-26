using System.Configuration;
using System.Data;
using System.Windows;

namespace app
{
    /// <summary>
    /// App starting point
    /// </summary>
    public partial class App : Application
    {
        [STAThread] // Single threaded application
        public static void Main()
        {
            var application = new App();
            var window = new MainWindow();

            //window.Show();

            application.Run(window);

        }


    }




}
