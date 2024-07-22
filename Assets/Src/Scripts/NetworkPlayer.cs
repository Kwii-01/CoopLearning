using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour {
    [SerializeField] private PlayerController _controller;


    public override void OnNetworkSpawn() {
        if (this.IsLocalPlayer == false && this.IsHost == false) {
            this._controller.Simulated();
        } else {
            this._controller.Played(this.IsLocalPlayer);
        }
    }
}
