using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Bomb")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-bomb-component/")]
    public class RayfireBomb : MonoBehaviour
    {
        public enum RangeType
        {
            Spherical = 0,
            Directional = 3
        }

        // Strength fade Type
        public enum FadeType
        {
            Linear      = 0,
            Exponential = 1,
            ByCurve     = 3,
            None        = 2
        }

        // Projectiles class
        [Serializable]
        public class Projectile
        {
            public Vector3          positionPivot;
            public Vector3          positionClosest;
            public Vector3          expPos;
            public float            fade;
            public Rigidbody        rb;
            public RayfireRigid     rigid;
            public Quaternion       rotation;
            public RFShard          shard;
            public RayfireRigidRoot rigidRoot;
        }
        
        // UI
        public bool           showGizmo;
        public RangeType      rangeType;
        public FadeType       fadeType;
        public float          range   = 5f;
        public Vector2        boxSize = new Vector2 (6, 2);
        public int            deletion;
        public float          strength    = 1f;
        public int            variation   = 50;
        public int            chaos       = 30;
        public bool           forceByMass = true;
        public bool           affectInactive;
        public bool           affectKinematic;
        public float          heightOffset;
        public float          delay;
        public bool           atStart;
        public bool           destroy;
        public bool           obst_enable;
        public bool           obst_static;
        public bool           obst_kinematik ;
        public List<Collider> obstacleCollidersList;
        public bool           applyDamage;
        public float          damageValue;
        public bool           play;
        public float          volume = 1f;
        public AudioClip      clip;
        public int            mask      = -1;
        public string         tagFilter = "Untagged";
        
        public AnimationCurve curve = new AnimationCurve (
            new Keyframe (0,    1, -1, 0), new Keyframe (0.5f, 1, 0, 0),
            new Keyframe (0.7f, 0, -1, 0), new Keyframe (1,    0, 0,  -1));
        
        // Event
        public RFExplosionEvent explosionEvent = new RFExplosionEvent();

        int    maxObstacleHits = 10;
        string untagged        = "Untagged";
        
        // Non Serialized
        [NonSerialized] Vector3           bombPosition;
        [NonSerialized] Vector3           bombDirection;
        [NonSerialized] Vector3           explPosition;
        [NonSerialized] Collider[]        explColliders;
        [NonSerialized] HashSet<Collider> obst_hash;
        [NonSerialized] List<Rigidbody>   rigidbodies         = new List<Rigidbody>();
        [NonSerialized] List<Projectile>  projectiles         = new List<Projectile>();
        [NonSerialized] List<Projectile>  deletionProjectiles = new List<Projectile>();

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Clear
            ClearLists();
        }
        
        // Auto explode
        void Start()
        {
            if (Application.isPlaying == true)
                if (atStart == true)
                    Explode (delay);
        }
        
        // Copy properties from another Rigs
        public void CopyFrom (RayfireBomb scr)
        {
            rangeType       = scr.rangeType;
            fadeType        = scr.fadeType;
            range           = scr.range;
            deletion        = scr.deletion;
            strength        = scr.strength;
            variation       = scr.variation;
            chaos           = scr.chaos;
            forceByMass     = scr.forceByMass;
            affectKinematic = scr.affectKinematic;
            heightOffset    = scr.heightOffset;
            delay           = scr.delay;
            applyDamage     = scr.applyDamage;
            damageValue     = scr.damageValue;
            clip            = scr.clip;
            volume          = scr.volume;
        }

        /// /////////////////////////////////////////////////////////
        /// Explode
        /// /////////////////////////////////////////////////////////

        // Explode bomb
        public void Explode (float delayLoc)
        {
            if (delayLoc == 0)
                Explode();
            else if (delayLoc > 0)
                StartCoroutine (ExplodeCor());
        }

        // Init delay before explode
        IEnumerator ExplodeCor()
        {
            // Wait delay time
            yield return new WaitForSeconds (delay);

            // Explode
            Explode();
        }

        // Explode bomb
        void Explode()
        {
            // Set bomb and explosion positions
            SetPositions();

            // Setup collider, projectiles and rigidbodies
            if (Setup() == false)
                return;
            
            // Recollect projectiles if damage with demolition.
            if (SetRigidDamage() == true)
                if (Setup() == false)
                    return;
            
            // Deletion
            Deletion();

            // Activate inactive and kinematic objects
            Activate();
            
            // Apply explosion force
            SetForce();
            
            // Event
            RFExplosionEvent.ExplosionEvent (this);

            // Explosion Sound
            PlayAudio();

            // Clear lists in runtime
            if (Application.isEditor == false)
                ClearLists();

            // Destroy
            if (destroy == true)
                Destroy (gameObject, 1f);
        }

        // Explosion Sound
        void PlayAudio()
        {
            if (play == true && clip != null)
            {
                // Fix volume
                if (volume < 0)
                    volume = 1f;

                // TODO Set volume bu range

                // Play clip
                AudioSource.PlayClipAtPoint (clip, transform.position, volume);
            }
        }

        // Setup collider, projectiles and rigidbodies
        bool Setup()
        {
            // Clear all lists
            ClearLists();

            // Set colliders by range type
            SetColliders();

            // Get obstacle colliders from all colliders in explosion range
            SetObstacleHash();
            
            // Filter exploded colliders by obstacles
            CollidersFilter();
            
            // Set rigidbodies by colliders
            SetProjectiles();
            
            // Nothing to explode
            if (projectiles.Count == 0)
                return false;

            return true;
        }

        // Reset all lists
        void ClearLists()
        {
            explColliders = null;
            rigidbodies.Clear();
            projectiles.Clear();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Restore
        /// /////////////////////////////////////////////////////////
        
        // Restore exploded objects transformation
        public void Restore()
        {
            RestoreProjectiles (projectiles);
            RestoreProjectiles (deletionProjectiles);
        }

        // Restore projectiles
        static void RestoreProjectiles (List<Projectile> prj)
        {
            for (int i = 0; i < prj.Count; i++)
                if (prj[i].rigid != null)
                    prj[i].rigid.ResetRigid();
                else if (prj[i].rb != null)
                {
                    prj[i].rb.velocity           = Vector3.zero;
                    prj[i].rb.angularVelocity    = Vector3.zero;
                    prj[i].rb.transform.SetPositionAndRotation (prj[i].positionPivot, prj[i].rotation);
                }
        }
            
        /// /////////////////////////////////////////////////////////
        /// Setups
        /// /////////////////////////////////////////////////////////

        // Set bomb and explosion positions
        void SetPositions()
        {
            // Set initial bomb and explosion positions
            bombPosition  = transform.position;
            bombDirection = transform.forward;
            explPosition  = bombPosition;

            // Consider height offset
            if (heightOffset != 0)
                explPosition = bombPosition + transform.TransformDirection (0f, heightOffset, 0f);
        }
        
        // Set colliders by range type
        void SetColliders()
        {
            // Set mask to everything because of obstacles
            int finalMask = mask;
            if (obst_enable == true)
                if (obst_static == true || obst_kinematik == true)
                    finalMask = -1;
            
            if (rangeType == RangeType.Spherical)
                explColliders = Physics.OverlapSphere (explPosition, range, finalMask);
            else if (rangeType == RangeType.Directional)
            {
                Vector3 center     = bombPosition + (range / 2f) * bombDirection;
                Vector3 halfExtent = new Vector3(boxSize.x / 2f, boxSize.y / 2f, range / 2f);
                explColliders = Physics.OverlapBox (center, halfExtent, transform.rotation, finalMask);
            }
        }

        // Get obstacle colliders hash from all colliders in explosion range
        void SetObstacleHash()
        {
            if (obst_enable == true)
            {
                // Final obstacle colliders
                List<Collider> obstColliders = new List<Collider>();
                if (obstacleCollidersList != null)
                    for (int i = 0; i < obstacleCollidersList.Count; i++)
                        if (obstacleCollidersList[i] != null)
                            obstColliders.Add (obstacleCollidersList[i]);

                // Collect obstacles colliders
                for (int j = 0; j < explColliders.Length; j++)
                {
                    // Obstacles colliders without rigidbody
                    if (obst_static == true && explColliders[j].attachedRigidbody == null)
                    {
                        obstColliders.Add (explColliders[j]);
                        explColliders[j] = null;
                        continue;
                    }

                    // Obstacles colliders without kinematik rigidbody
                    if (obst_kinematik == true &&
                        explColliders[j].attachedRigidbody != null &&
                        explColliders[j].attachedRigidbody.isKinematic == true)
                    {
                        obstColliders.Add (explColliders[j]);
                        explColliders[j] = null;
                    }
                }
                
                // Obstacles colliders as hash
                if (obstColliders.Count > 0)
                    obst_hash = new HashSet<Collider> (obstColliders);
            }
        }
        
        // Filter exploded colliders by obstacles and other properties
        void CollidersFilter()
        {
            // TODO check cluster explosion. sum expl force for each collider issue
            
            // Filter by obstacles
            if (obst_enable == true)
            {
                // Ray intersections -> obstacles -> projectiles 
                if (obst_hash != null && obst_hash.Count > 0)
                {
                    // Vars
                    int          num = 0;
                    Vector3      bombToFrag = Vector3.zero;
                    RaycastHit[] hits       = new RaycastHit[maxObstacleHits];
                    
                    // Check all 
                    for (int c = 0; c < explColliders.Length; c++)
                    {
                        // Null check
                        if (explColliders[c] == null)
                            continue;

                        // Vector from bomb to frag collider
                        if (rangeType == RangeType.Spherical)
                        {
                            bombToFrag = explColliders[c].bounds.center - explPosition;
                            num        = Physics.RaycastNonAlloc (explPosition, bombToFrag.normalized, hits, bombToFrag.magnitude);
                        }
                        // 90 Degree Vector from bomb plane to frag collider
                        else if (rangeType == RangeType.Directional)
                        {
                            Vector3 expPos = ClosestOnPlane (explColliders[c].bounds.center, bombPosition, bombDirection);
                            bombToFrag = explColliders[c].bounds.center - expPos;
                            num        = Physics.RaycastNonAlloc (expPos, bombDirection, hits, bombToFrag.magnitude);
                        }
                        
                        // Raycast
                        if (num > 0)
                            for (int h = 0; h < hits.Length; h++)
                                
                                // Check if collider in obstacle colliders hash
                                if (obst_hash.Contains (hits[h].collider) == true)
                                
                                    // Exclude collider if obstacle closer than frag
                                    if (hits[h].distance < bombToFrag.magnitude)
                                        explColliders[c] = null;
                    }
                }
            }
            
            // Filter by tag
            if (tagFilter != untagged)
            {
                for (int i = 0; i < explColliders.Length; i++)
                    if (explColliders[i] != null)
                        if (explColliders[i].gameObject.CompareTag (tagFilter) == false) 
                            explColliders[i] = null;
            }
            
            // Filter by layer if obstacle enabled and added its layers to finalmask
            if (mask != -1 && obst_enable == true)
            {
                for (int i = 0; i < explColliders.Length; i++)
                    if (explColliders[i] != null)
                        if (mask == (mask | 1 << explColliders[i].gameObject.layer) == false )
                            explColliders[i] = null;
            }
        }
        
        // Set projectiles by colliders
        void SetProjectiles()
        {
            // Collect all rigid bodies in range
            projectiles.Clear();
            foreach (Collider col in explColliders)
            {
                // Null check
                if (col == null)
                    continue;
                
                // Get attached rigid body
                Rigidbody rb = col.attachedRigidbody;

                // No rb
                if (rb == null)
                    continue;

                // Create projectile if rigid body new. Could be several colliders on one object. TODO change to hash
                if (rigidbodies.Contains (rb) == false)
                {
                    Projectile projectile = new Projectile
                    {
                        rb            = rb,
                        positionPivot = rb.transform.position,
                        rotation      = rb.transform.rotation,
                        expPos        = explPosition
                    };

                    // For directional bomb explosion point is different for every fragment
                    if (rangeType == RangeType.Directional)
                        projectile.expPos = ClosestOnPlane (projectile.positionPivot, bombPosition, bombDirection);

                    // Get position of closest point to explosion position
                    projectile.positionClosest = col.bounds.ClosestPoint (projectile.expPos);
  
                    // Get fade multiplier by range and distance
                    projectile.fade = Fade (projectile.expPos, projectile.positionClosest);
                    
                    // Skip fragments out of range
                    if (projectile.fade <= 0)
                        continue;
                    
                    // Check for Rigid script
                    projectile.rigid = projectile.rb.GetComponent<RayfireRigid>();

                    // TODO optional targets, for quick search
                    
                    // Set RigidRoot amd Shard
                    if (projectile.rigid == null)
                    {
                        projectile.rigidRoot = projectile.rb.GetComponentInParent<RayfireRigidRoot>();
                        if (projectile.rigidRoot != null)
                        {
                            if (projectile.rigidRoot.collidersHash == null)
                            {
                                List<Collider> collidersTemp = new List<Collider>(projectile.rigidRoot.inactiveShards.Count);
                                for (int s = 0; s < projectile.rigidRoot.inactiveShards.Count; s++)
                                    collidersTemp.Add (projectile.rigidRoot.inactiveShards[s].col);
                                projectile.rigidRoot.collidersHash = new HashSet<Collider>(collidersTemp);
                            }
                            
                            // Collider belongs to inactive shard
                            if (projectile.rigidRoot.collidersHash.Contains (col) == true)
                            {
                                for (int i = 0; i < projectile.rigidRoot.inactiveShards.Count; i++)
                                {
                                    if (projectile.rigidRoot.inactiveShards[i].col == col)
                                    {
                                        projectile.shard = projectile.rigidRoot.inactiveShards[i];
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Skip inactive fragments if affectInactive disabled
                    if (affectInactive == false)
                    {
                        if (projectile.rigid != null)
                            if (projectile.rigid.simulationType == SimType.Inactive)
                                continue;
                        
                        if (projectile.shard != null)
                            if (projectile.shard.sm == SimType.Inactive)
                                continue;
                    }

                    // Collect projectile
                    projectiles.Add (projectile);

                    // Remember rigid body
                    rigidbodies.Add (rb);
                }
            }
            
            // TODo nullify collider has list in RigidRoots
            // do not collect kinematic
            // collect rigid kinematic if can be activated
        }

        // Set RayFire Rigid refs for projectiles
        bool SetRigidDamage()
        {
            // Recollect state for new fragments after demolition
            bool recollectState = false;

            // Apply damage to rigid and demolish first
            if (applyDamage == true && damageValue > 0)
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    // Rigid exist and damage enabled
                    if (projectiles[i].rigid != null && projectiles[i].rigid.damage.en == true)
                    {
                        // Apply damage and demolish
                        if (projectiles[i].rigid.ApplyDamage (damageValue * projectiles[i].fade, projectiles[i].expPos, range) == true)
                            recollectState = true;
                    }
                }
            }

            return recollectState;
        }

        // Deletion
        void Deletion()
        {
            if (deletion > 0)
            {
                // Get deletion projectiles and remove from force projectiles list
                deletionProjectiles = new List<Projectile>();
                for (int i = projectiles.Count - 1; i >= 0; i--)
                    if (Vector3.Distance (projectiles[i].positionClosest, projectiles[i].expPos) < range * deletion / 100f)
                    {
                        deletionProjectiles.Add (projectiles[i]);
                        projectiles.RemoveAt (i);
                    }
                
                // Destroy
                if (deletionProjectiles.Count > 0)
                    for (int i = 0; i < deletionProjectiles.Count; i++)
                    {
                        if (deletionProjectiles[i].rigid != null)
                            RayfireMan.DestroyFragment (deletionProjectiles[i].rigid, null);
                        else
                            Destroy (deletionProjectiles[i].rb.gameObject); 
                    }
            }
        }
        
        // Activate inactive and kinematic objects
        void Activate()
        {
            // Activate disabled
            if (affectInactive == false && affectKinematic == false)
                return;
            
            foreach (Projectile projectile in projectiles)
            {
                // Outside of range
                if (projectile.fade <= 0)
                    return;

                // Affect Kinematic rigid body
                if (affectKinematic == true && projectile.rb.isKinematic == true)
                {
                    // Convert kinematic to dynamic via rigid script
                    if (projectile.rigid != null)
                        projectile.rigid.Activate();

                    // Activate kinematic rigidRoot shard
                    else if (projectile.shard != null)
                    {
                        if (projectile.shard.sm == SimType.Kinematic)
                            RFActivation.ActivateShard (projectile.shard, projectile.rigidRoot);
                    }
                    
                    // Convert regular kinematic to dynamic
                    else
                    {
                        projectile.rb.isKinematic = false;

                        // TODO Set mass

                        // Set convex
                        MeshCollider meshCol = projectile.rb.gameObject.GetComponent<MeshCollider>();
                        if (meshCol != null && meshCol.convex == false)
                            meshCol.convex = true;
                    }
                    
                    // Skip inactive object activation. 
                    continue;
                }
                
                // Affect inactive
                if (affectInactive == true)
                {
                    // Activate inactive via rigid script
                    if (projectile.rigid != null)
                    {
                        if (projectile.rigid.simulationType == SimType.Inactive)
                            projectile.rigid.Activate();
                    }
                    
                    // Activate inactive rigidRoot shard
                    else if (projectile.shard != null)
                    {
                        if (projectile.shard.sm == SimType.Inactive)
                            RFActivation.ActivateShard (projectile.shard, projectile.rigidRoot);
                    }
                }
            }
        }
        
        // Apply explosion force, vector and rotation to projectiles
        void SetForce()
        {
            // Set same random state
            Random.InitState (1);

            // Set forceMode by mass state
            ForceMode forceMode = ForceMode.Impulse;
            if (forceByMass == false)
                forceMode = ForceMode.VelocityChange;
            
            // Get str for each object by explode type with variation
            foreach (Projectile projectile in projectiles)
            {
                // TODO check if not activated and doesn't need to be forced

                // Get local velocity strength
                float strVar  = strength * variation / 100f + strength;
                float str     = Random.Range (strength, strVar);
                float strMult = projectile.fade * str * 10f;

                // Get explosion vector from explosion position to projectile center of mass
                Vector3 vector = ExplosionVector (projectile);
                
                // Apply force
                projectile.rb.AddForce (vector * strMult, forceMode);

                // Get local rotation strength 
                Vector3 rot = new Vector3 (Random.Range (-chaos, chaos), Random.Range (-chaos, chaos), Random.Range (-chaos, chaos));

                // Set rotation impulse
                projectile.rb.angularVelocity = rot;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Support
        /// /////////////////////////////////////////////////////////

        // Fade multiplier
        float Fade (Vector3 bombPos, Vector3 fragPos)
        {
            // Get rate by fade type
            float fade = 1f;

            // Linear or Exponential fade
            if (fadeType == FadeType.Linear)
                fade = 1f - Vector3.Distance (bombPos, fragPos) / range;

            // Exponential fade
            else if (fadeType == FadeType.Exponential)
            {
                fade =  1f - Vector3.Distance (bombPos, fragPos) / range;
                fade *= fade;
            }
            
            // By curve
            else if (fadeType == FadeType.ByCurve)
            {
                fade = curve.Evaluate (Vector3.Distance (bombPos, fragPos) / range);;
            }

            // Cap fade
            if (fade < 0.01f)
                fade = 0;

            return fade;
        }

        // Get explosion vector from explosion position to projectile center of mass
        Vector3 ExplosionVector (Projectile projectile)
        {
            return Vector3.Normalize (projectile.positionPivot - projectile.expPos);
        }
        
        // Get closest point on plane
        private Vector3 ClosestOnPlane(Vector3 pos, Vector3 plPos, Vector3 plNorm)
        {
            float f = -Vector3.Dot(plNorm, (pos - plPos)) / Vector3.Dot(plNorm, plNorm);
            return pos + f * plNorm;
        }
    }
}