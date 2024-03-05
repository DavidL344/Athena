namespace Athena.Core.Model.AppPicker;

public interface IAppPicker
{
    static Task<int> Show(AppPickerContext ctx)
        => Task.FromResult(-1);
}
