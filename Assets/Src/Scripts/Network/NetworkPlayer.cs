using System.Collections;
using System.Collections.Generic;

using Cinemachine;

using Unity.Netcode;

using UnityEngine;

namespace Network {
    public class NetworkPlayer : NetworkBehaviour {
        [SerializeField] private CinemachineVirtualCamera _camPrefab;
        private CinemachineVirtualCamera _cam;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (this.IsLocalPlayer == false) {
                return;
            }
            this._cam = Instantiate(this._camPrefab);
            this._cam.m_Follow = this.transform;
            this._cam.m_LookAt = this.transform;
        }


        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            Destroy(this._cam.gameObject);
        }
    }
}