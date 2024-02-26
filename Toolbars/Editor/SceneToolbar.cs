using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System;
using MyUtilities.PrefabBrush;
using UnityEngine;

namespace MyUtilities.Toolbars
{
    [Overlay(typeof(SceneView), "Panel Overlay Example", true)]
    public class SceneToolbar : ToolbarOverlay
    {
        private Button prefabBrushButton;
        private VisualElement root;
        private Settings settings;
        private KeyCode ShortcutKey = KeyCode.B;
        private EventModifiers modifiers = EventModifiers.None;
        public override VisualElement CreatePanelContent()
        {
            root = new VisualElement() { name = "ToolbarRoot" };    

            AddButton("Prefab Brush", PrefabBrushButtonClicked, out prefabBrushButton);

            SceneView s = SceneView.currentDrawingSceneView;

            SceneView.duringSceneGui += OnSceneGUI;
            return root;
        }

        private void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;

            

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == ShortcutKey & e.modifiers == modifiers)
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
        }
    }

}
