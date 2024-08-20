using UnityEngine.Rendering;
using UnityEngine;

namespace RayFire
{
    [System.Serializable]
    public class RFParticles
    {
        public enum BurstType
        {
            None         = 0,
            AmountPerUnit  = 2,
            AmountAndVariation = 3
        }
        
        // Static
        static ParticleSystem.MinMaxCurve stConstantMinMaxCurve = new ParticleSystem.MinMaxCurve(1, 2);
        static ParticleSystem.Burst       stBurst               = new ParticleSystem.Burst(0.0f, 5, 5, 1, 999f);
        static ParticleSystem.Burst[]     stBursts              = new [] { stBurst };
        
        // Static SetSizeOverLifeTime vars
        static Keyframe[] stSizeLifeKeys = {
            new Keyframe(0f,    0f),
            new Keyframe(0.01f, 1f),
            new Keyframe(0.90f, 1f),
            new Keyframe(1f,    0f),
        };
        static ParticleSystem.MinMaxCurve stSizeLifeCurve = new ParticleSystem.MinMaxCurve (1f, new AnimationCurve(stSizeLifeKeys));
        
        // SetColorOverLife vars
        static Gradient stGradient;
        static ParticleSystem.MinMaxGradient stMinMaxGradient;

        // SetRotationOverLifeTime vars
        static AnimationCurve             stCurveMaxRotationLife;
        static AnimationCurve             stCurveMinRotationLife;
        static ParticleSystem.MinMaxCurve stCurveMinMaxRotationLife;
        
        // SetRotationBySpeed
        static AnimationCurve             stCurveMinRotationSpeed = new AnimationCurve (new Keyframe(0f, 1f), new Keyframe(0.5f, 0f));
        static AnimationCurve             stCurveMaxRotationSpeed = new AnimationCurve (new Keyframe(0f, 1f), new Keyframe(0.5f, 0f));
        static ParticleSystem.MinMaxCurve stCurveRotationSpeed    = new ParticleSystem.MinMaxCurve(1f, stCurveMinRotationSpeed, stCurveMaxRotationSpeed);

        /// /////////////////////////////////////////////////////////
        /// Main Module
        /// /////////////////////////////////////////////////////////

        // Set main module
        public static void SetMain (ParticleSystem.MainModule main, 
            float lifeMin, float lifeMax, 
            float sizeMin, float sizeMax, 
            float gravityMin, float gravityMax, 
            float speedMin, float speedMax,
            float divergence, int maxParticles,
            float duration)
        {
            main.duration            = duration;
            main.loop                = false;
            main.simulationSpace     = ParticleSystemSimulationSpace.World;
            main.maxParticles        = maxParticles;
            main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Transform;
            
            stConstantMinMaxCurve.constantMin = lifeMin;
            stConstantMinMaxCurve.constantMax = lifeMax;
            main.startLifetime = stConstantMinMaxCurve;
            
            stConstantMinMaxCurve.constantMin = speedMin;
            stConstantMinMaxCurve.constantMax = speedMax;
            main.startSpeed = stConstantMinMaxCurve;
            
            stConstantMinMaxCurve.constantMin = sizeMin;
            stConstantMinMaxCurve.constantMax = sizeMax;
            main.startSize = stConstantMinMaxCurve;
            
            stConstantMinMaxCurve.constantMin = -divergence;
            stConstantMinMaxCurve.constantMax = divergence;
            main.startRotation = stConstantMinMaxCurve; // Max 6.25f = 360 degree
            
            stConstantMinMaxCurve.constantMin = gravityMin;
            stConstantMinMaxCurve.constantMax = gravityMax;
            main.gravityModifier = stConstantMinMaxCurve;
            
            // Destroy after stop
            main.stopAction = ParticleSystemStopAction.Destroy;
        }
        
