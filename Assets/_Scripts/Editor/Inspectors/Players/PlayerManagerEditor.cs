using Discode.Breakout.Bricks;
using Discode.Breakout.Levels;
using Discode.Breakout.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(PlayerManager))]
    public class PlayerManagerEditor : InspectorEditorBase
    {
        private PlayerManager playerManager = null;

        protected override void OnStart()
        {
            showPropertiesFoldout = false;

            playerManager = target as PlayerManager;
        }

        protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Number of Players", playerManager.PlayerCount.ToString());

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
