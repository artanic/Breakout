using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Audio;
using Discode.Breakout.Paddles;
using UnityEngine.InputSystem;
using Discode.Breakout.Client;
using Discode.Breakout.Balls;

namespace Discode.Breakout.Players
{
	/// <summary>
	/// Handlers main player related functions.
	/// </summary>
	public class Player : NetworkBehaviour
	{
		public delegate void PlayerEventHandler(Player player);

		/// <summary>
		/// Reference to the players local game behaviour.
		/// </summary>
		[SerializeField, Required, Tooltip("Reference to the players local game behaviour.")]
		private ClientGameBehaviour clientGameController = null;

		/// <summary>
		/// Reference for the player input controls.
		/// </summary>
		[SerializeField, Required, Tooltip("Reference for the player input controls.")]
		private PlayerControls playerControls = null;

		[SerializeField]
		private Paddle currentPaddle;

		/// <summary>
		/// Current paddle assigned to the palyer.
		/// </summary>
		public Paddle CurrentPaddle { get => currentPaddle; set => currentPaddle = value; }


		public override void OnStartLocalPlayer()
		{
			clientGameController.GetBrickData();

			// Create new player controls.
			playerControls = new PlayerControls();
			playerControls.Enable();
			// Register to input events.
			playerControls.Player.Move.performed += OnMoveActionPerformed;
			playerControls.Player.Move.canceled += OnMoveActionCancelled;
			playerControls.Player.Launch.performed += OnLaunchActionPerformed;
			playerControls.Player.MuteMusic.performed += OnMuteActionPerformed;
			playerControls.Player.Quit.performed += OnQuitActionPerformed;
			playerControls.Player.ReloadLevel.performed += OnReloadLevelActionPerformed;

			PaddleManager.OnAdded += OnPaddleAdded;
		}


		private void OnDisable()
		{
			if (playerControls != null)
			{
				// Disable player controls.
				playerControls.Disable();
				// Unregister input events.
				playerControls.Player.Move.performed -= OnMoveActionPerformed;
				playerControls.Player.Move.canceled -= OnMoveActionCancelled;
				playerControls.Player.Launch.performed -= OnLaunchActionPerformed;
				playerControls.Player.MuteMusic.performed -= OnMuteActionPerformed;
				playerControls.Player.Quit.performed -= OnQuitActionPerformed;
				playerControls.Player.ReloadLevel.performed -= OnReloadLevelActionPerformed;
			}

			PaddleManager.OnAdded -= OnPaddleAdded;
		}

		private void OnMoveActionPerformed(InputAction.CallbackContext obj)
		{
			// Set the move direction on the paddle so not to spam the server.
			SetPaddleDirection(obj.ReadValue<float>());
		}

		private void OnMoveActionCancelled(InputAction.CallbackContext obj)
		{
			// Cancel move.
			SetPaddleDirection(0);
		}

		private void OnLaunchActionPerformed(InputAction.CallbackContext obj)
		{
			ReleaseBall();
		}

		private void OnMuteActionPerformed(InputAction.CallbackContext obj)
		{
			// Get the audio controller and toggle the mute of background music.
			GameObject controller = GameObject.FindGameObjectWithTag(UnityConstants.Tags.GameController);
			AudioController audioController = controller.GetComponent<AudioController>();
			audioController.ToggleMusic();
		}

		private void OnQuitActionPerformed(InputAction.CallbackContext obj)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		private void OnReloadLevelActionPerformed(InputAction.CallbackContext obj)
		{
			// Reset the game to starting state.
			clientGameController.TryResetLevel();
		}

		private void OnPaddleAdded(Paddle paddle)
		{
			if (CurrentPaddle == null)
			{
				GetPaddle();
			}
		}

		[Command]
		private void GetPaddle()
		{
			if (CurrentPaddle != null)
			{
				RpcOnGetPaddle(CurrentPaddle.netId);
			}
		}

		[TargetRpc]
		private void RpcOnGetPaddle(uint netId)
		{
			CurrentPaddle = PaddleManager.Instance.FindPaddle(netId);
		}

		/// <summary>
		/// Move the paddle left.
		/// </summary>
		[Client]
		private void SetPaddleDirection(float direction)
		{
			CurrentPaddle.DesiredDirection = direction;
			OnSetPaddleDirection(direction);
		}

		[Command]
		private void OnSetPaddleDirection(float direction)
		{
			CurrentPaddle.DesiredDirection = direction;
		}

		/// <summary>
		/// Release the ball being held by the paddle.
		/// </summary>
		[Command]
		private void ReleaseBall()
		{
			if (CurrentPaddle.HasBall)
			{
				uint ballNetId = CurrentPaddle.AssignedBall.netId;
				Vector3 velocity = CurrentPaddle.ReleaseBall();
				RpcOnReleaseBall(ballNetId, velocity);
			}
		}

		/// <summary>
		/// Sets the velocity of the released ball on the client.
		/// </summary>
		/// <param name="netId"></param>
		/// <param name="velocity"></param>
		[ClientRpc]
		private void RpcOnReleaseBall(uint netId, Vector3 velocity)
		{
			BallManager.Instance.FindBall(netId).Launch(velocity);
		}
	}
}