        // Set particle to be reused after stop
        public static void Reuse (ParticleSystem pSystem, RFPoolingEmitter emitter)
        {
            if (emitter.reuse == true)
            {
                SetMainStopAction (pSystem.main);
                RFParticleMan man = pSystem.gameObject.GetComponent<RFParticleMan>();
                if (man == null)
                    man = pSystem.gameObject.AddComponent<RFParticleMan>();
                man.em = emitter;
                man.ps = pSystem;
                man.tm = pSystem.transform;
            }
        }
        
        // Set main module
        static void SetMainStopAction (ParticleSystem.MainModule main)
        {
            // Callback script after stop
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////

        // Set emission
        public static void SetEmission(ParticleSystem.EmissionModule emissionModule, float distanceRate, int burstAmount, int burstVar)
        {
            emissionModule.enabled = true;
            emissionModule.rateOverTimeMultiplier = 0f;
            emissionModule.rateOverDistanceMultiplier = distanceRate;

            // Set burst
            if (burstAmount > 0)
            {
                stBursts[0].minCount = (short)burstAmount;
                stBursts[0].maxCount = (short)(burstAmount + burstVar);
                emissionModule.SetBursts(stBursts);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////
        
        // Set host emitter shape to mesh 
        public static void SetShapeMesh(ParticleSystem.ShapeModule shape, MeshFilter mf, Transform tm, Renderer mr, Material emissionMaterial)
        {
            if (mf != null)
            {
                int emitMatIndex = 0;
                if (emissionMaterial != null)
                {
                    if (mr == null)
                        mr = tm.GetComponent<Renderer>();
                    emitMatIndex = GetEmissionMatIndex (mr, emissionMaterial);
                }
                SetShapeModule (shape, mf.sharedMesh, emitMatIndex, tm.localScale);
            }
            else
                SetShapeHemisphere(shape);
        }
        
        // Set emitter mesh shape
        static int GetEmissionMatIndex(Renderer renderer, Material mat)
        {
            if (mat != null && renderer != null)
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    if (renderer.sharedMaterials[i] == mat)
                        return i;
            return -1;
        }
        
        // Set emitter mesh shape
        static void SetShapeModule (ParticleSystem.ShapeModule shapeModule, Mesh mesh, int emitMatIndex, Vector3 shapeScale)
        {
            shapeModule.shapeType     = ParticleSystemShapeType.Mesh;
            shapeModule.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shapeModule.mesh          = mesh;
            shapeModule.useMeshColors = false;
            shapeModule.normalOffset  = 0f;
            shapeModule.scale         = shapeScale;
            
            // Emit from inner surface
            if (emitMatIndex > 0)
            {
                shapeModule.useMeshMaterialIndex = true;
                shapeModule.meshMaterialIndex = emitMatIndex;
            }
        }

        // Set emitter mesh shape
        public static void SetShapeHemisphere(ParticleSystem.ShapeModule shapeModule)
        {
            shapeModule.shapeType = ParticleSystemShapeType.Hemisphere;
            shapeModule.radius = 0.2f;
            shapeModule.radiusThickness = 0f;
        }

        /// /////////////////////////////////////////////////////////
        /// Velocity
        /// /////////////////////////////////////////////////////////

        // Set velocity
        public static void SetVelocity(ParticleSystem.InheritVelocityModule velocity, RFParticleDynamicDebris dynamic)
        {
            if (dynamic.velocityMin > 0 || dynamic.velocityMax > 0)
            {
                velocity.enabled = true;
                velocity.mode    = ParticleSystemInheritVelocityMode.Initial;
                
                stConstantMinMaxCurve.constantMin = dynamic.velocityMin;
                stConstantMinMaxCurve.constantMax = dynamic.velocityMax;
                velocity.curve   = stConstantMinMaxCurve;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Rotation Over Lifetime
        /// /////////////////////////////////////////////////////////
        
        // Set Rotation
        public static void SetRotationOverLifeTime(ParticleSystem.RotationOverLifetimeModule rotation, RFParticleDynamicDust dynamic)
        {
            if (dynamic.rotation > 0)
            {
                rotation.enabled          = true;
                rotation.separateAxes     = true;
                stCurveMinRotationLife    = new AnimationCurve (new Keyframe(0f, dynamic.rotation  * 4f),  new Keyframe(1f, dynamic.rotation  * 4f * 0.1f));
                stCurveMaxRotationLife    = new AnimationCurve (new Keyframe(0f, -dynamic.rotation  * 4f), new Keyframe(1f, -dynamic.rotation  * 4f * 0.1f));
                stCurveMinMaxRotationLife = new ParticleSystem.MinMaxCurve (1f, stCurveMinRotationLife, stCurveMaxRotationLife);
                rotation.z                = stCurveMinMaxRotationLife;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Size Over Life Time
        /// /////////////////////////////////////////////////////////

        // Set size over life time. different axis. Increase almost instantly particles after birth
        public static void SetSizeOverLifeTime(ParticleSystem.SizeOverLifetimeModule sizeOverLifeTime)
        {
            sizeOverLifeTime.enabled = true;
            sizeOverLifeTime.size    = stSizeLifeCurve;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rotation by Speed
        /// /////////////////////////////////////////////////////////
        
        // Set Rotation by Speed
        public static void SetRotationBySpeed(ParticleSystem.RotationBySpeedModule rotationBySpeed, float rotationSpeed)
        {
            if (rotationSpeed > 0f)
            {
                rotationBySpeed.enabled = true;
                rotationBySpeed.range   = new Vector2 (1f, 0f);
                stCurveMinRotationSpeed = new AnimationCurve (new Keyframe(0f, rotationSpeed * 40f),  new Keyframe(0.5f, 0f));
                stCurveMaxRotationSpeed = new AnimationCurve (new Keyframe(0f, -rotationSpeed * 40f), new Keyframe(0.5f, 0f));
                stCurveRotationSpeed    = new ParticleSystem.MinMaxCurve(1f, stCurveMinRotationSpeed, stCurveMaxRotationSpeed);
                rotationBySpeed.z       = stCurveRotationSpeed;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Color Over Lifetime
        /// /////////////////////////////////////////////////////////
        
        // Set color over life time
        public static void SetColorOverLife(ParticleSystem.ColorOverLifetimeModule colorLife, float opacity)
        {
            colorLife.enabled                            = true;
            stGradient = new Gradient
            {
                alphaKeys = new []
                {
                    new GradientAlphaKey (0f,      0f), 
                    new GradientAlphaKey (opacity, 0.1f),
                    new GradientAlphaKey (opacity, 0.2f),
                    new GradientAlphaKey (0f,      1f)
                }
            };
            stMinMaxGradient = new ParticleSystem.MinMaxGradient (stGradient);
            colorLife.color  = stMinMaxGradient;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////

        // Set particle system noise
        public static void SetNoise (ParticleSystem.NoiseModule psNoise, RFParticleNoise scrNoise)
        {
            psNoise.enabled = scrNoise.enabled;
            if (scrNoise.enabled == true)
            {
                psNoise.strength     = new ParticleSystem.MinMaxCurve (scrNoise.strengthMin, scrNoise.strengthMax);
                psNoise.frequency    = scrNoise.frequency;
                psNoise.scrollSpeed  = scrNoise.scrollSpeed;
                psNoise.damping      = scrNoise.damping;
                psNoise.quality      = scrNoise.quality;
                psNoise.separateAxes = true;

                stConstantMinMaxCurve.constantMin = scrNoise.strengthMin;
                stConstantMinMaxCurve.constantMax = scrNoise.strengthMax;
                psNoise.strengthX         = stConstantMinMaxCurve;

                stConstantMinMaxCurve.constantMin = scrNoise.strengthMin * 0.3f;
                stConstantMinMaxCurve.constantMax = scrNoise.strengthMax * 0.3f;
                psNoise.strengthY         = stConstantMinMaxCurve;

                stConstantMinMaxCurve.constantMin = scrNoise.strengthMin;
                stConstantMinMaxCurve.constantMax = scrNoise.strengthMax;
                psNoise.strengthZ         = stConstantMinMaxCurve;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        // Set collision for debris
        public static void SetCollisionDebris (ParticleSystem.CollisionModule colMod, RFParticleCollisionDebris colDeb) {
            colMod.enabled                = true;
            colMod.type                   = ParticleSystemCollisionType.World;
            colMod.collidesWith           = colDeb.collidesWith;
            colMod.quality                = colDeb.quality;
            colMod.radiusScale            = colDeb.radiusScale;
            colMod.enableDynamicColliders = true;
            
            colDeb.SetMaterialProps ();
            stConstantMinMaxCurve.constantMin = colDeb.dampenMin;
            stConstantMinMaxCurve.constantMax = colDeb.dampenMax;
            colMod.dampen                     = stConstantMinMaxCurve;
            stConstantMinMaxCurve.constantMin = colDeb.bounceMin;
            stConstantMinMaxCurve.constantMax = colDeb.bounceMax;
            colMod.bounce                     = stConstantMinMaxCurve;
        }

        // Set collision for dust
        public static void SetCollisionDust (ParticleSystem.CollisionModule psCollision, RFParticleCollisionDust coll) {
            psCollision.enabled                = true;
            psCollision.type                   = ParticleSystemCollisionType.World;
            psCollision.collidesWith           = coll.collidesWith;
            psCollision.quality                = coll.quality;
            psCollision.radiusScale            = coll.radiusScale;
            psCollision.enableDynamicColliders = false;
            psCollision.dampenMultiplier       = 0f;
            psCollision.bounceMultiplier       = 0f;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Renderer
        /// /////////////////////////////////////////////////////////
        
        // Set renderer
        public static void SetParticleRendererDebris (ParticleSystemRenderer rn, RayfireDebris scr)
        {
            // Common vars
            rn.renderMode = ParticleSystemRenderMode.Mesh;
            rn.alignment  = ParticleSystemRenderSpace.World;
            rn.sortMode   = ParticleSystemSortMode.None;
            
            // Set predefined meshes
            if (RayfireDebris.stMsh.Length > 0)
            {
                rn.SetMeshes (RayfireDebris.stMsh);
                rn.mesh = RayfireDebris.stMsh[0];
            }

            // Set material
            rn.sharedMaterial = scr.debrisMaterial;

            // Common rendering properties
            SetParticleRendererCommon (rn, scr.rendering);

            // Reset
            RayfireDebris.stMsh = null;
        }
        
        // Set renderer
        public static void SetParticleRendererDust(ParticleSystemRenderer rn, RayfireDust scr)
        {
            // Common vars
            rn.renderMode = ParticleSystemRenderMode.Billboard;
            rn.alignment  = ParticleSystemRenderSpace.Facing;
            rn.sortMode   = ParticleSystemSortMode.OldestInFront;

            // Set material
            rn.sharedMaterial = scr.HasMaterials == true 
                ? scr.dustMaterials[Random.Range (0, scr.dustMaterials.Count)] 
                : scr.dustMaterial;
            
            // Dust vars
            rn.minParticleSize = 0.0001f;
            rn.maxParticleSize = 999999f;

            // Common rendering properties
            SetParticleRendererCommon (rn, scr.rendering);
        }
        
        // Common rendering properties
        static void SetParticleRendererCommon(ParticleSystemRenderer rn, RFParticleRendering rendering)
        {
            // Shadow casting
            rn.shadowCastingMode = rendering.castShadows == true 
                ? ShadowCastingMode.On 
                : ShadowCastingMode.Off;

            // Props
            rn.receiveShadows             = rendering.receiveShadows;
            rn.lightProbeUsage            = rendering.lightProbes;
            rn.motionVectorGenerationMode = rendering.motionVectors;
            
            // Tag
            if (rendering.t == true)
            {
                if (string.IsNullOrEmpty (rendering.tag))
                    rendering.tag = "Untagged";
                rn.gameObject.tag = rendering.tag;
            }
            else
                rn.gameObject.tag = "Untagged";

            // Layer
            rn.gameObject.layer = rendering.l == true 
                ? rendering.layer 
                : 0;
        }
    }
}


