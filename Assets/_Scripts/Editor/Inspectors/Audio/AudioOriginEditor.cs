using Discode.Breakout.Audio;
using Discode.Breakout.Balls;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(AudioOrigin))]
    public class AudioOriginEditor : InspectorEditorBase
    {
        private SerializedProperty audioSource = null;
        private SerializedProperty delay = null;

        private AudioOrigin audioOrigin = null;

		protected override void FindProperties()
		{
            audioSource = GetProperty(nameof(audioSource));
            delay = GetProperty(nameof(delay));
        }

		protected override void OnStart()
        {
            audioOrigin = target as AudioOrigin;
        }

		protected override void DrawProperties()
		{
            EditorGUILayout.PropertyField(delay);
        }

		protected override void DrawReferences()
		{
            EditorGUILayout.PropertyField(audioSource);
        }

		protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Is Active?", audioOrigin.IsActive ? "Yes" : "No");
            EditorGUILayout.LabelField("Is audio playing?", audioOrigin.IsPlaying ? "Yes" : "No");

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
