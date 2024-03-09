using UnityEditor;
using UnityEngine;
using System.IO;
using System.Data;
using System;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine.Video;

namespace BuildingVolumes.Streaming
{

    [CustomEditor(typeof(VideoPlayer))]
    [CanEditMultipleObjects]
    public class VideoPlayerGUI : Editor
    {

        private void OnEnable()
        {
            VideoPlayer player = (VideoPlayer)target;         
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            VideoPlayer player = (VideoPlayer)target;

            GUILayout.Space(20);
            GUILayout.Label("Playback Controls", EditorStyles.boldLabel);

            GUI.enabled = Application.isPlaying;

            GUILayout.BeginHorizontal();
            float desiredFrame = EditorGUILayout.Slider((float)player.frame, 0, (float)player.frameCount);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            //Stops the playback and makes it dissappear
            if (GUILayout.Button(EditorGUIUtility.IconContent("PreMatQuad")))
                player.Stop();

            //Rewinds to first frame
            if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey")))
                player.frame = 0;

            //Pause
            if (player.isPlaying)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("PauseButton")))
                    player.Pause();
            }

            //Play
            else
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.Play")))
                    player.Play();
            }

            GUILayout.EndHorizontal();
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }
    }
}