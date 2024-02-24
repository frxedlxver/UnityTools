using Duopus.Controller;
using System;

namespace MyUtilities.SerializableDictionary
{
    [Serializable]
    public class SerializableModeDictionary : UnitySerializedDictionary<ModeManager.Mode, bool> { }
}
