using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LANClientListener : MonoBehaviour
{
    [Header("Discovery Settings")]
    [SerializeField] private int discoveryPort = 8888;
    [SerializeField] private float staleTimeoutSeconds = 2f;

    private UdpClient udpClient;
    private bool isListening;

    private readonly ConcurrentQueue<GameRoomMetaData> pendingRooms = new();
    private readonly Dictionary<string, GameRoomMetaData> knownRooms = new(); // current rooms that are not stale, with the IP:port as the key and RoomMetaData as the value, dictionary makes sure the ip:port is unique

    public event Action<GameRoomMetaData> OnRoomDiscoveredOrUpdated;

    public void BeginListening()
    {
        if (isListening) return;

        try
        {
            udpClient = new UdpClient(discoveryPort);
            udpClient.EnableBroadcast = true;
            isListening = true;

            udpClient.BeginReceive(OnReceiveBroadcast, null);

            Debug.Log($"[LANClientListener] Listening on UDP discovery port {discoveryPort}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LANClientListener] BeginListening failed: {ex.Message}");
            StopListening();
        }
    }

    public void StopListening()
    {
        isListening = false;

        try
        {
            udpClient?.Close();
        }
        catch
        {
        }

        udpClient = null;
        knownRooms.Clear();

        while (pendingRooms.TryDequeue(out _)) { }

        Debug.Log("[LANClientListener] Stopped listening.");
    }

    private void OnReceiveBroadcast(IAsyncResult result)
    {
        if (!isListening || udpClient == null) return;

        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, discoveryPort);

        try
        {
            byte[] data = udpClient.EndReceive(result, ref remoteEndPoint);
            string payload = Encoding.UTF8.GetString(data);
            string[] parts = payload.Split(',');

            // fall back for missing parts in the broadcasted meta data payload
            string roomName = parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : "Untitled Game Room";
            ushort gamePort = 7777;
            if (parts.Length > 1) ushort.TryParse(parts[1], out gamePort);
            if (gamePort == 0) gamePort = 7777;
            int playerCount = 1;
            if (parts.Length > 2) int.TryParse(parts[2], out playerCount);
            int maxPlayers = 2;
            if (parts.Length > 3) int.TryParse(parts[3], out maxPlayers);
            

            GameRoomMetaData room = new GameRoomMetaData
            {
                RoomName = roomName,
                HostIpAddress = remoteEndPoint.Address.ToString(),
                GamePort = gamePort,
                PlayerCount = playerCount,
                MaxPlayers = maxPlayers,
                LastSeenRealtime = Time.realtimeSinceStartup
            };

            pendingRooms.Enqueue(room);
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LANClientListener] Receive failed: {ex.Message}");
        }
        finally
        {
            if (isListening && udpClient != null)
            {
                try
                {
                    udpClient.BeginReceive(OnReceiveBroadcast, null);
                }
                catch
                {
                }
            }
        }
    }

    public void PumpPendingRooms()
    {
        while (pendingRooms.TryDequeue(out GameRoomMetaData room))
        {
            string key = $"{room.HostIpAddress}:{room.GamePort}";
            knownRooms[key] = room;
            OnRoomDiscoveredOrUpdated?.Invoke(room);
        }

        RemoveStaleRooms();
    }

    public List<GameRoomMetaData> GetCurrentRooms()
    {
        RemoveStaleRooms();
        return new List<GameRoomMetaData>(knownRooms.Values);
    }
 
    private void RemoveStaleRooms()
    {
        float now = Time.realtimeSinceStartup;
        List<string> keysToRemove = null;
        
        foreach (var kvp in knownRooms)
        {
            if (now - kvp.Value.LastSeenRealtime > staleTimeoutSeconds)
            {
                if (keysToRemove == null ) keysToRemove = new List<string>();
                keysToRemove.Add(kvp.Key);
            }
        }

        if (keysToRemove == null) return;

        foreach (string key in keysToRemove)
        {
            knownRooms.Remove(key);
        }
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void OnDestroy()
    {
        StopListening();
    }
}

[Serializable]
public struct GameRoomMetaData
{
    public string RoomName;
    public string HostIpAddress;
    public ushort GamePort;
    public int PlayerCount;
    public int MaxPlayers;
    public float LastSeenRealtime;
}