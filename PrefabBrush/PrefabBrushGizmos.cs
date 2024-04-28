using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtilities.PrefabBrush {
    public class PrefabBrushGizmos : MonoBehaviour
    {
        public Vector3 position = Vector3.zero;

        [ExecuteAlways]
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(position, "", false);
        }
    }
}
