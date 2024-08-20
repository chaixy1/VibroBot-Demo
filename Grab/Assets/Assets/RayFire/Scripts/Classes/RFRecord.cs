#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace RayFire
{
    public static class RFRecorder
    {
        // Create animation clip
        public static void CreateAnimationClip(List<RFCache> cacheList, List<float> timeList, float threshold, int rate, string assetFolder, string clipName, bool optimizeKeys)
        {
            // Stop
            if (timeList.Count == 0)
                return;
            
            // Create main clip
            AnimationClip clip  = new AnimationClip();
            clip.legacy = false;
            clip.frameRate = rate;
            clip.name = clipName + "_animation";
            
            // Create curves for each object
            foreach (RFCache cache in cacheList)
            {
                if (cache.trm == null)
                    continue;
                
                // Position
                SetCurvePosition (ref clip, cache.pos, timeList, 0, cache.name, "localPosition.x", threshold, rate, optimizeKeys);
                SetCurvePosition (ref clip, cache.pos, timeList, 1, cache.name, "localPosition.y", threshold, rate, optimizeKeys);
                SetCurvePosition (ref clip, cache.pos, timeList, 2, cache.name, "localPosition.z", threshold, rate, optimizeKeys);
                    
                // Rotation
                SetCurveRotation (ref clip, cache.rot, timeList, 0, cache.name, "localRotation.x", threshold, rate, optimizeKeys);
                SetCurveRotation (ref clip, cache.rot, timeList, 1, cache.name, "localRotation.y", threshold, rate, optimizeKeys);
                SetCurveRotation (ref clip, cache.rot, timeList, 2, cache.name, "localRotation.z", threshold, rate, optimizeKeys);
                SetCurveRotation (ref clip, cache.rot, timeList, 3, cache.name, "localRotation.w", threshold, rate, optimizeKeys);
               
                // Active
                SetCurveActive (ref clip, cache.act, timeList, cache.name, "m_IsActive", threshold, rate, optimizeKeys);
            }

            // Set Folder
            if (Directory.Exists (assetFolder) == false)
                Directory.CreateDirectory(assetFolder);
            
            // Save clip asset
            string clipPath = assetFolder + clipName + "_animation.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            
            // Save controller
            string controllerPath = assetFolder + clipName + "_controller.controller";
            AnimatorController cont = AnimatorController.CreateAnimatorControllerAtPath (controllerPath);
            AnimatorStateMachine stateMachine = cont.layers[0].stateMachine;

             // Set clip and states
            SetStates (stateMachine, clip);
            
            // Asset ops
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        // Set clip and states
        static int SetStates (AnimatorStateMachine stateMachine, AnimationClip clip)
        {
            // Empty entry state
            AnimatorState emptyState = stateMachine.AddState ("EmptyState");
            emptyState.speed = 0;
            emptyState.writeDefaultValues = false;
            
            // Animation state
            AnimatorState recordState = stateMachine.AddState (clip.name);
            recordState.motion = clip;
            recordState.tag = clip.name + "Tag";
    
            // Exit transition
            AnimatorStateTransition exitTransition = recordState.AddExitTransition();
            exitTransition.duration = 0;
            exitTransition.hasExitTime = true;

            return recordState.nameHash;
        }
        
        /// //////////////////////////////////////////////////
        /// Demolition exports
        /// //////////////////////////////////////////////////
        
        // Save demolished rigid fragments
        public static void ExportAssets(RayfireRigid rigid,  RayfireRecorder recorder)
        {
            // Get fragments meshfilters
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            for (int i = 0; i < rigid.fragments.Count; i++)
                meshFilters.Add (rigid.fragments[i].meshFilter);
            
            // Instantiate mesh in order to export it into asset
            foreach (var mf in meshFilters)
            {
                Mesh tempMesh = Object.Instantiate(mf.sharedMesh);
                tempMesh.name = mf.name;
                mf.sharedMesh = tempMesh;
            }
            
            // Export meshes into asset
            ExportMeshes (meshFilters, rigid.rootChild.name, recorder.clipName);

            // Export prefabs
            ExportPrefabs (rigid, recorder);
        }

        // Export meshes into asset
        static void ExportMeshes(List<MeshFilter> meshFilters, string rootName, string clipName)
        {
            // Create meshes folder
            string folderMeshes = "Assets/RayFireRecords/" + clipName + "_meshes/";
            if (Directory.Exists(folderMeshes) == false)
                Directory.CreateDirectory (folderMeshes);

            // Get save path
            string savePath = folderMeshes + rootName + ".asset";
            
            // Create asset
            AssetDatabase.CreateAsset(new Mesh(), savePath);
            AssetDatabase.AddObjectToAsset (new Mesh(), savePath);
            
            // Save each fragment mesh
            for (int i = 0; i < meshFilters.Count; i++)
                AssetDatabase.AddObjectToAsset (meshFilters[i].sharedMesh, savePath);

            // Save
            AssetDatabase.SaveAssets();
        }

        // Export prefabs
        static void ExportPrefabs(RayfireRigid rigid, RayfireRecorder recorder)
        {
            // Create prefab folder
            string folderPrefab = "Assets/RayFireRecords/" + recorder.clipName + "_prefabs/";
            if (Directory.Exists(folderPrefab) == false)
                Directory.CreateDirectory (folderPrefab);

            // Create prefab
            string     filePath = folderPrefab + rigid.rootChild.name + ".prefab";
            GameObject prefab   = PrefabUtility.SaveAsPrefabAsset (rigid.rootChild.gameObject, filePath);
            
            // Collect prefabs
            recorder.pfList.Add (prefab);
            
            // https://discussions.unity.com/t/editor-scripting-how-to-save-a-script-generated-mesh-as-an-asset-fbx/12050/2
            // https://forum.unity.com/threads/saving-modified-mesh.404212/
        }
        
        // Destroy prefab components
        public static void DestroyPrefabComponents(List<GameObject>   pfList)
        {
            if (pfList.Count == 0)
                return;

            foreach (var pf in pfList)
            {
                Rigidbody[] rbs = pf.GetComponentsInChildren<Rigidbody>();
                for (int i = rbs.Length - 1; i >= 0; i--)
                    Object.DestroyImmediate(rbs[i], true);
                
                RayfireRigid[] rgs = pf.GetComponentsInChildren<RayfireRigid>();
                for (int i = rgs.Length - 1; i >= 0; i--)
                    Object.DestroyImmediate(rgs[i], true);

                Collider[] cls = pf.GetComponentsInChildren<Collider>();
                for (int i = cls.Length - 1; i >= 0; i--)
                    Object.DestroyImmediate(cls[i], true);
                
                EditorUtility.SetDirty(pf);
                PrefabUtility.SavePrefabAsset(pf);
            }
        }
        
        /// //////////////////////////////////////////////////
        /// Create curves
        /// //////////////////////////////////////////////////
        
        // Set curve to clop
        static void SetCurvePosition (ref AnimationClip clip, List<Vector3> posList, List<float> timeList, int ind, string nameVar, string track, float threshold, int rate, bool optimizeKeys)
        {
            // Create keys
            Keyframe[] keys = new Keyframe[timeList.Count];
            for (int i = 0; i < timeList.Count; i++)
                keys[i] = new Keyframe(timeList[i], posList[i][ind], 0f, 0f, 0f, 0f);
            
            // Optimize
            if (optimizeKeys == true)
                keys = OptimizeKeys(keys, threshold, rate);

            // All keys was reduced
            if (keys.Length < 2)
                return;
            
            // Set keys to curve
            AnimationCurve curve = new AnimationCurve(keys);
            
            // Set key type
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
            }

            // Set curve to track
            clip.SetCurve(nameVar, typeof(Transform), track, curve);
        }
        
        // Set curve to clop
        static void SetCurveRotation (ref AnimationClip clip, List<Quaternion> rotList, List<float> timeList, int ind, string nameVar, string track, float threshold, int rate, bool optimizeKeys)
        {
            // Create keys
            Keyframe[] keys = new Keyframe[timeList.Count];
            for (int i = 0; i < timeList.Count; i++)
                keys[i] = new Keyframe (timeList[i], rotList[i][ind], 0f, 0f, 0f, 0f);

            // Optimize
            if (optimizeKeys == true)
                keys = OptimizeKeys(keys, threshold, rate);
            
            // All keys was reduced
            if (keys.Length < 2)
                return;
            
            // Set keys to curve
            AnimationCurve curve = new AnimationCurve(keys);

            // Set key type
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
            }

            // Set curve to track
            clip.SetCurve(nameVar, typeof(Transform), track, curve);
        }

        // Set curve to clop
        static void SetCurveActive(ref AnimationClip clip, List<bool> actList, List<float> timeList, string nameVar, string track, float threshold, int rate, bool optimizeKeys)
        {
            // Skip if always active
            if (actList.Contains (false) == false)
                return;

                // Create keys
            Keyframe[] keys = new Keyframe[timeList.Count];
            for (int i = 0; i < timeList.Count; i++)
                keys[i] = new Keyframe (timeList[i], (actList[i] == true ? 1f : 0f));
            
            // Optimize
            if (optimizeKeys == true)
                keys = OptimizeKeys(keys, threshold, rate);
            
            // All keys was reduced
            if (keys.Length < 2)
                return;
            
            // Set keys to curve
            AnimationCurve curve = new AnimationCurve(keys);
            
            // Set key type
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode (curve, i, AnimationUtility.TangentMode.Linear);
            }
            
            // Set curve to track
            clip.SetCurve(nameVar, typeof(GameObject), track, curve);
        }

        // Optimize keys
        static Keyframe[] OptimizeKeys(Keyframe[] keys, float threshold, int rate)
        {
            if (keys.Length <= 1)
                return keys;

            // Remove same keys
            List<int> removeInd = new List<int>();
            List<Keyframe> list = keys.ToList();
            
            // Collect indexes of all same keys between it's neibs
            for (int i = list.Count - 2; i > 1; i--)
                if (list[i].value - list[i - 1].value == 0 && list[i].value - list[i + 1].value == 0)
                    removeInd.Add (i);

            // Remove same keys
            for (int i = 0; i < removeInd.Count; i++)
                list.RemoveAt (removeInd[i]);
            if (list.Count == 1)
            {
                list.Clear();
                return list.ToArray();
            }
            
            // Remove by threshold
            if (threshold > 0 && list.Count > 6)
            {
                removeInd.Clear();
                float val = threshold / rate;
                for (int i = list.Count - 3; i > 3; i--)
                    if (Mathf.Abs (list[i].value - list[i - 1].value) > val && Mathf.Abs (list[i].value - list[i + 1].value) > val)
                        removeInd.Add (i);
                
                // Remove same keys
                for (int i = 0; i < removeInd.Count; i++)
                    list.RemoveAt (removeInd[i]);
                if (list.Count == 1)
                {
                    list.Clear();
                    return list.ToArray();
                }
            }
            
            return list.ToArray();
        }
    }
}

#endif
