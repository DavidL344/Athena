using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Athena.Core.Model;
using Athena.Wpf.Internal;
using Page = System.Windows.Controls.Page;

namespace Athena.Wpf.Pages;

public partial class AppPicker : Page
{
    public int MaxShownRows { get; init; } = 3;
    
    private readonly IOpener _opener;
    private readonly string _filePath;
    
    private static readonly string[] SampleEntries =
    [
        "Entry 1",
        "Entry 2",
        "Another Entry (athena.entry.another)",
        "Fourth Entry   |   /full/path/to/executable"
    ];

    public AppPicker() : this(new FileExtension { Name = "the file", AppList = [] },
        SampleEntries, "/full/path/to/the/file")
    {
        
    }
    
    public AppPicker(IOpener opener, IEnumerable<string> entries, string filePath)
    {
        InitializeComponent();
        if (MaxShownRows is < 3 or > 5) MaxShownRows = 3;
        _opener = opener;
        _filePath = filePath;
        
        // Set the labels
        FileTypeLabel.Text = FileTypeLabel.Text.Replace("$FILE_TYPE", opener.Name);
        FilePathLabel.Text = FilePathLabel.Text.Replace("$FILE_PATH", filePath);
        
        // Add items to the list box
        foreach (var entry in entries)
        {
            var listBoxItem = new ListBoxItem
            {
                Content = entry
            };

            Selection.Items.Add(listBoxItem);
        }
        
        // Connect any relevant events
        Selection.Loaded += SelectionBoxLoaded;
        Selection.MouseLeftButtonUp += SelectionOnMouseLeftButtonUp;
        Selection.KeyDown += SelectionOnKeyDown;
    }
    
    private void SelectionBoxLoaded(object sender, RoutedEventArgs e)
    {
        var onScreenItems = Selection.Items.Count > 4 ? 4 : Selection.Items.Count;
        var maxHeight = Selection.ActualHeight
            / (onScreenItems > 0 ? onScreenItems : 1)
            * MaxShownRows;
        
        Selection.Width = Selection.ActualWidth;
        if (Selection.ActualHeight > maxHeight) Selection.Height = maxHeight;
    }
    
    private void SelectionOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Run(Selection.SelectedIndex, _opener, _filePath, true);
    }
    
    private void SelectionOnKeyDown(object sender, KeyEventArgs e)
    {
        if (Selection.SelectedIndex == -1 && Selection.Items.Count > 0)
        {
            if (Keyboard.IsKeyDown(Key.Down)) Selection.SelectedIndex = 0;
            if (Keyboard.IsKeyDown(Key.Up)) Selection.SelectedIndex = Selection.Items.Count - 1;
        }
        
        if (Keyboard.IsKeyDown(Key.Enter)) Run(Selection.SelectedIndex, _opener, _filePath, false);
    }

    private void Run(int entryId, IOpener opener, string filePath, bool clicked)
    {
        if (entryId == -1) Environment.Exit(130);
        
        var handler = new Handler();

        try
        {
            var exitCode = handler.RunEntry(opener, entryId, filePath);
            Environment.Exit(exitCode);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message,"Athena",
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            
            if (clicked)
                Selection.SelectedIndex = -1;
        }
    }
}
