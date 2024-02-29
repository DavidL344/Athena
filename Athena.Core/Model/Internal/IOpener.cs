using Athena.Core.Model.DataStructures;

namespace Athena.Core.Model.Internal;

public interface IOpener
{
    public string Name { get; set; }
    public UniqueList<string> AppList { get; set; }
    public string DefaultApp { get; set; }
}
