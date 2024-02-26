using UnityEngine;

namespace MyUtilities.ClassExtensions
{
    public static class Vector2Extension
    {
        public static Vector2 To(this Vector2 start, Vector2 end)
        {
            return end - start;
        }
    }
}
