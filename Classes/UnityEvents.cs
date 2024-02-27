using Duopus.Controller;
using UnityEngine;
using UnityEngine.Events;

namespace MyUtilities.CustomEvents
{
    public class Vector2Event : UnityEvent<Vector2> { }
    public class FloatEvent : UnityEvent<float> { }
    public class IntEvent : UnityEvent<int> { }
    public class ModeEvent : UnityEvent<ModeManager.Mode> { }

    public class GameObjectEvent : UnityEvent<GameObject> { }

    public class TransformEvent : UnityEvent<Transform> { }
}
