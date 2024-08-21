using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Network.V2 {
    public class Movement : MonoBehaviour {
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _speed;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private float _jumpHeight;

        private Vector2 _lastInput;

        private void Gravity() {
            if (this._controller.isGrounded) {
                return;
            }
            this._controller.Move(Vector3.up * -9.81f * Time.deltaTime);
        }

        private void RotatePlayerTowardMove(Vector2 moveInput, float tickRate) {
            if (moveInput == Vector2.zero) {
                return;
            }
            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(direction), this._turnSpeed * tickRate);
        }

        private void Update() {
            this.Gravity();
        }


        public void MovePlayer(Vector2 moveInput, float tickRate) {
            this._lastInput = moveInput;
            Vector3 movements = new Vector3(moveInput.x, 0f, moveInput.y);
            this._animator.SetBool("Moving", moveInput != Vector2.zero);
            this._controller.Move(movements * this._speed * tickRate);
        }

        public void UpdateOnServerTick(float tickRate) {
            this._animator.SetBool("Moving", _lastInput != Vector2.zero);
            this.RotatePlayerTowardMove(_lastInput, tickRate);
        }

        public void TeleportPlayer(Vector3 position) {
            this._controller.enabled = false;
            this.transform.position = position;
            this._controller.enabled = true;
        }

        public void SetRotation(float yRotation) {
            this.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        public void SimulatePlayer(TransformState transformState) {
            Vector3 direction = (transformState.Position - this.transform.position).normalized;
            _lastInput = new Vector2(direction.x, direction.z);
            this._animator.SetBool("Moving", transformState.IsMoving);
            this.transform.position = transformState.Position;
        }
    }
}