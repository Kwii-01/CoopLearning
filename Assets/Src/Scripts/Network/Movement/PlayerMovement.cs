using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Netcode;


using UnityEngine;

public class PlayerMovement : NetworkBehaviour {
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float _speed;
    [SerializeField] private float _turnSpeed;
    // [SerializeField] private Transform _vcam;

    private int _tick;
    private float _tickRate = 1f / 60f;
    private float _tickDeltaTime;

    private const int BUFFER_SIZE = 1024;
    private InputState[] _inputs = new InputState[BUFFER_SIZE];
    private TransformState[] _tranformStates = new TransformState[BUFFER_SIZE];
    private NetworkVariable<TransformState> _serverTransformState = new();
    private TransformState _previousTransformState;

    private void MovePlayer(Vector2 moveInput) {
        Vector3 movements = new Vector3(moveInput.x, 0f, moveInput.y);
        // Vector3 movements = moveInput.x * this._vcam.right + moveInput.y * this._vcam.forward;
        //   movements.y = 0;
        if (this._controller.isGrounded == false) {
            movements.y = -9.81f;
        }
        this._controller.Move(movements * this._speed * this._tickRate);
    }

    private void RotatePlayer(Vector2 lookInput) {
        //ROTATE CAM
        this.transform.RotateAround(this.transform.position, this.transform.up, lookInput.x * this._turnSpeed * this._tickRate);
    }

    private void TeleportPlayer(TransformState serverState, int bufferIndex) {
        this._controller.enabled = false;
        this.transform.position = serverState.Position;
        this.transform.rotation = serverState.Rotation;
        this._controller.enabled = true;
        this._tranformStates[bufferIndex] = serverState;
    }

    private void OnServerStateChanged(TransformState previousValue, TransformState serverState) {
        if (this._previousTransformState == null) {
            this._previousTransformState = serverState;
        }
        int bufferIndex = Array.FindIndex(this._tranformStates, state => state != null && state.Tick == serverState.Tick);
        if (bufferIndex == -1) {
            return;
        }
        TransformState calculatedState = this._tranformStates[bufferIndex];
        if (calculatedState.Position != serverState.Position) {
            this.TeleportPlayer(serverState, bufferIndex);

            IEnumerable<InputState> inputs = this._inputs
                .Where(input => input != null && (input.Tick > serverState.Tick || input.Tick <= this._tick % BUFFER_SIZE))
                .OrderBy(input => input.Tick)
                .ToArray();
            foreach (InputState input in inputs) {
                MovePlayer(input.MovementInput);
                RotatePlayer(input.LookInput);
                TransformState transformState = new() {
                    Tick = input.Tick,
                    Position = this.transform.position,
                    Rotation = this.transform.rotation,
                    IsMoving = true
                };
                int index = Array.FindIndex(this._tranformStates, state => state != null && state.Tick == serverState.Tick);
                this._tranformStates[index] = transformState;
            }
        }
    }

    private void MovePlayerOnServer(int tick, Vector2 moveInput, Vector2 lookInput) {
        this.MovePlayer(moveInput);
        this.RotatePlayer(lookInput);

        TransformState state = new() {
            Tick = tick,
            Position = this.transform.position,
            Rotation = this.transform.rotation,
            IsMoving = true
        };
        this._previousTransformState = this._serverTransformState.Value;
        this._serverTransformState.Value = state;
    }

    [ServerRpc]
    private void MovePlayerServerRPC(int tick, Vector2 moveInput, Vector2 lookInput) {
        // if (tick != this._previousTransformState.Tick + 1) {

        // }
        this.MovePlayerOnServer(tick, moveInput, lookInput);
    }

    public override void OnNetworkDespawn() {
        this._serverTransformState.OnValueChanged -= this.OnServerStateChanged;
    }

    public override void OnNetworkSpawn() {
        if (this.IsLocalPlayer == false) {
            return;
        }
        this._serverTransformState.OnValueChanged += this.OnServerStateChanged;
    }

    public void LocalMovement(Vector2 moveInput, Vector2 lookInput) {
        this._tickDeltaTime += Time.deltaTime;
        if (this._tickDeltaTime < this._tickRate) {
            return;
        }
        int bufferIndex = this._tick % BUFFER_SIZE;
        if (this.IsServer == false) {
            this.MovePlayerServerRPC(this._tick, moveInput, lookInput);
            this.MovePlayer(moveInput);
            this.RotatePlayer(lookInput);
        } else {
            this.MovePlayerOnServer(this._tick, moveInput, lookInput);
        }

        InputState inputState = new() {
            Tick = this._tick,
            MovementInput = moveInput,
            LookInput = lookInput,
        };

        TransformState transformState = new() {
            Tick = this._tick,
            Position = this.transform.position,
            Rotation = this.transform.rotation,
            IsMoving = true
        };

        this._inputs[bufferIndex] = inputState;
        this._tranformStates[bufferIndex] = transformState;
        this._tickDeltaTime -= this._tickRate;
        this._tick++;
        if (this._tick >= BUFFER_SIZE) {
            this._tick = 0;
        }
    }

    public void SimulateMovement() {
        this._tickDeltaTime += Time.deltaTime;
        if (this._tickDeltaTime < this._tickRate) {
            return;
        }
        if (this._serverTransformState.Value.IsMoving) {
            this.transform.position = this._serverTransformState.Value.Position;
            this.transform.rotation = this._serverTransformState.Value.Rotation;
        }
        this._tickDeltaTime -= this._tickRate;
        this._tick++;
        if (this._tick >= BUFFER_SIZE) {
            this._tick = 0;
        }
    }
}
