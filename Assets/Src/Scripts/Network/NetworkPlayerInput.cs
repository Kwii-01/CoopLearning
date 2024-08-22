using System.Collections;
using System.Collections.Generic;

using Chara;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Network {
    public class NetworkPlayerInput : NetworkBehaviour {
        [SerializeField] private PlayerInput _input;
        [SerializeField] private Character _character;
        [SerializeField] private NetworkPlayerMovementSync _movementSync;
        private InputAction _moveAction;

        public Vector2 moveInput => this._moveAction.ReadValue<Vector2>();


        private void Awake() {
            this.enabled = false;
            InputActionMap actionMap = this._input.actions.FindActionMap("Player");
            this._moveAction = actionMap.FindAction("Move");
            // TEMP => for testing purpose
            this._input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (this.IsLocalPlayer == false) {
                return;
            }
            this.enabled = true;
            this._input.ActivateInput();
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            this._input.DeactivateInput();
        }
    }
}