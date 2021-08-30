using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Players;
using Discode.Breakout.Networking;

namespace Discode.Breakout.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private ServerGameBehaviour gameBehaviourTemplate = null;

		[SerializeField]
		private BrickManager brickManagerTemplate = null;

		[SerializeField]
		private Scorer scorerTemplate = null;

		[SerializeField]
		private Material backgroundMaterial = null;

		[SerializeField]
		private int backgroundDepth = 15;

		public Scorer CurrentScorer { get; private set; }

		private void Awake()
		{
			MyNetworkManager.OnServerStart += OnServerStart;
		}

		private void OnDestroy()
		{
			MyNetworkManager.OnServerStart -= OnServerStart;
		}

		private void Start()
		{
			GenerateBackground();
		}

		private void OnServerStart()
		{
			BrickManager newBrickManager = Instantiate(brickManagerTemplate);
			NetworkServer.Spawn(newBrickManager.gameObject);

			ServerGameBehaviour newBehaviour = Instantiate(gameBehaviourTemplate);
			NetworkServer.Spawn(newBehaviour.gameObject);

			Scorer newScorer = Instantiate(scorerTemplate);
			CurrentScorer = newScorer;
			NetworkServer.Spawn(CurrentScorer.gameObject);
		}

		private void GenerateBackground()
		{
			Vector3 screenTopLeft = new Vector3(0, Screen.height, 0);
			Vector3 topLeftPosition = Camera.main.ScreenToWorldPoint(screenTopLeft);

			new GameObject("TopLeft").transform.position = topLeftPosition;

			Vector3 screenBottomRight = new Vector3(Screen.width, 0, 0);
			Vector3 bottomRightPosition = Camera.main.ScreenToWorldPoint(screenBottomRight);

			new GameObject("BottomRight").transform.position = bottomRightPosition;

			float width = bottomRightPosition.x - topLeftPosition.x;
			float height = topLeftPosition.y - bottomRightPosition.y;

			Vector3 center = new Vector3
			{
				x = topLeftPosition.x,
				y = bottomRightPosition.y,
				z = backgroundDepth
			};

			GameObject backgroundGO = new GameObject("Background");
			MeshRenderer meshRenderer = backgroundGO.AddComponent<MeshRenderer>();
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.sharedMaterial = backgroundMaterial;

			MeshFilter meshFilter = backgroundGO.AddComponent<MeshFilter>();

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
			backgroundGO.transform.position = center;
		}

		public int GetCurrentScore() => CurrentScorer != null ? CurrentScorer.CurrentScore : 0;
	}
}
