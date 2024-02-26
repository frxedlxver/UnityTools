using UnityEditor;
using UnityEngine;

namespace MyUtilities.PrefabBrush
{
    public class Settings : ScriptableObject
    {
        public PrefabPalette[] Palettes;
        public int selectedPalette = 0;

        private static readonly string fileName = "\\SettingsBase.asset";

        public static Settings LoadSettingsFromDirectory(string directory)
        {
            string settingsPath = directory + fileName;

            Settings s = AssetDatabase.LoadAssetAtPath<Settings>(settingsPath);

            if (s == null)
            {
                s = CreateInstance<Settings>();

                // Create the asset in the specified path
                AssetDatabase.CreateAsset(s, settingsPath);
                AssetDatabase.SaveAssets(); // Save changes to the asset database
            }

            return s;
        }

        public enum KeyboardActions
        {
            NextPrefab,
            PrevPrefab
        }
    }
}
