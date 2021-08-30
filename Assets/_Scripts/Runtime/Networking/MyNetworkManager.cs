using Discode.Breakout.Gameplay;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Networking
{
    public class MyNetworkManager : NetworkManager
    {
		public delegate void NetworkEventHandler(NetworkConnection connection);

		public static event Action OnServerStart = null;
		public static event Action OnServerStop = null;
		public static event Action OnClientStart = null;
		public static event Action OnClientStop = null;
		public static event Action OnServerConnection = null;
		public static event Action OnServerDisconnection = null;
		public static event NetworkEventHandler OnAddPlayer;

		public override void OnStartServer() => OnServerStart?.Invoke();

		public override void OnStopServer() => OnServerStop?.Invoke();

		public override void OnServerAddPlayer(NetworkConnection conn) => OnAddPlayer?.Invoke(conn);

		public override void OnStartClient() => OnClientStart?.Invoke();

		public override void OnStopClient() => OnClientStop?.Invoke();

		public override void OnServerConnect(NetworkConnection conn) => OnServerConnection?.Invoke();

		public override void OnServerDisconnect(NetworkConnection conn)
		{
			OnServerDisconnection?.Invoke();

			base.OnServerDisconnect(conn);
		}
	}
}
