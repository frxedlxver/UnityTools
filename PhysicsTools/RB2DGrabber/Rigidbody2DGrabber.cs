using MyUtilities.ClassExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyUtilities.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Rigidbody2DGrabber : MonoBehaviour
    {
        private Camera cam; // Reference to the camera

        private FixedJoint2D activeJoint; // The joint used to drag the rigidbody
        private Rigidbody2D rb;

        private PlayerActions playerActions;

        private bool mouseDown;
        private bool mouseUp;
        private float grabbedRBOriginalGravityScale;
        public Color GrabbedHighlightColor = Color.magenta;
        private Color grabbedRBOriginalColor;

        private List<Rigidbody2D> grabbedRBAndChildren;
        private Dictionary<Rigidbody2D, LayerMask> rbExcludedLayerDictionary;
        public bool IgnoreCollisionWhenGrabbing;
        Vector2 MouseWorldPos { get
            {
                if (cam != null)
                {
                    return cam.ScreenToWorldPoint(Mouse.current.position.value);
                } else
                {
                    return Vector2.zero;
                }
            } 
        }

        [Tooltip("The distance from the grabbed object to the mouse at which the object will be released.")]
        public float MaxDistanceFromMouse = 10;

        private void Awake()
        {
            cam = Camera.main;
            activeJoint = (FixedJoint2D) GetComponent<Joint2D>();
            rb = GetComponent<Rigidbody2D>();
            activeJoint.autoConfigureConnectedAnchor = false;
            activeJoint.enabled = false;
            rb.simulated = false;
        }

        private void OnEnable()
        {
            playerActions ??= new();
            playerActions.debug.Enable();
            playerActions.debug.Click.started += (InputAction.CallbackContext ctx) => { mouseDown = MouseInsideGameView(); };
            playerActions.debug.Click.canceled += (InputAction.CallbackContext ctx) => { mouseUp = MouseInsideGameView(); };
        }

        private void OnDisable()
        {
            playerActions.debug.Disable();
        }
        void Update()
        {
            if (MouseInsideGameView() && activeJoint.connectedBody == null)
            {
                this.transform.position = MouseWorldPos;
            }
            if (rb != null)
            {
                MoveGrabberToMouse();
            }

            if (activeJoint.connectedBody == null && mouseDown && MouseInsideGameView()) // Check for the initial click
            {
                Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.value);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, rb.includeLayers);

                if (hit.collider != null && hit.rigidbody != null && hit.rigidbody.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.position = Mouse.current.position.value;
                    rb.simulated = true;
                    GrabRigidbody2D(hit.rigidbody);
                }
            }

            if (activeJoint.connectedBody != null && (mouseUp || !MouseInsideGameView())) // Release the object
            {
                ReleaseGrabbedRigidbody2D();
                rb.simulated = false;
            }


            mouseDown = mouseUp = false;
        }

        private bool MouseInsideGameView()
        {
            return cam.pixelRect.Contains(Mouse.current.position.value);
        }

        
        private void ReleaseGrabbedRigidbody2D()
        {
            Rigidbody2D toRelease = activeJoint.connectedBody;
            toRelease.gravityScale = grabbedRBOriginalGravityScale;

            if (toRelease.TryGetComponent(out SpriteRenderer sprite))
            {
                sprite.color = grabbedRBOriginalColor;
            }

            if (IgnoreCollisionWhenGrabbing)
            {
                foreach (Rigidbody2D rb2d in grabbedRBAndChildren)
                {

                    rb2d.excludeLayers = rbExcludedLayerDictionary[rb2d];
                }
            }
            activeJoint.connectedBody = null;
            activeJoint.enabled = false;

            rbExcludedLayerDictionary = null;
            grabbedRBAndChildren = null;
        }

        private void GrabRigidbody2D(Rigidbody2D toGrab)
        {
            MoveGrabberToMouse();
            grabbedRBAndChildren = toGrab.GetComponentsInChildren<Rigidbody2D>().ToList();
            rbExcludedLayerDictionary = new();

            foreach(Rigidbody2D rb2d in grabbedRBAndChildren)
            {
                if (IgnoreCollisionWhenGrabbing)
                {
                    rbExcludedLayerDictionary[rb2d] = rb2d.excludeLayers;
                    rb2d.excludeLayers = Physics2D.AllLayers;
                }
            }
            if (toGrab.TryGetComponent(out SpriteRenderer sprite))
            {
                grabbedRBOriginalColor = sprite.color;
                sprite.color = GrabbedHighlightColor;
            }

            grabbedRBOriginalGravityScale = toGrab.gravityScale;
            toGrab.gravityScale = 0;
            activeJoint.enabled = true;
            Vector2 rbOriginalPos = toGrab.position;
            toGrab.MovePosition(rb.position);

            var translation = rbOriginalPos.To(toGrab.position);
            activeJoint.connectedBody = toGrab;
            foreach (var rb in grabbedRBAndChildren)
            {
                if (rb != toGrab)
                {
                    rb.MovePosition(rb.position + translation);
                }
                rb.velocity = Vector2.zero;
            }
        }

        void MoveGrabberToMouse()
        {
            Vector2 mousePos = MouseWorldPos;
            rb.MovePosition(mousePos);
        }

        void OnJointBreak2D(Joint2D joint)
        {
            if (joint.connectedBody != null && joint.connectedBody.position.To(MouseWorldPos).magnitude > MaxDistanceFromMouse)
            {
                mouseUp = true;
            }
        }
    }
}

