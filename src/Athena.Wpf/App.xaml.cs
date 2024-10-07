using System.Diagnostics;
using System.Windows;

namespace Athena.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        
        if (args.Length != 1)
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            
            MessageBox.Show($"Usage: {processName} <file to open>","",
                MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
            
            Environment.Exit(0);
        }
        
        new MainWindow(args).Show();
    }
}
