using UnityEngine;

namespace MyUtilities.ClassExtensions
{
    public static class LayerMaskExtension
    {
        public static bool Includes(
            this LayerMask mask,
            int layer)
        {
            return (mask.value & 1 << layer) > 0;
        }
    }
}

