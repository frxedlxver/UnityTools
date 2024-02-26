using JetBrains.Annotations;
using MyUtilities.ClassExtensions;
using System;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace MyUtilities.PrefabBrush
{
    public class GhostPrefabManager
    {
        internal GameObject ghostPrefab;
        internal bool active { get {  return ghostPrefab ? ghostPrefab.activeSelf : false; } }
        internal Vector2 ghost2DPosition = Vector2.zero;
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
            if (prefab != null)
            {
                ghostPrefab = GameObject.Instantiate(prefab);
                ghostPrefab.name = "PrefabPainterPreview";
                if (parent != null) ghostPrefab.transform.SetParent(parent.transform, false);
                ghostPrefab.transform.localScale = Vector3.one * prefabScale;
                SetGhostPosition(ghost2DPosition);
                SetGhostPrefabMaterial(Color.white);
                ghostPrefab.hideFlags = hideInHierarchy ? HideFlags.HideAndDontSave : HideFlags.DontSave;
            }

            SetVisibility(true);

            return ghostPrefab;
        }

        internal void HandleMouseMovement(SceneView sceneView, Event e)
        {
            // Determine if the mouse is within the Scene view
            bool isMouseInSceneView = sceneView.cameraViewport.Contains(e.mousePosition);

            if (isMouseInSceneView)
            {
                if (!active) SetVisibility(true);
                if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                {

                    Vector3 mouseWindowPos = new(e.mousePosition.x, sceneView.camera.pixelHeight - e.mousePosition.y);
                    ghost2DPosition = sceneView.camera.ScreenToWorldPoint(mouseWindowPos);

                    SetGhostPosition(ghost2DPosition);
                }
            }
            else if (active)
            {
                SetVisibility(false);
            }
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

        public void SetVisibility(bool show)
        {
            if (ghostPrefab == null) return;

            ghostPrefab.SetActive(show);

            Debug.Log($"Showing: {show}");

            SceneView.RepaintAll();
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
