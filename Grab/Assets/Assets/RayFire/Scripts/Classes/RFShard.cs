using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFShard : IComparable<RFShard>
    {
        // Main
        public int     id;
        public float   sz;  // Size
        public bool    uny; // Unyielding
        public bool    act; // Activatable
        public Bounds  bnd;
        public SimType sm;
        public float   dm;
        
        // Neib info
        public int         nAm;   // Initial amount of neibs
        public List<int>   nIds;
        public List<float> nArea;
        
        // Stress info
        public List<int>   sIds; // Shards id which support this shard
        public List<float> nSt;  // Dir; // 0.0 - UP; 0.5 - SIDE; 1.0 - DOWN; // Rat;  // < 1 - Neib less; > 1 - Neib bigger
        public float       sSt;  // Size stress multiplier
        
        // Components
        public Transform    tm;
        public MeshFilter   mf;
        public Collider     col;
        public Rigidbody    rb;
        public RayfireRigid rigid;
        
        // Non Serialized
        [NonSerialized] public Quaternion       rot; 
        [NonSerialized] public Vector3          scl;
        [NonSerialized] public Vector3          pos;  // global position.
        [NonSerialized] public Vector3          los;  // local position. Used for Local offset activation
        [NonSerialized] public float            m;    // Mass, calculates once
        [NonSerialized] public int              lb;   // Layer backup to restore if shard has activation layer
        [NonSerialized] public int              fade; // 1-Living, 2-Fading, 3-Faded
        [NonSerialized] public float            fo;   // Fade offset distance
        [NonSerialized] public List<RFTriangle> tris;
        [NonSerialized] public List<RFFace>     poly;
        [NonSerialized] public List<RFShard>    neibShards;
        [NonSerialized] public RFCluster        cluster;
        [NonSerialized] public bool             check;
        [NonSerialized] public bool[]           sCheck;

        /*
        [NonSerialized] public float v;   // Velocity activation
        [NonSerialized] public float o;   // Offset activation,
        [NonSerialized] public int   imp; // Impact activation
        [NonSerialized] public int   acv; // Activator activation,
        */
        
        /// Static
        public static float neibPosThreshold  = 0.01f;
        public static float neibAreaThreshold = 0.01f;
        public static float bBoxSharedAreaDiv = 4f;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFShard ()
        {
            scl        = Vector3.one;
            sSt        = 1f;
        }
        
        // Constructor
        public RFShard (RFShard source)
        {
            // Main
            id  = source.id;
            sz  = source.sz;
            uny = source.uny;
            act = source.act;
            bnd = source.bnd;
            pos = source.pos;
            rot = source.rot;
            scl = source.scl;
            sm  = source.sm;
            los = source.los;
            
            // Neib
            nAm = source.nAm;
            if (source.nIds != null)
            {
                nIds = new List<int>(source.nIds.Count);
                for (int i = 0; i < source.nIds.Count; i++)
                    nIds.Add (source.nIds[i]);
               
                nArea = new List<float>(source.nArea.Count);
                for (int i = 0; i < source.nArea.Count; i++)
                    nArea.Add (source.nArea[i]);

                neibShards = new List<RFShard>(source.nArea.Count);
            }
            
            // TODO copy stress data
            
            // Components
            tm    = source.tm;
            col   = source.col;
            rb    = source.rb;
            rigid = source.rigid;
            mf    = source.mf;

            // Debug.Log (rigid);
            
            // IMPORTANT: Cluster, neibShards init after all shards copied
        }
        
        // Constructor
        public RFShard (Transform Tm)
        {
            // Get mesh filter
            mf  = Tm.GetComponent<MeshFilter>();
            tm  = Tm;
            pos = Tm.position;
            rot = Tm.rotation;
            scl = Tm.localScale;
            los = Tm.localPosition;
            
            // Set bounds
            Renderer mr = Tm.GetComponent<Renderer>();
            if (mr != null)
                bnd = mr.bounds;
            
            // TODO add property to expand bounds, consider small and big size objects
            bnd.Expand(0.01f);  
            
            sz = bnd.size.magnitude;
        }
        
        // Constructor
        public RFShard (RayfireRigid scr)
        {
            sz    = scr.limitations.bboxSize;
            uny   = scr.activation.uny;
            act   = scr.activation.atb;
            bnd   = scr.limitations.bound;
            pos   = scr.transform.position;
            rot   = scr.transform.rotation;
            scl   = scr.transform.localScale;
            los   = scr.transform.localPosition;
            sm    = scr.simulationType;
            tm    = scr.transform;
            mf    = scr.meshFilter;
            col   = scr.physics.meshCollider;
            rb    = scr.physics.rigidBody;
            rigid = scr;
        }
        
        // Compare by size
        public int CompareTo(RFShard otherShard)
        {
            if (sz > otherShard.sz)
                return -1;
            if (sz < otherShard.sz)
                return 1;
            return 0;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set Mesh data
        /// /////////////////////////////////////////////////////////

        // Set Triangles and Faces data
        public static void SetMeshData (List<RFShard> shards, ConnectivityType type)
        {
            if (type != ConnectivityType.ByBoundingBox)
                for (int i = 0; i < shards.Count; i++)
                    RFTriangle.SetTriangles(shards[i]);
            if (type == ConnectivityType.ByPolygons || type == ConnectivityType.ByBoundingBoxAndPolygons)
                for (int i = 0; i < shards.Count; i++)
                    RFFace.SetPolys(shards[i]);
        }
        
        // Set Triangles and Faces data
        public static void SetMeshData (RFShard shard, ConnectivityType type)
        {
            if (type != ConnectivityType.ByBoundingBox)
                RFTriangle.SetTriangles(shard);
            if (type == ConnectivityType.ByPolygons || type == ConnectivityType.ByBoundingBoxAndPolygons)
                RFFace.SetPolys(shard);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set Shards
        /// /////////////////////////////////////////////////////////

        // Prepare shards. Set bounds, set neibs
        public static void SetShards(RFCluster cluster, ConnectivityType connectivity, bool setRigid = false)
        {
            // Get all children tms
            Transform[] tmList = new Transform[cluster.tm.childCount];
            for (int i = 0; i < cluster.tm.childCount; i++)
                tmList[i] = cluster.tm.GetChild(i);
            
            // Connected cluster with deep children TODO: reset empty roots.
            // List<Transform> tmList = cluster.tm.GetComponentsInChildren<Transform>().ToList();
            
            // Get child shards
            SetShardsByTransformList (cluster, tmList, connectivity, setRigid);
        }
        
        // Prepare shards. Set bounds, set neibs. For Connected Cluster
        public static void SetShardsByTransformList(RFCluster cluster, Transform[] tmList, ConnectivityType connectivity, bool setRigid = false)
        {
            cluster.shards = new List<RFShard>(tmList.Length);
            for (int i = 0; i < tmList.Length; i++)
            {
                // Create new shard
                RFShard shard = new RFShard(tmList[i]);
                
                // Child has no mesh
                if (shard.mf == null || shard.mf.sharedMesh == null)
                    continue;

                shard.id      = i;
                shard.cluster = cluster;
                
                // Set mesh data
                SetMeshData (shard, connectivity);

                // Collect shard
                cluster.shards.Add(shard);
            }
            
            // Set rigid component
            if (setRigid == true)
                for (int i = 0; i < cluster.shards.Count; i++)
                    cluster.shards[i].rigid = cluster.shards[i].tm.GetComponent<RayfireRigid>();
        }

        // Prepare shards. Set bounds, set neibs. For Mesh Root
        public static void SetShardsByRigidList(RFCluster cluster, List<RayfireRigid> rigids, ConnectivityType connectivity)
        {
            for (int i = 0; i < rigids.Count; i++)
            {
                // Create new shard
                RFShard shard = new RFShard(rigids[i]);
                shard.cluster = cluster;
                shard.id      = i;
                
                // Set mesh data
                SetMeshData (shard, connectivity);
                
                // Collect shard
                cluster.shards.Add(shard);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Neibs
        /// /////////////////////////////////////////////////////////
        
        // Get shared area with another shard TODO add pos difference check
        float NeibAreaByPoly(RFShard otherShard)
        {
            float areaDif;
            float area = 0f;
            for (int i = 0; i < poly.Count; i++)
            {
                for (int j = 0; j < otherShard.poly.Count; j++)
                {
                    // Area check
                    areaDif = Mathf.Abs (poly[i].area - otherShard.poly[j].area);
                    if (areaDif < neibAreaThreshold)
                    {
                        // Normal check
                        if (poly[i].normal == -otherShard.poly[j].normal)
                        {
                            area += poly[i].area;
                            break;
                        }
                    }
                }
            }

            return area;
        }
        
        // Get shared area with another shard
        float NeibAreaByTris(RFShard otherShard)
        {
            float posDif;
            float areaDif;
            float area = 0f;
            for (int i = 0; i < tris.Count; i++)
            {
                for (int j = 0; j < otherShard.tris.Count; j++)
                {
                    // Area check
                    areaDif = Mathf.Abs (tris[i].area - otherShard.tris[j].area);
                    if (areaDif < neibAreaThreshold)
                    {
                        // Position check
                        posDif = Vector3.Distance (tris[i].pos, otherShard.tris[j].pos);
                        if (posDif < neibPosThreshold)
                        {
                            area += tris[i].area;
                            break;
                        }
                    }
                }
            }

            return area;
        }
        
        // Set shard neibs
        public static void SetShardNeibs(List<RFShard> shards, ConnectivityType type, float minArea = 0, float minSize = 0, int perc = 0, int seed = 0)
        {
            // Set list
            for (int i = 0; i < shards.Count; i++)
            {
                shards[i].neibShards = new List<RFShard>();
                shards[i].nArea = new List<float>();
                shards[i].nIds = new List<int>();
                shards[i].nAm = 0;
            }

            //float t1 = Time.realtimeSinceStartup;

            // Random of fixed connections seed
            if (seed > 0)
                Random.InitState (seed);
            
            // Set neib and area info
            float area;
            for (int i = 0; i < shards.Count; i++)
            {
                // Skip by size
                if (minSize > 0 && shards[i].sz < minSize)
                    continue;

                for (int s = 0; s < shards.Count; s++)
                {
                    // Skip itself
                    if (s == i)
                        continue;
                    
                    // Skip by size
                    if (minSize > 0 && shards[s].sz < minSize)
                        continue;
                    
                    // Set random state for same pair
                    if (perc > 0 && Random.Range (0, 100) < perc)
                        continue;

                    // Check if shard was not added as neib before
                    if (shards[s].nIds.Contains(shards[i].id) == false)
                    {
                        // Bounding box intersection check
                        if (shards[i].bnd.Intersects(shards[s].bnd) == true)
                        {
                            // Get areas
                            area = 0;
                            if (type == ConnectivityType.ByBoundingBox)
                                area = (shards[i].sz + shards[s].sz) / bBoxSharedAreaDiv;
                            else if (type == ConnectivityType.ByTriangles || type == ConnectivityType.ByBoundingBoxAndTriangles)
                                area = shards[i].NeibAreaByTris (shards[s]);
                            else if (type == ConnectivityType.ByPolygons || type == ConnectivityType.ByBoundingBoxAndPolygons)
                                area = shards[i].NeibAreaByPoly(shards[s]);

                            // Area still 0, get by bounds if ByBoundingBoxAndMesh
                            if (area == 0)
                                if (type == ConnectivityType.ByBoundingBoxAndTriangles || type == ConnectivityType.ByBoundingBoxAndPolygons)
                                    area = (shards[i].sz + shards[s].sz) / bBoxSharedAreaDiv;

                            // Skip low area neibs TODO filter after all connected, leave one biggest ??
                            if (minArea > 0 && area < minArea)
                                continue;
                            
                            // Collect
                            if (area > 0)
                            {
                                // Roundup for 3 digits after comma
                                area = (int)(area * 1000.0f) / 1000.0f;
                                
                                shards[i].neibShards.Add (shards[s]);
                                shards[i].nArea.Add (area);
                                shards[i].nIds.Add (shards[s].id);

                                shards[s].neibShards.Add (shards[i]);
                                shards[s].nArea.Add (area);
                                shards[s].nIds.Add (shards[i].id);
                            }
                        }
                    }
                }

                // Set original neib amount to know if neibs was removed
                shards[i].nAm = shards[i].nIds.Count;
            }
            
            //float t2 = Time.realtimeSinceStartup;
            //Debug.Log ("Time " + (t2 - t1));
            //Debug.Log ("Checks " + checks);
            
            // Clear triangles data
            if (type == ConnectivityType.ByTriangles || type == ConnectivityType.ByBoundingBoxAndTriangles)
                for (int i = 0; i < shards.Count; i++)
                    RFTriangle.Clear (shards[i]);
            if (type == ConnectivityType.ByPolygons || type == ConnectivityType.ByBoundingBoxAndPolygons)
                for (int i = 0; i < shards.Count; i++)
                    RFFace.Clear (shards[i]);
        }

        // Remove neib shards which are not in current cluster anymore
        public static void ReinitNeibs (List<RFShard> shards)
        {
            if (shards.Count > 0)
                
                // Remove detach shards from neib. Edit neib shards data
                for (int i = 0; i < shards.Count; i++)
                    
                    // Check very neib shard
                    for (int n = shards[i].neibShards.Count - 1; n >= 0; n--)
                        
                        // Neib shard was detached
                        if (shards[i].neibShards[n].cluster != shards[i].cluster)
                            shards[i].RemoveNeibAt (n);
        }

        // Clear neib lists
        public void RemoveNeibAt(int ind)
        {
            nIds.RemoveAt (ind);
            nArea.RemoveAt (ind);
            neibShards.RemoveAt (ind);

            if (StressState == true)
            {
                nSt.RemoveAt (ind*3+2);
                nSt.RemoveAt (ind*3+1);
                nSt.RemoveAt (ind*3);
            }
        }
        
        // Clear neib lists
        public void ClearNeib()
        {
            nIds.Clear();
            nArea.Clear();
            neibShards.Clear();

            if (StressState == true)
                nSt.Clear();
        }

        // Set positive id for shards for checks
        public static void SetUnchecked (List<RFShard> shards)
        {
            for (int i = 0; i < shards.Count; i++)
                shards[i].check = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Slice
        /// /////////////////////////////////////////////////////////
        
        // Get slice plane at middle of longest bound edge
        public static Plane GetSlicePlane (Bounds bound)
        {
            Vector3 normal;
            Vector3 size = bound.size;
            Vector3 point = bound.center;
            if (size.x >= size.y && size.x >= size.z)
                normal = Vector3.right;
            else if (size.y >= size.x && size.y >= size.z)
                normal = Vector3.up;
            else
                normal = Vector3.forward;
            return new Plane(normal, point);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Sort by distance
        /// /////////////////////////////////////////////////////////
        
        // Sort list by distance to point
        public static RFShard[] SortByDistanceToPoint(RFShard[] shards, Vector3 point, int amount)
        {
            int     shardsCount = shards.Length;
            float[] distances   = new float[shardsCount];
            for (int s = 1; s < shardsCount; s++)
                distances[s] = Vector3.Distance (point, shards[s].tm.position);
            Array.Sort (distances, shards);
            Array.Resize (ref shards, amount);
            return shards;
        }
        
        // Sort list by distance to point
        public static RFShard[] SortByDistanceToPlane(RFShard[] shards, Vector3 point, Vector3 normal, int amount)
        {
            int     shardsCount = shards.Length;
            float[] distances   = new float[shardsCount];
            Plane   plane       = new Plane(normal, point);
            for (int s = 1; s < shardsCount; s++)
                distances[s] = Math.Abs(plane.GetDistanceToPoint (shards[s].tm.position));
            Array.Sort (distances, shards);
            Array.Resize (ref shards, amount);
            return shards;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set Unyielding
        /// /////////////////////////////////////////////////////////
        
        // Check if cluster has unyielding shards
        public static bool UnyieldingByShard (List<RFShard> shards)
        {
            for (int i = 0; i < shards.Count; i++)
                if (shards[i].uny == true)
                    return true;
            return false;
        }
        
        // Check if cluster has unyielding shards
        public static bool AllUnyShards (List<RFShard> shards)
        {
            for (int i = 0; i < shards.Count; i++)
                if (shards[i].uny == false)
                    return false;
            return true;
        }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Has supported shards
        public bool StressState { get { return nSt != null && nSt.Count > 0; } }
        
        // Has supported shards
        public bool SupportState { get { return sIds.Count > 0; } }
        
        // Activatable type
        public bool InactiveOrKinematic { get { return (sm == SimType.Inactive || sm == SimType.Kinematic); } }
        
        // Check if visible
        public bool Visible { get {
                if (tm != null)
                {
                    MeshRenderer mr = tm.GetComponent<MeshRenderer>();
                    if (mr != null)
                        return mr.isVisible;
                }
                return false; 
        }}

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Get biggest cluster
        public static RFShard GetShardByCollider(List<RFShard> shards, Collider collider)
        {
            for (int i = 0; i < shards.Count; i++)
                if (shards[i].col == collider)
                    return shards[i];
            return null;
        }
        
        // Get biggest cluster
        public static List<RFShard> GetShardsByColliders(List<RFShard> shards, List<Collider> colliders)
        {
            List<RFShard>     colliderShards = new List<RFShard>();
            HashSet<Collider> collidersHash  = new HashSet<Collider>(colliders);
            for (int i = 0; i < shards.Count; i++)
                if (collidersHash.Contains (shards[i].col) == true)
                    colliderShards.Add (shards[i]);
            return colliderShards;
        }
    }
}

