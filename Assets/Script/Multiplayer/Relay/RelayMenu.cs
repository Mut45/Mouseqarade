using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports;
using Unity.Netcode.Transports.UTP;
using TMPro;

using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using UnityEngine.SceneManagement;

public class RelayMenu : MonoBehaviour
{
    [Header("UI Display")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text statusText;

    [Header("UI Game Objects")]
    [SerializeField] private GameObject createRoomButtonObject;
    [SerializeField] private GameObject joinCodeInputObject;
    [SerializeField] private GameObject joinButtonObject;
    [SerializeField] private GameObject joinCodeTextObject;
    [SerializeField] private GameObject backButtonObject;
    [SerializeField] private GameObject orText;

    [Header("Unity Networking")]
    private NetworkManager networkManager;
    [SerializeField] private UnityTransport transport;

    [Header("Relay")]
    [SerializeField] private string connectionType = "dtls"; 
    [SerializeField] private string gameplaySceneName = "IngameScene";

    private bool servicesInitialized;
    
    private async void Awake()
    {
        await InitializeUnityServicesAsync();

        if (networkManager == null)
            networkManager = NetworkManager.Singleton;

        if (transport == null && networkManager != null)
            transport = networkManager.GetComponent<UnityTransport>();
    }

    private bool ValidateNetworkingReferences()
    {
        if (networkManager == null)
        {
            SetStatus("NetworkManager is missing.");
            return false;
        }

        if (transport == null)
        {
            SetStatus("UnityTransport is missing.");
            return false;
        }

        return true;
    }

    private async System.Threading.Tasks.Task InitializeUnityServicesAsync()
    {
        if (servicesInitialized) return;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            servicesInitialized = true;
            SetStatus("Unity Services ready.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            SetStatus($"Services init failed: {ex.Message}");
        }
    } 

    public async void CreateRelayRoom()
    {
        await InitializeUnityServicesAsync();
        if (!ValidateNetworkingReferences()) return;
        try
        {
            // Relay system initialization start boilerplate
            const int maxPlayers = 2;
            const int maxConnections = maxPlayers - 1;
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            var relayServerData = allocation.ToRelayServerData(connectionType);
            transport.SetRelayServerData(Unity.Services.Relay.Models.AllocationUtils.ToRelayServerData(allocation, connectionType));

            bool ifStarted = networkManager.StartHost();
            if (!ifStarted)
            {
                SetStatus("Failed to start host.");
                return;
            }

            if (joinCodeText != null)
            {
                joinCodeText.text = $"Join Code: \n {relayJoinCode}";
            }

            SetHostCodeState();
            SetStatus("Room created.");

            // Match flow starts
            MatchStartFlow matchFlow = FindFirstObjectByType<MatchStartFlow>();

            if (matchFlow != null)
            {
                matchFlow.InitAfterServerStart();
            }

        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
            SetStatus($"Relay create failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            SetStatus($"Host start failed: {ex.Message}");
        }
        
    }

    public async void JoinRelayRoom()
    {
        await InitializeUnityServicesAsync();

        if (!ValidateNetworkingReferences()) return;

        string code = joinCodeInput != null ? joinCodeInput.text.Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(code))
        {
            SetStatus("Enter a valid join code, please.");
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            transport.SetRelayServerData(joinAllocation.ToRelayServerData(connectionType));

            bool started = networkManager.StartClient();
            SetStatus(started ? "Joining room..." : "Failed to start client.");
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
            SetStatus($"Relay join failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            SetStatus($"Client start failed: {ex.Message}");
        }
    }

    public void OnBackButtonPressed()
    {
        NetworkManager nm = NetworkManager.Singleton;

        if (nm != null && (nm.IsClient || nm.IsServer || nm.IsListening))
        {
            Debug.Log("[RelayMenu] Shutting down active network session.");
            nm.Shutdown();
        }

        if (joinCodeInput != null)
        {
            joinCodeInput.text = string.Empty;
        }

        if (joinCodeText != null)
        {
            joinCodeText.text = string.Empty;
        }

        SetDefaultMenuState();
        SetStatus("Returned to menu.");
    }

    private void SetDefaultMenuState()
    {
        SetActiveSafe(createRoomButtonObject, true);
        SetActiveSafe(joinCodeInputObject, true);
        SetActiveSafe(joinButtonObject, true);
        SetActiveSafe(orText, true);

        SetActiveSafe(joinCodeTextObject, false);
        SetActiveSafe(backButtonObject, false);
    }

    private void SetHostCodeState()
    {
        SetActiveSafe(createRoomButtonObject, false);
        SetActiveSafe(joinCodeInputObject, false);
        SetActiveSafe(joinButtonObject, false);
        SetActiveSafe(orText, false);

        SetActiveSafe(joinCodeTextObject, true);
        SetActiveSafe(backButtonObject, true);
    }

    private void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }        
    }
    private void SetStatus(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
        
    }
}