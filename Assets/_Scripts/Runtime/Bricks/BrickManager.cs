using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Discode.Breakout.Bricks
{
	/// <summary>
	/// Manager for handling bricks.
	/// </summary>
    public class BrickManager : MonoBehaviour
    {
		public static event Brick.BrickEventHandler OnBrickAdded = null;
		public static event Brick.BrickEventHandler OnBrickRemoved = null;

		/// <summary>
		/// Pseudo singleton reference for active instance of the manager.
		/// </summary>
		public static BrickManager Instance { get; private set; }

		/// <summary>
		/// List of created bricks.
		/// </summary>
		private Dictionary<uint, Brick> bricks = new Dictionary<uint, Brick>();

#if UNITY_EDITOR
		public int EditorBrickCount => bricks.Count;
#endif

		/// <summary>
		/// Pseudo singleton logic to ensure only one is allowed.
		/// </summary>
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
		/// Registers brick with the manager.
		/// </summary>
		/// <param name="brick">Brick to be registered</param>
		public void RegisterBrick(Brick brick)
		{
			if (bricks.ContainsKey(brick.netId))
			{
#if UNITY_EDITOR
				Debug.LogWarning($"Brick registered under a net Id that already existed ({brick.netId})");
#endif
				return;
			}
			bricks.Add(brick.netId, brick);
			OnBrickAdded?.Invoke(brick);
		}

		/// <summary>
		/// Unregisters the brick with the manager.
		/// </summary>
		/// <param name="brick">Brick to be unregsitered</param>
		public void UnregisterBrick(Brick brick)
		{
			bricks.Remove(brick.netId);
			OnBrickRemoved?.Invoke(brick);
		}

		/// <summary>
		/// Gets a array of bricks that currently exist.
		/// </summary>
		/// <returns></returns>
		public Brick[] Getbricks() => bricks.Values.ToArray();

		/// <summary>
		/// Find a brick that has been registered.
		/// </summary>
		/// <param name="id">Brick ID</param>
		/// <returns>Brick or null</returns>
		public Brick FindBrick(uint netId)
		{
			if (bricks.ContainsKey(netId))
			{
				return bricks[netId];
			}

			return null;
		}

		[Server]
		public BrickSyncData[] GetBrickSyncData()
		{
			BrickSyncData[] syncData = new BrickSyncData[bricks.Count];
			int i = 0;
			foreach(Brick brick in bricks.Values)
			{
				syncData[i] = new BrickSyncData(brick.netId, brick.ColorId);
				i++;
			}
			return syncData;
		}

		[Client]
		public void UpdateBrickState(BrickSyncData[] syncData)
		{
			foreach(BrickSyncData  data in syncData)
			{
				if (bricks.TryGetValue(data.netId, out Brick brick))
				{
					brick.ColorId = data.ColorId;
				}			
			}
		}
	}
}
