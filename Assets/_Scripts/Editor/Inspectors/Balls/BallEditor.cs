using Discode.Breakout.Balls;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(Ball))]
    public class BallEditor : InspectorEditorBase
    {
        private SerializedProperty sphereCollider = null;
        private SerializedProperty ballRigidbody = null;
        private SerializedProperty bounceSoundTemplate = null;
        private SerializedProperty launchBallSoundTemplate = null;
        private SerializedProperty minVelocity = null;

        private Ball ball = null;

        protected override void OnStart()
        {
            ball = target as Ball;
        }

        protected override void FindProperties()
        {
            sphereCollider = GetProperty(nameof(sphereCollider));
            ballRigidbody = GetProperty(nameof(ballRigidbody));
            bounceSoundTemplate = GetProperty(nameof(bounceSoundTemplate));
            launchBallSoundTemplate = GetProperty(nameof(launchBallSoundTemplate));
            minVelocity = GetProperty(nameof(minVelocity));
        }

		protected override void DrawProperties()
		{
            EditorGUILayout.PropertyField(minVelocity);
        }

		protected override void DrawReferences()
        {
            EditorGUILayout.PropertyField(sphereCollider);
            EditorGUILayout.PropertyField(ballRigidbody);
            EditorGUILayout.PropertyField(bounceSoundTemplate);
            EditorGUILayout.PropertyField(launchBallSoundTemplate);
        }

        protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            ball.EditorDebug = EditorGUILayout.Toggle("Enable Debug", ball.EditorDebug);
            EditorGUILayout.LabelField("Is Moving?", ball.Moving ? "Yes" : "No");
            EditorGUILayout.LabelField("Last Collision Object", ball.LastCollectionObject != null ? ball.LastCollectionObject.name : "-");
            EditorGUILayout.LabelField("Second Last Collision Object", ball.EditorSecondLastCollisionObject != null ? ball.EditorSecondLastCollisionObject.name : "-");
            EditorGUILayout.LabelField("Velocity Offset Multiplier", ball.EditorVelocityOffsetMultiplier.ToString());

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
