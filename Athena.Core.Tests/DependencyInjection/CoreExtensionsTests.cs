using System.Reflection;
using System.Text.Json;
using Athena.Core.Configuration;
using Athena.Core.DependencyInjection;
using Athena.Core.Parser;
using Athena.Core.Parser.Shared;
using Athena.Core.Runner;
using Athena.Core.Runner.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Tests.DependencyInjection;

public class CoreExtensionsTests : IDisposable
{
    private readonly string _mainConfigDir;
    private readonly string _testsConfigDir;
    private readonly string _userWorkingDir;
    private readonly string _version;

    public CoreExtensionsTests()
    {
        var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        _mainConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            OperatingSystem.IsWindows() ? "Athena" : "athena");
        
        _userWorkingDir = Path.Combine(workingDir, "user");
        _testsConfigDir = Path.Combine(workingDir, "user-core-extensions-tests");
        
        _version = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
        
        if (!Directory.Exists(_userWorkingDir))
            Directory.CreateDirectory(_userWorkingDir);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Internal.Samples.Remove(_userWorkingDir);
        Internal.Samples.Remove(_testsConfigDir);
    }
    
    [Fact]
    public void AddAthenaCore__RegistersServices__WhenTheConfigurationIsDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore();
        
        // Act
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        Assert.NotNull(serviceProvider.GetRequiredService<ILogger<CoreExtensionsTests>>());
        Assert.NotNull(serviceProvider.GetRequiredService<ConfigPaths>());
        Assert.NotNull(serviceProvider.GetRequiredService<JsonSerializerOptions>());
        Assert.NotNull(serviceProvider.GetRequiredService<PathParser>());
        Assert.NotNull(serviceProvider.GetRequiredService<OpenerParser>());
        Assert.NotNull(serviceProvider.GetRequiredService<AppParser>());
        Assert.NotNull(serviceProvider.GetRequiredService<RunnerOptions>());
        Assert.NotNull(serviceProvider.GetRequiredService<AppRunner>());
        Assert.NotNull(serviceProvider.GetRequiredService<Config>());
    }
    
    [Fact]
    public void AddAthenaCore__RegistersCorrectConfigPaths__WhenTheConfigurationIsCustom()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var configFile = Path.Combine(_testsConfigDir, "config.json");
        var entriesDir = Path.Combine(_testsConfigDir, "entries");
        var filesDir = Path.Combine(_testsConfigDir, "files");
        var protocolsDir = Path.Combine(_testsConfigDir, "protocols");
        
        // Act
        services.AddAthenaCore(_testsConfigDir);
        var serviceProvider = services.BuildServiceProvider();
        var configPaths = serviceProvider.GetRequiredService<ConfigPaths>();
        
        // Assert
        Assert.Equal(entriesDir, configPaths.Subdirectories[ConfigType.Entries]);
        Assert.Equal(filesDir, configPaths.Subdirectories[ConfigType.Files]);
        Assert.Equal(protocolsDir, configPaths.Subdirectories[ConfigType.Protocols]);
        
        Assert.True(File.Exists(configFile));
        Assert.True(Directory.Exists(entriesDir));
        Assert.True(Directory.Exists(filesDir));
        Assert.True(Directory.Exists(protocolsDir));
    }
    
    [Fact]
    public void GetAppDataDir__ReturnsPortableConfigDir__WhenThePortableConfigDirExists()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore();
        
        // Act
        var appDataDir = CoreExtensions.GetAppDataDir();
        
        // Assert
        Assert.Equal(_userWorkingDir, appDataDir);
        Assert.True(Directory.Exists(_userWorkingDir));
    }
    
    [Fact]
    public void GetConfig__ReturnsConfig__WhenTheConfigIsRequested()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore(_testsConfigDir);
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var config = serviceProvider.GetRequiredService<Config>();
        
        // Assert
        Assert.NotNull(config);
    }
    
    [Fact]
    public void GetConfig__ReturnsDefaultConfig__WhenTheConfigFileDoesNotExist()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore(_testsConfigDir);
        
        var configFile = Path.Combine(_testsConfigDir, "config.json");
        var configFileContents = File.ReadAllText(configFile);
        
        // Act
        File.Delete(configFile);
        
        services.AddAthenaCore(_testsConfigDir);
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<Config>();
        
        File.WriteAllText(configFile, configFileContents);
        
        // Assert
        Assert.Equivalent(new Config { Version = _version }, config);
    }
    
    [Fact]
    public void GetConfig__UpdatesConfig__WhenTheConfigVersionIsOlder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore(_testsConfigDir);
        
        var configFile = Path.Combine(_testsConfigDir, "config.json");
        var configFileContents = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<Config>(configFileContents)!;
        
        // Act
        config.Version = "-1.0.0";
        File.WriteAllText(configFile, JsonSerializer.Serialize(config));
        
        var newConfig = CoreExtensions.GetConfig(_testsConfigDir, new JsonSerializerOptions());
        
        // Assert
        Assert.Equal(_version, newConfig.Version);
    }
    
    [Fact]
    public void GetAppDataDir__ReturnsUserConfigDir__WhenThePortableConfigDirDoesNotExist()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore();
        
        // Act
        Directory.Delete(_userWorkingDir, true);
        var appDataDir = CoreExtensions.GetAppDataDir();
        
        // Assert
        Assert.Equal(_mainConfigDir, appDataDir);
    }
    
    [Fact]
    public void GetConfig__CreatesConfig__WhenTheConfigFileDoesNotExist()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore(_testsConfigDir);
        
        var configFile = Path.Combine(_testsConfigDir, "config.json");
        
        // Act
        File.Delete(configFile);
        CoreExtensions.GetConfig(_testsConfigDir, new JsonSerializerOptions());
        
        var configFileContents = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<Config>(configFileContents)!;
        
        // Assert
        Assert.True(File.Exists(configFile));
        Assert.Equivalent(new Config { Version = _version }, config);
    }
    
    [Fact]
    public void GetConfig__ReturnsConfig__WhenTheConfigFileExists()
    {
        // Arrange
        if (!Directory.Exists(_testsConfigDir))
            Directory.CreateDirectory(_testsConfigDir);
        
        var configFile = Path.Combine(_testsConfigDir, "config.json");
        File.WriteAllText(configFile, JsonSerializer.Serialize(new Config
        {
            EnableProtocolHandler = true,
            StreamableProtocolPrefixes = [ "one", "two", "three" ],
            Version = _version
        }));
        
        var configFileContents = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<Config>(configFileContents)!;
        
        // Act
        var result = CoreExtensions.GetConfig(_testsConfigDir, new JsonSerializerOptions());
        
        // Assert
        Assert.Equivalent(config, result);
    }
    
    [Fact]
    public void IsConfigUpToDate__CorrectsVersion__WhenTheConfigIsCreated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAthenaCore(_testsConfigDir);
        
        var configFile = Path.Combine(_testsConfigDir, "config.json");
        var configFileContents = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<Config>(configFileContents)!;
        
        // Act
        var isUpToDate = CoreExtensions.IsConfigUpToDate(config, out var newConfig);
        
        // Assert
        Assert.False(isUpToDate);
        Assert.Equal(config with { Version = _version }, newConfig);
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
        var config = new Config { Version = version };
        
        // Act
        var exception = Record.Exception(() => CoreExtensions.IsConfigUpToDate(config, out _));
        
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
        var exception = Record.Exception(() => CoreExtensions.IsConfigUpToDate(config, out _));
        
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
        var isUpToDate = CoreExtensions.IsConfigUpToDate(config, out _);
        
        // Assert
        Assert.True(isUpToDate);
    }
    
    [Fact]
    public void IsConfigUpToDate__ReturnsFalse__WhenTheConfigVersionIsOlder()
    {
        // Arrange
        var config = new Config { Version = "-1.0.0" };
        
        // Act
        var isUpToDate = CoreExtensions.IsConfigUpToDate(config, out _);
        
        // Assert
        Assert.False(isUpToDate);
    }
}
