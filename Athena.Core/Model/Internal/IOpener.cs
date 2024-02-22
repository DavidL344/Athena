namespace Athena.Core.Model.Internal;

public interface IOpener
{
    public string Name { get; set; }
    public string[] AppList { get; set; }
    public string DefaultApp { get; set; }
}
