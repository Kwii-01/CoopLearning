using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
namespace Network.V1 {
    public class PlayerController : MonoBehaviour {
        private enum Type_e {
            None,
            Played,
            Simulated
        }

        [SerializeField] private PlayerInput _input;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private PlayerMovement _movements;

        private Type_e _type;

        private InputAction _moveAction;
        private InputAction _lookAtAction;

        private void Awake() {
            InputActionMap actionMap = this._input.actions.FindActionMap("Player");
            this._moveAction = actionMap.FindAction("Move");
            this._lookAtAction = actionMap.FindAction("Look");
            actionMap.FindAction("Jump").performed += this.OnJumpPressed;
            _input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);

        }

        private void OnJumpPressed(InputAction.CallbackContext context) {

        }

        private void Update() {
            if (this._type == Type_e.Played) {
                Vector2 moveDirection = this._moveAction.ReadValue<Vector2>();
                Vector2 lookDirection = this._lookAtAction.ReadValue<Vector2>();
                this._movements.LocalMovement(moveDirection, lookDirection);
            } else if (this._type == Type_e.Simulated) {
                this._movements.SimulateMovement();
            }
        }

        public void Simulated() {
            this._type = Type_e.Simulated;
            this._input.DeactivateInput();
            this._controller.enabled = false;
        }

        public void Played(bool IsLocalPlayer) {
            if (IsLocalPlayer) {
                this._type = Type_e.Played;
                this._input.ActivateInput();
            }
        }

    }
}