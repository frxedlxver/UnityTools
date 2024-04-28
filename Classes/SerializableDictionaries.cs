using Duopus.Controller;
using Duopus.Level.Mechanisms;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyUtilities.SerializableDictionary
{
    [Serializable]
    public class SerialModeDict : UnitySerializedDictionary<ModeManager.Mode, bool> { }
    public class SerAxisObjectDict : UnitySerializedDictionary<AxisControlledObject, AxisControlledObject.Axis> { }

    public class SerObjectEventDict : UnitySerializedDictionary<GameObject, UnityEvent> { }
    public class SerLayerEventDict : UnitySerializedDictionary<LayerMask, UnityEvent> { }
}
