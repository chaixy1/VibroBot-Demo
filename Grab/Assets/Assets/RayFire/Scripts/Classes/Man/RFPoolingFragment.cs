using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace RayFire
{
    [Serializable]
    public class RFPoolingFragment
    {
        // UI
        public bool enable;
        [FormerlySerializedAs ("capacity")]
        public int  minCap;
        public bool reuse;
        public int  maxCap;

        // Non serialized
        [NonSerialized]        Transform           root;
        [NonSerialized]        GameObject          host;
        [NonSerialized]        MeshFilter          mf;
        [NonSerialized]        MeshRenderer        mr;
        [NonSerialized]        RayfireRigid        rg;
        [NonSerialized]        Rigidbody           rb;
        [NonSerialized] public RayfireRigid        rgInst;
        [NonSerialized] public Queue<RayfireRigid> queue;
        [NonSerialized] public bool                inProgress;

        // Static
        public static int rate = 2;

        // Constructor
        public RFPoolingFragment()
        {
            enable = true;
            minCap = 60;
            reuse  = false;
            maxCap = 120;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Create pool root
        public void CreatePoolRoot (Transform manTm)
        {
            // Already has pool root
            if (root != null)
                return;
            
            GameObject poolGo = new GameObject ("Pool_Fragments");
            root          = poolGo.transform;
            root.position = manTm.position;
            root.parent   = manTm;
        }

        // Create pool object
        public void CreateInstance (Transform manTm)
        {
            // Return if not null
            if (rgInst != null)
                return;

            // Create pool instance
            rgInst = CreateRigidInstance();

            // Set tm
            rgInst.transForm.SetPositionAndRotation (manTm.position, manTm.rotation);
            rgInst.transForm.parent   = root;
        }

        // Create pool object
        public RayfireRigid CreateRigidInstance()
        {
            host = new GameObject ("rg");
            host.SetActive (false);
            
            mf                        = host.AddComponent<MeshFilter>();
            mr                        = host.AddComponent<MeshRenderer>();
            rg                        = host.AddComponent<RayfireRigid>();
            rb                        = host.AddComponent<Rigidbody>();
            rb.interpolation          = RayfireMan.inst.interpolation;
            rb.collisionDetectionMode = RayfireMan.inst.meshCollision;
            rg.initialization         = RayfireRigid.InitType.AtStart;
            rg.transForm              = host.transform;
            rg.meshFilter             = mf;
            rg.meshRenderer           = mr;
            rg.physics.rigidBody      = rb;

            return rg;
        }

        // Get pool object
        public RayfireRigid GetPoolObject (Transform manTm)
        {
            if (enable == true)
            {
                while (queue.Count > 0)
                {
                    // Check if destroyed with demolished cluster
                    if (queue.Peek() == null)
                        queue.Dequeue();
                    else
                        return queue.Dequeue();
                }
            }

            return CreatePoolObject (manTm);
        }

        // Create pool object
        RayfireRigid CreatePoolObject (Transform manTm)
        {
            // Create instance if null
            if (rgInst == null)
                CreateInstance (manTm);

            // Create
            return Object.Instantiate (rgInst, root);
        }

        // Destroy Rigid or reset back to pool
        public void DestroyOrReset(RayfireRigid rgBack, float lifeTime)
        {
            // Destroy if backpooling disabled or max capacity reached
            if (reuse == false || queue.Count > maxCap)
            {
                if (lifeTime <= 0)
                    Object.Destroy (rgBack.gameObject, lifeTime);
                else
                    Object.Destroy (rgBack.gameObject);
            }

            // Add to backpooling
            else
            {
                RigidPoolReset (rgBack);
            }
        }
        
        // Keep full pool 
        public IEnumerator StartPoolingCor (Transform manTm)
        {
            const float delayTime = 0.5f;
            queue = new Queue<RayfireRigid>(minCap);
            WaitForSeconds delay = new WaitForSeconds (delayTime);
            
            // Create some in advance for quick test demolitions
            for (int i = 0; i < 30; i++)
                if (queue.Count < minCap)
                    queue.Enqueue (CreatePoolObject (manTm));
            
            // Pooling loop
            inProgress = true;
            while (enable == true)
            {
                // Create if not enough
                if (queue.Count < minCap)
                    for (int i = 0; i < rate; i++)
                        queue.Enqueue (CreatePoolObject (manTm));

                // Wait next frame
                yield return delay;
            }
            inProgress = false;
        }
        
        // Reset Rigid for pooling
        void RigidPoolReset(RayfireRigid rgBack)
        {
            // Set tm
            rgBack.transForm.parent        = root;
            rgBack.transForm.localPosition = Vector3.zero;
            rgBack.transForm.localRotation = Quaternion.identity;
            rgBack.transForm.localScale    = Vector3.one;

            // Reset properties
            GlobalReset (rgBack);
            
            // Add back to queue
            queue.Enqueue (rgBack);
        }
        
        // Reset Rigid back to pool
        public static void GlobalReset(RayfireRigid scr)
        {
            scr.initialization = RayfireRigid.InitType.ByMethod;
            scr.simulationType = SimType.Dynamic;
            scr.objectType     = ObjectType.Mesh;
            scr.demolitionType = DemolitionType.None;
            
            scr.physics.GlobalReset(); // TODO reset rigidbody and mesh collider props
            scr.activation.GlobalReset();
            scr.limitations.GlobalReset();; // TODO bound and contact point
            scr.meshDemolition.GlobalReset();
            scr.clusterDemolition.GlobalReset();
            scr.referenceDemolition.GlobalReset();
            scr.materials.GlobalReset();
            scr.damage.GlobalReset();
            scr.fading.GlobalReset();
            scr.reset.GlobalReset();
            
            // Hidden
            scr.initialized   = false;
            scr.rfMeshes      = null;
            scr.fragments     = null;
            scr.cacheRotation = Quaternion.identity;
            scr.rootChild     = null;
            scr.rootParent    = null;
            scr.rest          = null;
            scr.sound         = null;
            
            // Non Serialized
            scr.corState     = false;
            scr.particleList = null;
            scr.debrisList   = null;
            scr.dustList     = null;
            scr.subIds       = null;
            scr.pivots       = null;
            scr.meshes       = null;
            scr.meshRoot     = null;
            scr.rigidRoot    = null;
            
            
            // Reset components
            scr.meshFilter.sharedMesh        = null;
            scr.meshRenderer.sharedMaterial  = null;
            scr.meshRenderer.sharedMaterials = new Material[]{null}; // TODO reset other properties
            scr.skr                          = null;

            // TODO Reset
            /*
            scr.demolitionEvent  = new RFDemolitionEvent();
            scr.activationEvent  = new RFActivationEvent();
            scr.restrictionEvent = new RFRestrictionEvent();
            */
        }
        
    }
}
