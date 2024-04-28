using MyUtilities.Physics;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace MyUtilities.Physics.MidiParameters
{
    public class PhysicsWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset visualTreeAsset = default;
        private PlayerActions actions;
        private PhysicsParameterController physicsParameters;
        private Dictionary<string, ControlData> controlDataDictionary = new();

        [MenuItem("Window/PhysicsWindow")]
        public static void ShowWindow()
        {
            PhysicsWindow wnd = GetWindow<PhysicsWindow>();
            wnd.titleContent = new GUIContent("PhysicsWindow");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlaymodeStateChange;
            InitializeMidiActions();

        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChange;
            EditorApplication.update -= OnEditorUpdate;
            actions.midi.Disable();
        }

        public void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);
        }

        private void InitializeMidiActions()
        {
            actions ??= new PlayerActions();

            actions.midi.Enable();

            foreach (var action in actions.midi.Get())
            {
                action.performed += OnMidiValueChanged;
            }
        }

        private void InitializeControlData()
        {
            Type parameterType = typeof(PhysicsParameterController);
            PropertyInfo[] properties = parameterType.GetProperties();

            foreach (var property in properties)
            {
                MidiControlAttribute midiControlAttribute = property.GetCustomAttribute<MidiControlAttribute>();
                if (midiControlAttribute != null)
                {
                    string controlName = midiControlAttribute.ControlName;

                    if (controlDataDictionary.ContainsKey(controlName)) continue;

                    VisualElement control = rootVisualElement.Q(name: controlName);

                    if (control != null)
                    {
                        FloatField paramField = control.Q<FloatField>(className: "VarField");
                        FloatField minField = control.Q<FloatField>(className: "MinField");
                        FloatField maxField = control.Q<FloatField>(className: "MaxField");

                        void updateAction(float value) => property.SetValue(physicsParameters, value);

                        string labelText = property.Name;
                        ControlData controlData = new(updateAction, labelText, paramField, minField, maxField);


                        control.Add(new Button(controlData.SaveCurrentState)
                        {
                            text = "Save"
                        });

                        control.Add(new Button(controlData.LoadSavedState)
                        {
                            text = "Reset"
                        });


                        // whatever the value is currently set to will be used as a default value
                        float defaultValue = (float)property.GetValue(physicsParameters);
                        controlData.LoadSavedState(defaultValue);

                        controlDataDictionary.Add(controlName, controlData);
                    }
                }
            }
        }
        private void OnEditorUpdate()
        {
            if (Application.isPlaying && physicsParameters != null && controlDataDictionary.Keys.Count == 0)
            {
                InitializeControlData();
                Repaint(); // Attempt to repaint after initialization
                EditorApplication.update -= OnEditorUpdate;
            }
        }

        private void OnPlaymodeStateChange(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                physicsParameters ??= GameObject.FindGameObjectWithTag("PhysicsParameters").GetComponent<PhysicsParameterController>();
                EditorApplication.update += OnEditorUpdate;
            }
            else
            {
                controlDataDictionary = new();
                physicsParameters = null;
            }
        }

        private void OnMidiValueChanged(InputAction.CallbackContext ctx)
        {

            if (controlDataDictionary.TryGetValue(ctx.action.name, out ControlData controlData))
            {
                controlData.Value = Mathf.Lerp(controlData.Min, controlData.Max, ctx.action.ReadValue<float>());
            }
        }
    }
}


