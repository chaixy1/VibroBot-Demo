using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFSound
    {
        // UI
        public bool            enable;
        public bool            once;
        public float           multiplier;
        public AudioClip       clip;
        public List<AudioClip> clips;
        public AudioMixerGroup outputGroup;
        public int             priority;
        public float           spatial;
        public float           minDist;
        public float           maxDist;
        
        // Non Serialized
        [NonSerialized] public bool played;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFSound()
        {
            enable     = false;
            multiplier = 1f;
            priority   = 128;
            spatial    = 1f;
            minDist    = 1;
            maxDist    = 500;
        }
        
        // Copy from
        public RFSound (RFSound source)
        {
            enable = source.enable;
            multiplier = source.multiplier;
            clip = source.clip;
            
            if (source.HasClips == true)
            {
                clips = new List<AudioClip>(source.clips.Count);
                for (int i = 0; i < source.clips.Count; i++)
                    clips.Add (source.clips[i]);
            }

            outputGroup = source.outputGroup;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Copy
        /// /////////////////////////////////////////////////////////
        
        // Copy sound to Rigid
        public static void CopySound (RayfireSound source, RayfireRigid target)
        {
            if (source != null && source.enabled == true)
            {
                if (source.initialization.enable == true ||
                    source.demolition.enable == true ||
                    source.activation.enable == true ||
                    source.collision.enable == true)
                {
                    target.sound = target.gameObject.AddComponent<RayfireSound>();
                    target.sound.CopyFrom (source);
                    target.sound.rigid = target;
                }
            }
        }
        
        // Copy sound to Rigids
        public static void CopySound (RayfireSound source, List<RayfireRigid> targets)
        {
            if (source != null && source.enabled == true)
            {
                if (source.initialization.enable == true ||
                    source.demolition.enable == true ||
                    source.activation.enable == true ||
                    source.collision.enable == true)
                    for (int i = 0; i < targets.Count; i++)
                    {
                        targets[i].sound = targets[i].gameObject.AddComponent<RayfireSound>();
                        targets[i].sound.CopyFrom (source);
                        targets[i].sound.rigid = targets[i];
                    }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Play on events
        /// /////////////////////////////////////////////////////////

        // Play
        public static void Play(RayfireSound scr, RFSound sound, float volume)
        {
            // Has output group
            if (sound.outputGroup != null)
            {
                // Get audio source
                GameObject  audioObject = new GameObject("RFSoundSource");
                audioObject.transform.parent = RayfireMan.inst.transform;
                Object.Destroy (audioObject, sound.clip.length + 1f);
                audioObject.transform.position = scr.gameObject.transform.position;
                AudioSource audioSource = audioObject.AddComponent<AudioSource>();
                
                // Setup
                audioSource.clip                  = sound.clip;
                audioSource.playOnAwake           = false;
                audioSource.outputAudioMixerGroup = sound.outputGroup;
                
                // Properties
                audioSource.priority     = sound.priority;
                audioSource.spatialBlend = sound.spatial;
                audioSource.minDistance  = sound.minDist;
                audioSource.maxDistance  = sound.maxDist;
                
                audioSource.Play ();
            }
            else
                AudioSource.PlayClipAtPoint (sound.clip, scr.gameObject.transform.position, volume);
        }
        
        // Initialization sound
        public static void InitializationSound (RayfireSound scr, float size)
        {
            // Null
            if (scr == null)
                return;

            // Turned off
            if (scr.initialization.enable == false)
                return;

            // Filtering
            if (FilterCheck(scr, size) == false)
                return;
            
            // Get play clip
            if (scr.initialization.clip == null && scr.initialization.HasClips == true)
                scr.initialization.clip = scr.initialization.clips[Random.Range (0, scr.activation.clips.Count)];
            
            // Has no clip
            if (scr.initialization.clip == null)
                return;

            // Played
            if (scr.initialization.once == true && scr.initialization.played == true)
                return;

            // Get volume
            float volume = GeVolume (scr, size) * scr.initialization.multiplier;
            
            // Play
            Play (scr, scr.initialization, volume);
            
            // State
            scr.initialization.played = true;
        }
        
        // Activation sound
        public static void ActivationSound (RayfireSound scr, float size)
        {
            // Null
            if (scr == null)
                return;

            // Turned off
            if (scr.activation.enable == false)
                return;
            
            // Filtering
            if (FilterCheck(scr, size) == false)
                return;
            
            // Get play clip
            if (scr.activation.clip == null && scr.activation.HasClips == true)
                scr.activation.clip = scr.activation.clips[Random.Range (0, scr.activation.clips.Count)];
            
            // Has no clip
            if (scr.activation.clip == null)
                return;
            
            // Played
            if (scr.activation.once == true && scr.activation.played == true)
                return;
            
            // Get volume
            float volume = GeVolume (scr, size) * scr.activation.multiplier;;
            
            // Play
            Play (scr, scr.activation, volume);
                        
            // State
            scr.activation.played = true;
        }

        // Demolition sound
        public static void DemolitionSound (RayfireSound scr, float size)
        {
            // Null
            if (scr == null)
                return;
            
            // Turned off
            if (scr.demolition.enable == false)
                return;
            
            // Filtering
            if (FilterCheck(scr, size) == false)
                return;
           
            // Get play clip
            if (scr.demolition.clip == null && scr.demolition.HasClips == true)
                scr.demolition.clip = scr.demolition.clips[Random.Range (0, scr.demolition.clips.Count)];

            // Has no clip
            if (scr.demolition.clip == null)
                return;

            // Played
            if (scr.demolition.once == true && scr.demolition.played == true)
                return;
            
            // Get volume
            float volume = GeVolume (scr, size) * scr.demolition.multiplier;

            // Play
            Play (scr, scr.demolition, volume);
                                    
            // State
            scr.demolition.played = true;
        }
        
        // Demolition sound
        public static void CollisionSound (RayfireSound scr, float size)
        {
            // Null
            if (scr == null)
                return;
            
            // Turned off
            if (scr.collision.enable == false)
                return;
            
            // Filtering
            if (FilterCheck(scr, size) == false)
                return;
           
            // Get play clip
            if (scr.collision.clip == null && scr.collision.HasClips == true)
                scr.collision.clip = scr.collision.clips[Random.Range (0, scr.collision.clips.Count)];

            // Has no clip
            if (scr.collision.clip == null)
                return;

            // Played
            if (scr.collision.once == true && scr.collision.played == true)
                return;
            
            // Get volume
            float volume = GeVolume (scr, size) * scr.collision.multiplier;

            // Play
            Play (scr, scr.collision, volume);
                                    
            // State
            scr.collision.played = true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Get volume
        public static float GeVolume (RayfireSound scr, float size)
        {
            // Get size if not defined
            if (size <= 0)
                if (scr.rigid != null)
                    size = scr.rigid.limitations.bboxSize;
            
            // Get volume
            float volume = scr.baseVolume;
            if (scr.sizeVolume > 0)
                volume += size * scr.sizeVolume;
            
            return volume;
        }
        
        // Filters check
        static bool FilterCheck (RayfireSound scr, float size)
        {
            // Small size
            if (scr.minimumSize > 0)
                if (size < scr.minimumSize)
                    return false;

            // Far from camera
            if (scr.cameraDistance > 0)
                if (Camera.main != null)
                    if (Vector3.Distance (Camera.main.transform.position, scr.transform.position) > scr.cameraDistance)
                        return false;
            return true;
        }
        
        // Has clips
        public bool HasClips { get { return clips != null && clips.Count > 0; } }
    }
}

