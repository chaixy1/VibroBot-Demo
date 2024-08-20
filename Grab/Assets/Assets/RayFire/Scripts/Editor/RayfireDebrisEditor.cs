using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireDebris))]
    public class RayfireDebrisEditor : Editor
    {
        RayfireDebris debris;
        List<string>  layerNames;

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        const int space = 3;
        static bool exp_emit;
        static bool exp_dyn;
        static bool exp_noise;
        static bool exp_coll;
        static bool exp_lim;
        static bool exp_rend;
        static bool exp_pool;

        static readonly GUIContent gui_emit_dml     = new GUIContent ("Demolition",     "");
        static readonly GUIContent gui_emit_act     = new GUIContent ("Activation",     "");
        static readonly GUIContent gui_emit_imp     = new GUIContent ("Impact",         "");
        static readonly GUIContent gui_main_ref     = new GUIContent ("Reference",      "");
        static readonly GUIContent gui_main_mat     = new GUIContent ("Material",       "");
        static readonly GUIContent gui_ems_tp       = new GUIContent ("Type",           "");
        static readonly GUIContent gui_ems_am       = new GUIContent ("Amount",         "");
        static readonly GUIContent gui_ems_var      = new GUIContent ("Variation",      "");
        static readonly GUIContent gui_ems_rate     = new GUIContent ("Rate",           "");
        static readonly GUIContent gui_ems_dur      = new GUIContent ("Duration",       "");
        static readonly GUIContent gui_ems_life_min = new GUIContent ("Life Min",       "");
        static readonly GUIContent gui_ems_life_max = new GUIContent ("Life Max",       "");
        static readonly GUIContent gui_ems_size_min = new GUIContent ("Size Min",       "");
        static readonly GUIContent gui_ems_size_max = new GUIContent ("Size Max",       "");
        static readonly GUIContent gui_ems_mat      = new GUIContent ("Material",       "");
        static readonly GUIContent gui_dn_speed_min = new GUIContent ("Speed Min",      "");
        static readonly GUIContent gui_dn_speed_max = new GUIContent ("Speed Max",      "");
        static readonly GUIContent gui_dn_vel_min   = new GUIContent ("Velocity Min",   "");
        static readonly GUIContent gui_dn_vel_max   = new GUIContent ("Velocity Max",   "");
        static readonly GUIContent gui_dn_grav_min  = new GUIContent ("Gravity Min",    "");
        static readonly GUIContent gui_dn_grav_max  = new GUIContent ("Gravity Max",    "");
        static readonly GUIContent gui_dn_rot       = new GUIContent ("Rotation Speed", "");
        static readonly GUIContent gui_ns_en        = new GUIContent ("Enable",         "");
        static readonly GUIContent gui_ns_qual      = new GUIContent ("Quality",        "");
        static readonly GUIContent gui_ns_str_min   = new GUIContent ("Strength Min",   "");
        static readonly GUIContent gui_ns_str_max   = new GUIContent ("Strength Max",   "");
        static readonly GUIContent gui_ns_freq      = new GUIContent ("Frequency",      "");
        static readonly GUIContent gui_ns_scroll    = new GUIContent ("Scroll Speed",   "");
        static readonly GUIContent gui_ns_damp      = new GUIContent ("Damping",        "");
        static readonly GUIContent gui_col_mask     = new GUIContent ("Collides With",  "");
        static readonly GUIContent gui_col_qual     = new GUIContent ("Quality",        "");
        static readonly GUIContent gui_col_rad      = new GUIContent ("Radius Scale",   "");
        static readonly GUIContent gui_col_dmp_tp   = new GUIContent ("Type",           "");
        static readonly GUIContent gui_col_dmp_min  = new GUIContent ("Minimum",        "");
        static readonly GUIContent gui_col_dmp_max  = new GUIContent ("Maximum",        "");
        static readonly GUIContent gui_col_bnc_tp   = new GUIContent ("Type",           "");
        static readonly GUIContent gui_col_bnc_min  = new GUIContent ("Minimum",        "");
        static readonly GUIContent gui_col_bnc_max  = new GUIContent ("Maximum",        "");
        static readonly GUIContent gui_lim_min      = new GUIContent ("Min Particles",  "");
        static readonly GUIContent gui_lim_max      = new GUIContent ("Max Particles",  "");
        static readonly GUIContent gui_lim_vis      = new GUIContent ("Visible",        "Emit debris if emitting object is visible in camera view");
        static readonly GUIContent gui_lim_perc     = new GUIContent ("Percentage",     "");
        static readonly GUIContent gui_lim_size     = new GUIContent ("Size Threshold", "");
        static readonly GUIContent gui_ren_cast     = new GUIContent ("Cast",           "");
        static readonly GUIContent gui_ren_rec      = new GUIContent ("Receive",        "");
        static readonly GUIContent gui_ren_prob     = new GUIContent ("Light Probes",   "");
        static readonly GUIContent gui_ren_vect     = new GUIContent ("Motion Vectors", "");
        static readonly GUIContent gui_ren_t        = new GUIContent ("Set Tag",        "");
        static readonly GUIContent gui_ren_tag      = new GUIContent ("Tag",            "");
        static readonly GUIContent gui_ren_l        = new GUIContent ("Set Layer",      "");
        static readonly GUIContent gui_ren_lay      = new GUIContent ("Layer",          "");
        static readonly GUIContent gui_pl_max       = new GUIContent ("Capacity",       "");
        static readonly GUIContent gui_pl_re        = new GUIContent ("Reuse",          "");
        static readonly GUIContent gui_pl_ov        = new GUIContent ("   Overflow",       "");
        static readonly GUIContent gui_pl_ph        = new GUIContent ("Warmup",         "Create all pool particles in Awake");
        static readonly GUIContent gui_pl_sk        = new GUIContent ("Skip",           "Do not instantiate debris particles if there are no any particles in the pool.");
        static readonly GUIContent gui_pl_rt        = new GUIContent ("Rate",           "Amount of particle systems that will be instantiated in pool every frame");
        static readonly GUIContent gui_pl_id        = new GUIContent ("Id",             "Emitter Pool Id");
        
        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////

        private void OnEnable()
        {
            if (EditorPrefs.HasKey ("rf_de") == true) exp_emit  = EditorPrefs.GetBool ("rf_de");
            if (EditorPrefs.HasKey ("rf_dd") == true) exp_dyn   = EditorPrefs.GetBool ("rf_dd");
            if (EditorPrefs.HasKey ("rf_dn") == true) exp_noise = EditorPrefs.GetBool ("rf_dn");
            if (EditorPrefs.HasKey ("rf_dc") == true) exp_coll  = EditorPrefs.GetBool ("rf_dc");
            if (EditorPrefs.HasKey ("rf_dl") == true) exp_lim   = EditorPrefs.GetBool ("rf_dl");
            if (EditorPrefs.HasKey ("rf_dr") == true) exp_rend  = EditorPrefs.GetBool ("rf_dr");
            if (EditorPrefs.HasKey ("rf_dp") == true) exp_pool  = EditorPrefs.GetBool ("rf_dp");
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            debris = target as RayfireDebris;
            if (debris == null)
                return;
            
            GUILayout.Space (8);

            UI_Buttons();
            
            GUILayout.Space (space);
            
            UI_Emit();

            GUILayout.Space (space);

            UI_Main();

            GUILayout.Space (space);

            UI_Properties();
            
            GUILayout.Space (8);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////

        void UI_Buttons()
        {
            GUILayout.BeginHorizontal();

            if (Application.isPlaying == true)
            {
                if (GUILayout.Button ("Emit", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDebris != null)
                            (targ as RayfireDebris).Emit();

                if (GUILayout.Button ("Clean", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDebris != null)
                            (targ as RayfireDebris).Clean();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// /////////////////////////////////////////////////////////
        /// Emit
        /// /////////////////////////////////////////////////////////
        
        void UI_Emit()
        {
            GUILayout.Space (space);
            GUILayout.Label ("  Emit Event", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            // EditorGUILayout.EnumFlagsField(gui_emit_dml, debris.emission.burstType);

            EditorGUI.BeginChangeCheck();
            debris.onDemolition = EditorGUILayout.Toggle (gui_emit_dml, debris.onDemolition);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireDebris scr in targets)
                {
                    scr.onDemolition = debris.onDemolition;
                    SetDirty (scr);
                }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            debris.onActivation = EditorGUILayout.Toggle (gui_emit_act, debris.onActivation);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireDebris scr in targets)
                {
                    scr.onActivation = debris.onActivation;
                    SetDirty (scr);
                }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            debris.onImpact = EditorGUILayout.Toggle (gui_emit_imp, debris.onImpact);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireDebris scr in targets)
                {
                    scr.onImpact = debris.onImpact;
                    SetDirty (scr);
                }
        }

        /// /////////////////////////////////////////////////////////
        /// Main
        /// /////////////////////////////////////////////////////////
       
        void UI_Main()
        {
            GUILayout.Space (space);
            GUILayout.Label ("  Debris", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            debris.debrisReference = (GameObject)EditorGUILayout.ObjectField (gui_main_ref, debris.debrisReference, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireDebris scr in targets)
                {
                    scr.debrisReference = debris.debrisReference;
                    SetDirty (scr);
                }

            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            debris.debrisMaterial = (Material)EditorGUILayout.ObjectField (gui_main_mat, debris.debrisMaterial, typeof(Material), true);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireDebris scr in targets)
                {
                    scr.debrisMaterial = debris.debrisMaterial;
                    SetDirty (scr);
                }
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_Properties()
        {
            GUILayout.Space (space);
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            UI_Pool();
            
            GUILayout.Space (space);
            
            UI_Emission();

            GUILayout.Space (space);

            UI_Dynamic();

            GUILayout.Space (space);

            UI_Noise();

            GUILayout.Space (space);

            UI_Collision();

            GUILayout.Space (space);

            UI_Limitations();
            
            GUILayout.Space (space);

            UI_Rendering();
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////

        void UI_Emission()
        {
            SetFoldoutPref (ref exp_emit, "rf_de", "Emission", true);
            if (exp_emit == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                GUILayout.Label ("      Burst", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.emission.burstType = (RFParticles.BurstType)EditorGUILayout.EnumPopup (gui_ems_tp, debris.emission.burstType);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.burstType = debris.emission.burstType;
                        SetDirty (scr);
                    }
                
                if (debris.emission.burstType != RFParticles.BurstType.None)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.emission.burstAmount = EditorGUILayout.IntSlider (gui_ems_am, debris.emission.burstAmount, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.emission.burstAmount = debris.emission.burstAmount;
                            SetDirty (scr);
                        }
                    
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.emission.burstVar = EditorGUILayout.IntSlider (gui_ems_var, debris.emission.burstVar, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.emission.burstVar = debris.emission.burstVar;
                            SetDirty (scr);
                        }
                }
                
                GUILayout.Space (space);
                GUILayout.Label ("      Distance", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.emission.distanceRate = EditorGUILayout.Slider (gui_ems_rate, debris.emission.distanceRate, 0f, 5f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.distanceRate = debris.emission.distanceRate;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.emission.duration = EditorGUILayout.Slider (gui_ems_dur, debris.emission.duration, 0.5f, 10);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.duration = debris.emission.duration;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Lifetime", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.emission.lifeMin = EditorGUILayout.Slider (gui_ems_life_min, debris.emission.lifeMin, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.lifeMin = debris.emission.lifeMin;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.emission.lifeMax = EditorGUILayout.Slider (gui_ems_life_max, debris.emission.lifeMax, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.lifeMax = debris.emission.lifeMax;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Size", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.emission.sizeMin = EditorGUILayout.Slider (gui_ems_size_min, debris.emission.sizeMin, 0.001f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.sizeMin = debris.emission.sizeMin;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.emission.sizeMax = EditorGUILayout.Slider (gui_ems_size_max, debris.emission.sizeMax, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emission.sizeMax = debris.emission.sizeMax;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Material", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.emissionMaterial = (Material)EditorGUILayout.ObjectField (gui_ems_mat, debris.emissionMaterial, typeof(Material), true);
                if (EditorGUI.EndChangeCheck())
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.emissionMaterial = debris.emissionMaterial;
                        SetDirty (scr);
                    }

                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Dynamic
        /// /////////////////////////////////////////////////////////
        
        void UI_Dynamic()
        {
            SetFoldoutPref (ref exp_dyn, "rf_dd", "Dynamic", true);
            if (exp_dyn == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                GUILayout.Label ("      Speed", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.dynamic.speedMin = EditorGUILayout.Slider (gui_dn_speed_min, debris.dynamic.speedMin, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.speedMin = debris.dynamic.speedMin;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.speedMax = EditorGUILayout.Slider (gui_dn_speed_max, debris.dynamic.speedMax, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.speedMax = debris.dynamic.speedMax;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Inherit Velocity", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.dynamic.velocityMin = EditorGUILayout.Slider (gui_dn_vel_min, debris.dynamic.velocityMin, 0f, 3f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.velocityMin = debris.dynamic.velocityMin;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.velocityMax = EditorGUILayout.Slider (gui_dn_vel_max, debris.dynamic.velocityMax, 0f, 3f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.velocityMax = debris.dynamic.velocityMax;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Gravity", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.dynamic.gravityMin = EditorGUILayout.Slider (gui_dn_grav_min, debris.dynamic.gravityMin, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.gravityMin = debris.dynamic.gravityMin;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                debris.dynamic.gravityMax = EditorGUILayout.Slider (gui_dn_grav_max, debris.dynamic.gravityMax, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.gravityMax = debris.dynamic.gravityMax;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Rotation", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.dynamic.rotationSpeed = EditorGUILayout.Slider (gui_dn_rot, debris.dynamic.rotationSpeed, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.dynamic.rotationSpeed = debris.dynamic.rotationSpeed;
                        SetDirty (scr);
                    }

                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////
        
        void UI_Noise()
        {
            SetFoldoutPref (ref exp_noise, "rf_dn", "Noise", true);
            if (exp_noise == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                GUILayout.Label ("      Main", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.noise.enabled = EditorGUILayout.Toggle (gui_ns_en, debris.noise.enabled);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.noise.enabled = debris.noise.enabled;
                        SetDirty (scr);
                    }

                if (debris.noise.enabled == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.quality = (ParticleSystemNoiseQuality)EditorGUILayout.EnumPopup (gui_ns_qual, debris.noise.quality);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.quality = debris.noise.quality;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);
                    GUILayout.Label ("      Strength", EditorStyles.boldLabel);
                    GUILayout.Space (space);
                    
                    EditorGUI.BeginChangeCheck();
                    debris.noise.strengthMin = EditorGUILayout.Slider (gui_ns_str_min, debris.noise.strengthMin, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.strengthMin = debris.noise.strengthMin;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.strengthMax = EditorGUILayout.Slider (gui_ns_str_max, debris.noise.strengthMax, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.strengthMax = debris.noise.strengthMax;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);
                    GUILayout.Label ("      Other", EditorStyles.boldLabel);
                    GUILayout.Space (space);
                    
                    EditorGUI.BeginChangeCheck();
                    debris.noise.frequency = EditorGUILayout.Slider (gui_ns_freq, debris.noise.frequency, 0.001f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.frequency = debris.noise.frequency;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.scrollSpeed = EditorGUILayout.Slider (gui_ns_scroll, debris.noise.scrollSpeed, 0f, 2f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.scrollSpeed = debris.noise.scrollSpeed;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.noise.damping = EditorGUILayout.Toggle (gui_ns_damp, debris.noise.damping);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.noise.damping = debris.noise.damping;
                            SetDirty (scr);
                        }
                }

                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////
        
        void UI_Collision()
        {
            SetFoldoutPref (ref exp_coll, "rf_dc", "Collision", true);
            if (exp_coll == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                GUILayout.Label ("      Common", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                // Layer mask
                if (layerNames == null)
                {
                    layerNames = new List<string>();
                    for (int i = 0; i <= 31; i++)
                        layerNames.Add (i + ". " + LayerMask.LayerToName (i));
                }

                EditorGUI.BeginChangeCheck();
                debris.collision.collidesWith = EditorGUILayout.MaskField (gui_col_mask, debris.collision.collidesWith, layerNames.ToArray());
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.collidesWith = debris.collision.collidesWith;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.collision.quality = (ParticleSystemCollisionQuality)EditorGUILayout.EnumPopup (gui_col_qual, debris.collision.quality);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.quality = debris.collision.quality;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.collision.radiusScale = EditorGUILayout.Slider (gui_col_rad, debris.collision.radiusScale, 0.1f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.radiusScale = debris.collision.radiusScale;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Dampen", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.collision.dampenType = (RFParticleCollisionDebris.RFParticleCollisionMatType)EditorGUILayout.EnumPopup 
                    (gui_col_dmp_tp, debris.collision.dampenType);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.dampenType = debris.collision.dampenType;
                        SetDirty (scr);
                    }

                if (debris.collision.dampenType == RFParticleCollisionDebris.RFParticleCollisionMatType.ByProperties)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.dampenMin = EditorGUILayout.Slider (gui_col_dmp_min, debris.collision.dampenMin, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.dampenMin = debris.collision.dampenMin;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.dampenMax = EditorGUILayout.Slider (gui_col_dmp_max, debris.collision.dampenMax, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.dampenMax = debris.collision.dampenMax;
                            SetDirty (scr);
                        }
                }
                
                GUILayout.Space (space);
                GUILayout.Label ("      Bounce", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.collision.bounceType = (RFParticleCollisionDebris.RFParticleCollisionMatType)EditorGUILayout.EnumPopup 
                    (gui_col_bnc_tp, debris.collision.bounceType);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.collision.bounceType = debris.collision.bounceType;
                        SetDirty (scr);
                    }

                if (debris.collision.bounceType == RFParticleCollisionDebris.RFParticleCollisionMatType.ByProperties)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.bounceMin = EditorGUILayout.Slider (gui_col_bnc_min, debris.collision.bounceMin, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.bounceMin = debris.collision.bounceMin;
                            SetDirty (scr);
                        }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    debris.collision.bounceMax = EditorGUILayout.Slider (gui_col_bnc_max, debris.collision.bounceMax, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.collision.bounceMax = debris.collision.bounceMax;
                            SetDirty (scr);
                        }
                }
                
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////

        void UI_Limitations()
        {
            SetFoldoutPref (ref exp_lim, "rf_dl", "Limitations", true);
            if (exp_lim == true)
            {
                EditorGUI.indentLevel++;
             
                GUILayout.Space (space);
                GUILayout.Label ("      Particle System", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.minParticles = EditorGUILayout.IntSlider (gui_lim_min, debris.limitations.minParticles, 3, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.minParticles = debris.limitations.minParticles;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.maxParticles = EditorGUILayout.IntSlider (gui_lim_max, debris.limitations.maxParticles, 5, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.maxParticles = debris.limitations.maxParticles;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.visible = EditorGUILayout.Toggle (gui_lim_vis, debris.limitations.visible);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.visible = debris.limitations.visible;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                GUILayout.Label ("      Fragments", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.percentage = EditorGUILayout.IntSlider (gui_lim_perc, debris.limitations.percentage, 10, 100);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.percentage = debris.limitations.percentage;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.limitations.sizeThreshold = EditorGUILayout.Slider (gui_lim_size, debris.limitations.sizeThreshold, 0.05f, 5);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.limitations.sizeThreshold = debris.limitations.sizeThreshold;
                        SetDirty (scr);
                    }
                
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Rendering
        /// /////////////////////////////////////////////////////////

        void UI_Rendering()
        {
            SetFoldoutPref (ref exp_rend, "rf_dr", "Rendering", true);
            if (exp_rend == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                GUILayout.Label ("      Shadows", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.castShadows = EditorGUILayout.Toggle (gui_ren_cast, debris.rendering.castShadows);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.castShadows = debris.rendering.castShadows;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.receiveShadows = EditorGUILayout.Toggle (gui_ren_rec, debris.rendering.receiveShadows);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.receiveShadows = debris.rendering.receiveShadows;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                GUILayout.Label ("      Other", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.lightProbes = (LightProbeUsage)EditorGUILayout.EnumPopup (gui_ren_prob, debris.rendering.lightProbes);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.lightProbes = debris.rendering.lightProbes;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.motionVectors = (MotionVectorGenerationMode)EditorGUILayout.EnumPopup (gui_ren_vect, debris.rendering.motionVectors);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.motionVectors = debris.rendering.motionVectors;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.t = EditorGUILayout.Toggle (gui_ren_t, debris.rendering.t);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.t = debris.rendering.t;
                        SetDirty (scr);
                    }

                if (debris.rendering.t == true)
                {
                    GUILayout.Space (space);
                    
                    EditorGUI.indentLevel++;
                    
                    EditorGUI.BeginChangeCheck();
                    debris.rendering.tag = EditorGUILayout.TagField (gui_ren_tag, debris.rendering.tag);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.rendering.tag = debris.rendering.tag;
                            SetDirty (scr);
                        }

                    EditorGUI.indentLevel--;
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                debris.rendering.l = EditorGUILayout.Toggle (gui_ren_l, debris.rendering.l);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.rendering.l = debris.rendering.l;
                        SetDirty (scr);
                    }
                
                if (debris.rendering.l == true)
                {
                    GUILayout.Space (space);
                    
                    EditorGUI.indentLevel++;
                    
                    EditorGUI.BeginChangeCheck();
                    debris.rendering.layer = EditorGUILayout.LayerField (gui_ren_lay, debris.rendering.layer);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.rendering.tag = debris.rendering.tag;
                            SetDirty (scr);
                        }

                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Pool
        /// /////////////////////////////////////////////////////////

        void UI_Pool()
        {
            SetFoldoutPref (ref exp_pool, "rf_dp", "Pool", true);
            if (exp_pool == true)
            {
                EditorGUI.indentLevel++;
                
                GUILayout.Space (space);
                EditorGUI.BeginChangeCheck();
                debris.pool.id = EditorGUILayout.IntSlider (gui_pl_id, debris.pool.id, 0, 99);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.pool.id = debris.pool.id;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                EditorGUI.BeginChangeCheck();
                debris.pool.enable = EditorGUILayout.Toggle (gui_ns_en, debris.pool.enable);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.pool.Enable = debris.pool.enable;
                        SetDirty (scr);
                    }
                
                GUILayout.Space (space);
                EditorGUI.BeginChangeCheck();
                debris.pool.warmup = EditorGUILayout.Toggle (gui_pl_ph, debris.pool.warmup);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.pool.warmup = debris.pool.warmup;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                EditorGUI.BeginChangeCheck();
                debris.pool.cap = EditorGUILayout.IntSlider (gui_pl_max, debris.pool.cap, 3, 300);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.pool.Cap = debris.pool.cap;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                EditorGUI.BeginChangeCheck();
                debris.pool.rate = EditorGUILayout.IntSlider (gui_pl_rt, debris.pool.rate, 1, 15);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.pool.Rate = debris.pool.rate;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                EditorGUI.BeginChangeCheck();
                debris.pool.skip = EditorGUILayout.Toggle (gui_pl_sk, debris.pool.skip);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.pool.Skip = debris.pool.skip;
                        SetDirty (scr);
                    }

                GUILayout.Space (space);
                EditorGUI.BeginChangeCheck();
                debris.pool.reuse = EditorGUILayout.Toggle (gui_pl_re, debris.pool.reuse);
                if (EditorGUI.EndChangeCheck() == true)
                    foreach (RayfireDebris scr in targets)
                    {
                        scr.pool.Reuse = debris.pool.reuse;
                        SetDirty (scr);
                    }

                if (debris.pool.reuse == true)
                {
                    GUILayout.Space (space);
                    EditorGUI.BeginChangeCheck();
                    debris.pool.over = EditorGUILayout.IntSlider (gui_pl_ov, debris.pool.over, 0, 10);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireDebris scr in targets)
                        {
                            scr.pool.Over = debris.pool.over;
                            SetDirty (scr);
                        }
                }

                // Caption
                if (debris.pool.enable == true && Application.isPlaying == true)
                {
                    GUILayout.Space (space);
                    if (debris.pool.emitter != null)
                        GUILayout.Label ("     Available : " + debris.pool.emitter.queue.Count, EditorStyles.boldLabel);
                }
                
                // Edit
                if (Application.isPlaying == true)
                {
                    GUILayout.Space (space);
                    if (GUILayout.Button ("Edit Emitter Particles", GUILayout.Height (20)))
                        foreach (var targ in targets)
                            if (targ as RayfireDebris != null)
                                (targ as RayfireDebris).EditEmitterParticles();
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireDebris scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
            }
        }
        
        void SetFoldoutPref (ref bool val, string pref, string caption, bool state) 
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, state);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
        }
    }
}