using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;

public sealed class NetworkClient : MonoBehaviour
{
    [SerializeField] private Transform _networkTransform;

    private Socket _mainSocket;
    private IPEndPoint _serverRemoteEndpoint = new IPEndPoint(IPAddress.Parse("192.168.0.15"), 1201);
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private async UniTaskVoid Start()
    {
        var connectMessage = new EventMessage(nameof(NetworkEvent.OnClientConnected));

        _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        var result = await _mainSocket.SendToAsync(
            MemoryPackSerializer.Serialize(connectMessage),
            SocketFlags.None,
            _serverRemoteEndpoint);

        // SendPositionAsync().Forget();
    }

    private async UniTaskVoid SendPositionAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            PositionMessage position = new PositionMessage(
                _networkTransform.position.x,
                _networkTransform.position.y,
                _networkTransform.position.z);

            _mainSocket.SendToAsync(MemoryPackSerializer.Serialize(position), SocketFlags.None, _serverRemoteEndpoint)
                .AsUniTask()
                .Forget();

            await UniTask.Delay(1 / 30, false, PlayerLoopTiming.Update, _cts.Token);
        }
    }

    private void OnApplicationQuit()
    {
        var disconnectMessage = new EventMessage(
            nameof(NetworkEvent.OnClientDisconnected),
            new List<byte[]>() { MemoryPackSerializer.Serialize("Application has been closed") });

        _mainSocket.SendTo(
            MemoryPackSerializer.Serialize(disconnectMessage),
            SocketFlags.None,
            _serverRemoteEndpoint);

        _cts?.Cancel();
        _cts = null;

        _mainSocket?.Close();
        _mainSocket?.Dispose();
        _mainSocket = null;
    }
}
