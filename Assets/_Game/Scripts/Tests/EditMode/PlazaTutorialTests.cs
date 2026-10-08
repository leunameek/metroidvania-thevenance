using NUnit.Framework;

public class PlazaTutorialTests
{
    [Test]
    public void ObjectCannotCompleteWithoutMovement()
    {
        for (int i = 0; i < 3; i++)
        {
            var lesson = new PlazaLessonModel(i);
            lesson.Freeze(true, 10);
            Assert.IsFalse(lesson.Complete);
        }
    }
    [Test]
    public void LessonsRequireTheirOwnAxes()
    {
        var yaw = new PlazaLessonModel(0);
        var pitch = new PlazaLessonModel(1);
        for (int i = 0; i < 4; i++) { yaw.Move(0, 10, true); pitch.Move(10, 0, true); }
        Assert.IsFalse(yaw.Complete); Assert.IsFalse(pitch.Complete);
        for (int i = 0; i < 4; i++) { yaw.Move(10, 0, true); pitch.Move(0, 10, true); }
        Assert.IsTrue(yaw.Complete); Assert.IsTrue(pitch.Complete);
        Assert.IsTrue(yaw.UsedHands);
    }
    [Test]
    public void ThirdLessonNeedsBothAxesThenContinuousFreeze()
    {
        var lesson = new PlazaLessonModel(2);
        for (int i = 0; i < 4; i++) lesson.Move(10, 10, false);
        Assert.IsFalse(lesson.Complete);
        lesson.Freeze(true, 0.5f); lesson.Freeze(false, 0.1f); lesson.Freeze(true, 0.3f);
        Assert.IsFalse(lesson.Complete);
        lesson.Freeze(true, 0.4f);
        Assert.IsTrue(lesson.Complete);
        Assert.IsFalse(lesson.UsedHands, "Mouse completion is not evidence of real camera use.");
    }
    [Test]
    public void SingleTrackingSpikeDoesNotCompleteLesson()
    {
        var lesson = new PlazaLessonModel(0);
        lesson.Move(900, 0, true);
        Assert.IsFalse(lesson.Complete);
    }
    [Test]
    public void CombatIgnoresEarlyAndDuplicateActions()
    {
        var model = new PlazaCombatModel(); model.Start();
        Assert.IsFalse(model.Defend(PlazaDefense.Dodge));
        Assert.IsTrue(model.Attack());
        Assert.IsFalse(model.Attack());
        Assert.IsFalse(model.Defend(PlazaDefense.Dodge));
        Assert.AreEqual(1, model.Hits);
        Assert.AreEqual(0, model.Mistakes);
    }
    [Test]
    public void TimeoutRetriesTheSameDefenseWithoutFreeDamage()
    {
        var model = new PlazaCombatModel(); model.Start(); model.Attack();
        model.Tick(2); model.Tick(3);
        Assert.AreEqual(1, model.Mistakes);
        model.Tick(3);
        Assert.AreEqual(PlazaCombatPhase.Telegraph, model.Phase);
        Assert.AreEqual(1, model.Hits);
        Assert.AreEqual(PlazaDefense.Dodge, model.Expected);
    }
    [Test]
    public void WrongDefenseCannotAdvanceTraining()
    {
        var model = new PlazaCombatModel(); model.Start(); model.Attack(); model.Tick(2);
        Assert.IsTrue(model.Defend(PlazaDefense.Guard)); model.Tick(3);
        Assert.AreEqual(PlazaCombatPhase.Telegraph, model.Phase);
        Assert.AreEqual(1, model.Mistakes);
    }
    [Test]
    public void TwoGuidedBlowsThenTwoToReadWinTraining()
    {
        var model = new PlazaCombatModel(); model.Start();
        var expected = new[] { PlazaDefense.Dodge, PlazaDefense.Guard, PlazaDefense.Guard, PlazaDefense.Dodge };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.IsTrue(model.Attack()); model.Tick(2);
            Assert.AreEqual(PlazaCombatPhase.React, model.Phase);
            Assert.AreEqual(expected[i], model.Expected);
            Assert.AreEqual(i < PlazaCombatModel.GuidedLessons, model.ShowsAnswer, "blow " + (i + 1));
            Assert.AreEqual(expected[i] == PlazaDefense.Dodge ? DuelSignal.Sweep : DuelSignal.Front, model.Signal);
            Assert.IsTrue(model.Defend(expected[i])); model.Tick(2);
            Assert.AreEqual(PlazaCombatPhase.Attack, model.Phase);
        }
        Assert.IsTrue(model.Attack());
        Assert.AreEqual(PlazaCombatPhase.Won, model.Phase);
        Assert.AreEqual(5, PlazaCombatModel.AttacksToWin);
        Assert.IsFalse(model.Attack());
        model.Start();
        Assert.AreEqual(0, model.Hits);
        Assert.AreEqual(PlazaCombatPhase.Attack, model.Phase);
        model.Cancel(); Assert.IsFalse(model.IsActive);
    }
    [Test]
    public void AccessibilityShowsTheAnswerOfTheBlowsToRead()
    {
        var model = new PlazaCombatModel { ShowAnswers = true }; model.Start();
        for (int i = 0; i < 3; i++) { model.Attack(); model.Tick(2); model.Defend(model.Expected); model.Tick(2); }
        Assert.IsFalse(model.Guided);
        Assert.IsTrue(model.ShowsAnswer);
    }
    [Test]
    public void AMistakeExplainsWhatTheGuardianShowed()
    {
        var model = new PlazaCombatModel(); model.Start(); model.Attack(); model.Tick(2);
        model.Defend(PlazaDefense.Guard);
        StringAssert.Contains("era Esquivar", model.Reading);
    }
}
