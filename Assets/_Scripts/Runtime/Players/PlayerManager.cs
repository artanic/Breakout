using Discode.Breakout.Networking;
using Discode.Breakout.Players;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Discode.Breakout.Players
{
	/// <summary>
	/// Manager for handling bricks.
	/// </summary>
	public class PlayerManager : MonoBehaviour
    {
        public static event Player.PlayerEventHandler OnPlayerAdded = null;
        public static event Player.PlayerEventHandler OnPlayerRemoved = null;

		/// <summary>
		/// Pseudo singleton reference for active instance of the manager.
		/// </summary>
		public static PlayerManager Instance { get; private set; }

		/// <summary>
		/// Holds all players using their netId as identifier.
		/// </summary>
		private Dictionary<uint, Player> players = new Dictionary<uint, Player>();

		/// <summary>
		/// Get the number of registered players.
		/// </summary>
		public int PlayerCount => players.Count;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}
		
		/// <summary>
		/// Registers player with the manager.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="player"></param>
		public void RegisterPlayer(NetworkConnection connection, Player player)
		{
			if (players.ContainsKey(connection.identity.netId))
			{
#if UNITY_EDITOR
				Debug.LogWarning("Player was registered and already existed in dictionary");
#endif
				players[connection.identity.netId] = player;
			}
			else
			{
				players.Add(connection.identity.netId, player);
			}
			OnPlayerAdded?.Invoke(player);
		}

		/// <summary>
		/// Unregisters the player with the manager.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="player"></param>
		public void UnregisterPlayer(NetworkConnection connection, Player player)
		{
			if (players.Remove(connection.identity.netId))
			{
				OnPlayerRemoved?.Invoke(player);
			}
		}
		
		/// <summary>
		/// Try get a registered player.
		/// </summary>
		/// <param name="connection">Connection that owns the player.</param>
		/// <param name="player">Player found in dictionary.</param>
		/// <returns>Returns true if player found.</returns>
		public bool TryGetPlayer(NetworkConnection connection, out Player player)
		{
			if (connection != null
				&& connection.identity != null
				&& players.ContainsKey(connection.identity.netId))
			{
				player = players[connection.identity.netId];
				return true;
			}

			player = null;
			return false;
		}
	}
}
