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
        model.Tick(2);
        Assert.AreEqual(PlazaCombatPhase.Telegraph, model.Phase);
        Assert.AreEqual(1, model.Hits);
        Assert.AreEqual(PlazaDefense.Dodge, model.Expected);
    }
    [Test]
    public void WrongDefenseCannotAdvanceTraining()
    {
        var model = new PlazaCombatModel(); model.Start(); model.Attack(); model.Tick(2);
        Assert.IsTrue(model.Defend(PlazaDefense.Guard)); model.Tick(2);
        Assert.AreEqual(PlazaCombatPhase.Telegraph, model.Phase);
        Assert.AreEqual(1, model.Mistakes);
    }
    [Test]
    public void VictoryRequiresThreeAttacksAndBothDefenses()
    {
        var model = new PlazaCombatModel(); model.Start(); model.Attack(); model.Tick(2);
        model.Defend(PlazaDefense.Dodge); model.Tick(2); model.Attack(); model.Tick(2);
        Assert.AreEqual(PlazaDefense.Guard, model.Expected);
        model.Defend(PlazaDefense.Guard); model.Tick(2); model.Attack();
        Assert.AreEqual(PlazaCombatPhase.Won, model.Phase);
        Assert.IsFalse(model.Attack());
        model.Start();
        Assert.AreEqual(0, model.Hits);
        Assert.AreEqual(PlazaCombatPhase.Attack, model.Phase);
        model.Cancel(); Assert.IsFalse(model.IsActive);
    }
}
