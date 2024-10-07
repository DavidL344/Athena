using System.Diagnostics;
using System.Windows;
using Athena.Wpf.Internal;

namespace Athena.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        Frame.Navigate(new Pages.AppPicker());
    }

    public MainWindow(string[] args)
    {
        InitializeComponent();
        
        Run(args);
    }

    private void Run(string[] args)
    {
        try
        {
            var handler = new Handler();
           
            var opener = handler.GetOpener(args[0]);
            var entryId = handler.GetEntryId(opener);
            
            if (entryId == -1)
            {
                var appList = handler.GetFriendlyNames(opener);
                var appPicker = new Pages.AppPicker(opener, appList, args[0]);
                Frame.Navigate(appPicker);
            }
        }
        catch (ApplicationException e)
        {
            if (Debugger.IsAttached) throw;
            
            MessageBox.Show(e.Message,"",
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            
            Environment.Exit(1);
        }
    }
}
