using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Network.V2 {
    public class PlayerMovement : MonoBehaviour {
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _speed;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private float _jumpHeight;

        private Vector2 _lastInput;

        public void MovePlayer(Vector2 moveInput, float tickRate) {
            this._lastInput = moveInput;
            Vector3 movements = new Vector3(moveInput.x, 0f, moveInput.y);
            if (this._controller.isGrounded == false) {
                movements.y = -9.81f * tickRate;
            }
            this._animator.SetBool("Moving", moveInput != Vector2.zero);
            this._controller.Move(movements * this._speed * tickRate);
        }

        public void UpdateServer(float tickRate) {
            this._animator.SetBool("Moving", _lastInput != Vector2.zero);
            this.RotatePlayerTowardMove(_lastInput, tickRate);
        }

        // private void Update() {
        //     this.RotatePlayerTowardMove(_lastInput, this._tickRate);
        // }

        public void TeleportPlayer(Vector3 position) {
            this._controller.enabled = false;
            this.transform.position = position;
            this._controller.enabled = true;
        }

        public void RotatePlayerTowardMove(Vector2 moveInput, float tickRate) {
            if (moveInput == Vector2.zero) {
                return;
            }
            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(direction), this._turnSpeed * tickRate);
        }

        public void RotatePlayer(Vector2 lookInput, float tickRate) {
            //ROTATE CAM
            this.transform.RotateAround(this.transform.position, this.transform.up, lookInput.x * this._turnSpeed * tickRate);
        }

        public void JumpPlayer(bool jumpInput) {
            if (jumpInput == false) {
                return;
            }
            this._controller.Move(Vector3.up * Mathf.Sqrt(2 * 9.81f * this._jumpHeight));
        }

        public void SimulatePlayer(Vector3 position) {
            this.transform.position = position;
        }
    }
}