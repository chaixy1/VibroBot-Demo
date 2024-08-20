using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFHexagon
    {
        public PlaneType     plane;
        public int           row;
        public int           col;
        public float         size;
        public float         div;
        public bool          rest;
        public bool          enable;
        public bool          noPc;
        public List<Vector3> pc;
        public List<Vector3> pcBndIn;
        public List<Vector3> pcBndOut;
        Matrix4x4            matrix;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        public RFHexagon()
        {
            plane  = PlaneType.XZ;
            row    = 10;
            col    = 10;
            size   = 0.5f;
            div    = 0f;
            rest   = true;
            enable = true;
        }
        
        public RFHexagon(RFHexagon src)
        {
            plane  = src.plane;
            row    = src.row;
            col    = src.col;
            size   = src.size;
            div    = src.div;
            rest   = src.rest;
            enable = src.enable;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Get final point cloud for hexgrid fragmentation
        public static List<Vector3> GetHexPointCLoud (RFHexagon hexagon, Transform tm, Vector3 centerPos, Quaternion centerDirection, int seed, Bounds bound)
        {
            // Generate hex grid point cloud
            SetHexOutputCloud (hexagon);
            
            // Matrix multiply
            Quaternion quat   = centerDirection * tm.rotation;
            if (hexagon.plane == PlaneType.XY)
                quat = quat * Quaternion.Euler(90, 0, 0);
            if (hexagon.plane == PlaneType.YZ)
                quat = quat * Quaternion.Euler(0, 0, 90);
            hexagon.matrix = Matrix4x4.TRS(centerPos, quat.normalized, Vector3.one);
            for (int i = 0; i < hexagon.pc.Count; i++)
                hexagon.pc[i] = hexagon.matrix.MultiplyPoint (hexagon.pc[i]);;
            
            // Set points in bound
            SetCustomBoundPoints (hexagon, bound);
            
            // Stop if no points
            hexagon.noPc = false;
            if (hexagon.pcBndIn.Count <= 1)
                hexagon.noPc = true;
            
            return hexagon.pc; // TODO
        }
        
        // Generate hex grid point cloud
        static void SetHexOutputCloud (RFHexagon hexagon)
        {
            if (hexagon.pc == null)
                hexagon.pc = new List<Vector3>();
            else
                hexagon.pc.Clear();

            // Set grid size
            int rowP = hexagon.row / 2;
            int rowM = hexagon.row - rowP;
            int colP = hexagon.col / 2;
            int colM = hexagon.col - colP;
            
            float   z, h, x;
            bool    offset = true;
            for (int r = -rowM; r < rowP; r++)
            {
                // Set row coordinate for row hex centers
                z = 1.5f * hexagon.size * r;
                
                // Generate row hexes by offset
                offset = !offset;
                for (int c = -colM; c < colP; c++)
                {
                    h = (float)(hexagon.size * Math.Sqrt (3));
                    x = h * c;
                    if (offset == true)
                        x += h / 2f;
                    
                    // Collect
                    hexagon.pc.Add (new Vector3 (x, 0, z));
                }
            }
            
            // Divergence
            SetHexDivergence (hexagon);
        }
        
        // Divergence
        static void SetHexDivergence(RFHexagon hexagon)
        {
            if (hexagon.div > 0)
            {
                // Set variation values
                float varX = hexagon.div / 2f;
                float varY = hexagon.div / 2f;
                float varZ = hexagon.div / 2f;

                // Restrict to plane
                if (hexagon.rest == true)
                {
                    if (hexagon.plane == PlaneType.XZ)
                        varY = 0;
                    else if (hexagon.plane == PlaneType.XY)
                        varZ = 0;
                    else if (hexagon.plane == PlaneType.YZ)
                        varX = 0;
                }
                
                // Random offset
                Random.InitState (0);
                for (int i = 0; i < hexagon.pc.Count; i++)
                    hexagon.pc[i] += new Vector3(Random.Range (-varX, varX), Random.Range (-varY, varY), Random.Range (-varZ, varZ));
            }
        }

        // Filter world points by bound intersection
        static void SetCustomBoundPoints(RFHexagon hexagon, Bounds bound)
        {
            // Set outbound list
            if (hexagon.pcBndOut == null) 
                hexagon.pcBndOut = new List<Vector3>(); 
            else hexagon.pcBndOut.Clear();
            
            if (hexagon.pcBndIn == null) 
                hexagon.pcBndIn = new List<Vector3>(); 
            else hexagon.pcBndIn.Clear();
            
            // Filter points byu bound
            for (int i = hexagon.pc.Count - 1; i >= 0; i--)
                if (bound.Contains(hexagon.pc[i]) == false)
                    hexagon.pcBndOut.Add (hexagon.pc[i]);
                else
                    hexagon.pcBndIn.Add (hexagon.pc[i]);
        }
    }
}