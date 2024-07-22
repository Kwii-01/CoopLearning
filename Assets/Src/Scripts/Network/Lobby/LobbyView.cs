using System;
using System.Collections;
using System.Collections.Generic;

using Steamworks.Data;

using TMPro;

using Tools;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;

namespace Network.UI {
    public class LobbyView : MonoBehaviour {
        [SerializeField] private TMP_InputField _lobbyID;

        [Header("Outside Lobby")]
        [SerializeField] private GameObject _outsideLobbyView;
        [SerializeField] private Button _hostGame;
        [SerializeField] private Button _joinGame;

        [Header("Inside Lobby")]
        [SerializeField] private GameObject _insideLobbyView;
        [SerializeField] private Button _leaveLobby;
        [SerializeField] private Button _ready;
        [SerializeField] private Button _startGame;

        private void Awake() {
            this._hostGame.onClick.AddListener(this.HostGame);
            this._joinGame.onClick.AddListener(this.JoinGame);
            this._leaveLobby.onClick.AddListener(this.LeaveLobby);
            this._ready.onClick.AddListener(this.Ready);
            this._startGame.onClick.AddListener(this.StartGame);
        }

        private void Start() {
            LobbyManager lobbyManager = Singleton.Get<LobbyManager>();
            lobbyManager.onLobbyEntered.AddListener(this.OnLobbyJoined);
            lobbyManager.onLobbyLeft.AddListener(this.OnLobbyLeft);
            this.UpdateLobbyState(false);
        }

        private void UpdateLobbyState(bool isInLobby) {
            this._outsideLobbyView.SetActive(isInLobby == false);
            this._insideLobbyView.SetActive(isInLobby);
            if (isInLobby) {
                this._startGame.gameObject.SetActive(NetworkManager.Singleton.IsHost);
            }
        }


        private void OnLobbyJoined(Lobby lobby) {
            this._lobbyID.text = lobby.Id.ToString();
            this.UpdateLobbyState(true);
        }

        private void OnLobbyLeft() {
            this._lobbyID.text = "";
            this.UpdateLobbyState(false);
        }

        private void HostGame() {
            Singleton.Get<LobbyManager>().HostLobby(4);
        }

        private void LeaveLobby() {
            Singleton.Get<LobbyManager>().LeaveLobby();
        }

        private void Ready() {

        }

        private void StartGame() {
            Singleton.Get<LobbyManager>().StartGame("Game", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }

        private void JoinGame() {
            if (ulong.TryParse(this._lobbyID.text, out ulong lobbyID)) {
                Singleton.Get<LobbyManager>().JoinLobby(lobbyID);
            }
        }
    }
}