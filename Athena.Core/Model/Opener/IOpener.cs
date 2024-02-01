namespace Athena.Core.Model.Opener;

public interface IOpener
{
    public string[] AppList { get; set; }
    public string DefaultApp { get; set; }
}