// todo:
/*using MyUtilities.ClassExtensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyUtilities.Rigidbody2DGrabber
{
    [RequireComponent(typeof(RelativeJoint2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Rigidbody2DGrabber : MonoBehaviour
    {
        private Camera cam; // Reference to the camera

        private RelativeJoint2D activeJoint; // The joint used to drag the rigidbody
        private Rigidbody2D rb;
        private CircleCollider2D circleCollider;

        private PlayerActions playerActions;

        private Rigidbody2D targetRB2D;
        private List<Rigidbody2D> overlappedRBs;
        private bool clickedSinceUpdate;
        private bool mouseUp;
        private float grabbedRBGravityScale;
        private float selectedRBSpriteColor;

        Vector2 MouseWorldPos
        {
            get
            {
                if (cam != null)
                {
                    return cam.ScreenToWorldPoint(Mouse.current.position.value);
                }
                else
                {
                    return Vector2.zero;
                }
            }
        }

        private void Awake()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            cam = Camera.main;
            activeJoint = GetComponent<RelativeJoint2D>();
            rb = GetComponent<Rigidbody2D>();
            activeJoint.autoConfigureOffset = false;
            activeJoint.enabled = false;
            rb.simulated = false;
        }

        private void OnEnable()
        {
            playerActions ??= new();
            playerActions.debug.Enable();
            playerActions.debug.Click.started += (InputAction.CallbackContext ctx) => { clickedSinceUpdate = true; };
            playerActions.debug.Click.canceled += (InputAction.CallbackContext ctx) => { mouseUp = true; };
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            clickedSinceUpdate = MouseInsideGameView();
        }

        private void OnDisable()
        {
            playerActions.debug.Disable();
        }
        void Update()
        {
            if (MouseInsideGameView())
            {
                UpdateSelectedRB();
            }
            if (activeJoint.connectedBody == null && clickedSinceUpdate && MouseInsideGameView()) // Check for the initial click
            {

                if (hit.collider != null && hit.rigidbody != null && hit.rigidbody.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.position = Mouse.current.position.value;
                    rb.simulated = true;
                    GrabRigidbody2D(hit.rigidbody);
                }
            }

            if (activeJoint.connectedBody != null && (mouseUp || !MouseInsideGameView())) // Release the object
            {
                ReleaseGrabbedRigidbody2D();
                rb.simulated = false;
            }

            else if (activeJoint.connectedBody != null)
            {
                MoveJointToMouse();
            }

            clickedSinceUpdate = mouseUp = false;
        }

        private void UpdateSelectedRB()
        {
            if (overlappedRBs.Count == 0 && targetRB2D != null)
            {
                DeselectRB2d(targetRB2D);
            }
            else
            {
                float currentClosest = -1;
                Rigidbody2D newTargetRB2D = targetRB2D;
                foreach (var other in overlappedRBs)
                {
                    float curDistance = other.position.To(this.rb.position).magnitude;
                    if (curDistance < currentClosest || currentClosest == -1)
                    {
                        currentClosest = curDistance;
                        newTargetRB2D = other;
                    }
                }
                if (newTargetRB2D != targetRB2D)
                {
                    SelectRB2D(newTargetRB2D);
                }
            }
        }

        private void SelectRB2D(Rigidbody2D newTargetRB2D)
        {
            if (targetRB2D != null)
            {
                DeselectRB2d(targetRB2D);
            }

            targetRB2D = newTargetRB2D;

            tar
        }

        private void DeselectRB2d(Rigidbody2D targetRB2D)
        {
            throw new NotImplementedException();
        }

        private bool MouseInsideGameView()
        {
            return cam.pixelRect.Contains(Mouse.current.position.value);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Rigidbody2D collisionRB) && !collisionRB.isKinematic)
            {
                overlappedRBs.Add(collisionRB);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Rigidbody2D collisionRB))
            {
                overlappedRBs.Remove(collisionRB);
            }
        }


        private void ReleaseGrabbedRigidbody2D()
        {
            activeJoint.connectedBody.gravityScale = grabbedRBGravityScale;
            activeJoint.connectedBody = null;
            activeJoint.enabled = false;

        }

        private void GrabRigidbody2D(Rigidbody2D toGrab)
        {
            grabbedRBGravityScale = toGrab.gravityScale;
            toGrab.gravityScale = 0;
            toGrab.MovePosition(MouseWorldPos);
            activeJoint.enabled = true;
            activeJoint.connectedBody = toGrab;
        }

        void MoveJointToMouse()
        {
            Vector2 mousePos = MouseWorldPos;
            rb.MovePosition(mousePos);
        }
    }

}*/