using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MyUtilities.ClassExtensions
{
    public static class Collider2DExtension
    {
        public static bool CompletelyContains(this Collider2D collider, Collider2D other)
        {
            return (collider.bounds.Contains(other.bounds.max) && collider.bounds.Contains(other.bounds.min));
        }
    }
}
