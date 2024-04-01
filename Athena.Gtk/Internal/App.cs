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
                var appPicker = new MainWindow(opener.AppList);
                _app.AddWindow(appPicker);
                
                // The method returns 0 when the user closes the window manually
                entryId = appPicker.GetEntryId() - 1;
                Application.Run();
                
                var dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, entryId.ToString());
                dialog.Run();
                dialog.Dispose();
            }
           
            _ = handler.RunEntry(opener, entryId, args[0]);
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
