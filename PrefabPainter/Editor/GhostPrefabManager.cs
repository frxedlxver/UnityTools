using JetBrains.Annotations;
using MyUtilities.ClassExtensions;
using System;
using UnityEngine;

namespace MyUtilities.PrefabPainter
{
    public class GhostPrefabManager
    {
        internal GameObject ghostPrefab;
        internal float prefabScale = 1f; // Default scale
        internal float prefabZPos = 0f;
        internal float prefabZRotation = 0f;
        private Transform parent;

        public Vector3 PrefabPosition
        {
            get
            {
                return ghostPrefab.transform.position;
            }
        }

        public Quaternion PrefabRotation
        {
            get
            {
                return ghostPrefab.transform.rotation;
            }
        }

        public Vector3 PrefabScale
        {
            get
            {
                return ghostPrefab.transform.localScale;
            }
        }

        public GameObject CreateGhostPrefab(GameObject prefab, bool hideInHierarchy)
        {
            Vector3 curPos = Vector2.zero;
            if (ghostPrefab != null)
            {
                curPos = PrefabPosition;
                GameObject.DestroyImmediate(ghostPrefab);
            }

            if (prefab != null)
            {
                ghostPrefab = GameObject.Instantiate(prefab);
                ghostPrefab.name = "PrefabPainterPreview";
                if (parent != null) ghostPrefab.transform.SetParent(parent.transform, false);
                ghostPrefab.transform.localScale = Vector3.one * prefabScale;
                ghostPrefab.transform.position = curPos;
                SetGhostPrefabMaterial(Color.white);
                ghostPrefab.hideFlags = hideInHierarchy ? HideFlags.HideAndDontSave : HideFlags.DontSave;
            }

            return ghostPrefab;
        }

        public void DestroyGhost()
        {
            if (ghostPrefab != null) { GameObject.DestroyImmediate(ghostPrefab); }
        }

        public void SetGhostPrefabMaterial(Color color)
        {
            if (ghostPrefab == null) return;
            var renderer = ghostPrefab.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = color;
            }
        }

        public void ShowGhostPrefab(bool show)
        {
            if (ghostPrefab == null) return;

            var renderers = ghostPrefab.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = show;
            }
        }

        public void AdjustPrefabScale(float amount)
        {
            prefabScale += amount * -0.1f; // Adjust the scale factor as needed
            prefabScale = Mathf.Max(prefabScale, 0.1f); // Clamp the scale to prevent it from going too small
            if (ghostPrefab != null)
            {
                ghostPrefab.transform.localScale = Vector3.one * prefabScale;
            }
        }

        public void AdjustPrefabZPosition(float amount, float positionDelta)
        {
            prefabZPos += amount / 3f * -positionDelta;
            if (ghostPrefab != null)
            {
                ghostPrefab.transform.position = new Vector3(ghostPrefab.transform.position.x, ghostPrefab.transform.position.y, prefabZPos);
            }
        }

        public void AdjustPrefabZRotation(float amount, float rotationDelta)
        {
            Debug.Log(amount);
            prefabZRotation += amount * rotationDelta;

            // Ensure the rotation is within 0-360 degrees
            prefabZRotation = (prefabZRotation + 360f) % 360f;

            if (ghostPrefab != null)
            {
                ghostPrefab.transform.localEulerAngles = new Vector3(ghostPrefab.transform.eulerAngles.x, ghostPrefab.transform.eulerAngles.y, prefabZRotation);
            }
        }

        internal void SetGhostPosition(Vector3 mouseWorldPos)
        {
            if (ghostPrefab != null)
            {
                ghostPrefab.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, prefabZPos);
                ghostPrefab.transform.localEulerAngles = new Vector3(ghostPrefab.transform.eulerAngles.x, ghostPrefab.transform.eulerAngles.y, prefabZRotation);
            }
        }

        internal void SetParent(GameObject targetParent)
        {
            if (ghostPrefab != null)
            {
                if (targetParent != null)
                {
                    ghostPrefab.transform.SetParent(targetParent.transform, true);
                    parent = targetParent.transform;
                }
                else ghostPrefab.transform.parent = null;
            }

        }
    }
}
