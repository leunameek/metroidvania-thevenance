using NUnit.Framework;

public class ExplorationObjectiveModelTests
{
    [Test]
    public void RevisitingObjectDoesNotUnlockCompletionBeforeAllRequiredObjects()
    {
        var model = new ExplorationObjectiveModel(new[] { "a", "b", "c" });
        model.Analyze("a");
        model.Analyze("a");
        model.Analyze("unknown");
        Assert.AreEqual(1, model.AnalyzedCount);
        Assert.IsFalse(model.IsComplete);
        model.Analyze("b");
        model.Analyze("c");
        Assert.IsTrue(model.IsComplete);
    }

    [Test]
    public void MissingConfigurationIsNotReportedAsCompletion()
    {
        var model = new ExplorationObjectiveModel(new[] { "", null, " " });
        Assert.IsFalse(model.IsComplete);
        Assert.IsFalse(model.Analyze(null));
    }
}
