using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RayFire
{
    [Serializable]
    public class RFLimitations
    {
        [FormerlySerializedAs ("byCollision")]
        public bool   col;
        [FormerlySerializedAs ("solidity")]
        public float  sol;
        public string tag;
        public int    depth;
        public float  time;
        public float  size;
        [FormerlySerializedAs ("visible")]
        public bool   vis;
        [FormerlySerializedAs ("sliceByBlade")]
        public bool   bld;
        public Bounds bound;
        
        // Non serialized
        [NonSerialized] public List<Vector3> slicePlanes;
        [NonSerialized] public ContactPoint  contactPoint;
        [NonSerialized] public Vector3       contactVector3;
        [NonSerialized] public Vector3       contactNormal;
        [NonSerialized] public bool          demolitionShould;
        [NonSerialized] public bool          demolished;
        [NonSerialized] public float         birthTime;
        [NonSerialized] public float         bboxSize;
        [NonSerialized] public int           currentDepth;
        [NonSerialized] public bool          dmlCorState;
        
        // Blade props
        [NonSerialized] public float         sliceForce;
        [NonSerialized] public bool          affectInactive;
        
        // Family data. Do not nullify in Reset
        [NonSerialized] public RayfireRigid       anc;  // ancestor
        [NonSerialized] public List<RayfireRigid> desc; // descendants
        
        // Static
        static        float  kinematicCollisionMult = 7f;
        static        string rootStr                = "_root";
        public static string rigidStr               = "RayFire Rigid: ";
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFLimitations()
        {
            InitValues();
            LocalReset();
        }
        
        // Copy from
        public void CopyFrom (RFLimitations source)
        {
            col   = source.col;
            sol   = source.sol;
            depth = source.depth;
            time  = source.time;
            size  = source.size;
            tag   = source.tag;
            vis   = source.vis;
            bld   = source.bld;
            
            // Do not copy currentDepth. Set in other place
            
            LocalReset();
        }

        // Starting values
        void InitValues()
        {
            col   = true;
            sol   = 0.1f;
            depth = 1;
            time  = 0.2f;
            size  = 0.1f;
            tag   = "Untagged";
            vis   = false;
            bld   = false;
            anc   = null;
            desc  = null;
        }

        // Reset
        public void LocalReset()
        {
            slicePlanes      = null;
            contactVector3   = Vector3.zero;
            contactNormal    = Vector3.down;
            demolitionShould = false;
            demolished       = false;
            dmlCorState      = false;
            affectInactive   = false;
            currentDepth     = 0;
            birthTime        = 0f;
            sliceForce       = 0;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
            LocalReset();
            
            // TODO
            // bound = new Bounds ();
            // contactPoint = null;
        }

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
         
        // Cache velocity for fragments 
        public IEnumerator DemolishableCor(RayfireRigid scr)
        {
            // Stop if running 
            if (dmlCorState == true)
                yield break;
            
            // Set running state
            dmlCorState = true;
            
            while (scr.demolitionType != DemolitionType.None)
            {
                // Max depth reached
                if (scr.limitations.depth > 0 && scr.limitations.currentDepth >= scr.limitations.depth)
                    scr.demolitionType = DemolitionType.None;

                // Init demolition
                if (scr.limitations.demolitionShould == true)
                {
                    scr.Demolish();
                }

                // Check for slicing planes and init slicing
                else if (scr.limitations.bld == true && scr.limitations.slicePlanes != null && scr.limitations.slicePlanes.Count > 1)
                    scr.Slice();
                
                yield return null;
            }
            
            // Set state
            dmlCorState = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // MeshRoot Integrity check
        public static void MeshRootCheck (RayfireRigid scr)
        {
            // Null fragments
            if (scr.HasFragments == true)
            {
                for (int f = 0; f < scr.fragments.Count; f++)
                {
                    if (scr.fragments[f] == null)
                    {
                        if (RayfireMan.debugStatic == true)
                            Debug.Log (rigidStr + scr.name + " object has missing fragments. Reset Setup and used Editor Setup again", scr.gameObject);
                        scr.fragments = new List<RayfireRigid>();
                        break;
                    }
                }
            }
        }
        
        // Check for user mistakes
        public static void Checks (RayfireRigid scr)
        {
            // ////////////////
            // Sim Type
            // ////////////////
            
            // Static and demolishable
            if (scr.simulationType == SimType.Static)
            {
                if (scr.demolitionType != DemolitionType.None)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is not None. Static object can not be demolished. Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }
            }
            
            // Non static simulation but static property
            if (scr.simulationType != SimType.Static)
            {
                if (scr.gameObject.isStatic == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but object is Static. Turn off Static checkbox in Inspector.", scr.gameObject);
                }
            }
           
            // ////////////////
            // Object Type
            // ////////////////
            
            // Object can not be simulated as mesh
            if (scr.objectType == ObjectType.Mesh)
            {
                // Has no mesh
                if (scr.meshFilter == null || scr.meshFilter.sharedMesh == null)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no mesh. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
                
                // Not readable mesh 
                if (scr.demolitionType != DemolitionType.None && scr.demolitionType != DemolitionType.ReferenceDemolition)
                {
                    if (scr.meshFilter != null && scr.meshFilter.sharedMesh != null && scr.meshFilter.sharedMesh.isReadable == false)
                    {
                        if (RayfireMan.debugStatic == true)
                            Debug.Log (rigidStr + scr.name + " Mesh is not readable. Demolition type set to None. Open Import Settings and turn On Read/Write Enabled property", scr.meshFilter.gameObject);
                        scr.demolitionType         = DemolitionType.None;
                        scr.meshDemolition.badMesh = 10;
                    }
                }
            }
            
            // Object can not be simulated as cluster
            else if (scr.objectType == ObjectType.NestedCluster || scr.objectType == ObjectType.ConnectedCluster)
            {
                if (scr.transForm.childCount == 0)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no children. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
            }
            
            // Object can not be simulated as mesh
            else if (scr.objectType == ObjectType.SkinnedMesh)
            {
                if (scr.skr == null)
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no SkinnedMeshRenderer. Object Excluded from simulation.", scr.gameObject);
                
                // Excluded from sim by default
                scr.physics.exclude = true;
            }
            
            // ////////////////
            // Demolition Type
            // ////////////////
            
            // Demolition checks
            if (scr.demolitionType != DemolitionType.None)
            {
                // // Static
                // if (scr.simulationType == SimType.Static)
                // {
                //     Debug.Log (rigidStr + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to None.", scr.gameObject);
                //     scr.demolitionType = DemolitionType.None;
                // }
                
                // Set runtime demolition for clusters and skinned mesh
                if (scr.objectType == ObjectType.SkinnedMesh ||
                    scr.objectType == ObjectType.NestedCluster ||
                    scr.objectType == ObjectType.ConnectedCluster)
                {
                    if (scr.demolitionType != DemolitionType.Runtime && scr.demolitionType != DemolitionType.ReferenceDemolition)
                    {
                        if (RayfireMan.debugStatic == true)
                            Debug.Log (rigidStr + scr.name + " Object Type set to " + scr.objectType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to Runtime.", scr.gameObject);
                        scr.demolitionType = DemolitionType.Runtime;
                    }
                }
                
                // No Shatter component for runtime demolition with Use Shatter on

                if (scr.demolitionType == DemolitionType.Runtime ||
                    scr.demolitionType == DemolitionType.AwakePrecache ||
                    scr.demolitionType == DemolitionType.AwakePrefragment)
                {
                    if (scr.meshDemolition.use == true && scr.meshDemolition.sht == null)
                    {
                        if (RayfireMan.debugStatic == true)
                            Debug.Log (rigidStr + scr.name + "Demolition Type is " + scr.demolitionType.ToString() + ". Has no Shatter component, but Use Shatter property is On. Use Shatter property was turned Off.", scr.gameObject);
                        scr.meshDemolition.use = false;
                    }
                }
            }
            
            // None check
            if (scr.demolitionType == DemolitionType.None)
            {
                if (scr.HasMeshes == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to None. Had precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.objectType == ObjectType.Mesh && scr.HasFragments == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to None. Had prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log (rigidStr + scr.name + " Demolition Type set to None. Had precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
            }

            // Runtime check
            else if (scr.demolitionType == DemolitionType.Runtime)
            {
                if (scr.HasMeshes == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Runtime. Had precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.objectType == ObjectType.Mesh && scr.HasFragments == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Runtime. Had prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Runtime. Had precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
                
                // No runtime caching for rigid with shatter with tets/slices/glue
                if (scr.meshDemolition.use == true && scr.meshDemolition.ch.tp != CachingType.Disable)
                {
                    if (scr.meshDemolition.sht.type == FragType.Decompose ||
                        scr.meshDemolition.sht.type == FragType.Tets ||
                        scr.meshDemolition.sht.type == FragType.Slices || 
                        scr.meshDemolition.sht.clusters.enable == true)
                    {
                        if (RayfireMan.debugStatic == true)
                            Debug.Log (rigidStr + scr.name + " Demolition Type is Runtime, Use Shatter is On. Unsupported fragments type. Runtime Caching supports only Voronoi, Splinters, Slabs and Radial fragmentation types. Runtime Caching was Disabled.", scr.gameObject);
                        scr.meshDemolition.ch.tp = CachingType.Disable;
                    }
                }
            }

            // Awake precache check
            else if (scr.demolitionType == DemolitionType.AwakePrecache)
            {
                if (scr.HasMeshes == true)
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Precache. Had manually precached Unity meshes which were overwritten.", scr.gameObject);
                
                if (scr.HasFragments == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Precache. Has manually precached serialized meshes.", scr.gameObject);
                }
            }

            // Awake prefragmented check
            else if (scr.demolitionType == DemolitionType.AwakePrefragment)
            {
                if (RayfireMan.debugStatic == true)
                {
                    if (scr.HasFragments == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually prefragmented objects", scr.gameObject);
                    if (scr.HasMeshes == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually precached Unity meshes.", scr.gameObject);
                    if (scr.HasRfMeshes == true)
                        Debug.Log (rigidStr + scr.name + " Demolition Type set to Awake Prefragment. Has manually precached serialized meshes.", scr.gameObject);
                }
            }
            
            // Reference demolition
            else if (scr.demolitionType == DemolitionType.ReferenceDemolition)
            {
                if (scr.referenceDemolition.reference == null && scr.referenceDemolition.HasRandomRefs == false)
                {
                    //Debug.Log (rigidStr + scr.name + " Demolition Type set to Reference Demolition but Reference is not defined.", scr.gameObject);
                }
            }
        }
        
        // Set bound and size
        public static void SetBound (RayfireRigid scr)
        {
            if (scr.objectType == ObjectType.Mesh)
                scr.limitations.bound = scr.meshRenderer.bounds;
            else if (scr.objectType == ObjectType.SkinnedMesh)
                scr.limitations.bound = scr.skr.bounds;
            else if (scr.objectType == ObjectType.NestedCluster || scr.objectType == ObjectType.ConnectedCluster)
                scr.limitations.bound = RFCluster.GetChildrenBound (scr.transForm);
            scr.limitations.bboxSize = scr.limitations.bound.size.magnitude;
        }
        
        // Set ancestor
        public static void SetAncestor (RayfireRigid scr)
        {
            // Set ancestor to this if it is ancestor
            if (scr.limitations.anc == null)
                for (int i = 0; i < scr.fragments.Count; i++)
                    scr.fragments[i].limitations.anc = scr;
            else
                for (int i = 0; i < scr.fragments.Count; i++) 
                    scr.fragments[i].limitations.anc = scr.limitations.anc;
        }
        
        // Set descendants 
        public static void SetDescendants (RayfireRigid scr)
        {
            if (scr.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
                return;

            if (scr.limitations.anc == null)
                scr.limitations.desc.AddRange (scr.fragments);
            else
                scr.limitations.anc.limitations.desc.AddRange (scr.fragments);
        }
        
        // Create root
        public static void CreateRoot (RayfireRigid rfScr)
        {
           GameObject root = new GameObject(rfScr.gameObject.name + rootStr);
           rfScr.rootChild          = root.transform;
           rfScr.rootChild.position = rfScr.transForm.position;
           rfScr.rootChild.rotation = rfScr.transForm.rotation;
           rfScr.rootChild.SetParent (rfScr.transform.parent);
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        // Check if collision data needed
        public bool CollisionCheck(RayfireRigid rigid)
        {
            if (rigid.limitations.col == true)
                return true;
            if (rigid.damage.en == true && rigid.damage.col == true)
                return true;
            return false;
        }
        
        // Set Contact info
        public void SetContactInfo(ContactPoint contact)
        {
            contactPoint   = contact;
            contactVector3 = contactPoint.point;
            contactNormal  = contactPoint.normal;
        }
        
        // Collision with kinematic object. Uses collision.impulse
        public bool KinematicCollisionCheck(Collision collision, float finalSolidity)
        {
            if (collision.rigidbody != null && collision.rigidbody.isKinematic == true)
                if (collision.impulse.magnitude > finalSolidity * kinematicCollisionMult)
                {
                    SetContactInfo (collision.GetContact(0));
                    return true;
                }
            return false;
        }

        // Collision force checks. Uses relativeVelocity
        public bool ContactPointsCheck(Collision collision, float finalSolidity)
        {
            float collisionMagnitude = collision.relativeVelocity.magnitude;
            for (int i = 0; i < collision.contactCount; i++)
            {
                // Set contact point
                SetContactInfo (collision.GetContact(i));
                
                // Demolish if collision high enough
                if (collisionMagnitude > finalSolidity)
                    return true;
            }
            
            return false;
        }
        
        // Collision force checks. Uses relativeVelocity
        public bool DamagePointsCheck(Collision collision, RayfireRigid rigid)
        {
            float collisionMagnitude = collision.relativeVelocity.magnitude;
            for (int i = 0; i < collision.contactCount; i++)
            {
                // Set contact point
                SetContactInfo (collision.GetContact(i));
                
                // Collect damage by collision
                if (rigid.ApplyDamage (collisionMagnitude * rigid.damage.mlt, contactVector3, 0f, collision.contacts[i].thisCollider) == true)
                        return true;
            }
            
            return false;
        }
    }
}