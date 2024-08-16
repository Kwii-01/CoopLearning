using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Network.V2 {
    public class NetworkPlayer : NetworkBehaviour {
        [SerializeField] private NetworkPlayerInput _playerInput;
        [SerializeField] private PlayerMovement _playerMovement;

        private Tick _tick;
        private CircularBuffer<InputState> _inputs;
        private CircularBuffer<TransformState> _transforms;

        private NetworkVariable<TransformState> _serverTransformState = new();
        private const int BUFFER_SIZE = 1024;

        private void Awake() {
            this._tick = new Tick(60, BUFFER_SIZE);
            this._inputs = new CircularBuffer<InputState>(BUFFER_SIZE);
            this._transforms = new CircularBuffer<TransformState>(BUFFER_SIZE);
        }


        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            this._playerInput.enabled = this.IsLocalPlayer;
            if (this.IsLocalPlayer == false || this.IsHost) {
                return;
            }
            this._serverTransformState.OnValueChanged += this.OnServerStateChanged;
        }


        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            this._serverTransformState.OnValueChanged -= this.OnServerStateChanged;
        }

        // RECONCILIATE HERE
        private void OnServerStateChanged(TransformState previousValue, TransformState newValue) {
            int bufferIndex = newValue.Tick;
            if (bufferIndex == -1) {
                return;
            }
            TransformState calculatedState = this._transforms[bufferIndex];

            if (calculatedState.Position != newValue.Position) {

                this._playerMovement.TeleportPlayer(newValue.Position);
                this._transforms[bufferIndex] = newValue;

                Vector2 moveInput;
                int diff = bufferIndex > this._tick ? BUFFER_SIZE - bufferIndex + this._tick : this._tick - bufferIndex;
                int total = bufferIndex + diff;
                for (int i = bufferIndex; i < total; i++) {
                    moveInput = new Vector2(this._inputs[i].MoveX.DequantizeShort(-1f, 1f), this._inputs[i].MoveY.DequantizeShort(-1f, 1f));
                    this._playerMovement.MovePlayer(moveInput, this._tick.Delta);
                    TransformState transformState = new() {
                        Tick = this._inputs[i].Tick,
                        Position = this.transform.position
                    };
                    this._transforms[this._inputs[i].Tick] = transformState;
                }
            }
        }

        private void MovePlayerOnServer(int tick, Vector2 moveInput) {
            this._playerMovement.MovePlayer(moveInput, this._tick.Delta);

            TransformState transformState = new TransformState {
                Tick = tick,
                Position = this.transform.position
            };
            this._serverTransformState.Value = transformState;
        }

        [ServerRpc]
        private void MovePlayerServerRpc(InputState inputState) {
            Vector2 moveInput = new Vector2(inputState.MoveX.DequantizeShort(-1f, 1f), inputState.MoveY.DequantizeShort(-1f, 1f));
            this.MovePlayerOnServer(inputState.Tick, moveInput);
        }

        private void UpdateLocalPlayer() {
            int bufferIndex = this._tick;
            Vector2 moveInput = this._playerInput.MoveInput;

            InputState input = new InputState {
                Tick = this._tick,
                MoveX = moveInput.x.QuantizeFloat(-1f, 1f),
                MoveY = moveInput.y.QuantizeFloat(-1f, 1f)
            };

            if (this.IsServer == false) {
                this.MovePlayerServerRpc(input);
                this._playerMovement.MovePlayer(moveInput, this._tick.Delta);
            } else {
                this.MovePlayerOnServer(this._tick, moveInput);
            }

            TransformState transformState = new TransformState {
                Tick = this._tick,
                Position = this.transform.position
            };

            this._inputs[bufferIndex] = input;
            this._transforms[bufferIndex] = transformState;
        }

        private void SimulatePlayer() {
            this._playerMovement.SimulatePlayer(this._serverTransformState.Value.Position);
        }

        private void Update() {
            this._tick.Update(Time.deltaTime);
            while (this._tick.CanTick()) {
                if (this.IsLocalPlayer) {
                    this.UpdateLocalPlayer();
                } else if (this.IsHost == false) {
                    this.SimulatePlayer();
                }
            }
        }
    }
}