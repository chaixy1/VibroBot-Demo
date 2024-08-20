using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RayFire
{
    [Serializable]
    public class RFRuntimeCaching
    {
        [FormerlySerializedAs ("type")]
        public CachingType tp;
        [FormerlySerializedAs ("frames")]
        public int frm;
        [FormerlySerializedAs ("fragments")]
        public int frg;
        [FormerlySerializedAs ("skipFirstDemolition")]
        public bool skp;

        [NonSerialized] public bool inProgress;
        [NonSerialized] public bool wasUsed;
        [NonSerialized] public bool stop;
           
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFRuntimeCaching()
        {
            InitValues();
        }
        
        // Starting values
        public void InitValues()
        {
            tp  = CachingType.Disable;
            frm = 3;
            frg = 4;
            skp = false;
        }
        
        // Copy from
        public void CopyFrom (RFRuntimeCaching rc)
        {
            tp  = rc.tp;
            frm = rc.frm;
            frg = rc.frg;
            skp = rc.skp;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Get batches amount for continuous fragmentation
        public static List<int> GetBatchByFrames (int frames, int amount)
        {
            // Get basic list
            int       div         = amount / frames;
            List<int> batchAmount = new List<int>(frames + 1);
            for (int i = 0; i < frames; i++)
                batchAmount.Add (div);

            // Consider difference
            int dif = amount % frames;
            if (dif > 0)
                for (int i = 0; i < dif; i++)
                    batchAmount[i] += 1;

            // Remove 0
            if (frames > amount)
                for (int i = batchAmount.Count - 1; i >= 0; i--)
                    if (batchAmount[i] == 0)
                        batchAmount.RemoveAt (i);

            return batchAmount;
        }
        
        // Get batches amount for continuous fragmentation
        public static List<int> GetBatchByFragments (int fragments, int amount)
        {
            // Get basic list
            int       steps         = amount / fragments;
            List<int> batchAmount = new List<int>(steps + 1);
            if (steps > 0)
                for (int i = 0; i < steps; i++)
                    batchAmount.Add (fragments);

            // Consider difference
            int dif = amount % fragments;
            if (dif > 0)
                batchAmount.Add (dif);
            
            return batchAmount;
        }

        // Get list of marked elements index
        public static List<int> GetMarkedElements (int batchInd, List<int> batchAmount)
        {
            // Get offset
            int offset = 0;
            if (batchInd > 0)
                for (int i = 0; i < batchInd; i++)
                    offset += batchAmount[i];
            
            // Collect marked elements ids
            List<int> markedElements = new List<int>(batchAmount[batchInd]);
            for (int i = 0; i < batchAmount[batchInd]; i++)
                markedElements.Add (i + offset);

            return markedElements;
        }

        // Create tm reference
        public static GameObject CreateTmRef(RayfireRigid rfScr)
        {
            GameObject go = new GameObject("RFTempGo");
            go.SetActive (false);
            go.transform.position = rfScr.transForm.position;
            go.transform.rotation = rfScr.transForm.rotation;
            go.transform.localScale = rfScr.transForm.localScale;
            go.transform.parent = RayfireMan.inst.transform;
            return go;
        }
        
         /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Cor to fragment mesh over several frames
        public IEnumerator RuntimeCachingCor (RayfireRigid scr)
        {
            // Object should be demolished when cached all meshes but not during caching
            bool demolitionShouldLocal = scr.limitations.demolitionShould == true;
            scr.limitations.demolitionShould = false;
            
            // Input mesh, setup, record time
            float t1 = Time.realtimeSinceStartup;
            if (RFFragment.InputMesh (scr) == false)
                yield break;
                        
            // Set list with amount of mesh for every frame
            List<int> batchAmount = tp == CachingType.ByFrames
                ? GetBatchByFrames(frm, scr.meshDemolition.totalAmount)
                : GetBatchByFragments(frg, scr.meshDemolition.totalAmount);
            
            // Caching in progress
            inProgress = true;

            // Wait next frame if input took too much time or long batch
            float t2 = Time.realtimeSinceStartup - t1;
            if (t2 > 0.02f || batchAmount.Count > 5)
                yield return null;

            // Save tm for multi frame caching
            GameObject tmRefGo = CreateTmRef (scr);

            // Start rotation
            scr.meshDemolition.rotStart = scr.transForm.rotation;
            
            // Iterate every frame. Calc local frame meshes
            List<Mesh>         meshesList = new List<Mesh>();
            List<Vector3>      pivotsList = new List<Vector3>();
            List<RFDictionary> subList    = new List<RFDictionary>();
            for (int i = 0; i < batchAmount.Count; i++)
            {
                // Check for stop
                if (stop == true)
                {
                    ResetRuntimeCaching(scr, tmRefGo);
                    yield break;
                }
                
                // Cache defined points
                RFFragment.CacheMeshesMult (tmRefGo.transform, ref meshesList, ref pivotsList, ref subList, scr, batchAmount, i);
                // TODO create fragments for current batch
                // TODO record time and decrease batches amount if less 30 fps
                yield return null;
            }
            
            // Set to main data vars
            scr.meshes = meshesList.ToArray();
            scr.pivots = pivotsList.ToArray();
            scr.subIds = subList.ToArray();;

            // Clear
            scr.DestroyObject (tmRefGo);
            scr.meshDemolition.sht = null;
            
            // Set demolition ready state
            if (skp == false && demolitionShouldLocal == true)
                scr.limitations.demolitionShould = true;
            
            // Reset damage
            if (skp == true && demolitionShouldLocal == true)
                scr.damage.LocalReset();
            
            // Caching finished
            inProgress = false;
            wasUsed = true;
        }

        // Stop runtime caching and reset it
        public void StopRuntimeCaching()
        {
            if (inProgress == true)
                stop = true;
        }
        
        // Reset caching
        void ResetRuntimeCaching (RayfireRigid scr, GameObject tmRefGo)
        {
            scr.DestroyObject (tmRefGo);
            stop = false;
            inProgress = false;
            scr.meshDemolition.rfShatter = null;
            scr.DeleteCache();
        }
    }
}