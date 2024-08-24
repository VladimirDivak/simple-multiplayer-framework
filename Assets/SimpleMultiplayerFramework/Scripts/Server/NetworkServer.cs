using System;
using MemoryPack;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace DVA.SimpleMultiplayerFramework
{
    public sealed class NetworkServer : MonoBehaviour
    {
        private IPEndPoint _endpoint = new IPEndPoint(IPAddress.Any, 1201);
        private Socket _mainSocket;

        private List<NetworkClientHandler> _clients = new List<NetworkClientHandler>();

        private void Awake()
        {
            _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _mainSocket.Bind(_endpoint);

            ReceiveAsync().Forget();
            Debug.Log($"NetworkServer :: server has been started on {_endpoint.Port} port.");
        }

        private async UniTaskVoid ReceiveAsync()
        {
            EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[1024];

            while (true)
            {
                var result = await _mainSocket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEndpoint);

                try
                {
                    var message = MemoryPackSerializer.Deserialize<INetworkMessage>(buffer);
                    if (message.messageType != MessageType.Event) continue;

                    var networkEvent = message as EventMessage;

                    switch (networkEvent.eventName)
                    {
                        case nameof(NetworkEvent.OnClientConnected):
                            _clients.Add(new NetworkClientHandler(result.RemoteEndPoint, _mainSocket));
                            Debug.Log($"NetworkServer :: client {remoteEndpoint} has been connected.");
                            break;

                        case nameof(NetworkEvent.OnClientDisconnected):
                            var client = _clients.Find(client => client.remoteEndpoint == remoteEndpoint);
                            client.CloseConnection();
                            _clients.Remove(client);
                            Debug.Log($"NetworkServer :: client {remoteEndpoint} has been disconnected. Reason: {MemoryPackSerializer.Deserialize<string>(networkEvent.eventArgs[0])}.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"NetworkServer :: {ex.Message}");
                }
            }
        }

        public class NetworkClientHandler
        {
            public EndPoint remoteEndpoint { get; private set; }
            private CancellationTokenSource _cts = new CancellationTokenSource();

            public NetworkClientHandler(EndPoint remoteEndpoint, Socket serverSocket)
            {
                this.remoteEndpoint = remoteEndpoint;
                ReceiveFromClient(serverSocket, remoteEndpoint).Forget();
            }

            private async UniTaskVoid ReceiveFromClient(Socket socket, EndPoint remoteEndpoint)
            {
                byte[] buffer = new byte[1024];

                while (!_cts.Token.IsCancellationRequested)
                {
                    await socket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEndpoint);

                    try
                    {
                        var message = MemoryPackSerializer.Deserialize<PositionMessage>(buffer);
                    }
                    catch { }
                }
            }

            public void CloseConnection()
            {
                _cts?.Cancel();
                _cts = null;
            }
        }

        public void OnApplicationQuit()
        {
            _mainSocket?.Close();
            _mainSocket?.Dispose();
            _mainSocket = null;

            foreach (var client in _clients)
                client.CloseConnection();
        }
    }
}
