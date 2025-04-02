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
        PhotonNetwork.AutomaticallySyncScene = true;  // �V�[��������L���ɂ���

        InitializeLocalObjects();
    }

    void InitializeLocalObjects()
    {
        gameManager = GetComponent<GameManager>();
        Debug.Log("���[�J���I�u�W�F�N�g�̏���������");
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
        Debug.Log("Photon�ɐڑ�����");
        namePanel.SetActive(true);
    }

    public void OnClickEnterName()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.Log("���O����͂��Ă�������");
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
            Debug.Log("���[��������͂��Ă�������");
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
        waitText.text = $"�ҋ@��...\n {count}/{Rule.numberOfPlayers} �l�Q����";
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
        Debug.LogError($"���[���ւ̎Q���Ɏ��s���܂���: {message}");
    }

    // ���ׂĂ�Panel���\���ɂ���
    void HideAllPanels()
    {
        namePanel.SetActive(false);
        roomPanel.SetActive(false);
        waitPanel.SetActive(false);
    }
}
