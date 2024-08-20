using System;
using UnityEngine.Serialization;

namespace RayFire
{
	[Serializable]
	public class RFFragmentProperties
	{
		[FormerlySerializedAs ("colliderType")]
		public RFColliderType col;
		[FormerlySerializedAs ("sizeFilter")]
		public float          szF;
		[FormerlySerializedAs ("decompose")]
		public bool           dec;
		[FormerlySerializedAs ("removeCollinear")]
		public bool           rem;
		public bool           l; // Copy layer
		[FormerlySerializedAs ("layer")]
		public int            lay;
		public bool           t; // Copy tag
		public string         tag;

		/// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
		
		// Constructor
		public RFFragmentProperties()
		{
			InitValues();
		}
		
		// Starting values
		public void InitValues()
		{
			col = RFColliderType.Mesh;
			szF = 0;
			dec = false;
			rem = false;
			l   = true;
			lay = 0;
			t   = true;
			tag = string.Empty;
		}

		// Copy from
		public void CopyFrom (RFFragmentProperties props)
		{
			col = props.col;
			szF = props.szF;
			dec = false;
			rem = props.rem;
			l   = props.l;
			lay = props.lay;
			t   = props.t;
			tag = props.tag;
		}
		
		/// /////////////////////////////////////////////////////////
		/// Layer & Tag
		/// /////////////////////////////////////////////////////////
        
		// Get layer for fragments
		public static int GetLayer (RayfireRigid scr)
		{
			// Inherit layer
			if (scr.meshDemolition.prp.l == true)
				return scr.gameObject.layer;

			// Get custom layer
			return scr.meshDemolition.prp.lay;
		}
        
		// Get tag for fragments
		public static string GetTag (RayfireRigid scr)
		{
			// Inherit tag
			if (scr.meshDemolition.prp.t == true)
				return scr.gameObject.tag;
            
			// Set tag. Not defined -> Untagged
			if (scr.meshDemolition.prp.tag.Length == 0)
				return "Untagged";
            
			// Set tag.
			return scr.meshDemolition.prp.tag;
		}
		
		// Set tag for fragments
		public static void SetTag (RayfireRigid scr)
		{
			if (scr.meshDemolition.prp.t == false)
			{
				string baseTag = GetTag(scr);
				for (int i = 0; i < scr.fragments.Count; i++)
					scr.fragments[i].gameObject.tag = baseTag;
			}
		}

		// Set layer for fragments
		public static void SetLayer (RayfireRigid scr)
		{
			if (scr.meshDemolition.prp.l == false)
			{
				int baseLayer = GetLayer(scr);
				for (int i = 0; i < scr.fragments.Count; i++)
					scr.fragments[i].gameObject.layer = baseLayer;
			}
		}
	}
}