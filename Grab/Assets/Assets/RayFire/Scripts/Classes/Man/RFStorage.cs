using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    public class RFStorage
    {
        public Transform storageRoot;
        public bool      inProgress;
        float            rate = 1f;
        List<Transform>  storageList;

        // Constructor
        public RFStorage()
        {
            storageList = new List<Transform>();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Create storage
        public void CreateStorageRoot  (Transform manTm)
        {
            // Already has storage root
            if (storageRoot != null)
                return;
            
            GameObject storageGo = new GameObject ("Storage_Fragments");
            storageRoot          = storageGo.transform;
            storageRoot.position = manTm.transform.position;
            storageRoot.parent   = manTm.transform;
        }

        // Destroy empty storage roots
        public IEnumerator StorageCor()
        {
            WaitForSeconds delay = new WaitForSeconds (rate);
            
            // Pooling loop
            inProgress = true;
            while (inProgress == true)
            {
                // Destroy root without children
                for (int i = storageList.Count - 1; i >= 0; i--)
                {
                    // Remove destroyed, reset
                    if (storageList[i] == null)
                    {
                        storageList.RemoveAt (i);
                        continue;
                    }

                    // 
                    if (storageList[i].childCount == 0)
                    {
                        Object.Destroy (storageList[i].gameObject);
                        storageList.RemoveAt (i);
                    }
                }

                // Wait next frame
                yield return delay;
            }
            inProgress = false;
        }

        // Add new root to storage
        public void Register (Transform tm)
        {
            if (tm.childCount > 0)
                storageList.Add (tm);
        }

        // Destroy all storage objects
        public void DestroyAll()
        {
            for (int i = storageList.Count - 1; i >= 0; i--)
                if (storageList[i] != null)
                    Object.Destroy (storageList[i].gameObject);
            storageList.Clear();
        }
    }
}
