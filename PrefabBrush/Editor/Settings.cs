using MyUtilities.CustomEvents;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MyUtilities.PrefabBrush
{
    public class Settings : ScriptableObject
    {
        private static readonly string FILE_NAME = "\\Settings.asset";
        private static readonly int MAX_PARENT_HISTORY = 10;


        private int _currentPaletteIndex = 0;
        private int _currentPrefabIndex = 0;
        private Transform currentParent;
        internal List<Transform> recentParents = new();
        internal List<PrefabPalette> Palettes = new();

        public GameObjectEvent OnPrefabSelected = new();
        public TransformEvent OnParentSelected = new();


        internal int CurrentPrefabIndex
        {
            get { return _currentPrefabIndex; }
            set
            {
                _currentPrefabIndex = value;

                if (_currentPaletteIndex < Palettes.Count)
                {
                    var curPalette = Palettes[_currentPaletteIndex];

                    if (_currentPrefabIndex < curPalette.prefabs.Length)
                    {
                        _currentPrefabIndex = value;
                        OnPrefabSelected?.Invoke(curPalette.prefabs[_currentPrefabIndex]);
                    }
                }
                EditorUtility.SetDirty(this);
            }
        }
        internal int CurrentPaletteIndex
        {
            get { return _currentPaletteIndex; }
            set
            {
                _currentPaletteIndex = value;
                EditorUtility.SetDirty(this);
            }
        }


        internal Transform CurrentParent
        {
            get
            {
                return currentParent;
            }

            set
            {
                if (value != currentParent)
                {
                    currentParent = value;

                    if (value != null)
                    {
                        OnParentSelected?.Invoke(value);

                        if (recentParents.Contains(value))
                        {
                            recentParents.Remove(value);
                        }
                        recentParents = recentParents.Prepend(value).ToList();


                        if (recentParents.Count > MAX_PARENT_HISTORY)
                        {
                            recentParents.RemoveAt(recentParents.Count - 1);
                        }
                    }
                }

            }
        }

        public PrefabPalette CurrentPalette { 
            get {
                return CurrentPaletteIndex < Palettes.Count ? Palettes[CurrentPaletteIndex] : null;
            } 
        }

        public static Settings LoadSettingsFromDirectory(string directory)
        {
            string settingsPath = directory + FILE_NAME;

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

        public List<string> GetPaletteNames()
        {
            List<string> paletteNames = new List<string>();
            for (int i = 0; i < Palettes.Count; i++)
            {
                string paletteName = Palettes[i] != null ? Palettes[i].name : "Missing Palette";
                paletteNames.Add(paletteName);
            }

            return paletteNames;
        }

        public List<string> GetRecentParentNames()
        {
            List<string> parentNames = new List<string>();
            for (int i = 0; i < recentParents.Count; i++)
            {
                string transformName = recentParents[i] != null ? recentParents[i].name : "Missing Transform";
                parentNames.Add(transformName);
            }

            return parentNames;
        }

        public void AddPalette(PrefabPalette palette)
        {
            Palettes.Add(palette);
        }

        public void RemovePalette(PrefabPalette palette)
        {
            Palettes.Remove(palette);
        }

        public enum KeyboardActions
        {
            NextPrefab,
            PrevPrefab
        }
    }
}
