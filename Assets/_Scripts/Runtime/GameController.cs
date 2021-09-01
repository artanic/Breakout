using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Players;
using Discode.Breakout.Networking;
using Discode.Breakout.Audio;
using System;
using Discode.Breakout.Server;

namespace Discode.Breakout
{
	/// <summary>
	/// Main controller for handling specific game scope behavour.
	/// </summary>
    public class GameController : MonoBehaviour
    {
		public static event Action<Scorer> OnScorerChanged = null;
		public static event Action OnNewGame = null;

        [SerializeField, Required]
        private ServerGameBehaviour serverGameBehaviourTemplate = null;

		[SerializeField, Required]
		private Scorer scorerTempalte = null;

		[SerializeField, Required]
		private Material backgroundMaterial = null;

		[SerializeField]
		private int backgroundDepth = 15;

		private AudioController audioController = null;
		private GameObject backdrop = null;

		public Scorer Scorer { get; private set; } = null;

		private void Awake()
		{
			GameNetworkManager.OnServerStart += OnServerStart;
			GameNetworkManager.OnClientStart += OnClientStart;
			GameNetworkManager.OnClientStop += OnStopClient;
		}

		private void OnDestroy()
		{
			GameNetworkManager.OnServerStart -= OnServerStart;
			GameNetworkManager.OnClientStart -= OnClientStart;
			GameNetworkManager.OnClientStop -= OnStopClient;
		}

		private void Start()
		{
			audioController = GetComponent<AudioController>();
			GenerateBackdrop();
		}

		/// <summary>
		/// Invoked when the server becomes active.
		/// </summary>
		private void OnServerStart()
		{
			GameObject newObject = Instantiate(serverGameBehaviourTemplate.gameObject);
			NetworkServer.Spawn(newObject);

			Scorer = Instantiate(scorerTempalte);
			NetworkServer.Spawn(Scorer.gameObject);
			OnScorerChanged?.Invoke(Scorer);
		}

		private void OnClientStart()
		{
			NetworkClient.RegisterHandler<GameEventMessage>(OnGameEvent);
		}

		private void OnStopClient()
		{
			NetworkClient.UnregisterHandler<GameEventMessage>();
		}

		private void OnGameEvent(GameEventMessage message)
		{
			switch(message.GameEvent)
			{
				case GameEventType.Dropout:
					audioController.PlayDropoutSound();
					break;
				case GameEventType.NewGame:
					OnNewGame?.Invoke();
					break;
			}
		}

		/// <summary>
		/// Precedurally create a mesh black unlit backdrop. Used even if there is a background unless using skybox.
		/// </summary>
		private void GenerateBackdrop()
		{
			if (backdrop != null)
			{
				return;
			}

			// Get the top left position of the screen in 3D space.
			Vector3 screenTopLeft = new Vector3(0, Screen.height, 0);
			Vector3 topLeftPosition = Camera.main.ScreenToWorldPoint(screenTopLeft);

			// Get the bottom right position of the screen in 3D Space.
			Vector3 screenBottomRight = new Vector3(Screen.width, 0, 0);
			Vector3 bottomRightPosition = Camera.main.ScreenToWorldPoint(screenBottomRight);

			// Width/Height in world measurements.
			float width = bottomRightPosition.x - topLeftPosition.x;
			float height = topLeftPosition.y - bottomRightPosition.y;

			// The 'center' point is actually in the bottom left corner of the mesh.
			Vector3 center = new Vector3
			{
				x = topLeftPosition.x,
				y = bottomRightPosition.y,
				z = backgroundDepth
			};

			// Generate gameobject with mesh rendering components.
			backdrop = new GameObject("Backdrop");
			MeshRenderer meshRenderer = backdrop.AddComponent<MeshRenderer>();
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.sharedMaterial = backgroundMaterial;

			MeshFilter meshFilter = backdrop.AddComponent<MeshFilter>();

			Mesh mesh = new Mesh();

			Vector3[] vertices = new Vector3[4]
			{
				new Vector3(0 , 0, 0),
				new Vector3(width, 0 , 0),
				new Vector3(0, height, 0),
				new Vector3(width, height, 0)
			};
			mesh.vertices = vertices;

			int[] triangles = new int[6]
			{
				0, 2, 1,
				2, 3, 1
			};
			mesh.triangles = triangles;

			Vector3[] normals = new Vector3[4]
			{
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
			};
			mesh.normals = normals;

			Vector2[] uv = new Vector2[4]
			{
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(0, 1),
				new Vector2(1, 1)
			};
			mesh.uv = uv;

			meshFilter.mesh = mesh;
			backdrop.transform.position = center;
		}
	}
}
