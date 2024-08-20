using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    public class RFPoolingEmitter
    {
        public int  id;
        public bool enable;
        public int  cap;
        public int  rate;
        public bool skip;
        public bool reuse;
        public int  over;
        
        public bool                  empty;
        public bool                  need;
        public Transform             root;
        public Queue<ParticleSystem> queue;
        public List<Transform>       scripts;

        // Constructor
        RFPoolingEmitter (RFParticlePool pool)
        {
            id      = pool.id;
            enable  = pool.enable;
            cap     = pool.cap;
            rate    = pool.rate;
            skip    = pool.skip;
            reuse   = pool.reuse;
            over    = pool.over;
            empty   = false;
            need    = pool.enable; // In order to start pooling should be = enable
            queue   = new Queue<ParticleSystem>(cap);
            scripts = new List<Transform>();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set host / Demolition
        /// /////////////////////////////////////////////////////////
        
         // Init particles on demolition
        public static void SetHostDemolition(RayfireRigid rigid)
        { 
            // No frags. Reference demolition can create debris without fragments
            if (rigid.demolitionType != DemolitionType.ReferenceDemolition && rigid.HasFragments == false)
                return;

            // Create debris particles
            if (rigid.HasDebris == true)
                SetHostDemolitionDebris (rigid);
                
            // Create dust particles
            if (rigid.HasDust == true)
                SetHostDemolitionDust (rigid);

            // Detach child particles in case object has child particles and about to be deleted
            DetachParticles(rigid);
        }
        
         // Create debris particle system
        static void SetHostDemolitionDebris(RayfireRigid rigid)
        {
            // Has no fragments
            if (rigid.HasFragments == false)
                return;

            for (int f = 0; f < rigid.fragments.Count; f++)
            {
                // Create only for mesh rigid
                if (rigid.fragments[f].objectType != ObjectType.Mesh)
                    continue;
                
                if (rigid.fragments[f].HasDebris == true)
                {
                    for (int d = 0; d < rigid.fragments[f].debrisList.Count; d++)
                    {
                        // Demolition particle disabled
                        if (rigid.fragments[f].debrisList[d].onDemolition != true)
                            continue;
                        
                        // Filter by percentage
                        if (Random.Range(0, 100) > rigid.fragments[f].debrisList[d].limitations.percentage)
                            continue;
                    
                        // Set particle to fragment
                        SetHostDebris (
                            rigid.fragments[f].debrisList[d],
                            rigid.fragments[f].transForm,
                            rigid.fragments[f].meshFilter,
                            rigid.fragments[f].meshRenderer,
                            rigid.fragments[f].limitations.bboxSize);
                    }
                }
            }
        }
        
        // Create debris particle system
        static void SetHostDemolitionDust(RayfireRigid rigid)
        {
            // Has no fragments
            if (rigid.HasFragments == false)
                return;

            for (int f = 0; f < rigid.fragments.Count; f++)
            {
                // Create only for mesh rigid
                if (rigid.fragments[f].objectType != ObjectType.Mesh)
                    continue;
                
                if (rigid.fragments[f].HasDust == true)
                {
                    for (int d = 0; d < rigid.fragments[f].dustList.Count; d++)
                    {
                        // Demolition particle disabled
                        if (rigid.fragments[f].dustList[d].onDemolition != true)
                            continue;
 
                        // Filter by percentage
                        if (Random.Range(0, 100) > rigid.fragments[f].dustList[d].limitations.percentage)
                            continue;
                        
                        // Set particle to fragment
                        SetHostDust (
                            rigid.fragments[f].dustList[d],
                            rigid.fragments[f].transForm,
                            rigid.fragments[f].meshFilter,
                            rigid.fragments[f].meshRenderer,
                            rigid.fragments[f].limitations.bboxSize);
                    }
                }
            }
        }

        // Detach child particles in case object has child particles and about to be deleted
        static void DetachParticles(RayfireRigid rigid)
        {
            // Detach debris particle system if fragment was already demolished/activated before
            if (rigid.HasDebris == true) 
            {
                for (int i = 0; i < rigid.debrisList.Count; i++)
                {
                    if (rigid.debrisList[i].hostTm != null)
                    {
                        rigid.debrisList[i].hostTm.parent     = RayfireMan.inst.transForm;
                        rigid.debrisList[i].hostTm.localScale = Vector3.one;
                    }
                }
            }

            // Detach dust particle system if fragment was already demolished/activated before
            if (rigid.HasDust == true)
            {
                for (int i = 0; i < rigid.dustList.Count; i++)
                {
                    if (rigid.dustList[i].hostTm != null)
                    {
                        rigid.dustList[i].hostTm.parent     = RayfireMan.inst.transForm;
                        rigid.dustList[i].hostTm.localScale = Vector3.one;
                    }
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set host / Impact 
        /// /////////////////////////////////////////////////////////
        
        // Set Impact tm
        public static void SetHostImpact (List<RayfireDebris> debrisList, Vector3 impactPos, Vector3 impactNormal)
        {
            if (debrisList != null && debrisList.Count > 0)
                for (int i = 0; i < debrisList.Count; i++)
                    if (debrisList[i] != null)
                        if (debrisList[i].onImpact == true)
                            SetHostImpact (debrisList[i].pool, 
                                debrisList[i].limitations.visible, 
                                debrisList[i].emission.burstAmount,
                                debrisList[i].emission.burstVar,
                                impactPos, impactNormal);
        }
        
        // Set Impact tm
        public static void SetHostImpact (List<RayfireDust> dustList, Vector3 impactPos, Vector3 impactNormal)
        {
            if (dustList != null && dustList.Count > 0)
                for (int i = 0; i < dustList.Count; i++)
                    if (dustList[i] != null)
                        if (dustList[i].onImpact == true)
                            SetHostImpact (dustList[i].pool, 
                                dustList[i].limitations.visible, 
                                dustList[i].emission.burstAmount, 
                                dustList[i].emission.burstVar,
                                impactPos, impactNormal);
        }
        
        // Get particle from emitter pool and set to host
        static void SetHostImpact (RFParticlePool pool, bool visible, int burstAmount, int burstVar, Vector3 impactPos, Vector3 impactNormal)
        {
            // Skip if no pool particles available
            if (pool.emitter.ShouldSkip == true)
                return;
            
            // Visibility check
            if (VisibilityCheck(visible, Camera.current, impactPos) == false) 
                return;    
            
            // Set particle system to impact position without host
            ParticleSystem pSystem = pool.emitter.GetPoolObject(null);
            
            // Move and rotate at impact position
            pSystem.transform.position = impactPos;
            pSystem.transform.LookAt (impactPos + impactNormal);
            
            // Set host specific amount
            RFParticles.SetEmission(pSystem.emission, 0, burstAmount, burstVar);

            // Set host specific emitter shape if mesh
            RFParticles.SetShapeHemisphere(pSystem.shape);
   
            // Set Reuse, Activate and play
            ReuseActivatePlay (pSystem, pool.emitter);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set host / Activation / RigidRoot / Rigid 
        /// /////////////////////////////////////////////////////////
        
        // Init Shard particles on activation
        public static void SetHostRigidrootShardAct (RayfireRigidRoot rigidRoot, RFShard shard)
        {
            // RigidRoot shard
            if (shard.rigid == null)
            {
                // Create debris particles
                if (rigidRoot.HasDebris == true)
                    for (int i = 0; i < rigidRoot.debrisList.Count; i++)
                        if (rigidRoot.debrisList[i].onActivation == true)
                        {
                            // Filter by percentage
                            if (Random.Range(0, 100) > rigidRoot.debrisList[i].limitations.percentage)
                                continue;
                            
                            // Set host
                            SetHostDebris (rigidRoot.debrisList[i], shard.tm, shard.mf, null, shard.sz);
                            
                            // Collect particles to reset them
                            rigidRoot.particleList.Add (rigidRoot.debrisList[i].hostTm);
                        }
                
                // Create dust particles
                if (rigidRoot.HasDust == true)
                    for (int i = 0; i < rigidRoot.dustList.Count; i++)
                        if (rigidRoot.dustList[i].onActivation == true)
                        {
                            // Filter by percentage
                            if (Random.Range(0, 100) > rigidRoot.dustList[i].limitations.percentage)
                                continue;
                            
                            // Set host
                            SetHostDust (rigidRoot.dustList[i], shard.tm, shard.mf, null, shard.sz);
                            
                            // Collect particles to reset them
                            rigidRoot.particleList.Add (rigidRoot.dustList[i].hostTm);
                        }
            }

            // RigidRoot -> MeshRoot shard
            else if (shard.rigid.objectType == ObjectType.MeshRoot)
            {
                // Create debris particles
                if (shard.rigid.HasDebris == true)
                    for (int i = 0; i < shard.rigid.debrisList.Count; i++)
                        if (shard.rigid.debrisList[i].onActivation == true)
                        {
                            // Filter by percentage
                            if (Random.Range(0, 100) > rigidRoot.debrisList[i].limitations.percentage)
                                continue;
                            
                            // Set host
                            SetHostDebris (shard.rigid.debrisList[i], shard.tm, shard.mf, null, shard.sz);
                            
                            // Collect particles to reset them
                            rigidRoot.particleList.Add (rigidRoot.debrisList[i].hostTm);
                        }
                
                // Create dust particles
                if (shard.rigid.HasDust == true)
                    for (int i = 0; i < shard.rigid.dustList.Count; i++)
                        if (shard.rigid.dustList[i].onActivation == true)
                        {
                            // Filter by percentage
                            if (Random.Range(0, 100) > rigidRoot.dustList[i].limitations.percentage)
                                continue;
                            
                            // Set host
                            SetHostDust (shard.rigid.dustList[i], shard.tm, shard.mf, null, shard.sz);
                            
                            // Collect particles to reset them
                            rigidRoot.particleList.Add (rigidRoot.debrisList[i].hostTm);
                        }
            }
        }
        
        // Init Rigid particles on activation
        public static void SetHostRigidAct (RayfireRigid rigid)
        {
            // Create debris particles
            if (rigid.HasDebris == true)
                for (int i = 0; i < rigid.debrisList.Count; i++)
                    if (rigid.debrisList[i].onActivation == true)
                    {
                        if (rigid.objectType == ObjectType.Mesh)
                            SetHostDebris (rigid.debrisList[i], rigid.transForm, rigid.meshFilter, rigid.meshRenderer, rigid.limitations.bboxSize);
                        else if (rigid.IsCluster == true)
                            SetHostDebrisCluster (rigid, rigid.debrisList[i]);
                    }

            // Create dust particles
            if (rigid.HasDust == true)
                for (int i = 0; i < rigid.dustList.Count; i++)
                    if (rigid.dustList[i].onActivation == true)
                    {
                        if (rigid.objectType == ObjectType.Mesh)
                            SetHostDust (rigid.dustList[i], rigid.transForm, rigid.meshFilter, rigid.meshRenderer, rigid.limitations.bboxSize);
                        else if (rigid.IsCluster == true)
                            SetHostDustCluster (rigid, rigid.dustList[i]);
                    }
        }
        
        /// //////////////////////////////////////////////////////////////////////////////////
        /// Set host / Activation / Runtime Connected Cluster from RigidRoot or MeshRoot
        /// //////////////////////////////////////////////////////////////////////////////////
        
        // Create single debris particle system for Connected Cluster
        static void SetHostDebrisCluster(RayfireRigid rigid, RayfireDebris debris)
        {
            for (int j = rigid.clusterDemolition.cluster.shards.Count - 1; j >= 0; j--)
            {
                // If has detached neib shard
                if (rigid.clusterDemolition.cluster.shards[j].neibShards.Count < rigid.clusterDemolition.cluster.shards[j].nAm)
                {
                    // Filter by percentage
                    if (Random.Range(0, 100) > debris.limitations.percentage)
                        continue;
                    
                    // Cluster crated by RigidRoot shards
                    if (rigid.rigidRoot != null)
                    {
                        SetHostDebris (debris,
                            rigid.clusterDemolition.cluster.shards[j].tm,
                            rigid.clusterDemolition.cluster.shards[j].mf, null,
                            rigid.clusterDemolition.cluster.shards[j].sz);

                        // Collect particles to reset them
                        rigid.rigidRoot.particleList.Add (debris.hostTm);
                    }
                    
                    // Cluster created by MeshRoot fragments
                    else if (rigid.meshRoot != null)
                    {
                        SetHostDebris (debris, 
                            rigid.clusterDemolition.cluster.shards[j].tm, 
                            rigid.clusterDemolition.cluster.shards[j].mf, 
                            rigid.clusterDemolition.cluster.shards[j].rigid.meshRenderer,
                            rigid.clusterDemolition.cluster.shards[j].sz);
                        
                        // Collect particles to reset them
                        rigid.meshRoot.particleList.Add (debris.hostTm);
                    }
                }
            }
        }
        
        // Create single dust particle system for Connected Cluster
        static void SetHostDustCluster(RayfireRigid rigid, RayfireDust dust)
        {
            for (int j = rigid.clusterDemolition.cluster.shards.Count - 1; j >= 0; j--)
            {
                // If has detached neib shard
                if (rigid.clusterDemolition.cluster.shards[j].neibShards.Count < rigid.clusterDemolition.cluster.shards[j].nAm)
                {
                    // Filter by percentage
                    if (Random.Range(0, 100) > dust.limitations.percentage)
                        continue;
                    
                    // Cluster crated by RigidRoot shards
                    if (rigid.rigidRoot != null)
                    {
                        SetHostDust (dust,
                            rigid.clusterDemolition.cluster.shards[j].tm,
                            rigid.clusterDemolition.cluster.shards[j].mf, null,
                            rigid.clusterDemolition.cluster.shards[j].sz);
                        
                        // Collect particles to reset them
                        rigid.rigidRoot.particleList.Add (dust.hostTm);
                    }

                    // Cluster created by MeshRoot fragments
                    else if (rigid.meshRoot != null)
                    {
                        SetHostDust (dust, 
                            rigid.clusterDemolition.cluster.shards[j].tm, 
                            rigid.clusterDemolition.cluster.shards[j].mf, null,
                            rigid.clusterDemolition.cluster.shards[j].sz);

                        // Collect particles to reset them
                        rigid.meshRoot.particleList.Add (dust.hostTm);
                    }
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set host / Main / Set host Specific props
        /// /////////////////////////////////////////////////////////
        
        // Get particle from emitter pool and set to host
        public static void SetHostDebris (RayfireDebris debris, Transform tm, MeshFilter mf, MeshRenderer mr = null, float size = 1f)
        {
            // Skip if no pool particles available
            if (debris.pool.emitter.ShouldSkip == true)
                return;
            
            // Filter by size threshold
            if (size < debris.limitations.sizeThreshold)
                return;
            
            // Visibility check
            if (VisibilityCheck(debris.limitations.visible, tm, mr) == false)
                return;    
            
            // Get final amount for current host
            int hostBurstAmount = GetHostFinalAmount (size, debris.emission.burstType, debris.emission.burstAmount);
            
            // Low host burst amount
            if (hostBurstAmount < debris.limitations.minParticles)
                return;

            // Set particle system to host
            ParticleSystem pSystem = debris.pool.emitter.GetPoolObject(tm);
            
            // Set host specific amount
            RFParticles.SetEmission(pSystem.emission, debris.emission.distanceRate, hostBurstAmount, debris.emission.burstVar);

            // Set host specific emitter shape if mesh
            RFParticles.SetShapeMesh (pSystem.shape, mf, tm, mr, debris.emissionMaterial);
   
            // Set Reuse, Activate and play
            ReuseActivatePlay (pSystem, debris.pool.emitter);

            // Set particle script host
            debris.hostTm = pSystem.transform;
            
            // Collect particle
            RayfireMan.inst.particles.resetList.Add (pSystem);
        }
        
        // Get particle from emitter pool and set to host
        public static void SetHostDust (RayfireDust dust, Transform tm, MeshFilter mf, MeshRenderer mr = null, float size = 1f)
        {
            // Skip if no pool particles available
            if (dust.pool.emitter.ShouldSkip == true)
                return;
            
            // Filter by size threshold
            if (size < dust.limitations.sizeThreshold)
                return;
            
            // Visibility check
            if (VisibilityCheck(dust.limitations.visible, tm, mr) == false)
                return;
            
            // Get final amount for current host
            int hostBurstAmount = GetHostFinalAmount (size, dust.emission.burstType, dust.emission.burstAmount);
            
            // Low host burst amount
            if (hostBurstAmount < dust.limitations.minParticles)
                return;
            
            // Set particle system to host
            ParticleSystem pSystem = dust.pool.emitter.GetPoolObject(tm);

            // Set host specific amount
            RFParticles.SetEmission(pSystem.emission, dust.emission.distanceRate, hostBurstAmount, dust.emission.burstVar);
            
            // Set host emitter shape to mesh 
            RFParticles.SetShapeMesh (pSystem.shape, mf, tm, mr, dust.emissionMaterial);
            
            // Set Reuse, Activate and play
            ReuseActivatePlay (pSystem, dust.pool.emitter);
            
            // Set particle script host
            dust.hostTm = pSystem.transform;
            
            // Collect particle
            RayfireMan.inst.particles.resetList.Add (pSystem);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set emitters and first reference
        /// /////////////////////////////////////////////////////////

        // Create debris ps emitter
        public static void CreateEmitterDebris(RayfireDebris scr, Transform sourceTm)
        {
            // Check for exiting of specific emitter pool id
            if (EmitterCheck(scr.pool, scr.transform) == true)
                return;

            // Create new pool emitter 
            RFPoolingEmitter emitter = new RFPoolingEmitter(scr.pool);
            
            // Create emitter root and set as child for main emitter root
            CreateRoot (RayfireMan.inst.particles.root, emitter, sourceTm.name + "_Dbr_" + scr.debrisReference.name);
         
            // Get object from pool or create
            ParticleSystem ps = CreateEmitterParticle(emitter);

            // Set edited debris ps properties
            SetPsDebris (scr, ps);

            // Renderer
            RFParticles.SetParticleRendererDebris(ps.GetComponent<ParticleSystemRenderer>(), scr);

            // Common emitter ops
            SetupEmitter (emitter, scr.pool, scr.transform);
        }

        // Set edited debris ps properties
        static void SetPsDebris (RayfireDebris scr, ParticleSystem ps)
        {
            // Set main module
            RFParticles.SetMain(ps.main, scr.emission.lifeMin, scr.emission.lifeMax, scr.emission.sizeMin, 
                scr.emission.sizeMax, scr.dynamic.gravityMin, scr.dynamic.gravityMax, scr.dynamic.speedMin, 
                scr.dynamic.speedMax, 3.1f, scr.limitations.maxParticles, scr.emission.duration);
            
            // Set default emitter shape to hemisphere in case host won't have mesh
            RFParticles.SetShapeHemisphere(ps.shape);
            
            // Emission over distance
            RFParticles.SetEmission(ps.emission, scr.emission.distanceRate, scr.emission.burstAmount, scr.emission.burstVar);
            
            // Inherit velocity 
            RFParticles.SetVelocity(ps.inheritVelocity, scr.dynamic);
            
            // Size over lifetime
            RFParticles.SetSizeOverLifeTime(ps.sizeOverLifetime);

            // Rotation by speed
            RFParticles.SetRotationBySpeed(ps.rotationBySpeed, scr.dynamic.rotationSpeed);

            // Noise
            RFParticles.SetNoise (ps.noise, scr.noise);
            
            // Collision
            RFParticles.SetCollisionDebris(ps.collision, scr.collision);
        }
        
        // Create dust ps emitter
        public static void CreateEmitterDust(RayfireDust scr, Transform sourceTm)
        {
            // Check for exiting of specific emitter pool id
            if (EmitterCheck(scr.pool, scr.transform) == true)
                return;
            
            // Create new pool emitter 
            RFPoolingEmitter emitter = new RFPoolingEmitter(scr.pool);
            
            // Create emitter root and set as child for main emitter root
            CreateRoot (RayfireMan.inst.particles.root, emitter, sourceTm.name + "_Dst_");
         
            // Get object from pool or create
            ParticleSystem ps = CreateEmitterParticle(emitter);

            // Set edited debris ps properties
            SetPsDust (scr, ps);
            
            // Renderer
            RFParticles.SetParticleRendererDust(ps.GetComponent<ParticleSystemRenderer>(), scr);

            // Common emitter ops
            SetupEmitter (emitter, scr.pool, scr.transform);
        }
        
        // Set edited debris ps properties
        static void SetPsDust (RayfireDust scr, ParticleSystem ps)
        {
            // Set main module
            RFParticles.SetMain(ps.main, scr.emission.lifeMin, scr.emission.lifeMax, scr.emission.sizeMin,
                scr.emission.sizeMax, scr.dynamic.gravityMin, scr.dynamic.gravityMax, scr.dynamic.speedMin, 
                scr.dynamic.speedMax, 6f, scr.limitations.maxParticles, scr.emission.duration);

            // Set default emitter shape to hemisphere in case host won't have mesh
            RFParticles.SetShapeHemisphere(ps.shape);
            
            // Emission over distance
            RFParticles.SetEmission(ps.emission, scr.emission.distanceRate, scr.emission.burstAmount, scr.emission.burstVar);
            
            // Collision
            RFParticles.SetCollisionDust(ps.collision, scr.collision);

            // Color over life time
            RFParticles.SetColorOverLife(ps.colorOverLifetime, scr.opacity);

            // Rotation over lifetime
            RFParticles.SetRotationOverLifeTime (ps.rotationOverLifetime, scr.dynamic);
            
            // Noise
            RFParticles.SetNoise(ps.noise, scr.noise);
        }

        // Common emitter ops
        static void SetupEmitter(RFPoolingEmitter emitter, RFParticlePool pool, Transform tm)
        {
            // Set to particle emitter
            pool.emitter = emitter;
            
            // Collect particle tm to clean emitters
            pool.emitter.scripts.Add (tm);
            
            // Collect emitter to manager
            RayfireMan.inst.particles.emitters.Add (pool.emitter);
            
            // Warmup and start pooling
            if (pool.enable == true)
            {
                // Create all pool references in awake
                if (pool.warmup == true)
                    Warmup(pool.emitter);
                
                // Start Emitter Pooling
                RayfireMan.inst.StartEmitterPooling();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Edit emitter particles
        /// /////////////////////////////////////////////////////////
        
        // Edit emitter particles
        public static void EditEmitterParticles(RFPoolingEmitter emitter, RayfireDebris scr)
        {
            if (emitter.queue.Count > 1)
            {
                ParticleSystem[] psArray = emitter.queue.ToArray();
                for (int i = 0; i < psArray.Length; i++)
                    SetPsDebris (scr, psArray[i]);
            }
            
            // Edit first ps
            else SetPsDebris (scr, emitter.queue.Peek());
        }
        
        // Edit emitter particles
        public static void EditEmitterParticles(RFPoolingEmitter emitter, RayfireDust scr)
        {
            if (emitter.queue.Count > 1)
            {
                ParticleSystem[] psArray = emitter.queue.ToArray();
                for (int i = 0; i < psArray.Length; i++)
                    SetPsDust (scr, psArray[i]);
            }
            
            // Edit first ps
            else SetPsDust (scr, emitter.queue.Peek());
        }
        
        /// /////////////////////////////////////////////////////////
        /// Get pool particle and instantiation
        /// /////////////////////////////////////////////////////////
        
        // Instantiate first ps
        ParticleSystem InstantiateToHost(Transform hostTm)
        {
            ParticleSystem ps = Object.Instantiate (queue.Peek(), hostTm.position, hostTm.rotation, hostTm);
            ps.name = "ps";
            return ps;
        }
        
        // Instantiate first ps
        public void InstantiateToPool()
        {
            ParticleSystem ps = Object.Instantiate (queue.Peek(), root.position, root.rotation, root);
            ps.name = "ps";
            queue.Enqueue (ps);
            SetNeed(false);
        }
        
        // Get pool object
        ParticleSystem GetPoolObject (Transform hostTm)
        {
            // Set need state
            need = true;
            
            // Instantiate on the host if only one and set as empty
            if (queue.Count == 1)
            {
                // Set default tm in case Impact host without transform
                if (hostTm == null)
                    hostTm = root;

                empty = true;
                return InstantiateToHost (hostTm);
            }

            // Set transform to host to avoid double set parent and tm change
            if (hostTm != null)
            {
                queue.Peek().transform.position = hostTm.position;
                queue.Peek().transform.SetParent (hostTm);
            }
            
            // Out from queue
            return queue.Dequeue();
        }

        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////

        // Check if emitter should skip send particle to host
        bool ShouldSkip
        {
            get {return enable == true && skip == true && empty == true;}
        }

        // Check for exiting of specific emitter pool id
        static bool EmitterCheck(RFParticlePool pool, Transform tm)
        {
            // Already has connected emitter
            if (pool.emitter != null)
                return true;
            
            // Check for exiting emitter with the same pool id
            if (pool.id > 0)
            {
                int emitterIndex = RayfireMan.inst.particles.GetEmitterIndexById (pool.id);
                if (emitterIndex >= 0)
                {
                    pool.emitter = RayfireMan.inst.particles.emitters[emitterIndex];
                    pool.emitter.scripts.Add (tm);
                    return true;
                }
            }
            
            return false;
        }

        // Set Reuse, Activate and play
        static void ReuseActivatePlay(ParticleSystem pSystem, RFPoolingEmitter emitter)
        {
            RFParticles.Reuse (pSystem, emitter);
            pSystem.gameObject.SetActive (true);
            pSystem.Play();
        }
        
        // Object visibility check
        static bool VisibilityCheck(bool checkNeed, Transform tm, MeshRenderer mr)
        {
            // Visibility check disabled
            if (checkNeed == false)
                return true;

            // Object has no renderer
            if (mr == null)
            {
                mr = tm.GetComponent<MeshRenderer>();
                if (mr == null)
                    return true;
            }
            
            return mr.isVisible;
        }
        
        // Object visibility check
        static bool VisibilityCheck(bool checkNeed, Camera camera, Vector3 pos)
        {
            // Visibility check disabled
            if (checkNeed == false)
                return true;
            
            // Check if in frustum
            Vector3 p = camera.WorldToViewportPoint(pos);
            return p.x > 0 && p.x < 1 && p.y > 0 && p.y < 1;
        }

        // Set need state
        public void SetNeed(bool startPooling)
        {
            need = queue.Count < cap;

            // Start pooling
            if (startPooling == true && need == true)
                RayfireMan.inst.StartEmitterPooling();
        }
        
        // Create all pool references in awake
        static void Warmup(RFPoolingEmitter emitter)
        {
            for (int i = 1; i < emitter.cap; i++)
                emitter.InstantiateToPool();
        }

        // Create root for all ps instances
        static void CreateRoot(Transform emitterParent, RFPoolingEmitter emitter, string name)
        {
            GameObject rootGo = new GameObject (name);
            emitter.root          = rootGo.transform;
            emitter.root.position = emitterParent.position;
            emitter.root.parent   = emitterParent;
        }
        
        // Create dummy particle system object
        static ParticleSystem CreateEmitterParticle(RFPoolingEmitter emitter)
        {
            // Create object with ps
            GameObject go = new GameObject("ps");
            go.SetActive (false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop();
                        
            // Add to emitters list for instantiation
            emitter.queue.Enqueue (ps);
            
            // Set ps transform
            Transform psTm = ps.transform;
            psTm.SetPositionAndRotation (emitter.root.position, emitter.root.rotation);
            psTm.SetParent (emitter.root);
            psTm.localScale = Vector3.one;

            return ps;
        }
        
        // Get amount list
        static int GetHostFinalAmount(float sz, RFParticles.BurstType burstType, int burstAmount)
        {
            // No burst
            if (burstType == RFParticles.BurstType.None)
                return 0;

            // Same burst amount for every fragment
            if (burstType == RFParticles.BurstType.AmountAndVariation)
                return burstAmount;

            // Burst amount per particles per fragment size
            if (burstType == RFParticles.BurstType.AmountPerUnit)
                return (int)(burstAmount * sz);

            return 0;
        }
    }
}
