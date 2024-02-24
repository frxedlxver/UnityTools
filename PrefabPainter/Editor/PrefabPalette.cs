using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUtilities.PrefabPainter
{
    [CreateAssetMenu(fileName="New PrefabPalette", menuName="MyUtilities/Prefab Pallette")]
    public class PrefabPalette : ScriptableObject
    {
        public GameObject[] prefabs;
    }
}
