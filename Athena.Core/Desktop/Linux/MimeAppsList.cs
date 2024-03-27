using System.Reflection;
using Athena.Core.Internal.Samples;
using IniParser.Model;

namespace Athena.Core.Desktop.Linux;

internal static class MimeAppsList
{
    private const string AddedAssociations = "Added Associations";
    private const string DefaultApplications = "Default Applications";

    public static IniData Create()
    {
        var mimeApps = new IniData();
        
        // Default sections
        mimeApps.Sections.Add(DefaultApplications);
        mimeApps.Sections.Add(AddedAssociations);
        
        return mimeApps;
    }

    public static IniData AddSamples(IniData mimeApps, ISample sample, string desktopFileName)
    {
        foreach (var mimeType in sample.MimeTypes)
        {
            AssociateWithApp(mimeApps, mimeType, desktopFileName);
        }

        return mimeApps;

        /*
        // Archive samples
        mimeApps.Sections[AddedAssociations].Add("application/zip", "athena.desktop"); // ZIP
        mimeApps.Sections[AddedAssociations].Add("application/x-7z-compressed", "athena.desktop"); // 7z

        // Media samples
        mimeApps.Sections[AddedAssociations].Add("audio/mpeg", "athena.desktop"); // MP3
        mimeApps.Sections[AddedAssociations].Add("video/mp4", "athena.desktop"); // MP4
        mimeApps.Sections[AddedAssociations].Add("video/x-matroska", "athena.desktop"); // MKV
        mimeApps.Sections[AddedAssociations].Add("video/x-msvideo", "athena.desktop"); // AVI

        // Text samples
        mimeApps.Sections[AddedAssociations].Add("text/plain", "athena.desktop"); // TXT

        // Web Samples
        mimeApps.Sections[AddedAssociations].Add("text/html", "athena.desktop"); // HTML
        mimeApps.Sections[AddedAssociations].Add("x-scheme-handler/http", "athena.desktop"); // HTTP
        mimeApps.Sections[AddedAssociations].Add("x-scheme-handler/https", "athena.desktop"); // HTTPS
        */
    }
    
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
    
    public static IniData AssociateWithSampleApps(IniData mimeApps, string desktopFileName)
    {
        // Get all classes with the ISample interface
        var sampleTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(assemblyType => typeof(ISample).IsAssignableFrom(assemblyType) && !assemblyType.IsInterface);
        
        // Instantiate all classes with the ISample interface and add them to a list
        var samples = sampleTypes.Select(type => (ISample)Activator.CreateInstance(type)!).ToList();
        
        // Get all samples and add them to the MIME apps list
        foreach (var sample in samples)
        {
            mimeApps = AddSamples(mimeApps, sample, desktopFileName);
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
            if (string.IsNullOrWhiteSpace(mimeType.Value))
                mimeApps.Sections[AddedAssociations].RemoveKey(mimeType.KeyName);
        }
        
        return mimeApps;
    }
    
    public static IniData DissociateFromApp(IniData mimeApps, string mimeType, string desktopFileName)
    {
        if (!mimeApps.Sections[AddedAssociations].ContainsKey(mimeType))
            return mimeApps;

        mimeApps.Sections[AddedAssociations][mimeType] = RemoveFromValue(
                mimeApps.Sections[AddedAssociations][mimeType], desktopFileName);
        
        if (string.IsNullOrWhiteSpace(mimeApps.Sections[AddedAssociations][mimeType]))
            mimeApps.Sections[AddedAssociations].RemoveKey(mimeType);
        
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
        if (original.Contains(appended)) return original;
        if (string.IsNullOrWhiteSpace(original)) return $"{appended};";
        return original.EndsWith(';') ? $"{original}{appended};" : $"{original};{appended}";
    }
    
    private static string RemoveFromValue(string original, string removedValue)
    {
        if (!original.Contains(';')) return original.Replace(removedValue, string.Empty);
        
        return original.EndsWith(';')
            ? original.Replace($"{removedValue};", string.Empty)
            : original.Replace($";{removedValue}", string.Empty);
    }
}
