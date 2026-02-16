using Jitzu.Shell.Core;
using Jitzu.Shell.Core.Commands;
using Shouldly;

namespace Jitzu.Tests;

public class StreamingBuiltinCommandsTests
{
    [Test]
    public async Task CatCommand_ImplementsStreamingInterface()
    {
        // Assert - Verify the type implements the interface
        typeof(IStreamingCommand).IsAssignableFrom(typeof(CatCommand)).ShouldBeTrue();
    }

    [Test]
    public async Task GrepCommand_ImplementsStreamingInterface()
    {
        // Assert
        typeof(IStreamingCommand).IsAssignableFrom(typeof(GrepCommand)).ShouldBeTrue();
    }

    [Test]
    public async Task TeeCommand_ImplementsStreamingInterface()
    {
        // Assert
        typeof(IStreamingCommand).IsAssignableFrom(typeof(TeeCommand)).ShouldBeTrue();
    }

    [Test]
    public void StreamingPipeFunctions_ExistOnPipelineClass()
    {
        // Verify that all streaming functions are available
        var methods = typeof(StreamingPipeFunctions).GetMethods();

        methods.Any(m => m.Name == "FirstAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "LastAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "NthAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "GrepAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "HeadAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "TailAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "SortAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "UniqAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "WcAsync").ShouldBeTrue();
        methods.Any(m => m.Name == "TeeAsync").ShouldBeTrue();
    }

    [Test]
    public void IStreamingCommand_InterfaceExists()
    {
        // Verify the streaming interface exists and has correct signature
        var interfaceType = typeof(IStreamingCommand);
        interfaceType.ShouldNotBeNull();

        var method = interfaceType.GetMethod("StreamAsync");
        method.ShouldNotBeNull();
        method!.ReturnType.Name.ShouldBe("IAsyncEnumerable`1");
    }

}
