using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chara {
    public class PlayerMovement : MonoBehaviour, IMovement {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private CharacterController _controller;

        private Vector3 _lookDirection;

        public float Speed {
            get => this._moveSpeed;
            set => this._moveSpeed = value;
        }

        private void OnEnable() {
            this._controller.enabled = true;
        }

        private void OnDisable() {
            this._controller.enabled = false;
        }

        private void LookTowardDirection(Vector3 direction, float deltaTime) {
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(direction), this._turnSpeed * deltaTime);
        }

        public void Move(Vector3 direction, float deltaTime) {
            if (direction == Vector3.zero) {
                this.Stop();
                return;
            }
            this._controller.Move(direction * this._moveSpeed * deltaTime);
        }

        public void Rotate(Vector3 direction, float deltaTime) {
            if (direction == Vector3.zero) {
                return;
            }
            this._lookDirection = direction;
            this.LookTowardDirection(this._lookDirection, deltaTime);
        }

        public void Stop() {
        }

        public void Teleport(Vector3 position) {
            this._controller.enabled = false;
            this.transform.position = position;
            this._controller.enabled = true;
        }

        public void OnUpdate(float deltaTime) {
            if (this._controller.isGrounded == false) {
                this._controller.Move(Vector3.up * -9.81f * Time.deltaTime);
            }
            if (this._lookDirection == Vector3.zero) {
                return;
            }
            this.LookTowardDirection(this._lookDirection, deltaTime);
        }
    }
}