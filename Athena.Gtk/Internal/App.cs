using System;
using System.Diagnostics;
using Gtk;
using Application = Gtk.Application;

namespace Athena.Gtk.Internal;

public class App
{
    private readonly Application _app;
    private MessageDialog _dialog;
    
    public App(Application app)
    {
        _app = app;
    }
    
    public void Run(string[] args)
    {
        if (args.Length != 1)
        {
            _dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                "Usage: Athena.Gtk &lt;file to open&gt;");
            
            _dialog.Run();
            _dialog.Dispose();

            return;
        }
        
        try
        {
            var handler = new Handler();
           
            var opener = handler.GetOpener(args[0]);
            var entryId = handler.GetEntryId(opener);
           
            if (entryId == -1)
            {
                var appList = handler.GetFriendlyNames(opener);
                var appPicker = new MainWindow(opener.Name, appList, args[0]);
                _app.AddWindow(appPicker);
                
                appPicker.Show();
                Application.Run();
                
                entryId = appPicker.SelectedIndex;
                appPicker.Close();
                appPicker.Destroy();
                
                if (entryId == -1) Environment.Exit(130);
            }
            
            var exitCode = handler.RunEntry(opener, entryId, args[0]);
            Environment.Exit(exitCode);
        }
        catch (ApplicationException e)
        {
            if (Debugger.IsAttached) throw;
            
            var dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, e.Message);
            dialog.Run();
            dialog.Dispose();
        }
    }
}
