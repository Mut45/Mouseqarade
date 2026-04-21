using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEditor.PackageManager;


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

    [Header("Main Page UI Elements")]
    [SerializeField] private Button toCreateRoomFlowButton;
    [SerializeField] private Button toJoinRoomFlowButton;

    [Header("Host UI Elements")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button confirmCreateRoomButton;
    [SerializeField] private Button hostBackButton;

    [Header("Client UI Elements")]
    [SerializeField] private Button refreshRoomsButton;
    [SerializeField] private Button joinBackButton;
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomEntryPrefab;
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("LAN Connection Parameters")]
    [SerializeField] private ushort port = 7777;
    
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    [Header("Network")]
    [SerializeField] private UnityTransport unityTransport;
    private readonly Dictionary<string, GameObject> hostAddressToRoomEntryDict = new();
    void OnEnable()
    {
        if (clientDiscovery != null)
        {
            clientDiscovery.OnRoomDiscoveredOrUpdated += CreateRoomEntryUIElements;
        }
        
    }

    void OnDisable()
    {
        if (clientDiscovery != null)
        {
            clientDiscovery.OnRoomDiscoveredOrUpdated -= CreateRoomEntryUIElements;
        }
    }

    private void Update()
    {
        if (clientDiscovery != null)
        {
            clientDiscovery.PumpPendingRooms();
        }
    }

    void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        if (hostDiscovery != null) hostDiscovery.enabled = true;
        if (clientDiscovery != null) clientDiscovery.enabled = true;
        

        // UI Setup
        SwitchToMainMenuUI();
        if (connectionStatusText != null) 
        {
            connectionStatusText.gameObject.SetActive(false);
            connectionStatusText.text = string.Empty;
        }
        if (toCreateRoomFlowButton != null) toCreateRoomFlowButton.onClick.AddListener(OnToCreateRoomFlowButtonPressed);
        if (toJoinRoomFlowButton != null) toJoinRoomFlowButton.onClick.AddListener(OnToJoinRoomFlowButtonPresseed);


        // Host UI Buttons
        if (confirmCreateRoomButton != null) confirmCreateRoomButton.onClick.AddListener(OnCreateRoomButtonPressed);
        if (hostBackButton != null) hostBackButton.onClick.AddListener(OnHostBackButtonPressed);

        //Client UI Buttons
        if (refreshRoomsButton != null) refreshRoomsButton.onClick.AddListener(OnRefreshButtonPressed);
        if (joinBackButton != null) joinBackButton.onClick.AddListener(OnJoinBackButtonPressed);
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

    private void SwitchToEmptyUI()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (hostPanel != null) hostPanel.SetActive(false);
        if (joinPanel != null) joinPanel.SetActive(false);
    }

    public void OnToCreateRoomFlowButtonPressed()
    {
        SwitchToHostMenuUI();
    }

    public void OnToJoinRoomFlowButtonPresseed()
    {
        SwitchToJoinMenuUI();
        OnRefreshButtonPressed();
    }

    public void OnHostBackButtonPressed()
    {
        SwitchToMainMenuUI();
        hostDiscovery?.StopBroadcast();
        if (connectionStatusText != null) {
            connectionStatusText.gameObject.SetActive(false);
            connectionStatusText.text = string.Empty;
        }
    }

    public void OnJoinBackButtonPressed()
    {
        SwitchToMainMenuUI();
        clientDiscovery?.StopListening();
        ClearRoomList();
        
        if (connectionStatusText != null) {
            connectionStatusText.gameObject.SetActive(false);
            connectionStatusText.text = string.Empty;
        }
    }

    public void OnRefreshButtonPressed()
    {
        ClearRoomList();

        if (statusText != null) statusText.text = "Refreshing LAN Room List ...";

        if (clientDiscovery == null) return;
        clientDiscovery.BeginListening();
        clientDiscovery.PumpPendingRooms();
        List<GameRoomMetaData> rooms = clientDiscovery.GetCurrentRooms();
        if (rooms.Count == 0) return;

        foreach(GameRoomMetaData room in rooms)
        {
            CreateRoomEntryUIElements(room);
        }

        SetStatus($"{rooms.Count} room(s) found");

    }

    private void CreateRoomEntryUIElements(GameRoomMetaData room)
    {
        Debug.Log($"[LANConnectionMenu] Room Stats: {room.RoomName}");
        string addressPortKey = $"{room.HostIpAddress}:{room.GamePort}";
        if(!hostAddressToRoomEntryDict.TryGetValue(addressPortKey, out GameObject entry) || entry == null)
        {
            entry = Instantiate(roomEntryPrefab, roomListContainer);
            hostAddressToRoomEntryDict[addressPortKey] = entry;
            LANRoomEntry gameRoomEntry = entry.GetComponent<LANRoomEntry>();
            Button joinButton = gameRoomEntry.JoinButton;
            gameRoomEntry.SetRoomInfo(room);

            if (joinButton != null)
            {
                joinButton.onClick.RemoveAllListeners();
                joinButton.onClick.AddListener(() => {OnJoinRoomButtonPressed(room);});
            }
        }
    }

    private void OnJoinRoomButtonPressed(GameRoomMetaData room)
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

        clientDiscovery?.StopListening();
        // Start unity transport connection
        unityTransport.SetConnectionData(room.HostIpAddress, room.GamePort);
        bool started = NetworkManager.Singleton.StartClient();

        if (!started)
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Failed to start client.";
            }
            return;
        }

        if (connectionStatusText != null)
        {
            connectionStatusText.gameObject.SetActive(true);
            connectionStatusText.text = $"Joining {room.RoomName}...";
        }

        SwitchToEmptyUI();

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

        if (connectionStatusText != null)
        {
            connectionStatusText.gameObject.SetActive(true);
            connectionStatusText.text = $"Waiting for player to join...";
        }

        // Waiting for the players to join ui panel enable
        SwitchToEmptyUI();
    }

    void OnApplicationQuit()
    {
        try
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[LanConnectionMenu] NetworkManager shutdown warning: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        try
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[LanConnectionMenu] NetworkManager shutdown warning: {ex.Message}");
        }
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