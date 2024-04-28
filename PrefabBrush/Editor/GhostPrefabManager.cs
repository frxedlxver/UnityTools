using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyUtilities.PrefabBrush
{
    public class GhostPrefabManager
    {
        internal GameObject ghostPrefab;
        internal bool active { get {  return ghostPrefab ? ghostPrefab.activeSelf : false; } }
        internal Vector2 ghost2DPosition = Vector2.zero;
        internal float prefabScale = 1f; // Default scale
        internal int prefabOrderInLayer = 0;
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

        public GhostPrefabManager(Settings settings)
        {
            settings.OnPrefabSelected.AddListener(SetPrefab);
            settings.OnParentSelected.AddListener(SetParent);
        }

        public void SetPrefab(GameObject prefab)
        {
            DestroyGhost();
            CreateGhostPrefab(prefab, true);
        }

        void OnDisable()
        {
            DestroyGhost();
        }

        public GameObject CreateGhostPrefab(GameObject prefab, bool hideInHierarchy)
        {
            if (prefab != null)
            {
                ghostPrefab = GameObject.Instantiate(prefab);
                ghostPrefab.name = "PrefabPainterPreview";
                if (parent != null) ghostPrefab.transform.SetParent(parent.transform, true);
                ghostPrefab.transform.localScale = Vector3.one * prefabScale;
                SetGhostPosition(ghost2DPosition);
                SetGhostPrefabMaterial(new (255, 255, 255, 0.7f));
                ghostPrefab.hideFlags = HideFlags.HideAndDontSave;
            }

            SetVisibility(true);

            return ghostPrefab;
        }

        internal void HandleMouseMovement(SceneView sceneView, Event e)
        {
            // Determine if the mouse is within the Scene view
            Vector3 mousePosition = Event.current.mousePosition;
            bool isMouseInSceneView = sceneView.cameraViewport.Contains(e.mousePosition);


            if (isMouseInSceneView)
            {
                mousePosition = HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(0);

                if (!active) SetVisibility(true);


                SetGhostPosition(mousePosition);
            }
            else
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

        public void AdjustPrefabOrderInLayer(int Pos)
        {
            prefabOrderInLayer += Pos;
            if (ghostPrefab != null)
            {
                ghostPrefab.GetComponent<SpriteRenderer>().sortingOrder = prefabOrderInLayer;
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
                ghostPrefab.transform.localEulerAngles = new Vector3(ghostPrefab.transform.eulerAngles.x, ghostPrefab.transform.eulerAngles.y, 0);
            }
        }

        internal void SetGhostPosition(Vector3 mouseWorldPos)
        {
            if (ghostPrefab != null)
            {
                ghostPrefab.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, prefabOrderInLayer);
                ghostPrefab.transform.localEulerAngles = new Vector3(ghostPrefab.transform.eulerAngles.x, ghostPrefab.transform.eulerAngles.y, prefabZRotation);
            }
        }

        internal void SetParent(Transform targetParent)
        {
            if (ghostPrefab != null)
            {
                if (targetParent != null)
                {
                    ghostPrefab.transform.SetParent(targetParent, true);
                    parent = targetParent;
                }
                else ghostPrefab.transform.parent = null;
            }

        }

        
    }
}
