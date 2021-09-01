using Discode.Breakout.Audio;
using Discode.Breakout.Balls;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(AudioController))]
    public class AudioControllerEditor : InspectorEditorBase
    {
        private SerializedProperty dropoutSoundTemplate = null;
        private SerializedProperty musicAudioSource = null;
        private SerializedProperty playMusicOnJoin = null;

        private AudioController audioController = null;

		protected override void FindProperties()
		{
            dropoutSoundTemplate = GetProperty(nameof(dropoutSoundTemplate));
            musicAudioSource = GetProperty(nameof(musicAudioSource));
            playMusicOnJoin = GetProperty(nameof(playMusicOnJoin));
        }

		protected override void OnStart()
        {
            audioController = target as AudioController;
        }

		protected override void DrawProperties()
		{
            EditorGUILayout.PropertyField(playMusicOnJoin);
        }

		protected override void DrawReferences()
		{
            EditorGUILayout.PropertyField(dropoutSoundTemplate);
            EditorGUILayout.PropertyField(musicAudioSource);
        }

		protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Is music playing", audioController.IsMusicPlaying ? "Yes" : "No");

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
