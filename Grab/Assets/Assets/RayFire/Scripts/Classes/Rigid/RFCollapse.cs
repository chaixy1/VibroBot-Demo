using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFCollapse
    {
        public enum RFCollapseType
        {
            ByArea = 1,
            BySize = 3,
            Random = 5
        }
        
        public RFCollapseType type;
        public int   start;
        public int   end;
        public int   steps;
        public float duration;
        public int   var;
        public int   seed;

        [NonSerialized] public bool inProgress;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFCollapse()
        {
            type     = RFCollapseType.ByArea;
            start    = 0;
            end      = 75;
            steps    = 10;
            duration = 15f;
            var      = 0;
            seed     = 0;
        }

        /// /////////////////////////////////////////////////////////
        /// Rigid Collapse
        /// /////////////////////////////////////////////////////////

        // Start collapse
        public static void StartCollapse(RayfireRigid scr)
        {
            // Not initialized
            if (scr.initialized == false)
                return;
            
            // Already running
            if (scr.clusterDemolition.collapse.inProgress == true)
                return;
            
            // Not enough shards
            if (scr.clusterDemolition.cluster.shards.Count <= 1)
                return;
            
            // Set random seed
            if (scr.clusterDemolition.collapse.seed == 0)
                scr.clusterDemolition.collapse.seed = Random.Range (1, 99);

            // Start collapse cor
            scr.StartCoroutine(scr.clusterDemolition.collapse.CollapseCor (scr));
        }

        // Start collapse coroutine
        IEnumerator CollapseCor (RayfireRigid scr)
        {
            // Wait time
            WaitForSeconds wait = new WaitForSeconds (duration/steps);

            // Set state
            inProgress = true;
            
            // Iterate collapse
            float step = (end - start) / (float)steps;
            for (int i = 0; i < steps + 1; i++)
            {
                // Stop
                if (inProgress == false)
                    break;
                
                float percentage = start + step * i;
                if (type == RFCollapseType.ByArea)
                    AreaCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.BySize)
                    SizeCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.Random)
                    RandomCollapse (scr, (int)percentage);
                yield return wait;
            }
            
            // Set state
            inProgress = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connectivity Collapse
        /// /////////////////////////////////////////////////////////
        
        // Start collapse
        public static void StartCollapse(RayfireConnectivity scr)
        {
            // Already running
            if (scr.collapse.inProgress == true)
                return;

            // Not enough shards
            if (scr.cluster.shards.Count <= 1)
                return;
            
            // Set random seed
            if (scr.collapse.seed == 0)
                scr.collapse.seed = Random.Range (1, 99);

            // Start collapse cor
            scr.StartCoroutine(scr.collapse.CollapseCor (scr));
        }
        
        // Stop collapse
        public static void StopCollapse (RayfireConnectivity scr)
        {
            scr.collapse.inProgress = false;
        }

        // Start collapse coroutine
        IEnumerator CollapseCor (RayfireConnectivity scr)
        {
            // Wait time
            WaitForSeconds wait = new WaitForSeconds (duration/steps);
            
            // Set running state
            inProgress = true;
            
            // Iterate collapse
            float step = (end - start) / (float)steps;
            for (int i = 0; i < steps + 1; i++)
            {
                // Stop
                if (inProgress == false)
                    break;

                // Init collapse
                float percentage = start + step * i;
                if (type == RFCollapseType.ByArea)
                    AreaCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.BySize)
                    SizeCollapse (scr, (int)percentage);
                else if (type == RFCollapseType.Random)
                    RandomCollapse (scr, (int)percentage);
                
                yield return wait;
            }
            
            // Set state
            inProgress = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connected Cluster Collapse
        /// /////////////////////////////////////////////////////////

        // Collapse in percents
        public static void AreaCollapse (RayfireRigid scr, int areaPercentage)
        {
            areaPercentage = Mathf.Clamp (areaPercentage, 0, 100);
            AreaCollapse (scr, Mathf.Lerp (scr.clusterDemolition.cluster.minimumArea, scr.clusterDemolition.cluster.maximumArea, areaPercentage / 100f));
        }
        
        // Collapse in percents
        public static void SizeCollapse (RayfireRigid scr, int sizePercentage)
        {
            sizePercentage = Mathf.Clamp (sizePercentage, 0, 100);
            SizeCollapse (scr, Mathf.Lerp (scr.clusterDemolition.cluster.minimumSize, scr.clusterDemolition.cluster.maximumSize, sizePercentage / 100f));
        }
        
        // Break neib connection by shared are value and demolish cluster
        public static void AreaCollapse (RayfireRigid scr, float minAreaValue)
        {
            // Not initialized
            if (scr.initialized == false)
                return;
            
            // Value lower than last
            if (minAreaValue < scr.clusterDemolition.cluster.areaCollapse)
                return;

            // Set value
            scr.clusterDemolition.cluster.areaCollapse = minAreaValue;

            // Main cluster.
            int removed = RemNeibByArea (scr.clusterDemolition.cluster, minAreaValue, scr.clusterDemolition.collapse.var, scr.clusterDemolition.collapse.seed);
            if (removed > 0)
                CollapseCluster (scr);
        }

        // Break neib connection by size
        public static void SizeCollapse (RayfireRigid scr, float minSizeValue)
        {
             // Not initialized
             if (scr.initialized == false)
                 return;

             // Value lower than last
             if (minSizeValue < scr.clusterDemolition.cluster.sizeCollapse)
                 return;

             // Set value
             scr.clusterDemolition.cluster.sizeCollapse = minSizeValue;

             // Main cluster.
             int removed = RemNeibBySize (scr.clusterDemolition.cluster, minSizeValue, scr.clusterDemolition.collapse.var, scr.clusterDemolition.collapse.seed);
             if (removed > 0)
                 CollapseCluster (scr);
        }

        // Break neib connection randomly
        public static void RandomCollapse (RayfireRigid scr, int randomValue)
        {
            // Not initialized
            if (scr.initialized == false)
                return;
            
            // Value lower than last
            if (randomValue < scr.clusterDemolition.cluster.randomCollapse)
                return;

            // Set value
            scr.clusterDemolition.cluster.randomCollapse = randomValue;

            // Main cluster.
            int removed = RemNeibRandom (scr.clusterDemolition.cluster, randomValue, scr.clusterDemolition.collapse.seed);
            if (removed > 0)
                CollapseCluster (scr);
        }

        // Init collapse after connection loss
        static void CollapseCluster (RayfireRigid scr)
        {
            // Collect solo shards, remove from cluster, no need to reinit
            List<RFShard> detachShards = new List<RFShard>();
            RFCluster.DetachSoloShards (scr.clusterDemolition.cluster, detachShards);

            // Clear fragments in case of previous demolition
            if (scr.HasFragments == true)
                scr.fragments.Clear();

            // Dynamic cluster connectivity check, all clusters are equal, pick biggest to keep as original
            if (scr.simulationType == SimType.Dynamic || scr.simulationType == SimType.Sleeping)
            {
                // Check left cluster shards for connectivity and collect not connected child clusters. Should be before ClusterizeDetachShards
                RFCluster.ConnectivityCheck (scr.clusterDemolition.cluster);

                // Cluster is not connected. Set biggest child cluster shards to original cluster. Cant be 1 child cluster here
                RFCluster.ReduceChildClusters (scr.clusterDemolition.cluster);
            }

            // Kinematic/ Inactive cluster, Connectivity check if cluster has uny shards. Main cluster keeps all not activated
            else if (scr.simulationType == SimType.Kinematic || scr.simulationType == SimType.Inactive)
            {
                RFCluster.ConnectivityUnyCheck (scr.clusterDemolition.cluster);
            }
            
            // Init final cluster ops
            RFDemolitionCluster.PostDemolitionCluster (scr, detachShards);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connectivity Collapse
        /// /////////////////////////////////////////////////////////

        // Collapse in percents
        public static void AreaCollapse (RayfireConnectivity connectivity, int areaPercentage)
        {
            areaPercentage = Mathf.Clamp (areaPercentage, 0, 100);
            AreaCollapse (connectivity, Mathf.Lerp (connectivity.cluster.minimumArea, connectivity.cluster.maximumArea, areaPercentage / 100f));
        }
        
        // Collapse in percents
        public static void SizeCollapse (RayfireConnectivity connectivity, int sizePercentage)
        {
            sizePercentage = Mathf.Clamp (sizePercentage, 0, 100);
            SizeCollapse (connectivity, Mathf.Lerp (connectivity.cluster.minimumSize, connectivity.cluster.maximumSize, sizePercentage / 100f));
        }
        
        // Crumbling
        public static void AreaCollapse (RayfireConnectivity connectivity, float areaValue)
        {
            // Value lower than last
            if (areaValue < connectivity.cluster.areaCollapse)
                return;

            // Set value
            connectivity.cluster.areaCollapse = areaValue;
            
            // Main cluster
            int removed = RemNeibByArea (connectivity.cluster, areaValue, connectivity.collapse.var, connectivity.collapse.seed);
            if (removed > 0)
                connectivity.CheckConnectivity();
        }
        
        // Crumbling
        public static void SizeCollapse (RayfireConnectivity connectivity, float sizeValue)
        {
            // Value lower than last
            if (sizeValue < connectivity.cluster.sizeCollapse)
                return;
            
            // Set value
            connectivity.cluster.sizeCollapse = sizeValue;
            
            // Main cluster.
            int removed = RemNeibBySize (connectivity.cluster, sizeValue, connectivity.collapse.var, connectivity.collapse.seed);
            if (removed > 0)
                connectivity.CheckConnectivity();
        }

        // Crumbling
        public static void RandomCollapse (RayfireConnectivity connectivity, int randomPercentage)
        {
            // Clamp
            randomPercentage = Mathf.Clamp (randomPercentage, 0, 100);
            
            // Value lower than last
            if (randomPercentage < connectivity.cluster.randomCollapse)
                return;

            // Set value
            connectivity.cluster.randomCollapse = randomPercentage;

            // Main cluster.
            int removed = RemNeibRandom(connectivity.cluster, randomPercentage, connectivity.collapse.seed);
            if (removed > 0)
                connectivity.CheckConnectivity();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Neib removing
        /// /////////////////////////////////////////////////////////
        
        // Remove neibs by area
        static int RemNeibByArea (RFCluster cluster, float minArea, float variation, int seed)
        {
            int   removed = 0;
            float varMin  = 1f - variation / 100f;
            float varMax  = 1f + variation / 100f;
            int   count   = cluster.shards.Count;
            float finalArea;
            for (int s = 0; s < count; s++)
            {
                // Skip unyielding
                if (cluster.shards[s].uny == true)
                    continue;

                // Skip if too much neibs
                // if (minNeib > 0 && cluster.shards[s].neibShards.Count > minNeib) continue;
                
                // Check neibs
                for (int n = cluster.shards[s].neibShards.Count - 1; n >= 0; n--)
                {
                    // Define variation
                    finalArea = cluster.shards[s].nArea[n];
                    if (variation > 0)
                    {
                        // Set random state same for shard
                        Random.InitState (cluster.shards[s].id + cluster.shards[s].neibShards[n].id + seed);

                        // Random mult
                        finalArea *= Random.Range(varMin, varMax);
                    }
                    
                    if (finalArea < minArea)
                    {
                        // Remove self in neib's neib list
                        for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                            {
                                
                                cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }
                       
                        // Remove in self
                        cluster.shards[s].RemoveNeibAt (n);
                        removed++;
                    }
                }
            }
            return removed;
        }
        
        // Remove neibs by area
        static int RemNeibByAreaB (RFCluster cluster, float minArea, float variation, int seed)
        {
            int   removed = 0;
            float varMin  = 1f - variation / 100f;
            float varMax  = 1f + variation / 100f;
            int   count   = cluster.shards.Count;
            for (int s = 0; s < count; s++)
            {
                // Skip unyielding
                if (cluster.shards[s].uny == true)
                    continue;

                // Skip if too much neibs
                // if (minNeib > 0 && cluster.shards[s].neibShards.Count > minNeib) continue;
                
                // Check neibs
                for (int n = cluster.shards[s].neibShards.Count - 1; n >= 0; n--)
                {
                    // Define variation
                    float finalArea = cluster.shards[s].nArea[n];
                    if (variation > 0)
                    {
                        // Set random state same for shard
                        Random.InitState (cluster.shards[s].id + cluster.shards[s].neibShards[n].id + seed);

                        // Random mult
                        finalArea *= Random.Range(varMin, varMax);
                    }
                    
                    if (finalArea < minArea)
                    {
                        // Remove self in neib's neib list
                        for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                            {
                                cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }
                       
                        // Remove in self
                        cluster.shards[s].RemoveNeibAt (n);
                        removed++;
                    }
                }
            }
            return removed;
        }
        
        // Remove neibs by size
        static int RemNeibBySize (RFCluster cluster, float minSize, float variation, int seed)
        {
            int   removed = 0;
            float varMin  = 1f - variation / 100f;
            float varMax  = 1f + variation / 100f;
            int   count   = cluster.shards.Count;
            for (int s = 0; s < count; s++)
            {
                // Skip unyielding
                if (cluster.shards[s].uny == true)
                    continue;
                
                // Skip if too much neibs
                // if (minNeib > 0 && cluster.shards[s].neibShards.Count > minNeib) continue;

                // Define variation
                float finalSize = cluster.shards[s].sz;
                if (variation > 0)
                {
                    // Set random state same for shard
                    Random.InitState (cluster.shards[s].id + seed);

                    // Random mult
                    finalSize *= Random.Range (varMin, varMax);
                }

                // Check neibs
                if (finalSize < minSize)
                {
                    for (int n = cluster.shards[s].neibShards.Count - 1; n >= 0; n--)
                    {
                        // Remove self in neib's neib list
                        for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                            {
                                cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }
                    }
                        
                    // Remove in self
                    cluster.shards[s].ClearNeib();
                    removed++;
                }
            }
            return removed;
        }
        
        // Remove neibs by area
        static int RemNeibRandom (RFCluster cluster, int percent, int seed)
        {
            int removed = 0;
            int count   = cluster.shards.Count;
            for (int s = 0; s < count; s++)
            {
                // Skip unyielding
                if (cluster.shards[s].uny == true)
                    continue;
                
                // Skip if too much neibs
                // if (minNeib > 0 && cluster.shards[s].neibShards.Count > minNeib) continue;
                
                // Check neibs
                for (int n = cluster.shards[s].neibShards.Count - 1; n >= 0; n--)
                {
                    // Set random state for same pairs
                    Random.InitState (cluster.shards[s].id + cluster.shards[s].neibShards[n].id + seed);
                    
                    // Random percentage check
                    if (Random.Range (0, 100) < percent)
                    {
                        // Remove self in neib's neib list
                        for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
                        {
                            if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                            {
                                cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                                break;
                            }
                        }
                       
                        // Remove in self
                        cluster.shards[s].RemoveNeibAt (n);
                        removed++;
                    }
                }
            }
            return removed;
        }

        // Remove connection in cluster in s shard and for its n neib 
        static void RemoveConnection(RFCluster cluster, int s, int n)
        {
            // Remove self in neib's neib list
            for (int i = cluster.shards[s].neibShards[n].neibShards.Count - 1; i >= 0; i--)
            {
                if (cluster.shards[s].neibShards[n].neibShards[i] == cluster.shards[s])
                {
                    cluster.shards[s].neibShards[n].RemoveNeibAt (i);
                    break;
                }
            }
                       
            // Remove in self
            cluster.shards[s].RemoveNeibAt (n);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Range Data
        /// /////////////////////////////////////////////////////////
        
        // Set range for area and size
        public static void SetRangeData (RFCluster cluster, int perc = 0)
        {
            if (cluster.shards.Count == 0)
                return;

            // Start values
            cluster.maximumSize = cluster.shards[0].sz;
            cluster.minimumSize = cluster.shards[0].sz;
            cluster.maximumArea = 0f;
            cluster.minimumArea = 10000f;
            cluster.randomCollapse = perc;

            // Loop shards
            for (int i = 0; i < cluster.shards.Count; i++)
            {
                if (cluster.shards[i].sz > cluster.maximumSize)
                    cluster.maximumSize = cluster.shards[i].sz;
                if (cluster.shards[i].sz < cluster.minimumSize)
                    cluster.minimumSize = cluster.shards[i].sz;

                for (int j = 0; j < cluster.shards[i].nArea.Count; j++)
                {
                    if (cluster.shards[i].nArea[j] > cluster.maximumArea)
                        cluster.maximumArea = cluster.shards[i].nArea[j];
                    
                    if (cluster.shards[i].nArea[j] < cluster.minimumArea)
                        cluster.minimumArea = cluster.shards[i].nArea[j];
                }
            }

            // Fix
            if (cluster.minimumArea < 0.001f)
                cluster.minimumArea = 0f;
            
            cluster.areaCollapse = cluster.minimumArea;
            cluster.sizeCollapse = cluster.minimumSize;
        }

        // Copy Range data for runtime clusters
        public static void CopyRangeData(RFCluster cluster, RFCluster source)
        {
            cluster.maximumSize    = source.maximumSize;
            cluster.minimumSize    = source.minimumSize;
            cluster.maximumArea    = source.maximumArea;
            cluster.minimumArea    = source.minimumArea;
            cluster.randomCollapse = source.randomCollapse;
            cluster.areaCollapse   = cluster.minimumArea;
            cluster.sizeCollapse   = cluster.minimumSize;
        }
    }
}