using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using NUnit.Framework;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject namePanel;
    public GameObject roomPanel;
    public GameObject waitPanel;

    public TMP_InputField nameInput;
    public TMP_InputField roomInput;
    public TMP_Text waitText;

    public GameManager gameManager;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;  // シーン同期を有効にする

        InitializeLocalObjects();
    }

    void InitializeLocalObjects()
    {
        gameManager = GetComponent<GameManager>();
        Debug.Log("ローカルオブジェクトの初期化完了");
    }


    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log("connecting to photon");

        namePanel.SetActive(false);
        roomPanel.SetActive(false);
        waitPanel.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photonに接続完了");
        namePanel.SetActive(true);
    }

    public void OnClickEnterName()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.Log("名前を入力してください");
            return;
        }

        PhotonNetwork.NickName = nameInput.text;
        namePanel.SetActive(false);
        roomPanel.SetActive(true);
    }

    public void OnClickJoinRoom()
    {
        string roomName = roomInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.Log("ルーム名を入力してください");
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = Rule.numberOfPlayers;

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }


    public override void OnJoinedRoom()
    {
        HideAllPanels();
        roomPanel.SetActive(false);
        waitPanel.SetActive(true);
        UpdateWaitText();
        TryStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateWaitText();
        TryStartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateWaitText();
    }

    void UpdateWaitText()
    {
        int count = PhotonNetwork.CurrentRoom.PlayerCount;
        waitText.text = $"待機中...\n {count}/{Rule.numberOfPlayers} 人参加中";
    }

    void TryStartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == Rule.numberOfPlayers)
        {
            waitPanel.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                GameManager.instance.GameStart();
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"ルームへの参加に失敗しました: {message}");
    }

    // すべてのPanelを非表示にする
    void HideAllPanels()
    {
        namePanel.SetActive(false);
        roomPanel.SetActive(false);
        waitPanel.SetActive(false);
    }
}
