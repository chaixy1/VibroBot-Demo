using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RayFire
{
    // Class to store animation cache
    [Serializable]
    public class RFCache
    {
        // Vars
        public string           name;
        public List<bool>       act;
        public List<Vector3>    pos;
        public List<Quaternion> rot;
        public Transform        trm;

        // Constructor
        public RFCache (Transform recRootTm, Transform tm, int offset = 0)
        {
            trm  = tm;
            name = tm.name;
            act  = new List<bool>();
            pos  = new List<Vector3>();
            rot  = new List<Quaternion>();
            
            // Add activation offset to lists
            SetOffset (this, offset);
            
            // Not child of main recorder root. Name should include all parents names up to main recorder root
            if (tm.parent != recRootTm)
            {
                Transform lastParent = tm.parent;
                while (recRootTm != lastParent)
                {
                    name       = name.Insert (0, "/");
                    name       = name.Insert (0, lastParent.name);
                    lastParent = lastParent.parent;
                }
            }
        }

        // Set activation offset 
        static void SetOffset(RFCache cache, int offset)
        {
            if (offset <= 0)
                return;
            
            for (int i = 0; i < offset; i++)
            {
                cache.act.Add (false);
                cache.pos.Add (cache.trm.localPosition);
                cache.rot.Add (cache.trm.localRotation);
            }
        }
    }

    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Recorder")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-recorder-component/")]
    public class RayfireRecorder : MonoBehaviour
    {
        public enum AnimatorType
        {
            Disabled = 0,
            Record   = 2,
            Play     = 8
        }

        public enum RigidActionType
        {
            Disable      = 0,
            SetKinematik = 2
        }
        
        // Main
        public AnimatorType mode = AnimatorType.Record;

        // Record
        public string folder = "RayFireRecords/";
        public string clipName;
        public bool   recordOnStart = true;
        public float  duration      = 5f;
        public int    rate          = 15;
        public bool   reduceKeys    = true;
        public float  threshold;
        public bool   demolition = true;
        
        // Playback
        public bool                      playOnStart;
        public RigidActionType           rigidAction;
        public AnimationClip             animationClip;
        public RuntimeAnimatorController controller;
        
        // Public Non Serialized
        [NonSerialized] public bool  recorder;
        [NonSerialized] public float recordedTime;

        // Private Non Serialized
        [NonSerialized] public List<GameObject>   pfList;
        [NonSerialized]        string             assetFolder;
        [NonSerialized]        float              stepTime;
        [NonSerialized]        Animator           animator;
        [NonSerialized]        List<Transform>    tmList;
        [NonSerialized]        List<RFCache>      cacheList;
        [NonSerialized]        List<float>        timeList;
        [NonSerialized]        List<RayfireRigid> rigids;
        
        // Static
        string recordFolder = "RayFireRecords/";
        
        /// //////////////////////////////////////////////////
        /// Common
        /// //////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Set vars
            SetVariables();
        }

        // Start
        void Start()
        {
            // Collect rigid
            SetRigidPlay();

            // Start ops
            if (mode == AnimatorType.Record && recordOnStart == true)
                StartRecord();
            else if (mode == AnimatorType.Play && playOnStart == true)
                StartPlay();
        }

        // Set vars
        void SetVariables()
        {
            if (mode != AnimatorType.Disabled)
            {
                animator = GetComponent<Animator>();

                // Get list of cached transforms
                tmList = gameObject.GetComponentsInChildren<Transform> (false).ToList();
                tmList.Remove (transform);

                // No children
                if (tmList.Count == 0)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but object has no children. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Record set
                SetModeRecord();

                // Play set
                SetModePlay();
            }
        }
        
        // Play set
        void SetModePlay()
        {
            if (mode == AnimatorType.Play)
            {
                // Check for null controller
                if (controller == null)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but controller is not defined. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Check for null controller
                if (animationClip == null)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but animation clip is not defined. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Check for clip in controller
                bool hasClip = false;
                foreach (var anim in controller.animationClips)
                    if (anim == animationClip)
                        hasClip = true;
                if (hasClip == false)
                {
                    Debug.Log ("RayFire Record: " + gameObject.name + " Mode set to " + mode.ToString() + " but animation clip is not defined in controller. Mode set to None.", gameObject);
                    mode = AnimatorType.Disabled;
                    return;
                }

                // Create animator
                if (animator == null)
                    animator = gameObject.AddComponent<Animator>();
                animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

                // Set defined controller
                animator.runtimeAnimatorController = controller;
            }
        }

        /// //////////////////////////////////////////////////
        /// Record
        /// //////////////////////////////////////////////////

        // Record set
        void SetModeRecord()
        {
            if (mode == AnimatorType.Record)
            {
                // Null active controller 
                if (animator != null)
                    animator.runtimeAnimatorController = null;

                // Prepare cache list
                if (tmList.Count > 0)
                {
                    cacheList = new List<RFCache>();
                    for (int i = 0; i < tmList.Count; i++)
                        cacheList.Add (new RFCache (transform, tmList[i]));
                }
                
                // Time list
                timeList = new List<float>();
                
                // Clip folder
                assetFolder = "Assets/" + recordFolder;
                
                // Rigid Runtime Demolition
                if (demolition == true)
                    SetRigidRecord();
            }
        }
        
        // Start record
        public void StartRecord()
        {
            // Stop
            if (cacheList.Count == 0)
                return;
            
            // Set demolition parent to parent of demolished object in order to record animation clip
            RayfireMan.inst.advancedDemolitionProperties.parent       = RFManDemolition.FragmentParentType.GlobalParent;
            RayfireMan.inst.advancedDemolitionProperties.globalParent = transform;
            
            // Start recording cor
            StartCoroutine (RecordCor());
        }

        // Stop record
        public void StopRecord()
        {
            recorder = false;
        }
        
        // Reset
        void Reset()
        {
            clipName = gameObject.name;
        }
        
        // Record tm every frame
        IEnumerator RecordCor()
        {
            // Set time step
            stepTime = 1.0f / rate;
            
            // Set the playback framerate. IMPORTANT: use for smooth keys recording
            Time.captureDeltaTime = stepTime;
            
            recorder = true;
            while (recorder == true)
            {
                // Save data
                timeList.Add (recordedTime);
                CacheFrame();

                // Set time
                recordedTime += stepTime;
 
                // Temp
                if (duration > 0 && recordedTime > duration)
                    StopRecord();
                
                // Wait
                yield return new WaitForSeconds (stepTime);
            }
            
            #if UNITY_EDITOR
            
            // Create clip 
            RFRecorder.CreateAnimationClip (cacheList, timeList, threshold, rate, assetFolder, clipName, reduceKeys);
            
            // Destroy prefab components
            RFRecorder.DestroyPrefabComponents(pfList);
            #endif
        }
        
        // Cache frame data
        void CacheFrame()
        {
            for (int i = 0; i < tmList.Count; i++)
            {
                if (tmList[i] == null)
                    continue;
                    
                cacheList[i].act.Add (tmList[i].gameObject.activeSelf);
                cacheList[i].pos.Add (tmList[i].localPosition);
                cacheList[i].rot.Add (tmList[i].localRotation);
            }
        }

        /// //////////////////////////////////////////////////
        /// Play
        /// //////////////////////////////////////////////////
        
        // Start play
        public void StartPlay()
        {
            if (mode == AnimatorType.Play)
                animator.Play (animationClip.name);
        }
        
        /// //////////////////////////////////////////////////
        /// Rigid
        /// //////////////////////////////////////////////////
        
        // Set rigid props
        void SetRigidRecord()
        {
            // Destroy lists
            pfList = new List<GameObject>();
            
            // Get all Rigids
            rigids = gameObject.GetComponentsInChildren<RayfireRigid>().ToList();

            // Setup
            PrepareRigidRecord (rigids);
        }

        // Prepare Rigid for record
        void PrepareRigidRecord(List<RayfireRigid> rigidList)
        {
            for (int i = 0; i < rigidList.Count; i++)
            {
                if (rigidList[i].demolitionType == DemolitionType.Runtime || 
                    rigidList[i].demolitionType == DemolitionType.AwakePrecache || 
                    rigidList[i].demolitionType == DemolitionType.AwakePrefragment)
                {
                    // Used by Recorder record
                    rigidList[i].physics.rec = true;
                    
                    // One level of demolition TEMP
                    // rigids[i].limitations.depth = 1;
                    
                    // Do not destroy after demolition
                    rigidList[i].reset.action     = RFReset.PostDemolitionType.DeactivateToReset;
                    rigidList[i].limitations.desc = new List<RayfireRigid>();
                    
                    // Subscribe to demolition
                    #if UNITY_EDITOR
                    rigidList[i].demolitionEvent.LocalEvent += RigidDemolition;
                    #endif                    
                }
            }
        }
        
        // Set rigid props
        void SetRigidPlay()
        {
            if (mode == AnimatorType.Play)
            {
                rigids = gameObject.GetComponentsInChildren<RayfireRigid>().ToList();
                foreach (RayfireRigid rigid in rigids)
                {
                    if (rigid.physics.exclude == false)
                    {
                        // Used by Recorder playback
                        rigid.physics.rec = true;

                        // Disable Rigid
                        if (rigidAction == RigidActionType.Disable)
                        {
                            rigid.enabled = false;
                        }
                        
                        // Check for kinematic state
                        else if (rigidAction == RigidActionType.SetKinematik)
                        {
                            rigid.simulationType = SimType.Kinematic;
                            RFPhysic.SetSimulationType (rigid.physics.rigidBody, rigid.simulationType, rigid.objectType, rigid.physics.gr, rigid.physics.si, rigid.physics.st);
                        }
                    }
                }
            }
        }

        /// //////////////////////////////////////////////////
        /// Demolition event
        /// //////////////////////////////////////////////////
        
        // Runtime demolition ops
        void RigidDemolition(RayfireRigid rigid)
        {
            // Mesh to fragments demolition
            if (rigid.objectType == ObjectType.Mesh)
            {
                // Collect root and fragments to continue record animation
                tmList.Add (rigid.rootChild);
                cacheList.Add (new RFCache (transform, rigid.rootChild, timeList.Count));
                for (int i = 0; i < rigid.fragments.Count; i++)
                {
                    tmList.Add (rigid.fragments[i].transform);
                    cacheList.Add (new RFCache (transform, rigid.fragments[i].transform, timeList.Count));
                }
                
                // Prepare new Rigids for secondary demolition record
                PrepareRigidRecord (rigid.fragments);

                // Export fragments meshes into asset
                #if UNITY_EDITOR
                RFRecorder.ExportAssets (rigid, this);
                #endif   
            }
            
            // TODO other demolition types support
        }
    }
}