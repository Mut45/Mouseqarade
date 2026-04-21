using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LANRoomEntry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text roomInfoText;

    public Button JoinButton => joinButton;

    public void SetRoomInfo(GameRoomMetaData room)
    {
        roomInfoText.text = $"{room.RoomName}   ({room.PlayerCount}/{room.MaxPlayers})";
    }
}
