using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System;
using MyUtilities.PrefabBrush;
using UnityEngine;
using System.Collections.Generic;

namespace MyUtilities.Toolbars
{
    [Overlay(typeof(SceneView), "", true)]
    public class SceneToolbar : ToolbarOverlay
    {
        private Button prefabBrushButton;
        private Button removeHiddenPrefabsButton;
        private VisualElement root;
        private static readonly KeyCode SHORTCUT_KEY = KeyCode.B;
        private static readonly EventModifiers MODIFIERS = EventModifiers.None;

        private static readonly string PB_BUTTON_TEXT = $"PrefabBrush ({SHORTCUT_KEY})";
        public override VisualElement CreatePanelContent()
        {
            root = new VisualElement() { name = "ToolbarRoot" };

            AddButton("", PrefabBrushButtonClicked, out prefabBrushButton);
            UpdatePrefabBrushButtonText();

            AddButton("Destroy Hidden Prefabs", RemoveHiddenPrefabs, out removeHiddenPrefabsButton);

            SceneView s = SceneView.currentDrawingSceneView;

            SceneView.duringSceneGui += OnSceneGUI;
            return root;
        }

        private void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;

            

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == SHORTCUT_KEY & e.modifiers == MODIFIERS)
                {
                    PrefabBrushButtonClicked();
                }
            }
        }

        private void AddButton(string text, Action callback, out Button button)
        {
            button = new()
            {
                text = text,
            };

            button.clicked += callback;

            root.Add(button);
        }

        private void PrefabBrushButtonClicked()
        {
            PrefabBrushWindow.ToggleActive();

            UpdatePrefabBrushButtonText();
        }

        private void RemoveHiddenPrefabs()
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> toDestroy = new List<GameObject>();


            foreach (GameObject obj in allObjects)
            {
                if (obj.hideFlags != HideFlags.None && obj.name.Contains("PrefabPainterPreview"))
                {
                    toDestroy.Add(obj);
                }
            }

            while (toDestroy.Count > 0)
            {
                GameObject.DestroyImmediate(toDestroy[0]);
                toDestroy.RemoveAt(0);
            }
        }

        private void DestroyImmediate(GameObject obj)
        {
            throw new NotImplementedException();
        }

        private void UpdatePrefabBrushButtonText()
        {
            if (!PrefabBrushWindow.WindowOpen)
            {
                prefabBrushButton.text = $"Open {PB_BUTTON_TEXT}";
            }
            else if (PrefabBrushWindow.WindowActive)
            {
                prefabBrushButton.text = $"Enable {PB_BUTTON_TEXT}";
            }
            else
            {
                prefabBrushButton.text = $"Disable {PB_BUTTON_TEXT}";
            }
        }

    }

}
