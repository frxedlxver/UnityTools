using MyUtilities.PrefabPainter;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtilities.PrefabPainter
{
    [CreateAssetMenu(fileName = "PrefabPainterSettings", menuName = "Palette/Create Palette Data", order = 1)]
    public class PrefabPainterSettings : ScriptableObject
    {
        public PrefabPalette[] palettes;

        public enum KeyboardActions
        {
            NextPrefab,
            PrevPrefab
        }
    }
}
