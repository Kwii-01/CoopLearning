using System.Collections;
using System.Collections.Generic;

using Chara;

using Tools;

using Unity.Netcode;
using Unity.VisualScripting;

using UnityEngine;

namespace Network {
    /// <summary>
    /// Manage network movement for the player
    /// </summary>
    [RequireComponent(typeof(NetworkPlayerInput))]
    public class NetworkPlayerMovementSync : NetworkBehaviour {

        [SerializeField] private Animator _animator;

        /// <summary>
        ///  MOVEMENT GESTION
        /// </summary>
        private IMovement _playerMovement;
        private NetworkPlayerInput _playerInput;
        private Tick _tick;
        private CircularBuffer<InputState> _inputs;
        private CircularBuffer<TransformState> _transforms;
        private NetworkVariable<TransformState> _serverTransformState = new();
        private const int BUFFER_SIZE = 1024;

        private Vector3 _moveDirection;

        private void Awake() {
            this._playerMovement = this.GetComponent<IMovement>();
            this._playerInput = this.GetComponent<NetworkPlayerInput>();
        }

        private TransformState CurrentState(int tick, Vector2 moveInput) {
            return new() {
                Tick = tick,
                Position = this.transform.position,
                Rotation = this.transform.eulerAngles.y,
                IsMoving = moveInput != Vector2.zero
            };
        }

        private void Reconciliate(int tickIndex, TransformState serverState) {
            this._playerMovement.Teleport(serverState.Position);
            this._transforms[tickIndex] = serverState;

            Vector2 moveInput;
            int inputsToReplay = tickIndex > this._tick ? BUFFER_SIZE - tickIndex + this._tick : this._tick - tickIndex;
            int total = tickIndex + inputsToReplay;
            int currentTick;
            for (int i = tickIndex; i < total; i++) {
                currentTick = this._inputs[i].Tick;
                moveInput = new Vector2(this._inputs[i].MoveX.DequantizeShort(-1f, 1f), this._inputs[i].MoveY.DequantizeShort(-1f, 1f));
                moveInput.x = Mathf.Abs(moveInput.x) < .001f ? 0f : moveInput.x;
                moveInput.y = Mathf.Abs(moveInput.y) < .001f ? 0f : moveInput.y;
                this._playerMovement.Move(moveInput, this._tick.Delta);
                this.transform.rotation = Quaternion.Euler(0f, this._transforms[i].Rotation, 0f);
                this._transforms[currentTick] = this.CurrentState(currentTick, moveInput);
            }
        }

        private void OnServerStateChanged(TransformState previousValue, TransformState newValue) {
            int bufferIndex = newValue.Tick;
            if (bufferIndex == -1) {
                return;
            }
            TransformState calculatedState = this._transforms[bufferIndex];
            if (calculatedState.Position != newValue.Position) {
                this.Reconciliate(bufferIndex, newValue);
            }
        }

        private void MovePlayerOnServer(int tick, Vector2 moveInput) {
            this._moveDirection.x = moveInput.x;
            this._moveDirection.z = moveInput.y;
            this._playerMovement.Move(this._moveDirection, this._tick.Delta);
            this._playerMovement.Rotate(this._moveDirection, this._tick.Delta);
            this._serverTransformState.Value = this.CurrentState(tick, moveInput);
            this._animator.SetBool("Moving", this._serverTransformState.Value.IsMoving);
        }

        [ServerRpc]
        private void MovePlayerServerRpc(InputState inputState) {
            Vector2 moveInput = new Vector2(inputState.MoveX.DequantizeShort(-1f, 1f), inputState.MoveY.DequantizeShort(-1f, 1f));
            moveInput.x = Mathf.Abs(moveInput.x) < .001f ? 0f : moveInput.x;
            moveInput.y = Mathf.Abs(moveInput.y) < .001f ? 0f : moveInput.y;
            this.MovePlayerOnServer(inputState.Tick, moveInput);
        }

        private void UpdateClientLocalPlayer() {
            int bufferIndex = this._tick;
            Vector2 moveInput = this._playerInput.moveInput;

            InputState input = new InputState {
                Tick = this._tick,
                MoveX = moveInput.x.QuantizeFloat(-1f, 1f),
                MoveY = moveInput.y.QuantizeFloat(-1f, 1f)
            };

            this.MovePlayerServerRpc(input);

            this._moveDirection.x = moveInput.x;
            this._moveDirection.z = moveInput.y;

            this._playerMovement.Move(this._moveDirection, this._tick.Delta);
            this._playerMovement.Rotate(this._moveDirection, this._tick.Delta);

            this._inputs[bufferIndex] = input;
            this._transforms[bufferIndex] = this.CurrentState(this._tick, moveInput);
            this._animator.SetBool("Moving", this._transforms[bufferIndex].IsMoving);
        }

        private void SyncGhostMovement() {
            this._animator.SetBool("Moving", this._serverTransformState.Value.IsMoving);
            this.transform.position = this._serverTransformState.Value.Position;
            this.transform.rotation = Quaternion.Euler(0f, this._serverTransformState.Value.Rotation, 0f);
        }

        private void Update() {
            this._tick.Update(Time.deltaTime);
            while (this._tick.CanTick()) {
                if (this.IsHost && this.IsLocalPlayer) {
                    this.MovePlayerOnServer(this._tick, this._playerInput.moveInput);
                    this._playerMovement.OnUpdate(this._tick.Delta);
                } else if (this.IsLocalPlayer) {
                    this.UpdateClientLocalPlayer();
                    this._playerMovement.OnUpdate(this._tick.Delta);
                } else if (this.IsHost == false) {
                    this.SyncGhostMovement();
                }
            }
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            this._tick = new Tick(60, BUFFER_SIZE);

            if (this.IsHost) {
                return;
            }

            if (this.IsLocalPlayer == false) {
                (this._playerMovement as MonoBehaviour).enabled = false;
                return;
            }

            this._inputs = new CircularBuffer<InputState>(BUFFER_SIZE);
            this._transforms = new CircularBuffer<TransformState>(BUFFER_SIZE);

            this._serverTransformState.OnValueChanged += this.OnServerStateChanged;
        }


        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            this._serverTransformState.OnValueChanged -= this.OnServerStateChanged;
        }
    }
}