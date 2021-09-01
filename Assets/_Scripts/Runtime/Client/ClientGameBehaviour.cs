using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Levels;
using Discode.Breakout.Bricks;
using Discode.Breakout.Server;
using System;

namespace Discode.Breakout.Client
{
    /// <summary>
    /// Handles client to server lobic. Only one per player.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClientGameBehaviour : NetworkBehaviour
    {
        public static event Action OnResetGameRequest = null;

        /// <summary>
        /// Reference for generating content for the client.
        /// </summary>
        [SerializeField, Required("Reference to a Levelbuilder component is needed.")]
        private LevelBuilder levelBuilder = null;

		private void OnDisable()
		{
            GameController.OnNewGame -= OnNewGame;
        }

		public override void OnStartClient()
		{
            if (hasAuthority)
            {
                GameController.OnNewGame += OnNewGame;


                if (NetworkClient.isHostClient == false)
                {
                    levelBuilder.CreateBorderAndDropzone();
                }
            }
		}

		/// <summary>
		/// Fetch brick state data after new game setup.
		/// </summary>
		private void OnNewGame()
		{
			GetBrickData();
		}

		public override void OnStopClient()
		{
            if (hasAuthority)
            {
                if (NetworkClient.isHostClient == false)
                {
                    levelBuilder.Clear();
                }
            }
        }

		/// <summary>
		/// Retrieve current data on bricks from the server.
		/// </summary>
		[Command]
		public void GetBrickData()
		{
			var data = BrickManager.Instance.GetBrickSyncData();
			RpcBrickData(data);
		}

		/// <summary>
		/// Update the local client's bricks state.
		/// </summary>
		/// <param name="syncData">Array of latest brick data</param>
		[TargetRpc]
		public void RpcBrickData(BrickSyncData[] syncData)
		{
			BrickManager.Instance.UpdateBrickState(syncData);
		}

		/// <summary>
		/// Request the server resets the game to starting state.
		/// </summary>
		[Command]
        public void TryResetLevel()
		{
            OnResetGameRequest?.Invoke();
        }
    }
}
