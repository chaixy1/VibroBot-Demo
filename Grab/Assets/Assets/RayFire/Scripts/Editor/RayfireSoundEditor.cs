using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.Audio;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireSound))]
    public class RayfireSoundEditor : Editor
    {
        RayfireSound       sound;
        SerializedProperty сlipsInitProp;
        ReorderableList    clipsInitList; 
        SerializedProperty сlipsActProp;
        ReorderableList    clipsActList;  
        SerializedProperty сlipsDmlProp;
        ReorderableList    clipsDmlList;
        SerializedProperty сlipsColProp;
        ReorderableList    clipsColList; 
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        static int space = 3;
        
        static GUIContent gui_volBase    = new GUIContent ("Base Volume",       "Base volume. Can be increased by Size Volume property.");
        static GUIContent gui_colSize    = new GUIContent ("Size Volume",       "Additional volume per one unit size.");
        static GUIContent gui_eventsInit = new GUIContent ("Initialization",    "Enable Initialization sound.");
        static GUIContent gui_eventsAct  = new GUIContent ("Activation",        "Enable Activation sound");
        static GUIContent gui_eventsDml  = new GUIContent ("Demolition",        "Enable Demolition sound");
        static GUIContent gui_eventsCol  = new GUIContent ("Collision",         "Enable Collision sound");
        static GUIContent gui_once       = new GUIContent ("Play Once",         "");
        static GUIContent gui_soundMult  = new GUIContent ("Multiplier",        "Sound volume multiplier for this event.");
        static GUIContent gui_soundVel   = new GUIContent ("Relative Velocity", "Minimum Relative Velocity collision threshold to play collision sound.");
        static GUIContent gui_clip       = new GUIContent ("AudioClip",         "The AudioClip AAsset played by the AudioSource");
        static GUIContent gui_group      = new GUIContent ("Output",            "");
        static GUIContent gui_prior      = new GUIContent ("Priority",          "Sets the priority of the source. Note that a sound with a larger priority value will more likely be stolen by sounds with smaller priority values.");
        static GUIContent gui_spat       = new GUIContent ("Spatial Blend",     "0 = 2D, 1 = 3D");
        static GUIContent gui_mind       = new GUIContent ("Min Distance",      "Within Min Distance, te volume will stay at the loudest possible. Outside of this Min Distance it begins to attenuate.");
        static GUIContent gui_maxd       = new GUIContent ("Max Distance",      "Max Distance is the distance a sound stops attenuating at.");
        static GUIContent gui_filterSize = new GUIContent ("Minimum Size",      "Objects with size lower than defined value will not make sound.");
        static GUIContent gui_filterDist = new GUIContent ("Camera Distance",   "Objects with distance to main camera higher than defined value will not make sound.");

        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////
        
        private void OnEnable()
        {
            сlipsInitProp                     = serializedObject.FindProperty("initialization.clips");
            clipsInitList                     = new ReorderableList(serializedObject, сlipsInitProp, true, true, true, true);
            clipsInitList.drawElementCallback = DrawInitListItems;
            clipsInitList.drawHeaderCallback  = DrawInitHeader;
            clipsInitList.onAddCallback       = AddInit;
            clipsInitList.onRemoveCallback    = RemoveInit;

            сlipsActProp                     = serializedObject.FindProperty("activation.clips");
            clipsActList                     = new ReorderableList(serializedObject, сlipsActProp, true, true, true, true);
            clipsActList.drawElementCallback = DrawActListItems;
            clipsActList.drawHeaderCallback  = DrawActHeader;
            clipsActList.onAddCallback       = AddAct;
            clipsActList.onRemoveCallback    = RemoveAct;

            сlipsDmlProp                     = serializedObject.FindProperty("demolition.clips");
            clipsDmlList                     = new ReorderableList(serializedObject, сlipsDmlProp, true, true, true, true);
            clipsDmlList.drawElementCallback = DrawDmlListItems;
            clipsDmlList.drawHeaderCallback  = DrawDmlHeader;
            clipsDmlList.onAddCallback       = AddDml;
            clipsDmlList.onRemoveCallback    = RemoveDml;
            
            сlipsColProp                     = serializedObject.FindProperty("collision.clips");
            clipsColList                     = new ReorderableList(serializedObject, сlipsColProp, true, true, true, true);
            clipsColList.drawElementCallback = DrawColListItems;
            clipsColList.drawHeaderCallback  = DrawColHeader;
            clipsColList.onAddCallback       = AddCol;
            clipsColList.onRemoveCallback    = RemoveCol;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            sound = target as RayfireSound;
            if (sound == null)
                return;
            
            GUILayout.Space (8);
            
            UI_Vol();
            
            GUILayout.Space (space);
            
            UI_Events();

            GUILayout.Space (space);

            UI_Filters();
            
            GUILayout.Space (8);
            
            if (Application.isPlaying == true)
            {
                GUILayout.Label ("Info", EditorStyles.boldLabel);
                GUILayout.Space (space);
                
                GUILayout.Label ("  Volume: " + RFSound.GeVolume(sound, 0f));
                
                GUILayout.Space (5);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Volume
        /// /////////////////////////////////////////////////////////
        
        void UI_Vol()
        {
            GUILayout.Space (space);
            GUILayout.Label ("  Volume", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            UI_VolBase();
            
            UI_VolSize();
        }
        
        void UI_VolBase()
        {
            GUILayout.Space (space);

            EditorGUI.BeginChangeCheck();
            sound.baseVolume = EditorGUILayout.Slider (gui_volBase, sound.baseVolume, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    scr.baseVolume = sound.baseVolume;
                    SetDirty (scr);
                }

            //EditorGUILayout.MinMaxSlider (gui_volBase, ref sound.baseVolume, ref sound.sizeVolume, 0f, 1f);
            //EditorGUILayout.BeginFadeGroup ()
        }
        
        void UI_VolSize()
        {
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.sizeVolume = EditorGUILayout.Slider (gui_colSize, sound.sizeVolume, 0f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    scr.sizeVolume = sound.sizeVolume;
                    SetDirty (scr);
                }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Events
        /// /////////////////////////////////////////////////////////
        
        void UI_Events()
        {
            GUILayout.Space (space);
            GUILayout.Label ("  Events", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            UI_EventsInit();
            
            UI_EventsAct();
            
            UI_EventsDml();
            
            UI_EventsCol();
        }
        
        void UI_EventsInit()
        {
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.enable = EditorGUILayout.Toggle (gui_eventsInit, sound.initialization.enable);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.enable = sound.initialization.enable;
                    SetDirty (scr);
                }
            }

            if (sound.initialization.enable == true)
                UI_PropsInit();
        }
        
        void UI_EventsAct()
        {
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.activation.enable = EditorGUILayout.Toggle (gui_eventsAct, sound.activation.enable);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.enable = sound.activation.enable;
                    SetDirty (scr);
                }

            if (sound.activation.enable == true)
                UI_PropsAct();
        }
        
        void UI_EventsDml()
        {
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.enable = EditorGUILayout.Toggle (gui_eventsDml, sound.demolition.enable);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.enable = sound.demolition.enable;
                    SetDirty (scr);
                }

            if (sound.demolition.enable == true)
                UI_PropsDml();
        }
        
        void UI_EventsCol()
        {
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.collision.enable = EditorGUILayout.Toggle (gui_eventsCol, sound.collision.enable);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireSound scr in targets)
                {
                    scr.collision.enable = sound.collision.enable;
                    SetDirty (scr);
                }

            if (sound.collision.enable == true)
                UI_PropsCol();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_PropsInit()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Initialization Sound", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.InitializationSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).initialization.played = false;
                        }
            }

            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.once = EditorGUILayout.Toggle (gui_once, sound.initialization.once);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.once = sound.initialization.once;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.multiplier = EditorGUILayout.Slider (gui_soundMult, sound.initialization.multiplier, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.multiplier = sound.initialization.multiplier;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.initialization.clip = (AudioClip)EditorGUILayout.ObjectField (gui_clip, sound.initialization.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.initialization.clip = sound.initialization.clip;
                    SetDirty (scr);
                }
            }
            
            if (sound.initialization.clip == null)
            {
                GUILayout.Space (space);

                serializedObject.Update();
                clipsInitList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space (space);

            UI_Properties (sound.initialization, 0);
            
            EditorGUI.indentLevel--;
        }
        
        void UI_PropsAct()
        {
            // Initialize
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Activation Sound", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.ActivationSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).activation.played = false;
                        }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            sound.activation.once = EditorGUILayout.Toggle (gui_once, sound.activation.once);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.once = sound.activation.once;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.activation.multiplier = EditorGUILayout.Slider (gui_soundMult, sound.activation.multiplier, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.multiplier = sound.activation.multiplier;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.activation.clip = (AudioClip)EditorGUILayout.ObjectField (gui_clip, sound.activation.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.activation.clip = sound.activation.clip;
                    SetDirty (scr);
                }
            }

            if (sound.activation.clip == null)
            {
                GUILayout.Space (space);
                
                serializedObject.Update();
                clipsActList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space (space);

            UI_Properties (sound.activation, 1);
            
            EditorGUI.indentLevel--;
        }
        
        void UI_PropsDml()
        {
            // Initialize
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Demolition Sound", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.DemolitionSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).demolition.played = false;
                        }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.once = EditorGUILayout.Toggle (gui_once, sound.demolition.once);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.once = sound.demolition.once;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.multiplier = EditorGUILayout.Slider (gui_soundMult, sound.demolition.multiplier, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.multiplier = sound.demolition.multiplier;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.demolition.clip = (AudioClip)EditorGUILayout.ObjectField (gui_clip, sound.demolition.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    scr.demolition.clip = sound.demolition.clip;
                    SetDirty (scr);
                }
            }

            if (sound.demolition.clip == null)
            {
                GUILayout.Space (space);

                serializedObject.Update();
                clipsDmlList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space (space);
            
            UI_Properties (sound.demolition, 2);
            
            EditorGUI.indentLevel--;
        }
        
        void UI_PropsCol()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.Space (space);
                
                if (GUILayout.Button ("Collision Sound", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.CollisionSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).collision.played = false;
                        }
            }
            
            GUILayout.Space (space);
            
            EditorGUI.indentLevel++;
            
            GUILayout.Space (space);
            GUILayout.Label ("  Collision", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.relativeVelocity = EditorGUILayout.Slider (gui_soundVel, sound.relativeVelocity, 0.1f, 100f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    scr.relativeVelocity = sound.relativeVelocity;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            GUILayout.Label ("     Last Collision Relative Velocity: " + sound.lastCollision.ToString());
            GUILayout.Space (space);
            
            GUILayout.Space (space);
            GUILayout.Label ("  Sound", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.collision.once = EditorGUILayout.Toggle (gui_once, sound.collision.once);
            if (EditorGUI.EndChangeCheck())
                foreach (RayfireSound scr in targets)
                {
                    scr.collision.once = sound.collision.once;
                    SetDirty (scr);
                }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.collision.multiplier = EditorGUILayout.Slider (gui_soundMult, sound.collision.multiplier, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    scr.collision.multiplier = sound.collision.multiplier;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            sound.collision.clip = (AudioClip)EditorGUILayout.ObjectField (gui_clip, sound.collision.clip, typeof(AudioClip), true);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    scr.collision.clip = sound.collision.clip;
                    SetDirty (scr);
                }

            if (sound.collision.clip == null)
            {
                GUILayout.Space (space);
        
                serializedObject.Update();
                clipsColList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
            
            GUILayout.Space (space);

            UI_Properties (sound.collision, 3);
            
            EditorGUI.indentLevel--;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_Properties(RFSound snd, int ind)
        {
            EditorGUI.BeginChangeCheck();
            snd.outputGroup = (AudioMixerGroup)EditorGUILayout.ObjectField (gui_group, snd.outputGroup, typeof(AudioMixerGroup), true);
            if (EditorGUI.EndChangeCheck() == true)
            {
                foreach (RayfireSound scr in targets)
                {
                    if (ind == 0) scr.initialization.outputGroup  = snd.outputGroup;
                    else if (ind == 1) scr.activation.outputGroup = snd.outputGroup;
                    else if (ind == 2) scr.demolition.outputGroup = snd.outputGroup;
                    else if (ind == 3) scr.collision.outputGroup  = snd.outputGroup;
                    SetDirty (scr);
                }
            }
            
            GUILayout.Space (space);

            if (snd.outputGroup == null)
                return;
            
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            snd.priority = EditorGUILayout.IntSlider (gui_prior, snd.priority, 0, 256);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    if (ind == 0) scr.initialization.priority  = snd.priority;
                    else if (ind == 1) scr.activation.priority = snd.priority;
                    else if (ind == 2) scr.demolition.priority = snd.priority;
                    else if (ind == 3) scr.collision.priority  = snd.priority;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            snd.spatial = EditorGUILayout.Slider (gui_spat, snd.spatial, 0, 1);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    if (ind == 0) scr.initialization.spatial  = snd.spatial;
                    else if (ind == 1) scr.activation.spatial = snd.spatial;
                    else if (ind == 2) scr.demolition.spatial = snd.spatial;
                    else if (ind == 3) scr.collision.spatial  = snd.spatial;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            snd.minDist = EditorGUILayout.FloatField (gui_mind, snd.minDist);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    if (ind == 0) scr.initialization.minDist  = snd.minDist;
                    else if (ind == 1) scr.activation.minDist = snd.minDist;
                    else if (ind == 2) scr.demolition.minDist = snd.minDist;
                    else if (ind == 3) scr.collision.minDist  = snd.minDist;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            snd.maxDist = EditorGUILayout.FloatField (gui_maxd, snd.maxDist);
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    if (ind == 0) scr.initialization.maxDist  = snd.maxDist;
                    else if (ind == 1) scr.activation.maxDist = snd.maxDist;
                    else if (ind == 2) scr.demolition.maxDist = snd.maxDist;
                    else if (ind == 3) scr.collision.maxDist  = snd.maxDist;
                    SetDirty (scr);
                }
            
            GUILayout.Space (space);
            
            EditorGUI.indentLevel--;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void UI_Filters()
        {
            GUILayout.Space (space);
            GUILayout.Label ("  Filters", EditorStyles.boldLabel);
            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            
            sound.minimumSize = EditorGUILayout.Slider (gui_filterSize, sound.minimumSize, 0f, 1f);
            
            GUILayout.Space (space);
            
            sound.cameraDistance = EditorGUILayout.Slider (gui_filterDist, sound.cameraDistance, 0f, 999f);
            
            if (EditorGUI.EndChangeCheck() == true)
                foreach (RayfireSound scr in targets)
                {
                    scr.minimumSize    = sound.minimumSize;
                    scr.cameraDistance = sound.cameraDistance;
                    SetDirty (scr);
                }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = clipsInitList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Clips");
        }

        void AddInit(ReorderableList list)
        {
            if (sound.initialization.clips == null)
                sound.initialization.clips = new List<AudioClip>();
            sound.initialization.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (sound.initialization.clips != null)
            {
                sound.initialization.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawActListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = clipsActList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawActHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Clips");
        }

        void AddAct(ReorderableList list)
        {
            if (sound.activation.clips == null)
                sound.activation.clips = new List<AudioClip>();
            sound.activation.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveAct(ReorderableList list)
        {
            if (sound.activation.clips != null)
            {
                sound.activation.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawDmlListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = clipsDmlList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawDmlHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Clips");
        }

        void AddDml(ReorderableList list)
        {
            if (sound.demolition.clips == null)
                sound.demolition.clips = new List<AudioClip>();
            sound.demolition.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveDml(ReorderableList list)
        {
            if (sound.demolition.clips != null)
            {
                sound.demolition.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawColListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = clipsColList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawColHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Clips");
        }

        void AddCol(ReorderableList list)
        {
            if (sound.collision.clips == null)
                sound.collision.clips = new List<AudioClip>();
            sound.collision.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveCol(ReorderableList list)
        {
            if (sound.collision.clips != null)
            {
                sound.collision.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireSound scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
    }
}