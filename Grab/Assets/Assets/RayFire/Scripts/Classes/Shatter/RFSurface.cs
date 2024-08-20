using UnityEngine;
using UnityEngine.Serialization;

namespace RayFire
{
    [System.Serializable]
    public class RFSurface
    {
        [FormerlySerializedAs ("innerMaterial")]
        public Material iMat;
        [FormerlySerializedAs ("outerMaterial")]
        public Material oMat;
        [FormerlySerializedAs ("mappingScale")]
        public float    mScl;
        public bool     uvE;
        public Vector2  uvC;
        public bool     cE;
        public Color    cC;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
         
        // Constructor
        public RFSurface()
        {
            InitValues();
        }

        void InitValues()
        {
            iMat = null;
            oMat = null;
            mScl = 0.1f;
            uvE  = false;
            uvC  = Vector2.zero;
            cE   = false;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
        }
        
        // Copy from
        public void CopyFrom(RFSurface source)
        {
            iMat = source.iMat;
            oMat = source.oMat;
            mScl = source.mScl;
            uvE  = source.uvE;
            uvC  = source.uvC;
            cE   = source.cE;
            cC   = source.cC;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Set material to fragment by it's interior properties and parent material
        public static void SetMaterial(RFDictionary[] origSubMeshIdsRF, Material[] sharedMaterials, RFSurface interior, MeshRenderer targetRend, int i, int amount)
        {
            if (origSubMeshIdsRF != null && origSubMeshIdsRF.Length == amount)
            {
                Material[] mats = new Material[origSubMeshIdsRF[i].values.Length];
                for (int j = 0; j < origSubMeshIdsRF[i].values.Length; j++)
                {
                    int matId = origSubMeshIdsRF[i].values[j];
                    if (matId < sharedMaterials.Length)
                    {
                        if (interior.oMat == null)
                            mats[j] = sharedMaterials[matId];
                        else
                            mats[j] = interior.oMat;
                    }
                    else
                        mats[j] = interior.iMat;
                }

                targetRend.sharedMaterials = mats;
            }
        }
        
        // Get inner faces sub mesh id
        public static int SetInnerSubId(RayfireRigid scr)
        {
            // No inner material
            if (scr.materials.iMat == null) 
                return 0;
            
            // Get materials
            Material[] mats = scr.skr != null 
                ? scr.skr.sharedMaterials 
                : scr.meshRenderer.sharedMaterials;
            
            // Get outer id if outer already has it
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == scr.materials.iMat)
                    return i;
            
            return -1;
        }
        
        // Get inner faces sub mesh id
        public static int SetInnerSubId(RayfireShatter scr)
        {
            // No inner material
            if (scr.material.iMat == null) 
                return 0;
            
            // Get materials
            Material[] mats = scr.skinnedMeshRend != null 
                ? scr.skinnedMeshRend.sharedMaterials 
                : scr.meshRenderer.sharedMaterials;
            
            // Get outer id if outer already has it
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == scr.material.iMat)
                    return i;
            
            return -1;
        }
    }
}

