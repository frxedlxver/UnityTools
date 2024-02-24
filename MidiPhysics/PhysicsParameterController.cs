using Duopus.Controller;
using UnityEngine;


namespace MyUtilities.MidiPhysics
{
    [System.Serializable]
    public class PhysicsParameterController : MonoBehaviour
    {
        private ClimbModeController duopus;

        [MidiControl("K1")]
        public float Gravity
        {
            get
            {
                return Physics2D.gravity.y;
            }
            set
            {
                Physics2D.gravity = new(0, value);
            }
        }

        [MidiControl("K3")]
        public float TentacleForce
        {
            get
            {
                if (duopus == null)
                {
                    return 0;
                }
                else
                {
                    return duopus.tentacleForce;
                }
            }

            set
            {
                if (duopus != null)
                {
                    duopus.tentacleForce = value;
                }
            }
        }

        [MidiControl("K4")]
        public float GravityScale
        {
            get
            {
                if (duopus == null)
                {
                    return 0;
                }
                else
                {
                    return duopus.gravityScale;
                }
            }
            set
            {
                duopus.gravityScale = value;
            }
        }

        [MidiControl("K5")]
        public float headMass
        {
            get
            {
                return duopus.HeadMass;
            }
            set
            {
                duopus.HeadMass = value;
            }
        }

        [MidiControl("K6")]
        public float TentacleMass
        {
            get
            {
                return duopus.tentacleMass;
            }
            set
            {
                duopus.tentacleMass = value;
            }
        }

        public void Start()
        {
            duopus = GameObject.FindGameObjectWithTag("Duopus").GetComponent<ClimbModeController>();
        }
    }

}
