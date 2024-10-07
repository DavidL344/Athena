using System.Collections.Generic;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Athena.Gtk;

internal class MainWindow : Window
{
    [UI] private readonly Label _fileTypeLabel = default!;
    [UI] private readonly Label _filePathLabel = default!;
    [UI] private readonly ListBox _appPickerList = default!;
    
    public int SelectedIndex { get; private set; } = -1;
    
    private static readonly string[] SampleEntries =
    [
        "Entry 1",
        "Entry 2",
        "Another Entry (athena.entry.another)",
        "Fourth Entry   |   /full/path/to/executable"
    ];
    
    public MainWindow() : this("the file", SampleEntries, "/full/path/to/the/file")
    {
    }
    
    public MainWindow(string openerName, IEnumerable<string> entries, string filePath)
        : this(openerName, entries, filePath, new Builder("MainWindow.glade"))
    {
    }
    
    private MainWindow(string openerName, IEnumerable<string> entries, string filePath, Builder builder)
        : base(builder.GetRawOwnedObject("MainWindow"))
    {
        // Load the Glade file
        builder.Autoconnect(this);
        
        // Set the labels
        _fileTypeLabel.Text = _fileTypeLabel.Text.Replace("$FILE_TYPE", openerName);
        _filePathLabel.Text = _filePathLabel.Text.Replace("$FILE_PATH", filePath);
        
        // Add items to the list box
        foreach (var entry in entries)
        {
            var label = new Label(entry)
            {
                MarginTop = 10,
                MarginBottom = 10
            };
            
            _appPickerList.Add(label);
        }
        
        // Render the window
        SetWindowSize();
        ShowAll();
        
        // Connect any relevant events
        _appPickerList.ListRowActivated += AppPickerList_RowSelected;
        DeleteEvent += Window_DeleteEvent;
        ScreenChanged += (_, _) => SetWindowSize();
    }
    
    private void SetWindowSize()
    {
        var dpi = Gdk.Screen.Default.Resolution;
        
        // [(int)(Gdk.Screen.Default.Height / 4.5)] when keeping the relative size of the window
        var windowHeight = (int)(2.5 * dpi);
        
        // [Gdk.Screen.Default.Width / 4] when not keeping the 2:1 aspect ratio
        var windowWidth = windowHeight * 2;
        
        DefaultWidth = windowWidth;
        DefaultHeight = windowHeight;
        SetSizeRequest(windowWidth, windowHeight);
        
        Resizable = false;
        WindowPosition = WindowPosition.Center;
        
#if DEBUG
        Title = $"Athena GTK ({windowWidth}x{windowHeight} @ {Gdk.Screen.Default.Resolution} dpi)";
#endif
    }
    
    private void AppPickerList_RowSelected(object o, ListRowActivatedArgs args)
    {
        SelectedIndex = args.Row.Index;
        Close();
    }
    
    private void Window_DeleteEvent(object sender, DeleteEventArgs a)
    {
        var senderWindow = (Window)sender;
        a.RetVal = true;
        
        senderWindow.Hide();
        senderWindow.Destroy();
        Application.Quit();
    }
}
