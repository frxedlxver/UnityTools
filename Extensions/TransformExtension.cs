using UnityEngine;

namespace MyUtilities.ClassExtensions
{
    public static class TransformExtension
    {
        public static void CopyValuesFrom(this Transform transform, Transform other)
        {
            transform.position = other.position;
            transform.rotation = other.rotation;
            transform.localScale = other.localScale;
        }
    }
}
