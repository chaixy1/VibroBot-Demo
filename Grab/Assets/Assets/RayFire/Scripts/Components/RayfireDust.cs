using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [AddComponentMenu ("RayFire/Rayfire Dust")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-dust-component/")]
    public class RayfireDust : MonoBehaviour
    {
        // UI
        public bool                    onDemolition;
        public bool                    onActivation;
        public bool                    onImpact;
        public float                   opacity;
        public Material                dustMaterial;
        public List<Material>          dustMaterials;
        public Material                emissionMaterial;
        public RFParticleEmission      emission;
        public RFParticleDynamicDust   dynamic;
        public RFParticleNoise         noise;
        public RFParticleCollisionDust collision;
        public RFParticleLimitations   limitations;
        public RFParticleRendering     rendering;
        public RFParticlePool          pool;
        
        // Non Serialized
        [NonSerialized] public RayfireRigid      rigid;
        [NonSerialized] public Transform         hostTm;
        [NonSerialized] public bool              initialized;
        [NonSerialized] public List<RayfireDust> children;

        
        // auto alpha fade
        // few dust textures with separate alphas

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RayfireDust()
        {
            onDemolition     = false;
            onActivation     = false;
            onImpact         = false;
            dustMaterial     = null;
            opacity          = 0.25f;
            emissionMaterial = null;
            emission         = new RFParticleEmission();
            dynamic          = new RFParticleDynamicDust();
            noise            = new RFParticleNoise();
            collision        = new RFParticleCollisionDust();
            limitations      = new RFParticleLimitations();
            rendering        = new RFParticleRendering();
            pool             = new RFParticlePool();
        }

        // Copy from
        public void CopyFrom(RayfireDust source)
        {
            onDemolition     = source.onDemolition;
            onActivation     = source.onActivation;
            onImpact         = source.onImpact;
            opacity          = source.opacity;
            dustMaterial     = source.dustMaterial;
            dustMaterials    = source.dustMaterials;
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
            
            // Set dust material and initialize
            SetDustMaterials();
            
            // Create dust ps emitter
            if (initialized == true)
                RFPoolingEmitter.CreateEmitterDust (this, transform);
        }

        // Set dust material and initialize
        void SetDustMaterials()
        {
            // Remove null materials
            if (HasMaterials == true)
                for (int i = dustMaterials.Count - 1; i >= 0; i--)
                    if (dustMaterials[i] == null)
                        dustMaterials.RemoveAt (i);
            
            // No material
            if (dustMaterial == null && HasMaterials == false)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log ("RayFire Dust: " + name + ": Dust material not defined.", gameObject);
                initialized = false;
                return;
            }
            
            // Check for deprecated burst property. Will be OBSOLETE
            if ((int)emission.burstType == 1)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log ("RayFire Dust: " + name + ": Deprecated Burst Type property.", gameObject);
            }
            
            initialized = true;
        }

        // Emit particles. 
        public void Emit()
        {
            if (rigid != null)
                RFPoolingEmitter.SetHostDust (this, transform, rigid.meshFilter, rigid.meshRenderer, rigid.meshRenderer.bounds.size.magnitude);
            else
                RFPoolingEmitter.SetHostDust (this, transform, GetComponent<MeshFilter>(), GetComponent<MeshRenderer>(), GetComponent<MeshRenderer>().bounds.size.magnitude);
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
        
        public bool HasChildren { get { return children != null && children.Count > 0; } }
        public bool HasMaterials { get { return dustMaterials != null && dustMaterials.Count > 0; } }
    }
}