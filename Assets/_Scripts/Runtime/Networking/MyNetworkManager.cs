using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Networking
{
    public class MyNetworkManager : NetworkManager
    {
		public delegate void NetworkEventHandler(NetworkConnection connection);

		public static event NetworkEventHandler OnAddPlayer;

		public override void OnServerAddPlayer(NetworkConnection conn) => OnAddPlayer?.Invoke(conn);
	}
}
