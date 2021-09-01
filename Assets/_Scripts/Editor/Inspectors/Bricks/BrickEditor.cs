using Discode.Breakout.Bricks;
using Discode.Breakout.Levels;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(Brick))]
    public class BrickEditor : InspectorEditorBase
    {
        private bool showEffects = false;

        private SerializedProperty boxCollider = null;
        private SerializedProperty brickColorDefinition = null;
        private SerializedProperty breakSoundTemplate = null;
        private SerializedProperty breakEffects = null;

        private Brick brick = null;

        protected override void OnStart()
        {
            showPropertiesFoldout = false;

            brick = target as Brick;
        }

        protected override void FindProperties()
        {
            boxCollider = GetProperty(nameof(boxCollider));
            brickColorDefinition = GetProperty(nameof(brickColorDefinition));
            breakSoundTemplate = GetProperty(nameof(breakSoundTemplate));
            breakEffects = GetProperty(nameof(breakEffects));
        }

		protected override void DrawContents()
		{
            showEffects = Scribe.EditorGUILayoutFoldout("Effects", "Effects", showEffects);

            if (showEffects)
            {
                Scribe.EditorGUILayoutBegingGroup();
                Scribe.EditorGUILayoutBeginIndent();

                EditorGUILayout.PropertyField(breakEffects);

                Scribe.EditorGUILayoutEndIndent();
                Scribe.EditorGUILayoutEndGroup();
            }
        }

		protected override void DrawReferences()
        {
            EditorGUILayout.PropertyField(boxCollider);
            EditorGUILayout.PropertyField(brickColorDefinition);
            EditorGUILayout.PropertyField(breakSoundTemplate);
        }

        protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Net ID", brick.netId.ToString());
            EditorGUILayout.LabelField("Color ID", brick.ColorId.ToString());

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
