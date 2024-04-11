using System.Text;
using Athena.Core.Runner;
using Athena.Resources;
using IniParser;
using IniParser.Model;

namespace Athena.Core.Desktop.Linux;

internal static class DesktopEntry
{
    private const string DesktopEntrySection = "Desktop Entry";
    private const string MimeTypeKey = "MimeType";
    private const char Separator = ';';
    
    public static void Add(string desktopFilePath)
    {
        var resourceData = ResourceLoader.Load($"Desktop/{desktopFilePath.Split('/').Last()}")
            .Replace("$PARAM_TYPE", "%u");
        
        File.WriteAllText(desktopFilePath, resourceData);
    }
    
    public static void Remove(string desktopFilePath)
    {
        if (File.Exists(desktopFilePath)) File.Delete(desktopFilePath);
    }
    
    public static void AddMimeType(string desktopFilePath, string mimeType, FileIniDataParser parser)
    {
        var data = Read(desktopFilePath, parser);

        if (!data.Sections[DesktopEntrySection].ContainsKey(MimeTypeKey))
        {
            data.Sections[DesktopEntrySection].Add(MimeTypeKey, $"{mimeType}{Separator}");
            Write(desktopFilePath, data, parser);
            return;
        }
        
        var mimeTypes = data.Sections[DesktopEntrySection][MimeTypeKey]
            .Trim()
            .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        if (mimeTypes.Contains(mimeType)) return;
        
        mimeTypes.Add(mimeType);
        data.Sections[DesktopEntrySection][MimeTypeKey] = $"{string.Join(Separator, mimeTypes)}{Separator}";
        
        Write(desktopFilePath, data, parser);
    }
    
    public static void RemoveMimeType(string desktopFilePath, string mimeType, FileIniDataParser parser)
    {
        var data = Read(desktopFilePath, parser);
        
        if (!data.Sections[DesktopEntrySection].ContainsKey(MimeTypeKey)) return;
        
        var mimeTypes = data.Sections[DesktopEntrySection][MimeTypeKey]
            .Trim()
            .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        if (!mimeTypes.Contains(mimeType)) return;
        
        // If the MIME type that is being removed is the only one in the list, remove the key
        if (mimeTypes.Count == 1)
        {
            data.Sections[DesktopEntrySection].RemoveKey(MimeTypeKey);
            Write(desktopFilePath, data, parser);
            return;
        }
        
        mimeTypes.Remove(mimeType);
        data.Sections[DesktopEntrySection][MimeTypeKey] = $"{string.Join(Separator, mimeTypes)}{Separator}";
        
        Write(desktopFilePath, data, parser);
    }
    
    private static IniData Read(string desktopEntryPath, FileIniDataParser parser)
    {
        var data = parser.ReadFile(desktopEntryPath);
        return Check(data);
    }
    
    private static void Write(string desktopEntryPath, IniData data, FileIniDataParser parser)
    {
        var checkedData = Check(data);
        parser.WriteFile(desktopEntryPath, checkedData, Encoding.ASCII);
    }
    
    private static IniData Check(IniData data)
    {
        if (!data.Sections.Contains(DesktopEntrySection))
            throw new ApplicationException(
                $"The desktop entry file is missing the [{DesktopEntrySection}] section!");
        
        return data;
    }
    
    public static void Source(string desktopFileDir, AppRunner appRunner)
    {
        appRunner.RunAsync("update-desktop-database", desktopFileDir).GetAwaiter().GetResult();
    }
}
