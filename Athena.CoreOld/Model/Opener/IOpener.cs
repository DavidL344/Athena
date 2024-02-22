namespace Athena.CoreOld.Model.Opener;

public interface IOpener
{
    public string Name { get; set; }
    public string[] AppList { get; set; }
    public string DefaultApp { get; set; }
}
