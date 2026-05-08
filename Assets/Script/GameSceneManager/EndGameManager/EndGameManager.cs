using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkObject))]
public class EndGameManager : NetworkBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "InGameScene";
    [SerializeField] private string lanMenuSceneName = "LANMenu";
    
    [Header("UI References")]
    [SerializeField] private GameObject clientReadyMark;
    [SerializeField] private GameObject hostReadyMark;

    [Header("Network Reference")]
    [SerializeField] private MatchStartFlow matchStartFlow;

    private readonly NetworkVariable<bool> hostWantsRematch = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> clientWantsRematch = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        matchStartFlow = Object.FindFirstObjectByType<MatchStartFlow>();
        clientWantsRematch.OnValueChanged += ClientVoteStateChangeHandler;
        hostWantsRematch.OnValueChanged += HostVoteStateChangeHandler;
    }

    public override void OnNetworkDespawn()
    {
        clientWantsRematch.OnValueChanged -= ClientVoteStateChangeHandler;
        hostWantsRematch.OnValueChanged -= HostVoteStateChangeHandler;
    }

    public void OnClickRematch()
    {        
        SubmitRematchRequestViaServerRpc();
    }

    public void OnClickBackToMenu()
    {
        LeaveSessionAndNavToLANMenu();
    }

    private void ClientVoteStateChangeHandler(bool prevValue, bool currValue)
    {
        if (clientReadyMark == null) return;

        if (!prevValue && currValue)
        {
            clientReadyMark.SetActive(true);
        }
        if (prevValue && !currValue)
        {
            clientReadyMark.SetActive(false);
        }

    }

    private void HostVoteStateChangeHandler(bool prevValue, bool currValue)
    {
        if (hostReadyMark == null) return;

        if (!prevValue && currValue)
        {
            hostReadyMark.SetActive(true);
        }
        if (prevValue && !currValue)
        {
            hostReadyMark.SetActive(false);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitRematchRequestViaServerRpc(RpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        if (senderClientId == NetworkManager.ServerClientId)
        {
            hostWantsRematch.Value = true;
        }
        else
        {
            clientWantsRematch.Value = true;
        }
        
        if (hostWantsRematch.Value && clientWantsRematch.Value)
        {
            StartRematchFromServer();
        }
    }

    private void StartRematchFromServer()
    {
        if (!IsServer) return;

        hostWantsRematch.Value = false;
        clientWantsRematch.Value = false;

        if (matchStartFlow == null)
        {
            Debug.LogWarning("[EndGameManager] Can't find the match start flow component.");
            return;
        }
        
        matchStartFlow.StartMatchOrRematchFromServer();

    }

    private void LeaveSessionAndNavToLANMenu()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(lanMenuSceneName, LoadSceneMode.Single);
    }
}