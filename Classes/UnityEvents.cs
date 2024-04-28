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

    public class MultiGameObjEvt : UnityEvent<GameObject[]> { }

    /// <summary>
    /// Used to pass collision notifications to other GameObjects.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <typeparamref name="Collision2D"/> should be the <typeparamref name="Collision2D"/> from the original collision callback.
    /// </item>
    /// <item>
    /// <typeparamref name="GameObject"/> should be the original sender.
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="Trigger2DEvent"/>
    [System.Serializable] public class Collision2DEvent : UnityEvent<Collision2D, GameObject> { }

    /// <summary>
    /// Used to pass trigger collision notifications to other GameObjects.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <typeparamref name="Collider2D"/> should be the <typeparamref name="Collider2D"/> from the original callback.
    /// </item>
    /// <item>
    /// <typeparamref name="GameObject"/> should be the original sender.
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="Trigger2DEvent"/>
    [System.Serializable] public class Trigger2DEvent : UnityEvent<Collider2D, GameObject> { }
    public class TransformEvent : UnityEvent<Transform> { }
}
