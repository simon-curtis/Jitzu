using Jitzu.Shell;
using Shouldly;

namespace Jitzu.Tests;

public class HistoryManagerPredictionFilterTests
{
    private static async Task<HistoryManager> CreateWithHistory(params string[] commands)
    {
        var manager = new HistoryManager(persist: false);
        await manager.InitialiseAsync();
        foreach (var cmd in commands)
            await manager.WriteAsync(cmd);
        return manager;
    }

    [Test]
    public async Task GetPredictions_WithoutFilter_ReturnsAllMatches()
    {
        var manager = await CreateWithHistory("cd Foo", "cd Bar", "cd Baz");

        var predictions = manager.GetPredictions("cd ", 5);

        predictions.Count.ShouldBe(3);
    }

    [Test]
    public async Task GetPredictions_WithFilter_ExcludesRejected()
    {
        var manager = await CreateWithHistory("cd Foo", "cd Bar", "cd Baz");

        var predictions = manager.GetPredictions("cd ", 5, p => p != "cd Bar");

        predictions.Count.ShouldBe(2);
        predictions.ShouldNotContain("cd Bar");
    }

    [Test]
    public async Task GetPredictions_FilterRejectsAll_ReturnsEmpty()
    {
        var manager = await CreateWithHistory("cd Foo", "cd Bar");

        var predictions = manager.GetPredictions("cd ", 5, _ => false);

        predictions.ShouldBeEmpty();
    }

    [Test]
    public async Task GetPredictions_NullFilter_BehavesLikeNoFilter()
    {
        var manager = await CreateWithHistory("cd Foo", "cd Bar");

        var withNull = manager.GetPredictions("cd ", 5, null);
        var without = manager.GetPredictions("cd ", 5);

        withNull.Count.ShouldBe(without.Count);
    }

    [Test]
    public async Task GetPredictions_FilterWithMaxCount_RespectsLimit()
    {
        var manager = await CreateWithHistory("cd A", "cd B", "cd C", "cd D");

        // Filter passes all, but maxCount is 2
        var predictions = manager.GetPredictions("cd ", 2, _ => true);

        predictions.Count.ShouldBe(2);
    }

    [Test]
    public async Task GetPredictions_FilterWithMaxCount_CountsOnlyPassingItems()
    {
        var manager = await CreateWithHistory("cd A", "cd B", "cd C", "cd D");

        // Filter rejects "cd B" and "cd D", maxCount is 3
        var predictions = manager.GetPredictions("cd ", 3, p => p is not "cd B" and not "cd D");

        predictions.Count.ShouldBe(2);
        predictions.ShouldContain("cd C");
        predictions.ShouldContain("cd A");
    }

    [Test]
    public async Task GetPredictions_EmptyPrefix_ReturnsEmpty()
    {
        var manager = await CreateWithHistory("cd Foo");

        var predictions = manager.GetPredictions("", 5, _ => true);

        predictions.ShouldBeEmpty();
    }

    [Test]
    public async Task GetPredictions_NonCdCommands_NotAffectedByFilter()
    {
        var manager = await CreateWithHistory("ls -la", "ls -R");

        // Filter that rejects everything — but non-cd commands should still use it
        // The filter applies to ALL predictions, not just cd
        var predictions = manager.GetPredictions("ls ", 5, _ => false);

        predictions.ShouldBeEmpty();
    }

    [Test]
    public async Task GetPredictions_IntegrationWithHistoryPredictionFilter()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"jz_test_{Guid.NewGuid():N}");
        var subDir = Path.Combine(tempDir, "ValidDir");
        Directory.CreateDirectory(subDir);

        try
        {
            var manager = await CreateWithHistory("cd ValidDir", "cd GoneDir", "cd /absolute");

            var predictions = manager.GetPredictions("cd ", 5,
                p => HistoryPredictionFilter.IsValid(p, tempDir));

            predictions.ShouldContain("cd ValidDir");
            predictions.ShouldContain("cd /absolute");
            predictions.ShouldNotContain("cd GoneDir");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
