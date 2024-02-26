using MyUtilities.ClassExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace MyUtilities.PrefabBrush
{
    public class PrefabBrushWindow : EditorWindow
    {
        private static bool windowOpen;
        private static bool windowActive;
        private PrefabPalette activePalette;
        private Settings settings;
        private GhostPrefabManager ghostManager = new();
        private int selectedPrefabIndex;
        private float prefabZDelta = 1f;
        private float rotationDelta = 1f;
        private bool hidePrefabInHierarchy = true;

        private const KeyCode NextPrefabKey = KeyCode.RightArrow;
        private const KeyCode LastPrefabKey = KeyCode.LeftArrow;

        public GameObject TargetParent;
        public GameObject SelectedPrefab
        {
            get
            {
                if (activePalette == null || activePalette.prefabs == null || activePalette.prefabs.Length == 0 || selectedPrefabIndex == -1)
                    return null;
                else return activePalette.prefabs.ElementAt(selectedPrefabIndex);
            }
        }

        public int SelectedPrefabIndex
        {
            get { return selectedPrefabIndex; }
            set
            {
                if (activePalette == null || activePalette.prefabs == null) return;
                value = (value + activePalette.prefabs.Length) % activePalette.prefabs.Length;
                if (value != selectedPrefabIndex)
                {
                    selectedPrefabIndex = value;
                    ghostManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy); // Refresh ghost prefab for the new selection
                }
            }
        }

        public PrefabBrushWindow()
        {

        }

        private static PrefabBrushWindow GetWindow()
        {
            return GetWindow<PrefabBrushWindow>("Prefab Brush Window");
        }


        [MenuItem("Tools/Prefab Brush Window")]
        public static void ShowWindow()
        {
            GetWindow().Show();

        }

        public static void CloseWindow()
        {
            GetWindow().Close();
        }

        public static void ToggleActive()
        {
            SetActive(!windowActive);
        }

        public static void SetActive(bool active)
        {
            if (!windowOpen)
            {
                ShowWindow();
            }

            windowActive = active;

            if (windowActive)
            {
                GetWindow().OnBecameActive();
            } else
            {
                GetWindow().OnBecameInactive();
            }
        }

        private void OnBecameInvisible()
        {
            OnBecameInactive();
        }

        private void OnBecameVisible()
        {
            OnBecameActive();
        }

        private void OnBecameActive()
        {
            if (settings == null) LoadSettings();
            // remove, in case it has already been added
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            if (SelectedPrefab != null && ghostManager != null)
            {
                ghostManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy);
                ghostManager.prefabScale = 1f;
                ghostManager.prefabZPos = 0f;
                ghostManager.prefabZRotation = 0f;
            }

            SceneView.RepaintAll();
            SceneView.currentDrawingSceneView?.Focus();
        }

        private void OnBecameInactive()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            ghostManager.DestroyGhost();
            SceneView.currentDrawingSceneView?.Repaint();
            SceneView.currentDrawingSceneView?.Focus();
        }
        void OnEnable()
        {

            windowOpen = true;
            windowActive = true;
            OnBecameActive();
            if (settings.Palettes != null && settings.Palettes.Length > 0)
            {
                activePalette = settings.Palettes[settings.selectedPalette];
            }
        }

        void OnDisable()
        {
            windowOpen = false;
            windowActive = false;
            OnBecameInactive();
        }

        void OnGUI()
        {
            GUILayout.Label("Prefab Palettes", EditorStyles.boldLabel);

            DrawPaletteList();

            EditorGUILayout.Space(20);

            GUILayout.Label("Active Palette", EditorStyles.boldLabel);

            DrawPalletteSelector();

            EditorGUILayout.Space(20);

            GUILayout.Label("Parent", EditorStyles.boldLabel);
            TargetParent = (GameObject)EditorGUILayout.ObjectField(TargetParent, typeof(GameObject), true);

            if (GUILayout.Button("RemoveParent"))
            {
                TargetParent = null;
            }
            ghostManager.SetParent(TargetParent);


            if (activePalette != null)
            {
                EditorGUILayout.Space(20);

                GUILayout.Label("Prefabs", EditorStyles.boldLabel);

                DrawPrefabSelector();
            }

        }

        void OnSceneGUI(SceneView sceneView)
        {
            DrawFloatingWindow(sceneView);

            Event e = Event.current;

            ghostManager.HandleMouseMovement(sceneView, e);

            switch (e.type)
            {
                case EventType.KeyDown:
                    HandleKeyInput(e);
                    break;

                // Handling scaling only when Ctrl is pressed and mouse wheel is scrolled
                case EventType.ScrollWheel:
                    HandleScrollInput(e);
                    break;
                case EventType.MouseDown when e.button == 0:
                    HandleLMB(e);
                    break;
                case EventType.Repaint:
                    sceneView.Repaint();
                    break;
            }

            this.Repaint();
        }

        private void HandleKeyInput(Event e)
        {
            switch (e.keyCode)
            {
                case NextPrefabKey:
                    SelectedPrefabIndex += 1;
                    e.Use();
                    break;
                case LastPrefabKey:
                    SelectedPrefabIndex -= 1;
                    e.Use();
                    break;
                default:
                    break;
            }
        }

        private void DrawFloatingWindow(SceneView sceneView)
        {
            if (SelectedPrefab != null)
            {
                Handles.BeginGUI();

                int height = 225;
                int width = 150;
                int margin = 2;


                var window = GUILayout.Window(0, new Rect(margin, sceneView.cameraViewport.size.y - (height + margin), width, height), (id) =>
                {
                    GUILayout.Label("Scale", EditorStyles.boldLabel);
                    ghostManager.prefabScale = EditorGUILayout.FloatField("Scale", ghostManager.prefabScale);
                    GUILayout.Space(30);
                    GUILayout.Label("Z Position", EditorStyles.boldLabel);
                    ghostManager.prefabZPos = EditorGUILayout.FloatField("Z Position", ghostManager.prefabZPos);
                    prefabZDelta = EditorGUILayout.FloatField("Z position change on scroll", prefabZDelta);

                    GUILayout.Space(20);
                    GUILayout.Label("Z Rotation", EditorStyles.boldLabel);
                    ghostManager.prefabZRotation = EditorGUILayout.FloatField("Z Rotation", ghostManager.prefabZRotation);
                    rotationDelta = EditorGUILayout.FloatField("Z Rotation change on scroll", rotationDelta);
                }, "Prefab Brush");
                Handles.EndGUI();
            }
        }

        private void HandleLMB(Event e)
        {
            if (selectedPrefabIndex < 0 || selectedPrefabIndex >= activePalette.prefabs.Length || ghostManager.ghostPrefab == null)
            {
                Debug.LogWarning("Prefab Brush Tool: No prefab selected or index out of range.");
                return;
            }

            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(activePalette.prefabs[selectedPrefabIndex]);
            if (TargetParent != null)
            {
                instantiatedPrefab.transform.parent = TargetParent.transform;
            }
            instantiatedPrefab.transform.CopyValuesFrom(ghostManager.ghostPrefab.transform); // copy transform
            Undo.RegisterCreatedObjectUndo(instantiatedPrefab, "Instantiate Prefab");
            e.Use();
        }

        private void HandleScrollInput(Event e)
        {
            float scrollValue = e.delta.y;

            // holding shift returns a horizontal scroll value on windows
            if (e.shift)
            {
                scrollValue = e.delta.x;
            }

            if ((e.control || e.command) && e.shift)
            {
                ghostManager.AdjustPrefabZRotation(scrollValue, rotationDelta);
                e.Use();
            }
            else if (e.control || e.command)
            {
                ghostManager.AdjustPrefabScale(scrollValue);
                e.Use();

            }
            else if (e.alt)
            {
                ghostManager.AdjustPrefabZPosition(scrollValue, prefabZDelta);
                e.Use();
            }
        }

        private void LoadSettings()
        {
            // Get the MonoScript object for this EditorWindow
            MonoScript monoScript = MonoScript.FromScriptableObject(this);
            // Get the path to the script
            string scriptPath = AssetDatabase.GetAssetPath(monoScript);
            // Extract the directory from the path
            string currentDirectory = System.IO.Path.GetDirectoryName(scriptPath);

            this.settings = Settings.LoadSettingsFromDirectory(currentDirectory);
        }

        private void DrawPrefabSelector()
        {
            if (activePalette.prefabs == null || activePalette.prefabs.Length == 0)
            {
                EditorGUILayout.HelpBox("The Prefab Palette is empty.", MessageType.Info);
                return;
            }

            // Display a dropdown for selecting a prefab from the selectedPalette
            SelectedPrefabIndex = EditorGUILayout.Popup("Select Prefab", SelectedPrefabIndex, GetPrefabNames());
        }

        void DrawPaletteList()
        {
            // Ensure settings is not null
            if (settings == null)
            {
                LoadSettings(); // Make sure this method properly initializes settings
            }

            // Further ensure that settings and its palettes are initialized
            if (settings != null && settings.Palettes != null)
            {
                for (int i = 0; i < settings.Palettes.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    settings.Palettes[i] = (PrefabPalette)EditorGUILayout.ObjectField(settings.Palettes[i], typeof(PrefabPalette), false);


                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        List<PrefabPalette> tempList = new List<PrefabPalette>(settings.Palettes);
                        tempList.RemoveAt(i);
                        settings.Palettes = tempList.ToArray();
                        GUIUtility.ExitGUI(); // Prevents the rest of the GUI code from running after modifying the list
                    }

                    EditorUtility.SetDirty(settings); // Mark the PaletteData asset as dirty to ensure it saves

                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Add Palette"))
            {
                if (settings.Palettes == null) settings.Palettes = new PrefabPalette[0]; // Ensure the palettes array is initialized

                List<PrefabPalette> tempList = new List<PrefabPalette>(settings.Palettes);
                tempList.Add(null); // Adds a null entry, to be set in the Inspector
                settings.Palettes = tempList.ToArray();
                EditorUtility.SetDirty(settings); // Mark as dirty to save changes
            }
        }


        void DrawPalletteSelector()
        {
            // Palette List Dropdown
            if (settings.Palettes != null && settings.Palettes.Length > 0)
            {
                List<string> paletteNames = new List<string>();
                int currentPaletteIndex = -1;
                for (int i = 0; i < settings.Palettes.Length; i++)
                {
                    string paletteName = settings.Palettes[i] != null ? settings.Palettes[i].name : "Unnamed Palette";
                    paletteNames.Add(paletteName);
                    if (settings.Palettes[i] == activePalette)
                    {
                        currentPaletteIndex = i;
                        settings.selectedPalette = i;
                        EditorUtility.SetDirty(settings);
                    }
                }

                int newPaletteIndex = EditorGUILayout.Popup(currentPaletteIndex, paletteNames.ToArray());
                if (newPaletteIndex != currentPaletteIndex)
                {
                    // Update active palette and reset selected prefab
                    activePalette = settings.Palettes[newPaletteIndex];
                    selectedPrefabIndex = 0; // Automatically select the first prefab
                    ghostManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy); // Refresh ghost prefab for the new selection
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No Palettes available. Please add Palettes to the list.", MessageType.Info);
            }

            if (activePalette == null)
            {
                EditorGUILayout.HelpBox("Please assign a Prefab Palette.", MessageType.Warning);
                return;
            }
        }
        string[] GetPrefabNames()
        {
            string[] prefabNames = new string[activePalette.prefabs.Length];
            for (int i = 0; i < activePalette.prefabs.Length; i++)
            {
                prefabNames[i] = activePalette.prefabs[i] ? activePalette.prefabs[i].name : "Empty Slot";
            }
            return prefabNames;
        }
    }
}
