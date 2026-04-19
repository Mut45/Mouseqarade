using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class LANConnectionMenu : MonoBehaviour
{
    [Header("Network References & Parameters")]
    [SerializeField] private ushort gamePort = 7777;
    [SerializeField] private int maxPlayers = 2;
    [SerializeField] private MatchStartFlow matchStartFlow;

    [Header("Host & Client Discovery")]
    [SerializeField] private LANClientListener clientDiscovery;
    [SerializeField] private LANHostDiscovery hostDiscovery;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject hostPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject waitingPanel;

    [Header("Host UI Elements")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button confirmCreateRoomButton;
    [SerializeField] private Button hostBackButton;

    [Header("Client UI Elements")]
    [SerializeField] private Button refreshRoomsButton;
    [SerializeField] private Button joinBackButton;
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomEntryButtonPrefab;
    [SerializeField] private TMP_Text joinStatusText;


    [Header("LAN Connection Parameters")]
    [SerializeField] private ushort port = 7777;
    
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    private UnityTransport unityTransport;
    private readonly Dictionary<string, GameObject> hostAddressToRoomEntryDict = new();
    void Awake()
    {
        if (!NetworkManager.Singleton != null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        if (hostDiscovery != null) hostDiscovery.enabled = true;
        if (clientDiscovery != null) clientDiscovery.enabled = true;

        // UI Setup
        SwitchToMainMenuUI();
        if (joinStatusText != null) joinStatusText.text = string.Empty;
        if (confirmCreateRoomButton != null) confirmCreateRoomButton.onClick.AddListener(OnCreateRoomButtonPressed);

    }

    private void SwitchToHostMenuUI()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (hostPanel != null) hostPanel.SetActive(true);
        if (joinPanel != null) joinPanel.SetActive(false);
    }

    private void SwitchToMainMenuUI()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (hostPanel != null) hostPanel.SetActive(false);
        if (joinPanel != null) joinPanel.SetActive(false);
    }

    private void SwitchToJoinMenuUI()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (hostPanel != null) hostPanel.SetActive(false);
        if (joinPanel != null) joinPanel.SetActive(true);
    }

    private void SwitchToWaitingUI()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (hostPanel != null) hostPanel.SetActive(false);
        if (joinPanel != null) joinPanel.SetActive(false);
    }

    private void RefreshRoomList()
    {
        ClearRoomList();

        if (statusText != null) statusText.text = "Refreshing LAN Room List ...";

        if (clientDiscovery == null) return;

        List<GameRoomMetaData> rooms = clientDiscovery.GetCurrentRooms();
        if (rooms.Count == 0) return;

        foreach(GameRoomMetaData room in rooms)
        {
            
        }

    }

    private void CreateRoomEntryUIElements(GameRoomMetaData room)
    {
        string addressPortKey = $"{room.HostIpAddress}:{room.GamePort}";
        if(!hostAddressToRoomEntryDict.TryGetValue(addressPortKey, out GameObject entry) || entry == null)
        {
            entry = Instantiate(roomEntryButtonPrefab, roomListContainer);
            hostAddressToRoomEntryDict[addressPortKey] = entry;
        }
    }

    private void OnCreateRoomButtonPressed()
    {
        if (NetworkManager.Singleton == null || unityTransport == null)
        {
            Debug.LogError("[LANConnectionMenu] Missing NetworkManager or UnityTransport.");
            return;
        }

        string roomName = roomNameInput != null && !string.IsNullOrWhiteSpace(roomNameInput.text) ? roomNameInput.text.Trim(): "LAN Game Room";
        
        // Start host flow
        unityTransport.SetConnectionData("0.0.0.0", gamePort, "0.0.0.0");
        bool started = NetworkManager.Singleton.StartHost();
        if (!started) return;
        clientDiscovery?.StopListening();
        hostDiscovery?.InitBroadcast(roomName, gamePort, 1, maxPlayers);

        if (matchStartFlow != null) matchStartFlow.InitAfterServerStart();

        // Waiting for the players to join ui panel enable
        SwitchToWaitingUI();
    }


    public void StartLANClient(string hostIPAddress)
    {
        if (NetworkManager.Singleton == null || unityTransport == null)
        {
            Debug.LogError("[LanConnectionMenu] Missing NetworkManager or UnityTransport.");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[LanConnectionMenu] Network is already running.");
            return;
        }
        unityTransport.SetConnectionData(hostIPAddress, port);
        bool clientStarted = NetworkManager.Singleton.StartClient();
        string message = clientStarted? "Client has started sucessfully" : "Client start failed";
        SetStatus(message);


    }


    private void SetStatus(string message)
    {
        Debug.Log($"[LANConnectionMenu] {message}");

        if (statusText != null)
            statusText.text = message;
    }

    private void ClearRoomList()
    {
        foreach (var kvp in hostAddressToRoomEntryDict)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }

        hostAddressToRoomEntryDict.Clear();
    }
}