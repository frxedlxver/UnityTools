using MyUtilities.ClassExtensions;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MyUtilities.PrefabBrush
{
    public class PrefabBrushWindow : EditorWindow
    {
        public static bool WindowOpen;
        public static bool WindowActive;
        private PrefabPalette activePalette;
        [SerializeField] private Settings settings;
        private PrefabBrushGizmos pbg;
        internal Settings Settings
        {
            get
            {
                if (settings == null)
                {
                    LoadSettings();
                }

                return settings;
            }
        }
        internal GhostPrefabManager ghostManager;
        private float rotationDelta = 1f;
        private bool hidePrefabInHierarchy = true;

        private const KeyCode NextPrefabKey = KeyCode.RightArrow;
        private const KeyCode LastPrefabKey = KeyCode.LeftArrow;

        public GameObject TargetParent;
        public GameObject SelectedPrefab
        {
            get
            {
                if (activePalette == null || activePalette.prefabs == null || activePalette.prefabs.Length == 0 || SelectedPrefabIndex == -1)
                    return null;
                else return activePalette.prefabs.ElementAt(settings.CurrentPrefabIndex % (activePalette.prefabs.Length - 1));
            }
        }

        public int SelectedPrefabIndex
        {
            get { return settings.CurrentPrefabIndex; }
            set
            {
                if (activePalette == null || activePalette.prefabs == null) return;
                value = (value + activePalette.prefabs.Length) % activePalette.prefabs.Length;

                if (value != settings.CurrentPrefabIndex)
                    settings.CurrentPrefabIndex = value;
            }
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
            SetActive(!WindowActive);
        }

        public static void SetActive(bool active)
        {
            if (!WindowOpen)
            {
                ShowWindow();
            }

            WindowActive = active;

            if (WindowActive)
            {
                GetWindow().OnBecameActive();
            } else
            {
                GetWindow().OnBecameInactive();
            }

            SceneView.lastActiveSceneView.Focus();
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
            if (ghostManager == null) ghostManager = new(settings);
            // remove, in case it has already been added
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            if (SelectedPrefab != null)
            {
                ghostManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy);
                ghostManager.prefabScale = 1f;
                ghostManager.prefabOrderInLayer = SelectedPrefab.GetComponent<SpriteRenderer>().sortingOrder;
                ghostManager.prefabZRotation = 0f;
            }
        }

        private void OnBecameInactive()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            ghostManager.DestroyGhost();
        }
        void OnEnable()
        {
            WindowOpen = true;
            WindowActive = true;
            OnBecameActive();
            if (settings.Palettes != null && settings.Palettes.Count > 0)
            {
                activePalette = settings.Palettes[settings.CurrentPaletteIndex];
            }
            pbg = GameObject.FindAnyObjectByType<PrefabBrushGizmos>();
        }

        void OnDisable()
        {
            WindowOpen = false;
            WindowActive = false;
            OnBecameInactive();
            EditorUtility.SetDirty(this);
        }

        void OnGUI()
        {

            GUILayout.Label("Prefab Palettes", EditorStyles.boldLabel);

            DrawPaletteList();

            EditorGUILayout.Space(20);

            DrawPalletteSelector();

            EditorGUILayout.Space(20);

            GUILayout.Label("Target Parent", EditorStyles.boldLabel);
            DrawParentSelector();
            
            if (activePalette != null)
            {
                EditorGUILayout.Space(20);

                GUILayout.Label("Prefabs", EditorStyles.boldLabel);

                DrawPrefabSelector();
            }

        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (pbg == null)
            {
                pbg = FindObjectOfType<PrefabBrushGizmos>();
            }

            if (pbg != null)
            {
                pbg.position = ghostManager.PrefabPosition;
            }
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
                for (int i = 0; i < settings.Palettes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    settings.Palettes[i] = (PrefabPalette)EditorGUILayout.ObjectField(settings.Palettes[i], typeof(PrefabPalette), false);


                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        settings.Palettes.RemoveAt(i);
                        activePalette = null;
                        while (activePalette == null && settings.CurrentPaletteIndex > 0)
                        {
                            activePalette = settings.CurrentPalette;
                            settings.CurrentPaletteIndex -= 1;
                        }
                        LoadSettings();
                        GUIUtility.ExitGUI(); // Prevents the rest of the GUI code from running after modifying the list
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Add Palette"))
            {
                settings.AddPalette(null);
                EditorUtility.SetDirty(settings); // Mark as dirty to save changes
                LoadSettings();
            }
        }


        void DrawPalletteSelector()
        {
            // Palette List Dropdown
            if (settings.Palettes != null)
            {
                List<string> paletteNames = settings.GetPaletteNames();

                // temp var to check if palette index has changed
                int newPaletteIndex = EditorGUILayout.Popup("Select Palette", settings.CurrentPaletteIndex, paletteNames.ToArray());
                if (newPaletteIndex != settings.CurrentPaletteIndex)
                {
                    settings.CurrentPaletteIndex = newPaletteIndex;
                    // Update active palette and reset selected prefab
                    activePalette = settings.Palettes[newPaletteIndex];
                    ghostManager.CreateGhostPrefab(SelectedPrefab, hidePrefabInHierarchy); // Refresh ghost prefab for the new selection
                } else if (activePalette == null)
                {
                    activePalette = settings.CurrentPalette;
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

        private void DrawParentSelector()
        {
            GUILayout.BeginHorizontal();
            settings.CurrentParent = (Transform)EditorGUILayout.ObjectField(settings.CurrentParent, typeof(Transform), true);
            if (GUILayout.Button("Remove"))
            {
                settings.CurrentParent = null;
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(20);


            if (settings.recentParents != null && settings.recentParents.Count > 0)
            {
                // Assuming settings.recentParents is a List<Transform>
                // Generate an array of names for the popup
                string[] options = settings.GetRecentParentNames().ToArray();

                // Display the popup and get the new selected index
                int newIndex = EditorGUILayout.Popup("Select from recent parents", 0, options);

                // Update the current parent based on selection
                if (newIndex >= 0 && newIndex < settings.recentParents.Count)
                {
                    settings.CurrentParent = settings.recentParents[newIndex];
                }
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
                    ghostManager.prefabOrderInLayer = EditorGUILayout.IntField("Order In Layer", ghostManager.prefabOrderInLayer);

                    GUILayout.Space(20);
                    GUILayout.Label("Z Rotation", EditorStyles.boldLabel);
                    ghostManager.prefabZRotation = EditorGUILayout.FloatField("Z Rotation", ghostManager.prefabZRotation);
                    rotationDelta = EditorGUILayout.FloatField("Z Rotation change on scroll", rotationDelta);
                }, "Prefab Brush");
                Handles.EndGUI();
            }
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

        private void HandleLMB(Event e)
        {
            if (SelectedPrefabIndex >= activePalette.prefabs.Length || ghostManager.ghostPrefab == null)
            {
                Debug.LogWarning("Prefab Brush Tool: No prefab selected or index out of range.");
                return;
            }

            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(activePalette.prefabs[SelectedPrefabIndex]);
            if (TargetParent != null)
            {
                instantiatedPrefab.transform.parent = TargetParent.transform;
            }
            instantiatedPrefab.transform.CopyValuesFrom(ghostManager.ghostPrefab.transform); // copy transform

            instantiatedPrefab.GetComponent<SpriteRenderer>().sortingOrder = ghostManager.prefabOrderInLayer;
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
                ghostManager.AdjustPrefabOrderInLayer((int)Mathf.Sign(scrollValue));
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

            this.activePalette = settings.CurrentPalette;

            this.SelectedPrefabIndex = settings.CurrentPrefabIndex;

            EditorUtility.SetDirty(this);
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
