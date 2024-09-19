using Athena.Gtk.Internal;
using Gtk;

Application.Init();

var app = new Application("tech.langr.Athena.Gtk", GLib.ApplicationFlags.None);
app.Register(GLib.Cancellable.Current);

var application = new App(app);
application.Run(args);
