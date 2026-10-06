using System;
using UnityEngine;

namespace Nemequene.UI
{
    [CreateAssetMenu(menuName = "Nemequene/UI/Dialogue")]
    public sealed class DialogueData : ScriptableObject
    {
        [Serializable] public sealed class Line { public string speakerKey, textKey; }
        public Line[] lines;
    }
}
