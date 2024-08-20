using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFRigidRootDemolition
    {
        [Space (2)]
        public RFLimitations limitations = new RFLimitations();
        // [Space (2)]
        // public RFDemolitionMesh meshDemolition = new RFDemolitionMesh();
        [Space (2)]
        public RFDemolitionCluster clusterDemolition = new RFDemolitionCluster();
        // [Space (2)]
        // public RFSurface materials = new RFSurface();
    }

    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Rigid Root")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-rigid-root-component/")]
    public class RayfireRigidRoot : MonoBehaviour
    {
        public enum InitType
        {
            ByMethod = 0,
            AtStart  = 1
        }

        // UI
        public InitType              initialization = InitType.AtStart;
        public SimType               simulationType = SimType.Dynamic;
        public RFPhysic              physics        = new RFPhysic();
        public RFActivation          activation     = new RFActivation();
        public RFRigidRootDemolition demolition     = new RFRigidRootDemolition();
        public RFFade                fading         = new RFFade();
        public RFReset               reset          = new RFReset();
        
        // Hidden
        public bool               initialized;
        public bool               cached;
        public Transform          tm;
        public RFCluster          cluster;
        public List<RayfireRigid> meshRoots;
        public List<Collider>     collidersList;
        public List<RFShard>      meshRootShards;
        public List<RFShard>      rigidRootShards;
        
        // Non Serialized
        [NonSerialized] public float                   sizeSum;
        [NonSerialized] public RayfireSound            sound;
        [NonSerialized] public List<RFCluster>         clusters;
        [NonSerialized] public List<RFShard>           inactiveShards;
        [NonSerialized] public List<RFShard>           offsetFadeShards;
        [NonSerialized]        List<RFShard>           destroyShards; // TODO remove or use. not in use right now
        [NonSerialized]        List<RFShard>           meshRigidShards;
        [NonSerialized] public Transform[]             parentList;
        [NonSerialized] public List<RayfireDebris>     debrisList;
        [NonSerialized] public List<RayfireDust>       dustList;
        [NonSerialized] public RayfireUnyielding[]     unyList;
        [NonSerialized] public List<Transform>         particleList;
        [NonSerialized] public bool                    corState;
        [NonSerialized] public HashSet<Collider>       collidersHash;

        // Events
        public RFActivationEvent activationEvent  = new RFActivationEvent();

        // Static
        static          string strRoot = "RayFire RigidRoot: ";

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Awake
        void Awake()
        {
            if (initialization == InitType.AtStart)
            {
                Initialize();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Enable/Disable
        /// /////////////////////////////////////////////////////////

        // Disable
        void OnDisable()
        {
            // Set coroutines states
            corState                    = false;
            activation.inactiveCorState = false;
            fading.offsetCorState       = false;
        }

        // Activation
        void OnEnable()
        {
            if (gameObject.activeSelf == true && initialized == true && corState == false)
                StartAllCoroutines();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Awake ops
        /// /////////////////////////////////////////////////////////
        
        // Initialize 
        public void Initialize()
        {
            // Deactivated
            if (gameObject.activeSelf == false)
                return;
           
            // No children
            if (transform.childCount == 0)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log (strRoot + name + " has no children. RigidRoot should be used on object with children.", gameObject);
                return;
            }

            // Not initialized
            if (initialized == false)
            {
                // Init Awake methods
                AwakeMethods();
                
                // Init sound
                RFSound.InitializationSound(sound, cluster.bound.size.magnitude);
            }
        }

        // Init connectivity if has
        void InitConnectivity()
        {
            activation.cnt = GetComponent<RayfireConnectivity>();
            if (activation.cnt != null)
            {
                activation.cnt.cluster.shards.Clear();
                activation.cnt.rigidRootHost = this;
                
                // Cached RigidRoot but no Connectivity
                if (RayfireMan.debugStatic == true)
                    if (cached == true && activation.cnt.cluster.cachedHost == false)
                        Debug.Log (strRoot + name + " object has Editor Setup but its connection data is not cached. Reset Setup and use Editor Setup again.", gameObject);

                // Init connectivity
                activation.cnt.Initialize();
                
                // Clear shards list in Editor setup to avoid prefab double shard list
                if (Application.isPlaying == false)
                    activation.cnt.cluster.shards.Clear();
            }
            
            // Warnings
            if (RayfireMan.debugStatic == true)
            {
                if (activation.con == true && activation.cnt == null)
                    Debug.Log (strRoot + name + " object has enabled Connectivity activation but has no Connectivity component.", gameObject);
                if (activation.con == false && activation.cnt != null)
                    Debug.Log (strRoot + name + " object has Connectivity component but activation by Connectivity is disabled.", gameObject);
            }
        }
        
        // Reset object
        public void ResetRigidRoot()
        {
            RFReset.RigidRootReset (this);
        }

        /// /////////////////////////////////////////////////////////
        /// Setup
        /// /////////////////////////////////////////////////////////
        
        // Editor Setup
        public void EditorSetup()
        {
            // Check if manager should be destroyed after setup
            bool destroyMan = RayfireMan.inst == null;

            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Reset
            ResetSetup();
            
            // Set components
            SetComponents();
                
            // Set new cluster and set shards components
            SetShards();
            
            // Set shard colliders
            SetColliders();
            
            // Set unyielding shards
            SetUnyielding();
            
            // Init connectivity component.
            InitConnectivity();
            
            // Ignore collision. Editor mode
            RFPhysic.SetIgnoreColliders(physics, cluster.shards);

            // Destroy manager
            if (destroyMan == true)
                DestroyImmediate (RayfireMan.inst.transform.gameObject);

            cached = true;
        }
        
        // Editor Reset. EDITOR only
        public void ResetSetup()
        {
            /* TODO
             
            // Reset MeshRoot
            for (int i = 0; i < meshRoots.Count; i++)
            {
                meshRoots[i].rigidroot = null;
                meshRoots[i].debrisList = null;
                meshRoots[i].dustList = null;
            }
            
            for (int i = 0; i < rigids.Count; i++)
            {
                rigids[i].rigidroot = null;
                rigids[i].debrisList = null;
                rigids[i].dustList = null;
            }
            
            */
            
            // Reset connectivity shards
            if (activation.cnt != null)
                activation.cnt.ResetSetup();
            activation.cnt = null;
            
            // Destroy editor defined colliders
            if (collidersList != null && collidersList.Count > 0)
            {
                collidersHash = new HashSet<Collider>(collidersList);
                collidersList.Clear();
                for (int i = 0; i < rigidRootShards.Count; i++)
                    if (rigidRootShards[i].col != null)
                        if (collidersHash.Contains (rigidRootShards[i].col) == true)
                            DestroyImmediate (rigidRootShards[i].col);
                for (int i = 0; i < meshRootShards.Count; i++)
                    if (meshRootShards[i].col != null)
                        if (collidersHash.Contains (meshRootShards[i].col) == true)
                            DestroyImmediate (meshRootShards[i].col);
            }
            
            // Reset
            cluster        = new RFCluster();
            inactiveShards = new List<RFShard>();
            destroyShards  = new List<RFShard>();
            meshRoots      = new List<RayfireRigid>();
            
            
            physics.ignoreList = null;
            sound              = null;
            debrisList         = null;
            dustList           = null;
            unyList            = null;
            destroyShards      = null;

            cached = false;
            
            // TODO Reset colliders
        }
        
        /// /////////////////////////////////////////////////////////
        /// Init methods
        /// /////////////////////////////////////////////////////////
        
        // Awake ops
        void AwakeMethods()
        {
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Cluster Integrity check
            if (RFCluster.IntegrityCheck (cluster) == false)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log (strRoot + name + " has missing shards. Reset Setup and use Editor Setup again.", gameObject);
                ResetSetup();
            }

            // MeshRoots check
            if (MeshRootCheck() == false)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log (strRoot + name + " has missing Rigid component with MeshRoot object type. Reset Setup and use Editor Setup again.", gameObject);
                ResetSetup();
            }

            // Set components
            SetComponents();
            
            // Set shards components 
            SetShards();

            // Set shard colliders
            SetColliders();

            // Set colliders material
            SetCollidersMaterial();
            
            // Ignore collision
            RFPhysic.SetIgnoreColliders (physics, cluster.shards);
            
            // Set unyielding shards. Should be before SetPhysics to change simType
            SetUnyielding();
            
            // Set physics properties for shards
            RFPhysic.SetPhysics (this);
            
            // Set particles. After Physics set collider material
            if (Application.isPlaying == true)
                RFPoolingParticles.InitializeParticles (this);

            // Setup list for activation. After set simState because collect Inactive and Kinematic
            SetInactiveList ();
            
            // Setup list with fade by offset shards
            RFFade.SetOffsetFadeList (this);
            
            // Init Rigid shards
            if (Application.isPlaying == true)
                for (int i = 0; i < meshRigidShards.Count; i++)
                    meshRigidShards[i].rigid.Initialize();
                
            // Start all necessary coroutines
            StartAllCoroutines();
            
            // Initialize connectivity
            InitConnectivity();

            // Object initialized
            initialized = true;

            // TODO Fade destroyShards
        }
        
        // Define basic components
        void SetComponents()
        {
            // Components
            tm      = GetComponent<Transform>();
            unyList = GetComponents<RayfireUnyielding>();
        }

        // Check MeshRoots
        bool MeshRootCheck()
        {
            if (meshRoots != null && meshRoots.Count > 0)
                for (int i = 0; i < meshRoots.Count; i++)
                    if (meshRoots[i] == null)
                        return false;
            return true;
        }
        
        // Set shards components
        void SetShards()
        {
            // Set lists
            clusters = new List<RFCluster>();

            // Already cached: set changed properties
            if (cached == true)
            {
                // Custom Shards Lists
                SetCustomShardsLists();
                
                // Set simulation type
                SetShardsSimulationType();
                
                // Set parent list for all shards
                SetParentList();
                
                // Save tm
                cluster.pos = transform.position;
                cluster.rot = transform.rotation;
                cluster.scl = transform.localScale;
                
                return;
            }
            
            // Set lists
            meshRoots     = new List<RayfireRigid>();
            destroyShards = new List<RFShard>();
            
            // Set new cluster
            cluster               = new RFCluster();
            cluster.childClusters = new List<RFCluster>();
            cluster.pos           = transform.position;
            cluster.rot           = transform.rotation;
            cluster.scl           = transform.localScale;
            
            // Get children
            Transform[] children = new Transform[tm.childCount];
            for (int i = 0; i < tm.childCount; i++)
                children[i] = tm.GetChild (i);
            
            // Convert children to shards
            for (int i = 0; i < children.Length; i++)
            {
                // Skip inactive children
                if (children[i].gameObject.activeSelf == false)
                    continue;
                
                // Check if already has rigid
                RayfireRigid rigid = children[i].gameObject.GetComponent<RayfireRigid>();

                // Has no own rigid
                if (rigid == null)
                {
                    // Has no children. Collect as shard
                    if (children[i].childCount == 0)
                        AddShard (children[i]);

                    // Has children. Collect its children as shards
                    else
                    {
                        // Get empty root children
                        Transform tms;
                        for (int m = 0; m < children[i].childCount; m++)
                        {
                            tms = children[i].GetChild (m);
                            if (tms.childCount == 0)
                                AddShard (tms);
                        }
                    }
                }
                
                // Has own rigid
                else
                {
                    // Set own rigidroot
                    rigid.rigidRoot      = this;
                    rigid.reset.action   = reset.action;
                    rigid.initialization = RayfireRigid.InitType.ByMethod;
                    
                    // Mesh
                    if (rigid.objectType == ObjectType.Mesh)
                        AddShardWithRigid (rigid);

                    // Mesh Root
                    else if (rigid.objectType == ObjectType.MeshRoot)
                    {
                        // Collect
                        meshRoots.Add (rigid);
                        
                        // Set MeshRoot Rigid physics properties here once to use them later
                        rigid.physics.solidity     = rigid.physics.Solidity;
                        rigid.physics.destructible = rigid.physics.Destructible;
            
                        // Set physics material if not defined by user
                        if (rigid.physics.material == null)
                            rigid.physics.material = rigid.physics.PhysMaterial;
                        
                        // Get mesh root children
                        List<Transform> meshRootChildren = new List<Transform>(rigid.transform.childCount);
                        for (int m = 0; m < rigid.transform.childCount; m++)
                            meshRootChildren.Add (rigid.transform.GetChild (m));
                        
                        // Convert mesh root children to shards
                        for (int m = 0; m < meshRootChildren.Count; m++)
                        {
                            // Check if already has rigid
                            RayfireRigid meshRootRigid = meshRootChildren[m].GetComponent<RayfireRigid>();

                            // Has own rigid
                            if (meshRootRigid != null)
                            {
                                // Set own rigidroot
                                meshRootRigid.rigidRoot      = this;
                                meshRootRigid.reset.action   = reset.action;
                                meshRootRigid.initialization = RayfireRigid.InitType.ByMethod;
                                
                                // Mesh
                                if (meshRootRigid.objectType == ObjectType.Mesh) 
                                    AddShardWithRigid (meshRootRigid);
                            }

                            // Add MeshRoot children shard. Set MeshRoot as Rigid for shard to use its physics, activation, fade
                            else
                                AddShard (meshRootChildren[m], rigid);
                        }
                    }
                    
                    // Connected cluster
                    else if (rigid.objectType == ObjectType.ConnectedCluster)
                    {
                        
                        
                        
                    }
                }
            }
            
            // Set shards id
            for (int id = 0; id < cluster.shards.Count; id++)
                cluster.shards[id].id = id;

            // Custom Shards Lists
            SetCustomShardsLists();

            // Set simulation type. Should be before SetUnyielding because it changes simType.
            SetShardsSimulationType();

            // Set parent list for all shards
            SetParentList();

            // Set bound if has not
            cluster.bound = RFCluster.GetShardsBound (cluster.shards);
        }
        
        // Set Custom Shards List
        void SetCustomShardsLists()
        {
            rigidRootShards = new List<RFShard>();
            meshRigidShards = new List<RFShard>();
            meshRootShards  = new List<RFShard>();
            for (int i = 0; i < cluster.shards.Count; i++)
                if (cluster.shards[i].rigid == null)
                    rigidRootShards.Add (cluster.shards[i]);
                else
                {
                     if (cluster.shards[i].rigid.objectType == ObjectType.MeshRoot)
                        meshRootShards.Add (cluster.shards[i]);
                     else if (cluster.shards[i].rigid.objectType == ObjectType.Mesh)
                        meshRigidShards.Add (cluster.shards[i]);
                }

            // Backup original layer in case shard will change layer after activation
            RFActivation.BackupActivationLayer (this);
        }

        // Set physics properties
        void SetShardsSimulationType()
        {
            // Set sim type in case of change
            for (int i = 0; i < rigidRootShards.Count; i++)
                rigidRootShards[i].sm = simulationType;
            for (int i = 0; i < meshRootShards.Count; i++)
                meshRootShards[i].sm = meshRootShards[i].rigid.simulationType;
            for (int i = 0; i < meshRigidShards.Count; i++)
                meshRigidShards[i].sm = meshRigidShards[i].rigid.simulationType;
        }

        // Set parent list for all shards
        void SetParentList()
        {
            parentList = new Transform[cluster.shards.Count];
            for (int i = 0; i < cluster.shards.Count; i++)
                parentList[i] = cluster.shards[i].tm.parent;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Add shards
        /// /////////////////////////////////////////////////////////
        
        // Add shard without rigid component
        void AddShard(Transform shardTm, RayfireRigid rigid = null)
        {
            // Has children
            if (shardTm.childCount > 0)
                return;
            
            // Create shard
            RFShard shard = new RFShard (shardTm);

            // Filter
            if (ShardFilter(shard, this) == true)
            {
                // Set host rigid
                shard.rigid = rigid;

                // Collect
                cluster.shards.Add (shard);
            }
        }
        
        // Add shard with rigid component
        void AddShardWithRigid(RayfireRigid rigid)
        {
            // Disable runtime demolition TODO temp
            rigid.demolitionType = DemolitionType.None;
            
            // Init 
            rigid.Initialize();

            // Stop coroutines. Rigid Root runs own coroutines 
            rigid.StopAllCoroutines();

            // TODO check for exclude and missing components
            
            // Collect
            cluster.shards.Add (new RFShard (rigid));
        }

        /// /////////////////////////////////////////////////////////
        /// Collider ops
        /// /////////////////////////////////////////////////////////
        
        // Define collider
        void SetColliders()
        {
            // Add colliders if RigidRoot not cached
            if (cached == false)
            {
                collidersList = new List<Collider>();
                for (int i = 0; i < rigidRootShards.Count; i++)
                    RFPhysic.SetRigidRootCollider (this, physics, rigidRootShards[i]);
                for (int i = 0; i < meshRootShards.Count; i++)
                    RFPhysic.SetRigidRootCollider (this, meshRootShards[i].rigid.physics, meshRootShards[i]);
                collidersHash = new HashSet<Collider>(collidersList);
            }
        }
        
        // Define components
        void SetCollidersMaterial()
        {
            // Set material solidity and destructible
            physics.solidity     = physics.Solidity;
            physics.destructible = physics.Destructible;
            
            // Set physics material if not defined by user
            if (physics.material == null)
                physics.material = physics.PhysMaterial;
            
            // Set Collider material
            for (int i = 0; i < rigidRootShards.Count; i++)
                RFPhysic.SetColliderMaterial (physics, rigidRootShards[i]);
            for (int i = 0; i < meshRootShards.Count; i++)
                RFPhysic.SetColliderMaterial (meshRootShards[i].rigid.physics, meshRootShards[i]);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activation ops
        /// /////////////////////////////////////////////////////////
        
        // Setup inactive shards
        public void SetInactiveList()
        {
            if (inactiveShards == null)
                inactiveShards = new List<RFShard>();
            else
                inactiveShards.Clear();
            for (int s = 0; s < cluster.shards.Count; s++)
            {
                if (cluster.shards[s].InactiveOrKinematic == true)
                {
                    cluster.shards[s].pos = cluster.shards[s].tm.position;
                    cluster.shards[s].los = cluster.shards[s].tm.localPosition;
                    inactiveShards.Add (cluster.shards[s]);
                }
            }
        }
        
        // Set unyielding shards. Should be After collider set and Before SetPhysics because Changes simType.
        void SetUnyielding()
        {
            // Set by rigid root
            for (int i = 0; i < rigidRootShards.Count; i++)
            {
                rigidRootShards[i].uny = activation.uny;
                rigidRootShards[i].act = activation.atb;
            }
            
            // Set by rigid root
            for (int i = 0; i < meshRootShards.Count; i++)
            {
                meshRootShards[i].uny = meshRootShards[i].rigid.activation.uny;
                meshRootShards[i].act = meshRootShards[i].rigid.activation.atb;
            }

            // Set by uny components
            if (HasUny == true)
            {
                for (int i = 0; i < unyList.Length; i++)
                {
                    unyList[i].GetRigidRootUnyShardList (this);
                    unyList[i].SetRigidRootUnyShardList();
                }
            }
        }
        
        // Start all coroutines
        public void StartAllCoroutines()
        {
            // Stop if static
            if (simulationType == SimType.Static)
                return;
            
            // Inactive
            if (gameObject.activeSelf == false)
                return;
            
            // Prevent physics cors
            if (physics.exclude == true)
                return;
            
            // Init inactive every frame update coroutine TODO activation check per shard properties
            if (inactiveShards.Count > 0)
                StartCoroutine (activation.InactiveCor(this));
            
            // Offset fade
            if (offsetFadeShards.Count > 0)
            {
                fading.offsetEnum = RFFade.FadeOffsetCor (this);
                StartCoroutine (fading.offsetEnum);
            }
            
            // All coroutines are running
            corState = true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Children change
        /// /////////////////////////////////////////////////////////
        
        /*
         [NonSerialized] bool   childrenChanged;
         
        // Children change
        void OnTransformChildrenChanged()
        {
            childrenChanged = true; 
        }
        
        // Connectivity check cor
        IEnumerator ChildrenCor()
        {
            // Stop if running 
            if (childrenCorState == true)
                yield break;
            
            // Set running state
            childrenCorState = true;
            
            bool checkChildren = true;
            while (checkChildren == true)
            {
                // Get not connected groups
                if (childrenChanged == true)
                    connectivityCheckNeed = true;

                yield return null;
            }
            
            // Set state
            childrenCorState = false;
        }
        */
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Copy rigid root properties to rigid
        public void CopyPropertiesTo (RayfireRigid toScr)
        {
            // Set self as rigidRoot
            toScr.rigidRoot = this;

            // Object type
            toScr.objectType     = ObjectType.ConnectedCluster;
            toScr.demolitionType = DemolitionType.None;
            toScr.simulationType = SimType.Dynamic;
            
            // Copy physics
            toScr.physics.CopyFrom (physics);
            toScr.activation.CopyFrom (activation);
            toScr.limitations.CopyFrom (demolition.limitations);
            // toScr.meshDemolition.CopyFrom (demolition.meshDemolition);
            toScr.clusterDemolition.CopyFrom (demolition.clusterDemolition);
            // toScr.materials.CopyFrom (demolition.materials);
            
            // toScr.damage.CopyFrom (damage);
            toScr.fading.CopyFrom (fading);
            toScr.reset.CopyFrom (reset, toScr.objectType);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check if root is nested cluster
        static bool IsNestedCluster (Transform trans)
        {
            for (int c = 0; c < trans.childCount; c++)
                if (trans.GetChild (c).childCount > 0)
                    return true;
            return false;
        }
        
        // Shard filter
        static bool ShardFilter(RFShard shard, RayfireRigidRoot scr)
        {
            // No mesh filter
            if (shard.mf == null)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log (strRoot + shard.tm.name + " has no MeshFilter. Shard won't be simulated.", shard.tm.gameObject);
                scr.destroyShards.Add (shard);
                return false;
            }

            // No mesh
            if (shard.mf.sharedMesh == null)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log (strRoot + shard.tm.name + " has no mesh. Shard won't be simulated.", shard.tm.gameObject);
                scr.destroyShards.Add (shard);
                return false;
            }
            
            // Low vert check
            if (shard.mf.sharedMesh.vertexCount <= 3)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log (strRoot + shard.tm.name + " has 3 or less vertices. Shard can't get Mesh Collider and won't be simulated.", shard.tm.gameObject);
                scr.destroyShards.Add (shard);
                return false;
            }
            
            // Size check
            if (RayfireMan.colliderSizeStatic > 0)
            {
                if (shard.sz < RayfireMan.colliderSizeStatic)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (strRoot + shard.tm.name + " is very small and won't be simulated.", shard.tm.gameObject);
                    scr.destroyShards.Add (shard);
                    return false;
                }
            }

            // Optional coplanar check
            if (scr.physics.pc == true && shard.mf.sharedMesh.vertexCount < RayfireMan.coplanarVertLimit)
            {
                if (RFShatterAdvanced.IsCoplanar (shard.mf.sharedMesh, RFShatterAdvanced.planarThreshold) == true)
                {
                    if (RayfireMan.debugStatic == true)
                        Debug.Log (strRoot + shard.tm.name + " has planar low poly mesh. Shard can't get Mesh Collider and won't be simulated.", shard.tm.gameObject);
                    scr.destroyShards.Add (shard);
                    return false;
                }
            }

            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        public bool HasClusters { get { return clusters != null && clusters.Count > 0; } }
        public bool HasDebris { get { return debrisList != null && debrisList.Count > 0; } }
        public bool HasDust   { get { return dustList != null && dustList.Count > 0; } }
        public bool HasUny  { get { return unyList != null && unyList.Length > 0; } }
        
        public void CollideTest()
        {
            /*
            List<Transform> tmList = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
                tmList.Add (transform.GetChild (i));

            List<Collider> colliders = new List<Collider>();
            foreach (var tm in tmList)
            {
                Collider col = tm.GetComponent<Collider>();
                if (col == null)
                {
                    col                          = tm.gameObject.AddComponent<MeshCollider>();
                    (col as MeshCollider).convex = true;
                }
                colliders.Add (col);
            }

            */

           // Physics.Simulate (0.01f);
            // Physics.autoSimulation = true;

            // Physics.autoSyncTransforms = false;
            
            // https://forum.unity.com/threads/physics-simulate-for-a-single-object-possible.614404/
            // https://forum.unity.com/threads/separating-physics-scenes.597697/
            // https://stackoverflow.com/questions/50693509/can-we-detect-when-a-rigid-body-collides-using-physics-simulate-in-unity
        }
    }
}
