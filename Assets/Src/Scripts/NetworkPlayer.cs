using System.Collections;
using System.Collections.Generic;

using Cinemachine;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour {
    [SerializeField] private PlayerController _controller;
    [SerializeField] private CinemachineVirtualCamera _vcam;


    public override void OnNetworkSpawn() {
        if (this.IsLocalPlayer == false) {
            this._vcam.Priority = -10;
        }
        if (this.IsLocalPlayer == false && this.IsHost == false) {
            this._controller.Simulated();
        } else {
            this._controller.Played(this.IsLocalPlayer);
        }
    }
}
