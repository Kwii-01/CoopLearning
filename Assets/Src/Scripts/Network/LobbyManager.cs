using System;
using System.Collections;
using System.Collections.Generic;

using Netcode.Transports.Facepunch;

using Steamworks;
using Steamworks.Data;

using Unity.Netcode;

using UnityEngine.SceneManagement;

using UnityEngine;
using UnityEngine.Events;

namespace Network {
    public class LobbyManager : MonoBehaviour {
        [SerializeField] private FacepunchTransport _facepunchTransport;
        public Lobby? Lobby { get; private set; } = default;
        public UnityEvent<Lobby> onLobbyEntered;
        public UnityEvent onLobbyLeft;

        private void Awake() {
            SteamMatchmaking.OnLobbyCreated += this.OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += this.OnLobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested += this.OnGameLobbyJoinRequested;
        }

        private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId id) {
            await lobby.Join();
        }

        private void OnLobbyEntered(Lobby lobby) {
            this.Lobby = lobby;
            this.onLobbyEntered.Invoke(this.Lobby.Value);
            if (NetworkManager.Singleton.IsHost) {
                return;
            }
            this._facepunchTransport.targetSteamId = lobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
        }

        private void OnLobbyCreated(Result result, Lobby lobby) {
            if (result != Result.OK) {
                return;
            }
            this.Lobby = lobby;
            lobby.SetPublic();
            lobby.SetJoinable(true);
            NetworkManager.Singleton.StartHost();
        }

        public async void HostLobby(int lobbySize) {
            await SteamMatchmaking.CreateLobbyAsync(lobbySize);
        }

        public async void JoinLobby(ulong lobbyID) {
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
            int lobbyIndex = Array.FindIndex(lobbies, lobby => lobby.Id == lobbyID);
            if (lobbyIndex < 0) {
                //LOBBY NOT FOUND
                return;
            }
            await lobbies[lobbyIndex].Join();
        }

        public void LeaveLobby() {
            NetworkManager.Singleton.Shutdown();
            this.Lobby?.Leave();
            this.Lobby = null;
            this.onLobbyLeft.Invoke();
        }

        public void StartGame(string scene, LoadSceneMode loadSceneMode) {
            if (NetworkManager.Singleton.IsHost == false) {
                return;
            }
            NetworkManager.Singleton.SceneManager.LoadScene(scene, loadSceneMode);
        }
    }
}