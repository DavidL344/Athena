using IniParser.Model;

namespace Athena.Core.Desktop.Linux;

internal static class MimeAppsList
{
    private const string AddedAssociations = "Added Associations";
    private const string DefaultApplications = "Default Applications";
    
    public static IniData AssociateWithAllApps(IniData mimeApps, string desktopFileName)
    {
        foreach (var mimeType in mimeApps.Sections[AddedAssociations])
        {
            if (mimeType.Value.Contains(desktopFileName))
                continue;
            
            if (mimeType.Value is null || string.IsNullOrWhiteSpace(mimeType.Value))
            {
                mimeType.Value = AppendToValue(string.Empty, desktopFileName);
                continue;
            }
            
            mimeType.Value = AppendToValue(mimeType.Value, desktopFileName);
        }
        
        return mimeApps;
    }
    
    public static IniData AssociateWithApp(IniData mimeApps, string mimeType, string desktopFileName)
    {
        if (!mimeApps.Sections[AddedAssociations].ContainsKey(mimeType))
        {
            mimeApps.Sections[AddedAssociations].Add(mimeType, desktopFileName);
            return mimeApps;
        }
        
        mimeApps.Sections[AddedAssociations][mimeType] = AppendToValue(
            mimeApps.Sections[AddedAssociations][mimeType], desktopFileName);

        return mimeApps;
    }
    
    public static IniData DissociateFromAllApps(IniData mimeApps, string desktopFileName)
    {
        foreach (var mimeType in mimeApps.Sections[AddedAssociations])
        {
            if (!mimeType.Value.Contains(desktopFileName))
                continue;
            
            mimeType.Value = RemoveFromValue(mimeType.Value, desktopFileName);
        }
        
        return mimeApps;
    }
    
    public static IniData DissociateFromApp(IniData mimeApps, string mimeType, string desktopFileName)
    {
        if (!mimeApps.Sections[AddedAssociations].ContainsKey(mimeType))
            return mimeApps;

        mimeApps.Sections[AddedAssociations][mimeType] = RemoveFromValue(
                mimeApps.Sections[AddedAssociations][mimeType], desktopFileName);
        
        return mimeApps;
    }
    
    public static IniData SetAsDefaultForAllMimeTypes(IniData mimeApps, string desktopFileName)
    {
        foreach (var mimeType in mimeApps.Sections[DefaultApplications])
        {
            if (mimeType.Value.Contains(desktopFileName))
                continue;
            
            mimeType.Value = desktopFileName;
        }
        
        return mimeApps;
    }
    
    public static IniData SetAsDefaultForMimeType(IniData mimeApps, string mimeType, string desktopFileName)
    {
        if (!mimeApps.Sections[DefaultApplications].ContainsKey(mimeType))
        {
            mimeApps.Sections[DefaultApplications].Add(mimeType, desktopFileName);
            return mimeApps;
        }
        
        mimeApps.Sections[DefaultApplications][mimeType] = desktopFileName;

        return mimeApps;
    }
    
    public static IniData UnsetDefault(IniData mimeApps, string desktopFileName)
    {
        foreach (var mimeType in mimeApps.Sections[DefaultApplications])
        {
            if (!mimeType.Value.Contains(desktopFileName))
                continue;
            
            mimeType.Value = string.Empty;
        }
        
        return mimeApps;
    }
    
    public static IniData UnsetDefaultFrom(IniData mimeApps, string mimeType, string desktopFileName)
    {
        if (!mimeApps.Sections[DefaultApplications].ContainsKey(mimeType))
            return mimeApps;
        
        if (!mimeApps.Sections[DefaultApplications][mimeType].Contains(desktopFileName))
            return mimeApps;

        mimeApps.Sections[DefaultApplications][mimeType] = string.Empty;
        
        return mimeApps;
    }
    
    private static string AppendToValue(string original, string appended)
    {
        return original.EndsWith(';') ? $"{original}{appended};" : $"{original};{appended}";
    }
    
    private static string RemoveFromValue(string original, string removedValue)
    {
        return original.EndsWith(';')
            ? original.Replace($"{removedValue};", string.Empty)
            : original.Replace($";{removedValue}", string.Empty);
    }
}
