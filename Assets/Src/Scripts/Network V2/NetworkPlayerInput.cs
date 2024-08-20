using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Network.V2 {
    public class NetworkPlayerInput : MonoBehaviour {
        [SerializeField] private PlayerInput _input;

        private InputAction _moveAction;
        private InputAction _lookAtAction;

        public Vector2 MoveInput => this._moveAction.ReadValue<Vector2>();
        public Vector2 LookInput => this._lookAtAction.ReadValue<Vector2>();

        private void Awake() {
            InputActionMap actionMap = this._input.actions.FindActionMap("Player");
            this._moveAction = actionMap.FindAction("Move");
            this._lookAtAction = actionMap.FindAction("Look");
            // TEMP => for testing purpose
            this._input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
        }

        private void OnEnable() {
            this._input.ActivateInput();
        }

        private void OnDisable() {
            this._input.DeactivateInput();
        }
    }
}