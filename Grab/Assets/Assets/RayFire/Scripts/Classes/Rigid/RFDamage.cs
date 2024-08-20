using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RayFire
{
    [Serializable]
    public class RFDamage
    {
        [FormerlySerializedAs ("enable")]
        public bool  en;
        [FormerlySerializedAs ("maxDamage")]
        public float max;
        [FormerlySerializedAs ("currentDamage")]
        public float cur;
        [FormerlySerializedAs ("collect")]
        public bool  col;
        [FormerlySerializedAs ("multiplier")]
        public float mlt;
        [FormerlySerializedAs ("toShards")]
        public bool  shr;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDamage()
        {
            InitValues();
        }
        
        // Reset
        public void InitValues()
        {
            en  = false;
            max = 100f;
            cur = 0f;
            col = false;
            mlt = 1f;
            shr = false;
        }
        
        // Reset
        public void LocalReset()
        {
            cur = 0f;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
        }
        
        // Copy from
        public void CopyFrom(RFDamage source)
        {
            en  = source.en;
            max = source.max;
            col = source.col;
            mlt = source.mlt;
            shr = source.shr;
            
            LocalReset();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////     
        
        // Apply damage
        public static bool ApplyDamage (RayfireRigid scr, float value, Vector3 point, float radius, Collider collider)
        {
            // Initialize if not
            if (scr.initialized == false)
                scr.Initialize();
            
            // Already demolished or should be
            if (scr.limitations.demolished == true || scr.limitations.demolitionShould == true)
                return false;

            // Apply damage and get demolition state
            bool demolitionState = ApplyTo (scr, value, point, radius, collider);
            
            // TODO demolish first to activate only demolished fragments AND activate if object can't be demolished
            // TODO avoid demolition by radius in case of shard damage with radius
            
            // Set demolition info
            if (demolitionState == true)
            {
                // Demolition available check
                if (scr.DemolitionState() == false)
                    return false;
                
                // Set damage position
                scr.limitations.contactVector3     = point;
                scr.clusterDemolition.damageRadius = radius;

                // Set small radius for cluster demolition by gun with 0 radius. IMPORTANT
                if (radius == 0)
                    scr.clusterDemolition.damageRadius = 0.05f;

                // Demolish object
                scr.limitations.demolitionShould = true;
                
                
                
                // Demolish
                scr.Demolish();
                
                // Mesh Was demolished
                if (scr.limitations.demolished == true)
                    return true;
                
                // Cluster was 
                if (scr.IsCluster == true)
                    if (scr.HasFragments == true)
                        return true;
            }
            
            // Check for activation
            if (scr.activation.dmg > 0 && scr.damage.cur > scr.activation.dmg)
                scr.Activate();
            
            return false;
        }   
       
        // Add damage
        public static bool ApplyTo(RayfireRigid scr, float value, Vector3 point, float radius = 0f, Collider collider = null)
        {
            // Apply damage to connected cluster per shard level
            if (scr.objectType == ObjectType.ConnectedCluster && scr.damage.shr == true)
                return ApplyToShard (scr, value, point, radius, collider);
            
            // Apply to rigid itself
            return ApplyToRigid (scr, value);
        }
        
        /// /////////////////////////////////////////////////////////
        /// To rigid
        /// /////////////////////////////////////////////////////////     
        
        // Add damage to Rigid
        public static bool ApplyToRigid(RayfireRigid scr, float damageValue)
        {
            // Add damage
            scr.damage.cur += damageValue;
            
            // Check
            if (scr.damage.en == true && scr.damage.cur >= scr.damage.max)
                return true;

            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// To shards
        /// /////////////////////////////////////////////////////////     

        // Add damage to shard
        public static bool ApplyToShard (RayfireRigid scr, float value, Vector3 point, float radius, Collider collider)
        {
            // Add damage by collider
            if (collider != null)
                return ApplyToShard (scr, value, collider);

            // Add damage by radius
            if (radius > 0)
                return ApplyToShards (scr, value, point, radius);
            
            return false;
        }

        // Add damage to shard by collider
        public static bool ApplyToShard(RayfireRigid scr, float value, Collider collider)
        {
            for (int i = 0; i < scr.clusterDemolition.cluster.shards.Count; i++)
                if (scr.clusterDemolition.cluster.shards[i].col == collider)
                {
                    // Apply damage to shard
                    scr.clusterDemolition.cluster.shards[i].dm += value;

                    // Flag damaged shard
                    if (scr.clusterDemolition.cluster.shards[i].dm > scr.damage.max)
                        return true;
                    
                    // Skip checking whole list
                    break;
                }
            
            return false;
        }
        
        // Add damage to shards in radius
        public static bool ApplyToShards(RayfireRigid scr, float value, Vector3 point, float radius)
        {
            //  bool              maxReached    = false;
            int               mask          = scr.gameObject.layer; // TODO use mask
            Collider[]        colliders     = Physics.OverlapSphere (point, radius);
            HashSet<Collider> collidersHash = new HashSet<Collider>(colliders);

            // Apply damage to shard
            for (int i = 0; i < scr.clusterDemolition.cluster.shards.Count; i++)
                if (collidersHash.Contains (scr.clusterDemolition.cluster.shards[i].col) == true)
                    scr.clusterDemolition.cluster.shards[i].dm += value;
            
            // Flag damaged shard
            for (int i = 0; i < scr.clusterDemolition.cluster.shards.Count; i++)
                if (scr.clusterDemolition.cluster.shards[i].dm > scr.damage.max)
                    return true;
            
            return false;
        }
    }
}