using UnityEngine;

[CreateAssetMenu(menuName = "Nemequene/Week08/Technical demo configuration")]
public sealed class TechnicalDemoConfig : ScriptableObject
{
    [Min(1f)] public float interactionDistance = 3f;
    public float fallThreshold = -8f;
    [TextArea] public string introduction = "Nemequene ha llegado a un lugar sagrado donde los límites entre los tres mundos comienzan a debilitarse.";
    [TextArea] public string objective = "Explora la plaza y analiza los tres objetos temporales.";
    [TextArea] public string completion = "Las rutas han sido encontradas, pero Nemequene todavía no posee los recursos necesarios para atravesarlas.";
}
