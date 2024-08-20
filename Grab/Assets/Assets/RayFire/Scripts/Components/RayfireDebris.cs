using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [AddComponentMenu ("RayFire/Rayfire Debris")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-debris-component/")]
    public class RayfireDebris : MonoBehaviour
    {
        // UI
        public bool                      onDemolition;
        public bool                      onActivation;
        public bool                      onImpact;
        public GameObject                debrisReference;
        public Material                  debrisMaterial;
        public Material                  emissionMaterial;
        public RFParticleEmission        emission;
        public RFParticleDynamicDebris   dynamic;
        public RFParticleNoise           noise;
        public RFParticleCollisionDebris collision;
        public RFParticleLimitations     limitations;
        public RFParticleRendering       rendering;
        public RFParticlePool            pool;
        
        // Non serialized
        [NonSerialized] public bool                initialized;
        [NonSerialized] public RayfireRigid        rigid;
        [NonSerialized] public Transform           hostTm;
        [NonSerialized] public List<RayfireDebris> children;

        // Static
        static        Renderer     stRnd;
        static        MeshFilter[] stMfs;
        public static Mesh[]       stMsh;

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RayfireDebris()
        {
            onDemolition     = false;
            onActivation     = false;
            onImpact         = false;
            debrisReference  = null;
            debrisMaterial   = null;
            emissionMaterial = null;
            emission         = new RFParticleEmission();
            dynamic          = new RFParticleDynamicDebris();
            noise            = new RFParticleNoise();
            collision        = new RFParticleCollisionDebris();
            limitations      = new RFParticleLimitations();
            rendering        = new RFParticleRendering();
            pool             = new RFParticlePool();
        }

        // Copy from
        public void CopyFrom(RayfireDebris source)
        {
            onDemolition     = source.onDemolition;
            onActivation     = source.onActivation;
            onImpact         = source.onImpact;
            debrisReference  = source.debrisReference;
            debrisMaterial   = source.debrisMaterial;
            emissionMaterial = source.emissionMaterial;
            emission.CopyFrom (source.emission);
            dynamic.CopyFrom (source.dynamic);
            noise.CopyFrom (source.noise);
            collision.CopyFrom (source.collision);
            limitations.CopyFrom (source.limitations);
            rendering.CopyFrom (source.rendering);
            pool.CopyFrom (source.pool);
            pool.emitter.scripts.Add (transform);
            
            // Hidden
            initialized = source.initialized;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// ///////////////////////////////////////////////////////// 
        
        // Initialize
        public void Initialize()
        {
            // Do not initialize if already initialized or parent was initialized
            if (initialized == true)
                return;

            // Set debris ref meshes
            SetReferenceMeshes (debrisReference);

            // Create debris ps emitter
            if (initialized == true)
                RFPoolingEmitter.CreateEmitterDebris (this, transform);
        }
        
        // Emit particles
        public void Emit()
        {
            if (rigid != null)
                RFPoolingEmitter.SetHostDebris (this, transform, rigid.meshFilter, rigid.meshRenderer, rigid.meshRenderer.bounds.size.magnitude);
            else
                RFPoolingEmitter.SetHostDebris (this, transform, GetComponent<MeshFilter>(), GetComponent<MeshRenderer>(), GetComponent<MeshRenderer>().bounds.size.magnitude);
        }
        
        // Edit particles. 
        public void EditEmitterParticles()
        {
            RFPoolingEmitter.EditEmitterParticles(pool.emitter, this);
        }
        
        // Clean particle systems
        public void Clean()
        {
            // Destroy own particles
            if (hostTm != null)
                Destroy (hostTm.gameObject);

            // Destroy particles on children debris
            if (HasChildren == true)
                for (int i = 0; i < children.Count; i++)
                    if (children[i] != null)
                        if (children[i].hostTm != null)
                            Destroy (children[i].hostTm.gameObject);
        }

        /// /////////////////////////////////////////////////////////
        /// Renderer
        /// /////////////////////////////////////////////////////////
        
        // Get reference meshes
        void SetReferenceMeshes(GameObject refs)
        {
            // No reference. Use own mesh
            if (refs == null)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log ("RayFire Debris: " + name + ": Debris reference not defined.", gameObject);
                return;
            }
            
            // Add local mf
            if (refs.transform.childCount > 0)
            {
                stMfs = refs.GetComponentsInChildren<MeshFilter>();
            }
            else if (stMfs == null || stMfs[0] == null)
            {
                stMfs = new []{refs.GetComponent<MeshFilter>()};
            }

            // No mesh filters
            if (stMfs.Length == 0)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log ("RayFire Debris: " + name + ": Debris reference mesh is not defined.", gameObject);
                return;
            }

            // Get all meshes
            stMsh = new Mesh[4];
            for (int i = 0; i < stMfs.Length; i++)
            {
                // Limit by 4. Particle system can't take mor than 4 ref meshes
                if (i == 4)
                    break;

                if (stMfs[i].sharedMesh != null && stMfs[i].sharedMesh.vertexCount > 3)
                    stMsh[i] = stMfs[i].sharedMesh;
                else if (RayfireMan.debugStatic == true)
                    Debug.Log ("RayFire Debris: " + stMfs[i].name + ": No mesh or amount of vertices too low.", gameObject);
            }
            
            // Check for deprecated burst property. Will be OBSOLETE
            if ((int)emission.burstType == 1)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log ("RayFire Debris: " + name + ": Deprecated Burst Type property.", gameObject);
            }

            // Set debris material
            SetDebrisMaterial (stMfs);
            
            initialized = true;
            stMfs         = null;
        }
        
        // Set debris material
        void SetDebrisMaterial(MeshFilter[] mfs)
        {
            // Already defined
            if (debrisMaterial != null)
                return;
            
            for (int i = 0; i < mfs.Length; i++)
            {
                stRnd = mfs[i].GetComponent<Renderer>();
                if (stRnd != null)
                {
                    if (stRnd.sharedMaterial != null)
                    {
                        debrisMaterial = stRnd.sharedMaterial;
                        return;
                    }
                }
            }

            // Set original object material
            if (debrisMaterial == null)
                debrisMaterial = GetComponent<Renderer>().sharedMaterial;
            stRnd = null;
        }

        public bool HasChildren { get { return children != null && children.Count > 0; } }
    }
}
