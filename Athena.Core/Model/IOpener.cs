using Athena.DataStructures;

namespace Athena.Core.Model;

public interface IOpener
{
    public string Name { get; set; }
    public UniqueList<string> AppList { get; set; }
    public string DefaultApp { get; set; }
}
