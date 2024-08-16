using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Network.V2 {
    public class PlayerMovement : MonoBehaviour {
        [SerializeField] private CharacterController _controller;
        [SerializeField] private float _speed;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private float _jumpHeight;

        public void MovePlayer(Vector2 moveInput, float tickRate) {
            Vector3 movements = new Vector3(moveInput.x, 0f, moveInput.y);
            if (this._controller.isGrounded == false) {
                movements.y = -9.81f * tickRate;
            }
            this._controller.Move(movements * this._speed * tickRate);
        }

        public void TeleportPlayer(Vector3 position) {
            this._controller.enabled = false;
            this.transform.position = position;
            this._controller.enabled = true;
        }

        public void RotatePlayer(Vector2 lookInput, float tickRate) {
            //ROTATE CAM
            this.transform.RotateAround(this.transform.position, this.transform.up, lookInput.x * this._turnSpeed * tickRate);
        }

        public void JumpPlayer(bool jumpInput) {
            if (jumpInput == false) {
                return;
            }
            Debug.Log("JUMP");
            this._controller.Move(Vector3.up * Mathf.Sqrt(2 * 9.81f * this._jumpHeight));
        }

        public void SimulatePlayer(Vector3 position) {
            this.transform.position = position;
        }
    }
}