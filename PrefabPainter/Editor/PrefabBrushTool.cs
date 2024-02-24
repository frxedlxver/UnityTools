using MyUtilities.ClassExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MyUtilities.PrefabPainter
{
    public class PrefabBrushTool : EditorWindow
    {
        private PrefabPalette activePalette;
        private PrefabPainterSettings settings;
        private GhostPrefabManager ghostPrefabManager = new();
        private int selectedPrefabIndex = -1;
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
                if (activePalette == null || activePalette.prefabs == null || activePalette.prefabs.Length == 0)
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
                    ghostPrefabManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy); // Refresh ghost prefab for the new selection
                    ghostPrefabManager.ShowGhostPrefab(true);
                }
            }
        }



        [MenuItem("Tools/Prefab Brush Tool")]
        public static void ShowWindow()
        {
            GetWindow<PrefabBrushTool>("Prefab Brush");
        }

        private void OnBecameInvisible()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            ghostPrefabManager.DestroyGhost();
        }

        private void OnBecameVisible()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            ghostPrefabManager.DestroyGhost();
        }

        void OnEnable()
        {  
            LoadSettings();
            if (SelectedPrefab != null)
            {
                ghostPrefabManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy);
                ghostPrefabManager.prefabScale = 1f;
                ghostPrefabManager.prefabZPos = 0f;
                ghostPrefabManager.prefabZRotation = 0f;
            }
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            ghostPrefabManager.DestroyGhost();
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
            ghostPrefabManager.SetParent(TargetParent);


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

            HandleMouseMovement(sceneView, e);

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

                float height = 225;
                float width = 150;
                float margin = 2;
                GUILayout.Window(0, new Rect(margin, sceneView.cameraViewport.size.y - (height + margin), width, height), (id) =>
                {
                    GUILayout.Label("Scale", EditorStyles.boldLabel);
                    ghostPrefabManager.prefabScale = EditorGUILayout.FloatField("Scale", ghostPrefabManager.prefabScale);
                    GUILayout.Space(30);
                    GUILayout.Label("Z Position", EditorStyles.boldLabel);
                    ghostPrefabManager.prefabZPos = EditorGUILayout.FloatField("Z Position", ghostPrefabManager.prefabZPos);
                    prefabZDelta = EditorGUILayout.FloatField("Z position change on scroll", prefabZDelta);

                    GUILayout.Space(20);
                    GUILayout.Label("Z Rotation", EditorStyles.boldLabel);
                    ghostPrefabManager.prefabZRotation = EditorGUILayout.FloatField("Z Rotation", ghostPrefabManager.prefabZRotation);
                    rotationDelta = EditorGUILayout.FloatField("Z Rotation change on scroll", rotationDelta);
                }, "Prefab Brush");
                Handles.EndGUI();
            }
        }

        private void HandleLMB(Event e)
        {
            if (selectedPrefabIndex < 0 || selectedPrefabIndex >= activePalette.prefabs.Length || ghostPrefabManager.ghostPrefab == null)
            {
                Debug.LogWarning("Prefab Brush Tool: No prefab selected or index out of range.");
                return;
            }

            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(activePalette.prefabs[selectedPrefabIndex]);
            if (TargetParent != null)
            {
                instantiatedPrefab.transform.parent = TargetParent.transform;
            }
            instantiatedPrefab.transform.CopyValuesFrom(ghostPrefabManager.ghostPrefab.transform); // copy transform
            Undo.RegisterCreatedObjectUndo(instantiatedPrefab, "Instantiate Prefab");
            e.Use();

        }

        private void HandleMouseMovement(SceneView sceneView, Event e)
        {
            // Determine if the mouse is within the Scene view
            bool isMouseInSceneView = sceneView.cameraViewport.Contains(e.mousePosition);

            if (isMouseInSceneView)
            {
                ghostPrefabManager.ShowGhostPrefab(true);
                if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                {

                    Vector3 mouseWindowPos = new(e.mousePosition.x, sceneView.camera.pixelHeight - e.mousePosition.y);
                    Vector3 mouseWorldPos = sceneView.camera.ScreenToWorldPoint(mouseWindowPos);

                    ghostPrefabManager.SetGhostPosition(mouseWorldPos);
                }
            }
            else
            {
                ghostPrefabManager.ShowGhostPrefab(false);
            }
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
                ghostPrefabManager.AdjustPrefabZRotation(scrollValue, rotationDelta);
                e.Use();
            }
            else if (e.control || e.command)
            {
                ghostPrefabManager.AdjustPrefabScale(scrollValue);
                e.Use();

            }
            else if (e.alt)
            {
                ghostPrefabManager.AdjustPrefabZPosition(scrollValue, prefabZDelta);
                e.Use();
            }
        }

        void LoadSettings()
        {
            // Get the MonoScript object for this EditorWindow
            MonoScript monoScript = MonoScript.FromScriptableObject(this);
            // Get the path to the script
            string scriptPath = AssetDatabase.GetAssetPath(monoScript);
            // Extract the directory from the path
            string currentDirectory = System.IO.Path.GetDirectoryName(scriptPath);
            string settingsPath = currentDirectory + "\\PrefabPainterSettings.asset";

            settings = AssetDatabase.LoadAssetAtPath<PrefabPainterSettings>(settingsPath);

            if (settings == null)
            {
                settings = CreateInstance<PrefabPainterSettings>();

                // Create the asset in the specified path
                AssetDatabase.CreateAsset(settings, settingsPath);
                AssetDatabase.SaveAssets(); // Save changes to the asset database
            }
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
            if (settings != null && settings.palettes != null)
            {
                for (int i = 0; i < settings.palettes.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    settings.palettes[i] = (PrefabPalette)EditorGUILayout.ObjectField(settings.palettes[i], typeof(PrefabPalette), false);


                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        List<PrefabPalette> tempList = new List<PrefabPalette>(settings.palettes);
                        tempList.RemoveAt(i);
                        settings.palettes = tempList.ToArray();
                        GUIUtility.ExitGUI(); // Prevents the rest of the GUI code from running after modifying the list
                    }

                    EditorUtility.SetDirty(settings); // Mark the PaletteData asset as dirty to ensure it saves

                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Add Palette"))
            {
                if (settings.palettes == null) settings.palettes = new PrefabPalette[0]; // Ensure the palettes array is initialized

                List<PrefabPalette> tempList = new List<PrefabPalette>(settings.palettes);
                tempList.Add(null); // Adds a null entry, to be set in the Inspector
                settings.palettes = tempList.ToArray();
                EditorUtility.SetDirty(settings); // Mark as dirty to save changes
            }
        }


        void DrawPalletteSelector()
        {
            // Palette List Dropdown
            if (settings.palettes != null && settings.palettes.Length > 0)
            {
                List<string> paletteNames = new List<string>();
                int currentPaletteIndex = -1;
                for (int i = 0; i < settings.palettes.Length; i++)
                {
                    string paletteName = settings.palettes[i] != null ? settings.palettes[i].name : "Unnamed Palette";
                    paletteNames.Add(paletteName);
                    if (settings.palettes[i] == activePalette)
                    {
                        currentPaletteIndex = i;
                    }
                }

                int newPaletteIndex = EditorGUILayout.Popup(currentPaletteIndex, paletteNames.ToArray());
                if (newPaletteIndex != currentPaletteIndex)
                {
                    // Update active palette and reset selected prefab
                    activePalette = settings.palettes[newPaletteIndex];
                    selectedPrefabIndex = 0; // Automatically select the first prefab
                    ghostPrefabManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy); // Refresh ghost prefab for the new selection
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No palettes available. Please add palettes to the list.", MessageType.Info);
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
