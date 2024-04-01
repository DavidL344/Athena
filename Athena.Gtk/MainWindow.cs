using System.Collections.Generic;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Athena.Gtk;

internal class MainWindow : Window
{
    [UI] private readonly Label _fileTypeLabel = null;
    [UI] private readonly Label _filePathLabel = null;
    [UI] private readonly ListBox _appPickerList = null;

    private int _selectedIndex = -1;
    
    private static readonly string[] SampleEntries =
    [
        "Entry 1",
        "Entry 2",
        "Another Entry (athena.entry.another)",
        "Fourth Entry   |   /full/path/to/executable"
    ];
    
    public MainWindow() : this(SampleEntries, new Builder("MainWindow.glade"))
    {
    }
    
    public MainWindow(IEnumerable<string> entries) : this(entries, new Builder("MainWindow.glade"))
    {
    }
    
    private MainWindow(IEnumerable<string> entries, Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
    {
        // Load the Glade file
        builder.Autoconnect(this);
        
        // Add items to the list box
        foreach (var entry in entries)
        {
            var label = new Label(entry)
            {
                MarginTop = 10,
                MarginBottom = 10
            };
            
            _appPickerList!.Add(label);
        }
        
        ShowAll();
        
        DeleteEvent += Window_DeleteEvent;
    }

    public int GetEntryId()
    {
        Show();
        return _appPickerList.SelectedRow.Index;
    }
    
    private static void Window_DeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
    }
}
