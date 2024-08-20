using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Restriction")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-restriction-component/")]
    public class RayfireRestriction : MonoBehaviour
    {
        public enum RFBoundActionType
        {
            Fade  = 2,
            Reset = 4,

            //Demolish   = 6,
            PostDemolitionAction = 9
        }

        public enum RFDistanceType
        {
            InitializePosition = 0,
            TargetPosition     = 2
        }

        public enum RFBoundTriggerType
        {
            Inside  = 0,
            Outside = 2
        }
        
        // UI
        public bool               enable;
        public RFBoundActionType  breakAction;
        public float              actionDelay;
        public float              checkInterval;
        public float              distance;
        public RFDistanceType     position;
        public Transform          target; 
        public Collider           Collider;
        public RFBoundTriggerType region;
        public RayfireRigid       rigid;

        // Non serialized
        [NonSerialized] public bool broke;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RayfireRestriction()
        {
            enable        = false;
            checkInterval = 5;
            breakAction   = RFBoundActionType.PostDemolitionAction;
            distance      = 30f;
            position      = RFDistanceType.InitializePosition;
            target        = null;
            Collider      = null;
            region        = RFBoundTriggerType.Inside;

            Reset();
        }

        // Copy from
        public void CopyFrom (RayfireRestriction rest)
        {
            enable        = rest.enable;
            checkInterval = rest.checkInterval;
            breakAction   = rest.breakAction;
            distance      = rest.distance;
            position      = rest.position;
            target        = rest.target;
            Collider      = rest.Collider;
            region        = rest.region;

            Reset();
        }

        // Turn of all activation properties
        public void Reset()
        {
            broke = false;
        }

        void Start()
        {
            // Set rigid
            if (rigid == null)
                rigid = GetComponent<RayfireRigid>();
            
            // Set self in Rigid
            if (rigid != null)
                rigid.rest = this;
            
            // Init restriction check
            InitRestriction (rigid);
        }
        
        // Disable
        void OnDisable()
        {

        }

        // Activation
        void OnEnable()
        {
            /*
            // Start cors // TODO add support for fragment caching and the rest cors:skinned
            if (gameObject.activeSelf == true && initialized == true && corState == false)
            {
                // Init restriction check
                InitRestriction (rigid);
            }
            */
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Init restriction check
        public void InitRestriction (RayfireRigid scr)
        {
            // Skip in Editor
            if (Application.isPlaying == false)
                return; 
            
            // Has no Rigid
            if (scr == null)
                return; 
            
            // Rigid has no restriction
            if (scr.rest == null)
                return; 
            
            // No action required
            if (enable == false)
                return;
            
            // Already broke
            if (broke == true)
                return;
            
            // Init distance check
            if (distance > 0)
            {
                // Init position distance
                if (position == RFDistanceType.InitializePosition)
                    StartCoroutine (RestrictionDistanceCor (scr));

                // Init target position
                else
                {
                    if (target != null)
                        StartCoroutine (RestrictionDistanceCor (scr));
                    else
                        Debug.Log ("Target is not defined", scr.gameObject);
                }
            }

            // Init trigger check
            if (Collider != null)
            {
                // Check if trigger
                if (Collider.isTrigger == false)
                    Debug.Log ("Collider is not trigger", scr.gameObject);

                // Init
                StartCoroutine (RestrictionTriggerCor (scr));
            }
        }

        // Init broke restriction
        static void BrokeRestriction (RayfireRigid scr)
        {
            // Set state
            scr.rest.broke = true;

            // Event
            RFRestrictionEvent.RestrictionEvent (scr);

            // Destroy/Deactivate
            if (scr.rest.breakAction == RFBoundActionType.PostDemolitionAction)
                RayfireMan.DestroyFragment (scr, scr.rootParent);

            // Fade
            else if (scr.rest.breakAction == RFBoundActionType.Fade)
                RFFade.FadeRigid (scr);

            // Reset
            else if (scr.rest.breakAction == RFBoundActionType.Reset)
                RFReset.ResetRigid (scr);
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////

        // Start distance check cor
        static IEnumerator RestrictionDistanceCor (RayfireRigid scr)
        {
            // Wait random time
            yield return new WaitForSeconds (Random.Range (0f, 0.1f));

            // Delays
            WaitForSeconds intervalDelay = new WaitForSeconds (scr.rest.checkInterval);
            WaitForSeconds actionDelay   = new WaitForSeconds (scr.rest.actionDelay);

            // Check position
            Vector3 checkPosition = scr.physics.initPosition;

            // Repeat
            while (scr.rest.broke == false)
            {
                // Wait frequency second and check
                yield return intervalDelay;

                // Target position
                if (scr.rest.position == RFDistanceType.TargetPosition)
                    if (scr.rest.target != null)
                        checkPosition = scr.rest.target.position;

                // Get distance
                float dist = Vector3.Distance (checkPosition, scr.transForm.position);
                
                // Check distance
                if (dist > scr.rest.distance)
                {
                    // Delay
                    if (scr.rest.actionDelay > 0)
                        yield return actionDelay;
                    
                    BrokeRestriction (scr);
                }
            }
        }

        // Start Trigger check
        IEnumerator RestrictionTriggerCor (RayfireRigid scr)
        {
            // Wait random time
            yield return new WaitForSeconds (Random.Range (0f, 0.2f));

            // Delays
            WaitForSeconds intervalDelay = new WaitForSeconds (scr.rest.checkInterval);
            WaitForSeconds delay   = new WaitForSeconds (scr.rest.actionDelay);

            // Vars
            float   dist;
            Vector3 direction;
            bool    brokeState = false;

            // Repeat
            while (scr.rest.broke == false)
            {
                // Wait frequency second and check
                yield return intervalDelay;

                // No trigger
                if (scr.rest.Collider == null)
                    yield break;

                // Check penetration
                bool col = Physics.ComputePenetration (
                    scr.rest.Collider,
                    scr.rest.Collider.transform.position,
                    scr.rest.Collider.transform.rotation,
                    scr.physics.meshCollider,
                    scr.transForm.position,
                    scr.transForm.rotation,
                    out direction, out dist);

                // Check break
                if (col == false && scr.rest.region == RFBoundTriggerType.Inside)
                    brokeState = true;
                else if (col == true && scr.rest.region == RFBoundTriggerType.Outside)
                    brokeState = true;

                // Check distance
                if (brokeState == true)
                {
                    // Delay
                    if (scr.rest.actionDelay > 0)
                        yield return delay;

                    BrokeRestriction (scr);
                }
            }
        }
    }
}