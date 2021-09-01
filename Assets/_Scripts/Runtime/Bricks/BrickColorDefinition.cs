using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Bricks
{
    [CreateAssetMenu( fileName ="Row Color Store", menuName = "Breakout/Row Color Store")]
    public class BrickColorDefinition : ScriptableObject
    {
        /// <summary>
        /// Colors related to a layer.
        /// </summary>
        [SerializeField, Tooltip("Colors related to a layer")]
        private Color[] layerColors = null;

        /// <summary>
        /// Get Color based upon layer.
        /// </summary>
        /// <param name="layer">Color ID</param>
        /// <returns>Color</returns>
        public Color GetColor(int colorId) => colorId < layerColors.Length ? layerColors[colorId] : Color.white;
    }
}
