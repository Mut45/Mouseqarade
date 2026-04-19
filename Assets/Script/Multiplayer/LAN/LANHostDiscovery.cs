using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class LANHostDiscovery : MonoBehaviour
{
    [SerializeField] private int broadcastPort = 8888;
    

    private UdpClient udpClient;
    private IPEndPoint endpoint;
    private bool isBroadcasting;

    private string currentRoomName = "Mousequrade Room";
    private ushort currentGameConnectionPort = 7777;
    private int currentPlayerCount = 1;
    private int maxPlayerCount = 2;

    private void OnDisable()
    {
        StopBroadcast();
    }

    private void OnDestroy()
    {
        StopBroadcast();
    }

    public void SetRoomMetaData(int playerCount, int maxCount)
    {
        currentPlayerCount = playerCount;
        maxPlayerCount = maxCount;
    }

    public void InitBroadcast(string roomName, ushort connectionPort, int playerCount, int maxCount)
    {
        if (isBroadcasting) return;

        currentRoomName = String.IsNullOrWhiteSpace(roomName) ? "Untitled Game Room" : roomName.Trim();
        currentGameConnectionPort = connectionPort;
        currentPlayerCount = playerCount;
        maxPlayerCount = maxCount;
        
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        endpoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
        isBroadcasting = true;
        InvokeRepeating(nameof(Broadcast), 0.5f, 1f);
    }

    private void StopBroadcast()
    {
        if (!isBroadcasting) return;

        isBroadcasting = false;
        CancelInvoke(nameof(Broadcast));
        udpClient?.Close();
        udpClient = null;
    }

    private void Broadcast()
    {
        if (!isBroadcasting | udpClient == null) return;

        try
        {
            string roomMetaDataPayload = $"{currentRoomName},{currentGameConnectionPort},{currentPlayerCount},{maxPlayerCount} ";
            byte[] data = Encoding.UTF8.GetBytes(roomMetaDataPayload);
            udpClient.Send(data, data.Length, endpoint);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LANHostDiscovery] Broadcast failed: {ex.Message}");
        }
        
    }

}