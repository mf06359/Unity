using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    public static GameManager instance;
    //private PhotonView photonView;
    public PlayerManager playerManager;
    [SerializeField] Wall wall;

    public List<int> points, dora, uraDora;


    public int waitingCount = 0, activePlayerId = 0, activeTile = 0, numberOfPlayers ,turnCount = 0, placeWind = 0, honba = 0;

    public int parentId = 0;
    public GameObject playerManagerPrefab;  

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            numberOfPlayers = Rule.numberOfPlayers;
            playerManager = FindFirstObjectByType<PlayerManager>();
            waitingCount = numberOfPlayers - 1;
        }
    }


    // WRITTEN BELOW IS FUNCTIONS AFTER CONNECTING
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/


    public void GameStart()
    {
        photonView.RPC("StartSetting", RpcTarget.All);
        for (int i = 0; i < numberOfPlayers; i++)
        {
            points.Add(Rule.startPoint);
        }
        NewRoundStart();
    }
    [PunRPC]
    public void StartSetting()
    {
        playerManager.StartSetting();
    }

    public void NewRoundStart()
    {
        wall = new(); wall.Reset();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            List<int> tempHand = new();
            for (int j = 0; j < 13; j++)
            {
                tempHand.Add(wall.NewTsumo());
            }
            tempHand.Sort();
            photonView.RPC("DealHand", RpcTarget.All, i, tempHand.ToArray());
        }
        photonView.RPC("DisplayFirstHand", RpcTarget.All);
        NewTurn(true);
    }
    [PunRPC]
    public void DealHand(int playerId, int[] tempHand)
    {
        if (playerId == playerManager.id) playerManager.DealHand(playerId, tempHand);
    }

    [PunRPC]
    public void DisplayFirstHand()
    {
        playerManager.DisplayFirstHand();
    }

    public void NewTurn(bool firstTsumo = false)
    {
        if (firstTsumo) activePlayerId = parentId;
        else activePlayerId = (activePlayerId + 1) % numberOfPlayers;
        activeTile = wall.NewTsumo();
        photonView.RPC("StartTurn", RpcTarget.All, activePlayerId, activeTile); //  points have to be included
    }

    [PunRPC]
    public void StartTurn(int activePlayerId,  int activeTile)
    {
        playerManager.activePlayerId = activePlayerId;
        playerManager.activeTile = activeTile;
        if (playerManager.id == activePlayerId) playerManager.StartTurn(activeTile);
    }

    // WRITTEN BELOW ARE FUNCTIONS AFTER   WAITING ACTION
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/

    [PunRPC]
    public void AnkanTsumo(int playerId, string action)
    {
    }
    [PunRPC]
    public void ReloadRiver(int playerid, int tileid)
    {
        playerManager.ReloadRIver(playerid, tileid);
    }

    // WRITTEN BELOW ARE FUNCTIONS AFTER   WAITING ACTION
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/
    [PunRPC]
    public void EndTurn()
    {
        if (playerManager.id == activePlayerId)
        {
            playerManager.EndTurn();
        }
    }



    // PM to GM
    [PunRPC]
    public void ActionFromPlayer_RPC(int playerId, string action, Tile tile)
    {
        Debug.Log($"Player {playerId} ‚ª {action} ‚ðŽÀs");

        if (action == "DiscardTile")
        {
            photonView.RPC("ReactToDiscardedTile", RpcTarget.All, playerId, tile);
        }
    }

    //[PunRPC]
    //public void ActionFromPlayer_RPC(int playerId, string action, int num, int tileId)
    //{
    //    Debug.Log($"Player {playerId} ‚ª {action} ‚ðŽÀs");

    //    if (action == "AddWaitingCount")
    //    {
    //        AddWaitingCount(num, tileId);
    //    }
    //    else if (action == "TsumoHo")
    //    {
    //        TsumoHo(num);
    //    }
    //    //else
    //    //{
    //    //    AnkanTsumo(playerId, num);
    //    //}
    //}

    public void TsumoHo(int num)
    {
        dora = wall.Dora();
        uraDora = wall.UraDora();
        if (num == parentId)
        {
            honba++;
            NewRoundStart();
        }
        else
        {
            parentId = (parentId + 1) % numberOfPlayers;
            if (parentId == 0)
            {
                placeWind++;
                if (placeWind == 2)
                {
                    GameOver();
                }
            }
        }
    }

    public void RonHo(int num)
    {
        dora = wall.Dora();
        uraDora = wall.UraDora();
        //Library.CalculatePoints(playerManager[num]);
    }

    [PunRPC]
    public void ReactToDiscardedTile(int discardPlayerId, int tileId)
    {
        if (playerManager.id != discardPlayerId)
        {
            playerManager.ReactToDiscardedTile(discardPlayerId, tileId);
        }
    }
    [PunRPC]
    public void AddWaitingCount(int num, int discardedTileId)
    {
        Debug.Log($"AddWaitingCount Called from {playerManager.id}");
        waitingCount -= num;
        if (waitingCount == 0)
        {
            waitingCount = numberOfPlayers - 1;

            photonView.RPC("EndTurn", RpcTarget.All);

            photonView.RPC("ReloadRiver", RpcTarget.All, activePlayerId, discardedTileId);

            NewTurn();
        }
    }

    // WRITTEN BELOW ARE FUNCTIONS AFTER GAMESET
    //   |  |
    //   |  |
    //   |  |
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/

    public void GameOver()
    {
        photonView.RPC("GameOver", RpcTarget.All, points);
    }
}