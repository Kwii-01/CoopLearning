using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Cinemachine;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.InputSystem;
using Tools;

namespace Network.V2 {
    public class NetworkPlayer : NetworkBehaviour {
        [SerializeField] private NetworkPlayerInput _playerInput;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private CinemachineVirtualCamera _vCam;

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
            if (this.IsLocalPlayer == false) {
                return;
            }
            CinemachineVirtualCamera cam = Instantiate(this._vCam);
            cam.m_Follow = this.transform;
            cam.m_LookAt = this.transform;
            if (this.IsHost) {
                return;
            }
            this._serverTransformState.OnValueChanged += this.OnServerStateChanged;
        }


        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            this._serverTransformState.OnValueChanged -= this.OnServerStateChanged;
        }

        private TransformState CreateState(int tick, Vector2 moveInput) {
            return new() {
                Tick = tick,
                Position = this.transform.position,
                Rotation = this.transform.rotation.y,
                IsMoving = moveInput != Vector2.zero
            };
        }

        private void Reconciliate(int tickIndex, TransformState serverState) {
            this._playerMovement.TeleportPlayer(serverState.Position);
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
                this._playerMovement.MovePlayer(moveInput, this._tick.Delta);
                this._playerMovement.SetRotation(this._transforms[currentTick].Rotation);
                this._transforms[currentTick] = this.CreateState(currentTick, moveInput);
            }
        }

        // RECONCILIATE HERE
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
            this._playerMovement.MovePlayer(moveInput, this._tick.Delta);
            this._serverTransformState.Value = this.CreateState(tick, moveInput);
        }

        [ServerRpc]
        private void MovePlayerServerRpc(InputState inputState) {
            Vector2 moveInput = new Vector2(inputState.MoveX.DequantizeShort(-1f, 1f), inputState.MoveY.DequantizeShort(-1f, 1f));
            moveInput.x = Mathf.Abs(moveInput.x) < .001f ? 0f : moveInput.x;
            moveInput.y = Mathf.Abs(moveInput.y) < .001f ? 0f : moveInput.y;
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

            this._inputs[bufferIndex] = input;
            this._transforms[bufferIndex] = this.CreateState(this._tick, moveInput);
        }

        private void Update() {
            this._tick.Update(Time.deltaTime);
            while (this._tick.CanTick()) {
                if (this.IsLocalPlayer) {
                    this.UpdateLocalPlayer();
                } else if (this.IsHost == false) {
                    this._playerMovement.SimulatePlayer(this._serverTransformState.Value);
                }
                this._playerMovement.UpdateOnServerTick(this._tick.Delta);
            }
        }
    }
}