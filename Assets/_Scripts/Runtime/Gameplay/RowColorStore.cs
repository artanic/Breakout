using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
    [CreateAssetMenu( fileName ="Row Color Store", menuName = "Breakout/Row Color Store")]
    public class RowColorStore : ScriptableObject
    {
        [SerializeField]
        private Color[] layerColors = null;

        public Color GetColor(int layer) => layer < layerColors.Length ? layerColors[layer] : Color.white;
    }
}
