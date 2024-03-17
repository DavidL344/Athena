using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Configuration;
using Athena.Core.Internal;
using Athena.Core.Internal.Helpers;

namespace Athena.Core.Tests.Internal.Helpers;

public class ConfigHelperTests : IDisposable
{
    private readonly string _localConfigDir;
    private readonly string _portableConfigDir;
    private readonly string _testsConfigDir;
    private readonly string _version;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ConfigHelperTests()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        _localConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            OperatingSystem.IsWindows() ? "Athena" : "athena");
        
        _portableConfigDir = Path.Combine(assemblyDir, "user");
        _testsConfigDir = Path.Combine(assemblyDir, "user-config-helper-tests");
        
        _version = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        Startup.CheckEntries(new ConfigPaths(_testsConfigDir), _jsonSerializerOptions)
            .GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    
    [Fact]
    public void GetAppDataDir__ReturnsLocalConfigDir_WhenPortableConfigDirDoesNotExist()
    {
        // Arrange
        if (Directory.Exists(_portableConfigDir))
            Directory.Delete(_portableConfigDir, true);
        
        // Act
        var result = ConfigHelper.GetAppDataDir();
        Directory.CreateDirectory(_portableConfigDir);
        Startup.CheckEntries(new ConfigPaths(_testsConfigDir), _jsonSerializerOptions).GetAwaiter();
        
        // Assert
        Assert.Equal(_localConfigDir, result);
    }
    
    [Fact]
    public void GetAppDataDir__ReturnsPortableConfigDir_WhenPortableConfigDirExists()
    {
        // Arrange
        Directory.CreateDirectory(_portableConfigDir);
        
        // Act
        var result = ConfigHelper.GetAppDataDir();
        
        // Assert
        Assert.Equal(_portableConfigDir, result);
    }
    
    [Fact]
    public void GetConfig__CreatesConfig__WhenTheConfigFileDoesNotExist()
    {
        // Arrange
        var configPaths = new ConfigPaths(_testsConfigDir);
        
        // Act
        var result = ConfigHelper.GetConfig(configPaths, _jsonSerializerOptions);
        
        // Assert
        Assert.Equivalent(new Config(), result);
        Assert.True(File.Exists(configPaths.ConfigFile));
    }
    
    [Fact]
    public void GetConfig__ReturnsConfig__WhenTheConfigFileExists()
    {
        // Arrange
        var configFile = Path.Combine(_testsConfigDir, "config.json");
        var config = new Config
        {
            EnableProtocolHandler = true,
            StreamableProtocolPrefixes = [ "one", "two" ],
            Editor = "test"
        };
        var fileContents = JsonSerializer.Serialize(config, _jsonSerializerOptions);
        File.WriteAllText(configFile, fileContents);
        
        // Act
        var configPaths = new ConfigPaths(_testsConfigDir);
        var result = ConfigHelper.GetConfig(configPaths, _jsonSerializerOptions);
        File.WriteAllText(configFile,
            JsonSerializer.Serialize(
                new Config { EnableProtocolHandler = false }, _jsonSerializerOptions));
        
        // Assert
        Assert.Equivalent(config, result);
    }
    
    [Fact]
    public void IsConfigUpToDate__CorrectsVersion__WhenTheConfigIsCreated()
    {
        // Arrange
        var config = new Config
        {
            Version = "0.0.0"
        };
        
        // Act
        var result = ConfigHelper.IsConfigUpToDate(config, out var newConfig);
        
        // Assert
        Assert.False(result);
        Assert.Equal(_version, newConfig.Version);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("1.0")]
    [InlineData("a.b.c")]
    [InlineData("1.0.0.0")]
    [InlineData("invalid")]
    public void IsConfigUpToDate__ThrowsException__WhenTheConfigVersionIsInvalid(string version)
    {
        // Arrange
        var config = new Config
        {
            Version = version
        };
        
        // Act
        var exception = Record.Exception(() => ConfigHelper.IsConfigUpToDate(config, out _));
        
        // Assert
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The configuration version is invalid!", exception.Message);
    }
    
    [Fact]
    public void IsConfigUpToDate__ThrowsException__WhenTheConfigVersionIsNewerThanTheAppVersion()
    {
        // Arrange
        var config = new Config { Version = $"{int.MaxValue}.{int.MaxValue}.{int.MaxValue}" };
        
        // Act
        var exception = Record.Exception(() => ConfigHelper.IsConfigUpToDate(config, out _));
        
        // Assert
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The configuration version is newer than the application version!", exception.Message);
    }
    
    [Fact]
    public void IsConfigUpToDate__ReturnsTrue__WhenTheConfigVersionMatches()
    {
        // Arrange
        var config = new Config { Version = _version };
        
        // Act
        var isUpToDate = ConfigHelper.IsConfigUpToDate(config, out _);
        
        // Assert
        Assert.True(isUpToDate);
    }
    
    [Fact]
    public void IsConfigUpToDate__ReturnsFalse__WhenTheConfigVersionIsOlder()
    {
        // Arrange
        var config = new Config { Version = "-1.0.0" };
        
        // Act
        var isUpToDate = ConfigHelper.IsConfigUpToDate(config, out _);
        
        // Assert
        Assert.False(isUpToDate);
    }
}
