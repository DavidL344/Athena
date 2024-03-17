using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Configuration;
using Athena.Core.Extensions.DependencyInjection;
using Athena.Core.Internal;
using Athena.Core.Options;
using Athena.Core.Parser;
using Athena.Core.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Tests.Extensions.DependencyInjection;

public class CoreExtensionsTests : IDisposable
{
    private readonly string _testsConfigDir;
    private readonly string _userWorkingDir;

    public CoreExtensionsTests()
    {
        var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        _userWorkingDir = Path.Combine(workingDir, "user");
        _testsConfigDir = Path.Combine(workingDir, "user-core-extensions-tests");
        
        if (!Directory.Exists(_userWorkingDir))
            Directory.CreateDirectory(_userWorkingDir);
        
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        Startup.CheckEntries(new ConfigPaths(_userWorkingDir), jsonSerializerOptions)
            .GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        if (Directory.Exists(_userWorkingDir))
            Directory.Delete(_userWorkingDir, true);
        
        if (Directory.Exists(_testsConfigDir))
            Directory.Delete(_testsConfigDir, true);
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
        Assert.NotNull(serviceProvider.GetRequiredService<AppEntryParser>());
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
        Assert.Equal(entriesDir, configPaths.Subdirectories[ConfigType.AppEntries]);
        Assert.Equal(filesDir, configPaths.Subdirectories[ConfigType.FileExtensions]);
        Assert.Equal(protocolsDir, configPaths.Subdirectories[ConfigType.Protocols]);
        
        Assert.True(File.Exists(configFile));
        Assert.True(Directory.Exists(entriesDir));
        Assert.True(Directory.Exists(filesDir));
        Assert.True(Directory.Exists(protocolsDir));
    }
}
