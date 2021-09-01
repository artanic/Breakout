using Discode.Breakout.Bricks;
using Discode.Breakout.Levels;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(BrickManager))]
    public class BrickManagerEditor : InspectorEditorBase
    {
        private BrickManager brickManager = null;

        protected override void OnStart()
        {
            showReferencesFoldout = false;
            showPropertiesFoldout = false;

            brickManager = target as BrickManager;
        }

        protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Number of bricks", brickManager.EditorBrickCount.ToString());

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
