using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chara {
    public class PlayerMovement : MonoBehaviour, IMovement {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private CharacterController _controller;

        private Vector3 _lookDirection;
        private float _deltaTime;

        public float Speed {
            get => this._moveSpeed;
            set => this._moveSpeed = value;
        }

        private void Update() {
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(this._lookDirection), this._turnSpeed * this._deltaTime);
        }

        public void SetDeltaTime(float deltaTime) {

        }

        public void Move(Vector3 direction) {
            if (direction == Vector3.zero) {
                this.Stop();
                return;
            }
            this._controller.Move(direction * this._moveSpeed * this._deltaTime);
        }

        public void Rotate(Vector3 direction) {
            if (direction == Vector3.zero) {
                return;
            }
            this._lookDirection = direction;
        }

        public void Stop() {
        }

        public void Teleport(Vector3 position) {
            this._controller.enabled = false;
            this.transform.position = position;
            this._controller.enabled = true;
        }
    }
}