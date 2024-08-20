using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFPoolingParticles
    {
        public bool enable;
        public int  reused;
        
        // Non serialized
        [NonSerialized] public List<ParticleSystem>   resetList;
        [NonSerialized] public bool                   poolProgress;
        [NonSerialized] public List<RFPoolingEmitter> emitters;
        [NonSerialized] public bool                   emitProgress;
        [NonSerialized] public Transform              root;

        // Static
        static RayfireDebris[] debrisArray;
        static RayfireDust[]   dustArray;
        
        // Constructor
        public RFPoolingParticles()
        {
            enable    = true;
            resetList = new List<ParticleSystem> ();
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////

        // Emitting coroutine 
        public IEnumerator StartEmitterPoolingCor ()
        {
            // Coroutine already running
            if (emitProgress == true)
                yield break;
            
            // Set running coroutine state
            emitProgress = true;
  
            while (enable == true)
            {
                if (emitters.Count > 0)
                {
                    bool stop = true;
                    for (int i = 0; i < emitters.Count; i++)
                    {
                        if (emitters[i].enable == true && emitters[i].need == true)
                        {
                            // Do not stop
                            stop = false;
                            
                            // Add new ref to pool by rate
                            for (int j = 0; j < emitters[i].rate; j++)
                                emitters[i].InstantiateToPool();
                            
                            // Emitter not empty
                            emitters[i].empty = false;
                            
                            // Wait next frame
                            yield return null;
                        }
                    }

                    // All emitters are full, stop pooling
                    if (stop == true)
                        break;
                }
            }
            
            // Coroutine stop running
            emitProgress = false;
        }
        
        
        
        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////
        
        // Setup emitters pooling
        public void SetupEmitters(Transform tm)
        {
            reused = 0;
            
            // Init list
            emitters = new List<RFPoolingEmitter>();
            
            // Create emit root
            CreateEmittersRoot (tm);
        }

        
        // Create emit root
        public void CreateEmittersRoot (Transform manTm)
        {
            // Already has emit root
            if (root != null)
                return;
            
            GameObject emitGo = new GameObject ("Pool_Emitters");
            root          = emitGo.transform;
            root.position = manTm.position;
            root.parent   = manTm;
        }
        
        // Check if any of emitters require pooling
        public bool NeedState()
        {
            for (int i = 0; i < emitters.Count; i++)
                if (emitters[i].need == true)
                    return true;
            return false;
        }
        
        // Get total amount of particles in pool
        public int GetTotalPoolAmount()
        {
            int s = 0;
            for (int i = 0; i < emitters.Count; i++)
                s += emitters[i].queue.Count;
            return s;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Initialize Particles / Rigid / Rigid Root
        /// /////////////////////////////////////////////////////////
        
        // Set Particle Components: Initialize, collect
        public static void InitializeParticles (RayfireRigid scr)
        {
            // If debrisList mot predefined by ancestor object at Debris copy
            if (scr.debrisState > 0)
            {
                // Get all Debris and initialize
                debrisArray = scr.GetComponents<RayfireDebris>();
                if (debrisArray.Length > 0)
                {
                    for (int i = 0; i < debrisArray.Length; i++)
                    {
                        debrisArray[i].rigid  = scr;
                        debrisArray[i].collision.matTyp = scr.physics.mt;
                        debrisArray[i].Initialize();
                    }

                    scr.debrisList = new List<RayfireDebris> (debrisArray.Length);
                    for (int i = 0; i < debrisArray.Length; i++)
                        if (debrisArray[i].initialized == true)
                            scr.debrisList.Add (debrisArray[i]);
                }
                debrisArray = null;
            }

            // If dustList mot predefined by ancestor object at Dust copy
            if (scr.dustState > 0)
            {
                // Get all Dust and initialize
                dustArray = scr.GetComponents<RayfireDust>();
                if (dustArray.Length > 0)
                {
                    for (int i = 0; i < dustArray.Length; i++)
                    {
                        dustArray[i].rigid = scr;
                        dustArray[i].Initialize();
                    }

                    scr.dustList = new List<RayfireDust> (dustArray.Length);
                    for (int i = 0; i < dustArray.Length; i++)
                        if (dustArray[i].initialized == true)
                            scr.dustList.Add (dustArray[i]);
                }
                dustArray = null;
            }
        }
        
        // Set Particle Components: Initialize, collect
        public static void InitializeParticles(RayfireRigidRoot scr)
        {
            // Set particle components for meshRoots
            for (int i = 0; i < scr.meshRoots.Count; i++)
                InitializeParticles (scr.meshRoots[i]);

            // Set size sum
            scr.sizeSum = 0;
            for (int i = 0; i < scr.rigidRootShards.Count; i++)
                scr.sizeSum += scr.rigidRootShards[i].sz;
            
            // Get all Debris and initialize
            debrisArray = scr.GetComponents<RayfireDebris>();
            if (debrisArray.Length > 0)
            {
                for (int i = 0; i < debrisArray.Length; i++)
                {
                    debrisArray[i].collision.matTyp = scr.physics.mt;
                    debrisArray[i].Initialize();
                }
                scr.debrisList = new List<RayfireDebris>(debrisArray.Length);
                for (int i = 0; i < debrisArray.Length; i++)
                    if (debrisArray[i].initialized == true)
                        scr.debrisList.Add (debrisArray[i]);
            }
            
            // Get all Dust and initialize
            dustArray = scr.GetComponents<RayfireDust>();
            if (dustArray.Length > 0)
            {
                for (int i = 0; i < dustArray.Length; i++)
                    dustArray[i].Initialize();
                scr.dustList = new List<RayfireDust> (dustArray.Length);
                for (int i = 0; i < dustArray.Length; i++)
                    if (dustArray[i].initialized == true)
                        scr.dustList.Add (dustArray[i]);
            }

            // List for creates particle system to reset them
            scr.particleList = new List<Transform>();
            
            debrisArray = null;
            dustArray   = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Copy Particles / Rigid / Rigid Root
        /// /////////////////////////////////////////////////////////
        
        // Copy debris and dust. Rigid to Rigid
        public static void CopyParticlesRigid(RayfireRigid source, RayfireRigid target)
        {
            // TODO reference to original component, do not create new
            
            // Copy debris
            if (source.HasDebris == true)
            {
                // Prepare target debris list
                if (target.debrisList == null)
                    target.debrisList = new List<RayfireDebris>(source.debrisList.Count);
                else
                    target.debrisList.Clear();
                
                // Copy every debris from source to target
                for (int i = 0; i < source.debrisList.Count; i++)
                {
                    RayfireDebris targetDebris = target.gameObject.AddComponent<RayfireDebris>();
                    targetDebris.CopyFrom (source.debrisList[i]);
                    targetDebris.rigid = target;

                    // Collect child debris in parent source debris
                    if (source.debrisList[i].HasChildren == false)
                        source.debrisList[i].children = new List<RayfireDebris>(source.debrisList.Count);
                    source.debrisList[i].children.Add (targetDebris);
                    
                    // Collect debris for target
                    target.debrisList.Add (targetDebris);
                }
            }
            else
            {
                target.debrisState = 0;
            }
            
            // Copy dust
            if (source.HasDust == true)
            {
                // Prepare target dust list
                if (target.dustList == null)
                    target.dustList = new List<RayfireDust>(source.dustList.Count);
                else
                    target.dustList.Clear();
                
                for (int i = 0; i < source.dustList.Count; i++)
                {
                    RayfireDust targetDust = target.gameObject.AddComponent<RayfireDust>();
                    targetDust.CopyFrom (source.dustList[i]);
                    targetDust.rigid = target;

                    if (source.dustList[i].HasChildren == false)
                        source.dustList[i].children = new List<RayfireDust>(source.dustList.Count);
                    source.dustList[i].children.Add (targetDust);
                    
                    // Collect debris for target
                    target.dustList.Add (targetDust);
                }
            }
            else
            {
                target.dustState = 0;
            }
        }
        
        // Copy debris and dust. Rigid to
        public static void CopyParticlesMeshroot(RayfireRigid source, List<RayfireRigid> targets)
        {
            // List for creates particle system to reset them
            source.particleList = new List<Transform>();
            
            // Clean null debris
            if (source.HasDebris == true)
                for (int i = source.debrisList.Count - 1; i >= 0; i--)
                    if (source.debrisList[i] == null)
                        source.debrisList.RemoveAt (i);
            
            // Copy debris. only initialized debris in this list
            if (source.HasDebris == true)
            {
                for (int d = 0; d < source.debrisList.Count; d++)
                {
                    // Set max amount
                    int maxAmount = targets.Count;
                    if (source.debrisList[d].limitations.percentage < 100)
                        maxAmount = targets.Count * source.debrisList[d].limitations.percentage / 100;
                    
                    // Copy component
                    for (int i = 0; i < targets.Count; i++)
                    {
                        // Max amount reached
                        if (maxAmount <= 0)
                            break;

                        // Filter by size threshold
                        if (targets[i].limitations.size < source.debrisList[d].limitations.sizeThreshold)
                            continue;
                        
                        // Filter by percentage
                        if (Random.Range (0, 100) > source.debrisList[d].limitations.percentage)
                            continue;

                        // Copy
                        RayfireDebris targetDebris = targets[i].gameObject.AddComponent<RayfireDebris>();
                        targetDebris.CopyFrom (source.debrisList[d]);
                        targetDebris.rigid = targets[i];
                        
                        // Collect debris for Rigid
                        if (targets[i].debrisList == null)
                            targets[i].debrisList = new List<RayfireDebris>(targets.Count);
                        targets[i].debrisList.Add (targetDebris);
                        
                        // Collect debris for parent debris
                        if (source.debrisList[d].children == null)
                            source.debrisList[d].children = new List<RayfireDebris>(targets.Count);
                        source.debrisList[d].children.Add (targetDebris);
                        
                        maxAmount--;
                    }
                }
            }
            
            // Clean null dust
            if (source.HasDust == true)
                for (int i = source.dustList.Count - 1; i >= 0; i--)
                    if (source.dustList[i] == null)
                        source.dustList.RemoveAt (i);
            
            // Copy dust
            if (source.HasDust == true)
            {
                for (int d = 0; d < source.dustList.Count; d++)
                {
                    // Set max amount
                    int maxAmount = targets.Count;
                    if (source.dustList[d].limitations.percentage < 100)
                        maxAmount = targets.Count * source.dustList[d].limitations.percentage / 100;

                    for (int i = 0; i < targets.Count; i++)
                    {
                        // Max amount reached
                        if (maxAmount <= 0)
                            break;

                        // Filter by size threshold
                        if (targets[i].limitations.size < source.dustList[d].limitations.sizeThreshold)
                            continue;
                        
                        // Filter by percentage
                        if (Random.Range (0, 100) > source.dustList[d].limitations.percentage)
                            continue;

                        // Copy
                        RayfireDust targetDust = targets[i].gameObject.AddComponent<RayfireDust>();
                        targetDust.CopyFrom (source.dustList[d]);
                        targetDust.rigid = targets[i];
                        
                        // Collect debris for Rigid
                        if (targets[i].dustList == null)
                            targets[i].dustList = new List<RayfireDust>(targets.Count);
                        targets[i].dustList.Add (targetDust);
                        
                        // Collect debris for parent debris
                        if (source.dustList[d].children == null)
                            source.dustList[d].children = new List<RayfireDust>(targets.Count);
                        source.dustList[d].children.Add (targetDust);
                        
                        maxAmount--;
                    }
                }
            }
        }
        
         // Copy debris and dust
        public static void CopyParticlesRigidroot(RayfireRigidRoot source, RayfireRigid target)
        {
            // Copy debris
            if (source.HasDebris == true)
            {
                // Prepare target debris list
                if (target.debrisList == null)
                    target.debrisList = new List<RayfireDebris>(source.debrisList.Count);
                else
                    target.debrisList.Clear();
                
                // Copy every debris from source to target
                for (int i = 0; i < source.debrisList.Count; i++)
                {
                    RayfireDebris targetDebris = target.gameObject.AddComponent<RayfireDebris>();
                    targetDebris.CopyFrom (source.debrisList[i]);
                    targetDebris.rigid = target;

                    // Collect child debris in parent source debris
                    if (source.debrisList[i].HasChildren == false)
                        source.debrisList[i].children = new List<RayfireDebris>(source.debrisList.Count);
                    source.debrisList[i].children.Add (targetDebris);
                    
                    // Collect debris for target
                    target.debrisList.Add (targetDebris);
                }
            }
            
            // Copy dust
            if (source.HasDust == true)
            {
                // Prepare target dust list
                if (target.dustList == null)
                    target.dustList = new List<RayfireDust>(source.dustList.Count);
                else
                    target.dustList.Clear();
                
                for (int i = 0; i < source.dustList.Count; i++)
                {
                    RayfireDust targetDust = target.gameObject.AddComponent<RayfireDust>();
                    targetDust.CopyFrom (source.dustList[i]);
                    targetDust.rigid = target;

                    if (source.dustList[i].HasChildren == false)
                        source.dustList[i].children = new List<RayfireDust>(source.dustList.Count);
                    source.dustList[i].children.Add (targetDust);
                    
                    // Collect debris for target
                    target.dustList.Add (targetDust);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Pooling
        /// /////////////////////////////////////////////////////////
        
        // Keep full pool 
        public IEnumerator StartPoolingCor ()
        {
            int            resetInterval = 0;
            int            cleanInterval = 0;
            const float    delayTime     = 1.0f;
            WaitForSeconds delay         = new WaitForSeconds (delayTime);
            
            // Pooling loop
            poolProgress = true;
            while (enable == true)
            {
                // All particles in scene
                resetInterval++;
                if (resetInterval == 10)
                {
                    ResetCheck();
                    resetInterval = 0;
                }
                
                cleanInterval++;
                if (cleanInterval == 100)
                {
                    EmitterScriptsCheck();
                    cleanInterval = 0;
                }
                
                // Start emitter pooling if needed
                if (NeedState() == true)
                    RayfireMan.inst.StartEmitterPooling();
                
                // Wait next frame
                yield return delay;
            }
            poolProgress = false;
        }

        // Check for particles amount in scene
        public int ResetCheck()
        {
            if (resetList.Count > 0)
                for (int i = resetList.Count - 1; i >= 0; i--)
                {
                    if (resetList[i] == null)
                        resetList.RemoveAt (i);
                    else if (resetList[i].isStopped == true)
                            resetList.RemoveAt (i);
                }
            return resetList.Count;
        }

        // Clean emitters
        void EmitterScriptsCheck()
        {
            for (int e = emitters.Count - 1; e >= 0; e--)
            {
                // Remove null transforms
                for (int i = emitters[e].scripts.Count - 1; i >= 0; i--)
                {
                    if (emitters[e].scripts[i] == null)
                       emitters[e].scripts.RemoveAt (i);
                }
            
                // Remove empty emitters
                if (emitters[e].scripts.Count == 0)
                {
                    Object.Destroy (emitters[e].root.gameObject);
                    emitters.RemoveAt (e);
                }
            }
        }

        // Destroy all particles
        public void DestroyAllParticles()
        {
            if (ResetCheck() > 0)
            {
                for (int i = resetList.Count - 1; i >= 0; i--)
                    Object.Destroy (resetList[i].gameObject);
                resetList.Clear();
            }
        } 
        
        // Get emitter by id
        public int GetEmitterIndexById (int id)
        {
            for (int e = 0; e < emitters.Count; e++)
                if (emitters[e].id == id)
                    return e;
            return -1;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Get / Set
        /// /////////////////////////////////////////////////////////
                
        // Enable state
        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                enable = value;
                
                // Start Emitter Pooling if needed
                if (enable == true && emitters != null)
                {
                    // Set need state for emitters
                    for (int i = 0; i < emitters.Count; i++)
                        emitters[i].SetNeed (false);
                    RayfireMan.inst.StartEmitterPooling();
                }
            }
        }
    }
}
