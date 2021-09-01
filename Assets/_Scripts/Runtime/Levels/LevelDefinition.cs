using Discode.Breakout.Bricks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Discode.Breakout.Levels
{
    public abstract class LevelDefinition : ScriptableObject
    {
        /// <summary>
        /// The depth in Z-axiss the main level contents are generated.
        /// </summary>
        [SerializeField, Tooltip("The depth in Z-axiss the main level contents are generated.")]
        private int levelDepth = 10;

        /// <summary>
        /// The depth in Z-axiss the main level contents are generated.
        /// </summary>
        public int LevelDepth => levelDepth;

        /// <summary>
        /// Generate the layout of bricks in a retangular bock shape.
        /// </summary>
        /// <param name="nextBrickId">Unused brick ID. Must be iterative.</param>
        /// <returns>last iterative Brick ID not used.</returns>
        public abstract void LayoutBricks(Func<Vector3, Brick> brickCreator);
    }
}
