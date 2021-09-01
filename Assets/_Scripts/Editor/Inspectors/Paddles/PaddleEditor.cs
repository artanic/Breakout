using Discode.Breakout.Paddles;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(Paddle))]
    public class PaddleEditor : InspectorEditorBase
    {
        private SerializedProperty boxCollider = null;
        private SerializedProperty speed = null;

        private Paddle paddle = null;

		protected override void OnStart()
		{
            paddle = target as Paddle;
		}

		protected override void FindProperties()
        {
            boxCollider = GetProperty(nameof(boxCollider));
            speed = GetProperty(nameof(speed));
        }

        protected override void DrawProperties()
        {
            EditorGUILayout.PropertyField(speed);
        }

        protected override void DrawReferences()
        {
            EditorGUILayout.PropertyField(boxCollider);
        }

		protected override void DrawWhilePlaying()
		{
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Has Ball?", paddle.HasBall ? "Yes" : "No");

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
		}
	}
}
