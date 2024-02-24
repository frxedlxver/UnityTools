// Custom data type to hold control information
using System;
using UnityEditor;
using UnityEngine.UIElements;

public class ControlData
{
    public Action<float> VariableSetter { get; set; }

    public string Label
    {
        get
        {
            return ParamField.label;
        }
        set
        {
            ParamField.label = value;
        }
    }

    public float Value
    {
        get
        {
            return ParamField.value;
        }
        set
        {
            ParamField.value = value;
            VariableSetter?.Invoke(value);
        }
    }

    public float Min
    {
        get
        {
            return MinField.value;
        }
        set
        {
            MinField.value = value;
        }
    }
    public float Max
    {
        get
        {
            return MaxField.value;
        }
        set
        {
            MaxField.value = value;
        }
    }

    private readonly FloatField ParamField;
    private readonly FloatField MinField;
    private readonly FloatField MaxField;

    public ControlData(Action<float> variableSetter, string label, FloatField paramField, FloatField minField, FloatField maxField)
    {
        VariableSetter = variableSetter;
        ParamField = paramField;
        MinField = minField;
        MaxField = maxField;
        paramField.label = label;

        paramField.RegisterValueChangedCallback<float>((e) =>
        {
            float value = e.newValue;
            if (value < minField.value) minField.value = value;

            if (value > maxField.value) maxField.value = value;

            VariableSetter.Invoke(value);
        });
    }

    public void SaveCurrentState()
    {
        EditorPrefs.SetFloat(Label + "_Min", Min);
        EditorPrefs.SetFloat(Label + "_Max", Max);
        EditorPrefs.SetFloat(Label + "_Value", Value);
    }

    public void LoadSavedState(float defaultValue)
    {
        Value = EditorPrefs.GetFloat(Label + "_Value", defaultValue);
        Min = EditorPrefs.GetFloat(Label + "_Min", Value);
        Max = EditorPrefs.GetFloat(Label + "_Max", Value);
    }

    public void LoadSavedState()
    {
        LoadSavedState(Value);
    }
}