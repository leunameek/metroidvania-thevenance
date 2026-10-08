using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// The project's MVC (Specs/Arquitectura.md): the Controller reads the input and drives the Model;
// the Model notifies through C# events and knows nothing of scenes; the View only presents.
public class ArchitectureTests
{
    private static string Scripts => Path.Combine(Application.dataPath, "_Game", "Scripts");

    private static string[] Sources(string layer) =>
        Directory.GetFiles(Path.Combine(Scripts, layer), "*.cs", SearchOption.AllDirectories);

    // Comments may name anything; only code is checked.
    private static string Code(string path) =>
        Regex.Replace(File.ReadAllText(path), @"//[^\n]*", "");

    [Test]
    public void TheModelHasNoSceneBehaviourAndNoDependencyOnTheOtherLayers()
    {
        foreach (var file in Sources("Model"))
        {
            string code = Code(file);
            Assert.IsFalse(Regex.IsMatch(code, @":\s*MonoBehaviour\b"), Path.GetFileName(file) + " is a MonoBehaviour in the Model");
            Assert.IsFalse(Regex.IsMatch(code, @"\b(GameObject|Transform|Renderer|Camera\.main|FindObjectOfType|FindFirstObjectByType)\b"),
                Path.GetFileName(file) + " touches the scene");
            Assert.IsFalse(Regex.IsMatch(code, @"\b(Keyboard|Mouse|Gamepad)\.current\b"), Path.GetFileName(file) + " reads the input");
        }
        // Its assembly cannot even see the Controller and View (Prototype.Runtime).
        string asmdef = File.ReadAllText(Path.Combine(Scripts, "Model", "Prototype.Model.asmdef"));
        StringAssert.DoesNotContain("Prototype.Runtime", asmdef);
    }

    [Test]
    public void OnlyTheControllerReadsTheInput()
    {
        foreach (var file in Sources("View"))
        {
            string code = Code(file);
            Assert.IsFalse(Regex.IsMatch(code, @"\b(Keyboard|Mouse|Gamepad)\.current\b|GameBindings\.(Pressed|Held)|wasPressedThisFrame|Input\.Get"),
                Path.GetFileName(file) + " reads the input in the View");
        }
    }

    [Test]
    public void TheViewDoesNotChangeTheModel()
    {
        foreach (var file in Sources("View"))
        {
            string code = Code(file);
            Assert.IsFalse(Regex.IsMatch(code, @"\.TakeDamage\(|\.Heal\(|\.Revive\(|CampaignProgress\.Set\(|MIProgress\.Set\(|MSProgress\.Set\("),
                Path.GetFileName(file) + " changes the game state from the View");
        }
    }

    [Test]
    public void TheModelsOfTheDiagramExistAndNotify()
    {
        var model = typeof(HealthModel).Assembly;
        foreach (var name in new[] { "HealthModel", "PlayerAbilityModel", "InventoryModel", "BossFightModel", "GameState",
                     "MeleeModel", "ArcherModel", "ShieldEnemyModel", "TurnDuelModel", "CampaignModel" })
            Assert.IsNotNull(model.GetType(name), name + " is missing from the Model");
        foreach (var notifying in new[] { typeof(HealthModel), typeof(InventoryModel), typeof(GameState), typeof(BossFightModel) })
            Assert.IsTrue(notifying.GetEvents().Any(), notifying.Name + " has no events");
    }
}
