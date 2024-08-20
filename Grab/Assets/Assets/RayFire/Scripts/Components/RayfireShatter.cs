using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_XBOXONE)
using RayFire.DotNet;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Shatter")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-shatter-component/")]
    public class RayfireShatter : MonoBehaviour
    {
        public enum FragLastMode
        {
            New    = 0,
            ToLast = 1
        }

        // UI
        public FragType          type      = FragType.Voronoi;
        public RFVoronoi         voronoi   = new RFVoronoi();
        public RFSplinters       splinters = new RFSplinters();
        public RFSplinters       slabs     = new RFSplinters();
        public RFRadial          radial    = new RFRadial();
        public RFHexagon         hexagon   = new RFHexagon();
        public RFCustom          custom    = new RFCustom();
        public RFMirrored        mirrored  = new RFMirrored();
        public RFSlice           slice     = new RFSlice();
        public RFBricks          bricks    = new RFBricks();
        public RFVoxels          voxels    = new RFVoxels();
        public RFTets            tets      = new RFTets();
        public FragmentMode      mode      = FragmentMode.Editor;
        public RFSurface         material  = new RFSurface();
        public RFShatterCluster  clusters  = new RFShatterCluster();
        public RFShatterAdvanced advanced  = new RFShatterAdvanced();
        public RFMeshExport      export    = new RFMeshExport();

        // Center
        public bool       showCenter;
        public Vector3    centerPosition;
        public Quaternion centerDirection;

        // Components
        public Transform           transForm;
        public MeshFilter          meshFilter;
        public MeshRenderer        meshRenderer;
        public SkinnedMeshRenderer skinnedMeshRend;
        public List<MeshFilter>    meshFilters;

        // Vars
        [NonSerialized] Mesh[]           meshes;
        [NonSerialized] Vector3[]        pivots;
        [NonSerialized] RFDictionary[]   rfOrigSubMeshIds;
        public          List<Transform>  rootChildList = new List<Transform>();
        public          List<GameObject> fragmentsAll  = new List<GameObject>();
        public          List<GameObject> fragmentsLast = new List<GameObject>();
        public          Material[]       materials;

        // Hidden
        public int     shatterMode = 1;
        public bool    colorPreview;
        public bool    scalePreview = true;
        public float   previewScale;
        public float   size;
        public float   rescaleFix = 1f;
        public Vector3 originalScale;
        public Bounds  bound;
        public bool    resetState;

        // Interactive
        public bool         interactive;
        public RFShatter    shatterInt;
        public GameObject   intGo;
        public MeshFilter   intMf;
        public MeshRenderer intMr;

        // Static
        static float minSize = 0.01f;

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Reset
        private void Reset()
        {
            InteractiveStop();
            ResetCenter();
        }

        // Set default vars before fragment
        void SetVariables()
        {
            size             = 0f;
            rescaleFix       = 1f;
            originalScale    = transForm.localScale;
            rfOrigSubMeshIds = null;
        }

        /// /////////////////////////////////////////////////////////
        /// Checks
        /// /////////////////////////////////////////////////////////

        // Basic proceed check
        bool MainCheck()
        {
            // Check if prefab
            if (gameObject.scene.rootCount == 0)
            {
                Debug.Log ("RayFire Shatter: " + name + " Can't fragment prefab because prefab unable to store Unity mesh. Fragment prefab in scene.", gameObject);
                return false;
            }

            // Single mesh mode
            if (advanced.combineChildren == false)
                if (SingleMeshCheck() == false)
                    return false;

            // Multiple mesh mode
            if (advanced.combineChildren == true)
            {
                // Has no children meshes
                if (meshFilters.Count == 1)
                    if (SingleMeshCheck() == false)
                        return false;

                // Remove no meshes
                if (meshFilters.Count > 0)
                    for (int i = meshFilters.Count - 1; i >= 0; i--)
                        if (meshFilters[i].sharedMesh == null)
                        {
                            Debug.Log ("RayFire Shatter: " + meshFilters[i].name + " MeshFilter has no Mesh, object excluded.", meshFilters[i].gameObject);
                            meshFilters.RemoveAt (i);
                        }

                // Remove no readable meshes
                if (meshFilters.Count > 0)
                    for (int i = meshFilters.Count - 1; i >= 0; i--)
                        if (meshFilters[i].sharedMesh.isReadable == false)
                        {
                            Debug.Log ("RayFire Shatter: " + meshFilters[i].name + " Mesh is not Readable, object excluded.", meshFilters[i].gameObject);
                            meshFilters.RemoveAt (i);
                        }

                // No meshes left
                if (meshFilters.Count == 0)
                    return false;
            }

            return true;
        }

        // Single mesh mode checks
        bool SingleMeshCheck()
        {
            // No mesh storage components
            if (meshFilter == null && skinnedMeshRend == null)
            {
                Debug.Log ("RayFire Shatter: " + name + " Object has no mesh to fragment.", gameObject);
                return false;
            }

            // Has mesh filter
            if (meshFilter != null)
            {
                // No shared mesh
                if (meshFilter.sharedMesh == null)
                {
                    Debug.Log ("RayFire Shatter: " + name + " Object has no mesh to fragment.", gameObject);
                    return false;
                }

                // Not readable mesh
                if (meshFilter.sharedMesh.isReadable == false)
                {
                    Debug.Log ("RayFire Shatter: " + name + "Mesh is not readable. Open Import Settings and turn On Read/Write Enabled", gameObject);
                    return false;
                }
            }

            // Has skinned mesh
            if (skinnedMeshRend != null && skinnedMeshRend.sharedMesh == null)
            {
                Debug.Log ("RayFire Shatter: " + name + " Object has no mesh to fragment.", gameObject);
                return false;
            }

            return true;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Cache variables
        bool DefineComponents()
        {
            // Mesh storage 
            transForm       = GetComponent<Transform>();
            meshFilter      = GetComponent<MeshFilter>();
            meshRenderer    = GetComponent<MeshRenderer>();
            skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();

            // Multymesh fragmentation
            meshFilters = new List<MeshFilter>();
            if (advanced.combineChildren == true)
                meshFilters = GetComponentsInChildren<MeshFilter>().ToList();

            // Basic proceed check
            if (MainCheck() == false)
                return false;

            // Mesh renderer
            if (skinnedMeshRend == null)
            {
                if (meshRenderer == null)
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                bound = meshRenderer.bounds;
            }

            // Skinned mesh
            if (skinnedMeshRend != null)
                bound = skinnedMeshRend.bounds;

            return true;
        }

        // Get bounds
        public Bounds GetBound()
        {
            // Mesh renderer
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    return meshRenderer.bounds;
            }
            else
                return meshRenderer.bounds;

            // Skinned mesh
            if (skinnedMeshRend == null)
            {
                skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRend != null)
                    return skinnedMeshRend.bounds;
            }

            return new Bounds();
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Fragment this object by shatter properties  List<GameObject>
        public void Fragment(FragLastMode fragmentMode = FragLastMode.New)
        {
            // Prepare to cache fragments
            if (PreCache() == false)
                return;

            // Cache
            RFFragment.CacheMeshes (ref meshes, ref pivots, ref rfOrigSubMeshIds, this);

            // Stop
            if (meshes == null)
                return;

            // Create fragments
            if (fragmentMode == FragLastMode.ToLast)
            {
                if (rootChildList[rootChildList.Count - 1] != null)
                    fragmentsLast = CreateFragments (rootChildList[rootChildList.Count - 1]);
                else
                    fragmentMode = FragLastMode.New;
            }

            // Create new fragments
            if (fragmentMode == FragLastMode.New)
                fragmentsLast = CreateFragments();

            // Post create fragments operations
            PostFragments();
        }

        // Prepare and cache fragments
        bool PreCache()
        {
            // Cache variables
            if (DefineComponents() == false)
                return false;

            // Cache default vars
            SetVariables();

            // Check if object is too small
            ScaleCheck();

            return true;
        }

        // Create fragments by mesh and pivots array
        List<GameObject> CreateFragments(Transform root = null)
        {
            // No mesh were cached
            if (meshes == null)
                return null;

            // Clear array for new fragments
            GameObject[] fragArray = new GameObject[meshes.Length];

            // Create root object
            if (root == null)
            {
                GameObject rootGo = new GameObject (gameObject.name + "_root");
                rootGo.transform.position = transForm.position;
                rootGo.transform.rotation = transForm.rotation;
                rootGo.tag                = gameObject.tag;
                rootGo.layer              = gameObject.layer;
                root                      = rootGo.transform;
                rootChildList.Add (root);
            }

            // Create instance for fragments
            GameObject fragInstance;
            if (advanced.copyComponents == true)
            {
                fragInstance                      = Instantiate (gameObject);
                fragInstance.transform.rotation   = Quaternion.identity;
                fragInstance.transform.localScale = Vector3.one;

                // Destroy shatter
                DestroyImmediate (fragInstance.GetComponent<RayfireShatter>());
            }
            else
            {
                fragInstance = new GameObject();
                fragInstance.AddComponent<MeshFilter>();
                fragInstance.AddComponent<MeshRenderer>();
            }

            // Get original mats. in case of combined meshes it is already defined in CombineShatter()
            if (advanced.combineChildren == false)
                materials = skinnedMeshRend != null
                    ? skinnedMeshRend.sharedMaterials
                    : meshRenderer.sharedMaterials;

            // Vars 
            string baseName = gameObject.name + "_sh_";

            // Create fragment objects
            MeshFilter   mf;
            GameObject   go;
            MeshCollider mc;
            MeshRenderer rn;
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Rescale mesh
                if (rescaleFix != 1f)
                    RFFragment.RescaleMesh (meshes[i], rescaleFix);

                // Instantiate. IMPORTANT do not parent when Instantiate
                go                      = Instantiate (fragInstance);
                go.transform.localScale = Vector3.one;

                // Set multymaterial
                rn = go.GetComponent<MeshRenderer>();
                RFSurface.SetMaterial (rfOrigSubMeshIds, materials, material, rn, i, meshes.Length);

                // Set fragment object name and tm
                go.name               = baseName + (i + 1);
                go.transform.position = root.transform.position + (pivots[i] / rescaleFix);
                go.transform.parent   = root.transform;
                go.tag                = gameObject.tag;
                go.layer              = gameObject.layer;

                // Set fragment mesh
                mf                 = go.GetComponent<MeshFilter>();
                mf.sharedMesh      = meshes[i];
                mf.sharedMesh.name = go.name;

                // Set mesh collider
                mc = go.GetComponent<MeshCollider>();
                if (mc != null)
                    mc.sharedMesh = meshes[i];

                // Add in array
                fragArray[i] = go;
            }

            // Root back to original parent
            root.transform.parent = transForm.parent;

            // Reset scale for mesh fragments. IMPORTANT: skinned mesh fragments root should not be rescaled 
            if (skinnedMeshRend == null)
                root.transform.localScale = Vector3.one;

            // Destroy instance
            DestroyImmediate (fragInstance);

            // Empty lists
            meshes           = null;
            pivots           = null;
            rfOrigSubMeshIds = null;

            return fragArray.ToList();
        }

        // Post create fragments operations
        void PostFragments()
        {
            // Limitation fragment
            RFShatterAdvanced.Limitations (this);

            // Collect to all fragments
            fragmentsAll.AddRange (fragmentsLast);

            // Reset original object back if it was scaled
            transForm.localScale = originalScale;
        }

        // Fragment by limitations
        public void LimitationFragment(int ind)
        {
            RayfireShatter shat = fragmentsLast[ind].AddComponent<RayfireShatter>();
            shat.voronoi.amount = 10;

            shat.Fragment();

            if (shat.fragmentsLast.Count > 0)
            {
                fragmentsLast.AddRange (shat.fragmentsLast);
                DestroyImmediate (shat.gameObject);
                fragmentsLast.RemoveAt (ind);

                // Parent and destroy root
                foreach (var frag in shat.fragmentsLast)
                    frag.transform.parent = rootChildList[rootChildList.Count - 1];
                DestroyImmediate (shat.rootChildList[rootChildList.Count - 1].gameObject);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Deleting
        /// /////////////////////////////////////////////////////////

        // Delete fragments from last Fragment method
        public void DeleteFragmentsLast(int destroyMode = 0)
        {
            // Destroy last fragments
            if (destroyMode == 1)
                for (int i = fragmentsLast.Count - 1; i >= 0; i--)
                    if (fragmentsLast[i] != null)
                        DestroyImmediate (fragmentsLast[i]);

            // Clean fragments list pre
            fragmentsLast.Clear();
            for (int i = fragmentsAll.Count - 1; i >= 0; i--)
                if (fragmentsAll[i] == null)
                    fragmentsAll.RemoveAt (i);

            // Check for all roots
            for (int i = rootChildList.Count - 1; i >= 0; i--)
                if (rootChildList[i] == null)
                    rootChildList.RemoveAt (i);

            // No roots
            if (rootChildList.Count == 0)
                return;

            // Destroy with root
            if (destroyMode == 0)
            {
                // Destroy root with fragments
                DestroyImmediate (rootChildList[rootChildList.Count - 1].gameObject);

                // Remove from list
                rootChildList.RemoveAt (rootChildList.Count - 1);
            }

            // Clean all fragments list post
            for (int i = fragmentsAll.Count - 1; i >= 0; i--)
                if (fragmentsAll[i] == null)
                    fragmentsAll.RemoveAt (i);
        }

        // Delete all fragments and roots
        public void DeleteFragmentsAll()
        {
            // Clear lists
            fragmentsLast.Clear();
            fragmentsAll.Clear();

            // Check for all roots
            for (int i = rootChildList.Count - 1; i >= 0; i--)
                if (rootChildList[i] != null)
                    DestroyImmediate (rootChildList[i].gameObject);
            rootChildList.Clear();
        }

        // Reset center helper
        public void ResetCenter()
        {
            centerPosition  = Vector3.zero;
            centerDirection = Quaternion.identity;

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
                centerPosition = transform.InverseTransformPoint (rend.bounds.center);
        }

        /// /////////////////////////////////////////////////////////
        /// Scale
        /// /////////////////////////////////////////////////////////

        // Check if object is too small
        void ScaleCheck()
        {
            // Geе size from renderers
            if (meshRenderer != null)
                size = meshRenderer.bounds.size.magnitude;
            if (skinnedMeshRend != null)
                size = skinnedMeshRend.bounds.size.magnitude;

            // Get rescaleFix if too small
            if (size != 0f && size < minSize)
            {
                // Get rescaleFix factor
                rescaleFix = 1f / size;

                // Scale small object up to shatter
                Vector3 newScale = transForm.localScale * rescaleFix;
                transForm.localScale = newScale;

                // Warning
                Debug.Log ("Warning. Object " + name + " is too small.");
            }
        }

        // Reset original object and fragments scale
        public void ResetScale(float scaleValue)
        {
            // Reset scale
            if (resetState == true && scaleValue == 0f)
            {
                if (skinnedMeshRend != null)
                    skinnedMeshRend.enabled = true;

                if (meshRenderer != null)
                    meshRenderer.enabled = true;

                if (fragmentsLast.Count > 0)
                    foreach (GameObject fragment in fragmentsLast)
                        if (fragment != null)
                            fragment.transform.localScale = Vector3.one;

                resetState = false;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Copy
        /// /////////////////////////////////////////////////////////

        // Copy shatter component
        public static void CopyRootMeshShatter(RayfireRigid source, List<RayfireRigid> targets)
        {
            // No shatter
            if (source.meshDemolition.sht == null)
                return;

            // Copy shatter
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].meshDemolition.sht = targets[i].gameObject.AddComponent<RayfireShatter>();
                targets[i].meshDemolition.sht.CopyFrom (source.meshDemolition.sht);
            }
        }

        // Copy from
        void CopyFrom(RayfireShatter shatter)
        {
            type = shatter.type;

            voronoi   = new RFVoronoi (shatter.voronoi);
            splinters = new RFSplinters (shatter.splinters);
            slabs     = new RFSplinters (shatter.slabs);
            radial    = new RFRadial (shatter.radial);
            custom    = new RFCustom (shatter.custom);
            slice     = new RFSlice (shatter.slice);
            tets      = new RFTets (shatter.tets);

            mode = shatter.mode;
            material.CopyFrom (shatter.material);
            clusters = new RFShatterCluster (shatter.clusters);
            advanced = new RFShatterAdvanced (shatter.advanced);
        }

        /// /////////////////////////////////////////////////////////
        /// Interactive
        /// /////////////////////////////////////////////////////////
        
        // TODO RFShatter reuse at property change
        // TODO Scale/Property change refrag with 0 seed
        // Refrag at slice planes move/rotate, custom points move

        // Create Interactive object
        void InteractiveCreate()
        {
            if (intGo == null)
            {
                intGo           = new GameObject (name + "_Interactive");
                intMf           = intGo.AddComponent<MeshFilter>();
                
                // Copy tm
                intGo.transform.position = transForm.position;
                
                intGo.tag   = gameObject.tag;
                intGo.layer = gameObject.layer;
            }
            if (meshRenderer != null)
            {
                if (intMr == null)
                {
                    intMr                 = intGo.AddComponent<MeshRenderer>();
                    intMr.sharedMaterials = meshRenderer.sharedMaterials;
                }
            }
            else if (skinnedMeshRend != null)
            {
                // TODO skinned mesh renderer support
            }
        }

        // Fragment all meshes into own mesh
        public void InteractiveStart()
        {
            #if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_XBOXONE)
            
            // Prepare to cache fragments
            if (PreCache() == false)
            {
                OriginalRenderer(true);;
                return;
            }

            // Create interactive object if there is no any
            InteractiveCreate();
            
            // Cache
            shatterInt = RFFragment.CacheMeshes (ref meshes, ref pivots, ref rfOrigSubMeshIds, this);

            // Stop
            if (meshes == null)
            {
                OriginalRenderer(true);;
                return;
            }

            // TODO
            List<Dictionary<int, int>> ids = new List<Dictionary<int, int>>();
            for (int i = 0; i < rfOrigSubMeshIds.Length; i++)
            {
                Dictionary<int, int> d = new Dictionary<int, int>
                {
                    {rfOrigSubMeshIds[i].keys[0], rfOrigSubMeshIds[i].values[0]}
                };
                ids.Add (d);
            }
            
            // Weld into one mesh and set to interactive object
            intMf.sharedMesh = RFShatter.WeldMeshes (ref meshes, ref pivots, ref ids, PreviewScale());
            
            // Disable own Renderer
            OriginalRenderer(false);
            
            #endif 
        }

        // Property changed
        public void InteractiveChange()
        {
            #if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_XBOXONE)
            
            if (interactive == false)
                return;
            
            // Prepare to cache fragments
            if (PreCache() == false)
            {
                OriginalRenderer(true);;
                return;
            }
            
            // Recache with new properties
            shatterInt = RFFragment.CacheMeshes (ref meshes, ref pivots, ref rfOrigSubMeshIds, this);

            // Stop
            if (meshes == null)
            {
                OriginalRenderer(true);;
                return;
            }

            // TODO
            List<Dictionary<int, int>> ids = new List<Dictionary<int, int>>();
            for (int i = 0; i < rfOrigSubMeshIds.Length; i++)
            {
                Dictionary<int, int> d = new Dictionary<int, int>
                {
                    {rfOrigSubMeshIds[i].keys[0], rfOrigSubMeshIds[i].values[0]}
                };
                ids.Add (d);
            }
            
            // Weld into one mesh and set to interactive object
            intMf.sharedMesh = RFShatter.WeldMeshes (ref meshes, ref pivots, ref ids, PreviewScale());
            
            // Disable own Renderer
            OriginalRenderer(false);
            
            #endif 
        }

        // Create interactively cached fragments
        public void InteractiveFragment()
        {
            // Create new fragments
            fragmentsLast = CreateFragments();

            // Post create fragments operations
            PostFragments();

            // Reset original mesh
            InteractiveStop();
        }

        // Revert original mesh
        public void InteractiveStop()
        {
            // Enable own Renderer
            OriginalRenderer(true);
            
            // Destroy interactive object
            if (intGo != null)
                DestroyImmediate (intGo);
            
            // Reset
            intGo       = null;
            intMf       = null;
            intMr       = null;
            shatterInt  = null;
            interactive = false;
        }

        // Set original renderer state
        void OriginalRenderer(bool state)
        {
            if (meshRenderer != null)
                meshRenderer.enabled = state;
            if (skinnedMeshRend != null)
                skinnedMeshRend.enabled = state;
        }
        
        public void InteractiveReset()
        {
            if (interactive == true)
            {
                InteractiveStop();
            }
        }

        // Final preview scale
        public float PreviewScale()
        {
            if (scalePreview == false)
                return 1f;
            return Mathf.Lerp (1f, 0.3f, previewScale);
        }

        /*
        enum PrefabMode
        {
        	Scene,
        	Asset,
        	PrefabEditingMode
        }
         
        // Get prefab mode
        PrefabMode GetPrefabMode (GameObject go)
        {
            // scene, prefab, mode
            // Debug.Log (go.scene.path); // fullpath.unity,  null, ""
            // Debug.Log (go.scene.name); // scene name, null, box_pf
            // Debug.Log (go.scene.rootCount); // 4, 0, 1
            // Debug.Log (go.scene.isLoaded); // true, false, true
            // Debug.Log (go.scene.IsValid()); // true, false, true
            // return PrefabMode.Asset;
            
            // Prefab is asset
            if (go.scene.path.EndsWith(".prefab"))
                return PrefabMode.Asset;
            
            // Prefab is in editing mode
            if (string.IsNullOrEmpty(go.scene.path))
                return PrefabMode.PrefabEditingMode;
            
            // Prefab is in scene
            return PrefabMode.Scene;
        }
        */
    }
}