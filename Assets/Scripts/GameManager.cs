using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviourPun
{
    public static GameManager instance;
    public PlayerManager playerManager;
    [SerializeField] Wall wall;

    public List<int> points, dora, uraDora;

    public float[] voiceVolume;

    public int waitingCount = 0, activePlayerId = 0, activeTile = 0, numberOfPlayers ,turnCount = 0, placeWind = 0, honba = 0;

    public int willFuro = 0, wontFuro = 0;

    public bool furoWaiting = false;
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
            voiceVolume = new float[numberOfPlayers];
        }
    }



    public void GameStart()
    {
        photonView.RPC("StartSetting", RpcTarget.All);
        for (int i = 0; i < numberOfPlayers; i++)
        {
            points.Add(Rule.startPoint);
        }
        NewRoundStart();
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
        ReloadDora();
        NewTurn(0);
    }

    public void ReloadDora()
    {
        photonView.RPC("TellOmoteAndUra", RpcTarget.All, wall.Dora().ToArray(), wall.UraDora().ToArray());
    }


    [PunRPC]
    public void NewTurn(int player = -1, bool furo = false)
    {
        if (player != -1) activePlayerId = player;
        else activePlayerId = (activePlayerId + 1) % numberOfPlayers;
        activeTile = wall.NewTsumo();
        if (furo == false) photonView.RPC("StartTurn", RpcTarget.All, activePlayerId, activeTile); //  points have to be included
    }


    //public void TsumoHo(int num)
    //{
    //    dora = wall.Dora();
    //    uraDora = wall.UraDora();
    //    if (num == parentId)
    //    {
    //        honba++;
    //        NewRoundStart();
    //    }
    //    else
    //    {
    //        parentId = (parentId + 1) % numberOfPlayers;
    //        if (parentId == 0)
    //        {
    //            placeWind++;
    //            if (placeWind == 2)
    //            {
    //                GameOver();
    //            }
    //        }
    //    }
    //}

    [PunRPC]
    public void RiichiCall()
    {
        // SoundPlayer "RIICHI!"
        // Panel Display "RIICHI"
        playerManager.RiichiCall_RPC();
    }

    //public void Kakan()
    //{
    //}

    //public void Ankan()
    //{
    //}



    [PunRPC]
    public void StartTurn(int activePlayerIdIn, int activeTileIn)
    {
        activePlayerId = activePlayerIdIn;
        activeTile = activeTileIn;
        if (playerManager.id == activePlayerIdIn) playerManager.StartTurn(activeTileIn);
    }

    [PunRPC]
    public void SetActivePlayer(int _activePlayerId)
    {
        activePlayerId = _activePlayerId;
    }



    [PunRPC]
    public void StartSetting()
    {
        playerManager.StartSetting();
    }

    [PunRPC]
    public void TellOmoteAndUra(int[] omote, int[] ura)
    {
        playerManager.TellOmoteAndUra(omote, ura);
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



    //[PunRPC]
    //public void AnkanTsumo(int playerId, string action)
    //{
    //}


    [PunRPC]
    public void EndTurn()
    {
        if (playerManager.id == activePlayerId)
        {
            playerManager.EndTurn();
        }
    }
    // WRITTEN BELOW IS FUNCTIONS with PunRPC
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/

    [PunRPC]
    public void ActionFromPlayer_RPC(int playerId, string action, Tile tile)
    {
        Debug.Log($"Player {playerId} ‚ª {action} ‚ðŽÀs");

        if (action == "DiscardTile")
        {
            photonView.RPC("ReactToDiscardedTile", RpcTarget.All, playerId, tile);
        }
    }



    [PunRPC]
    public void FuroStart()
    {
        furoWaiting = true;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            voiceVolume[i] = 0;
        }
        willFuro = 0;
        wontFuro = 0;
    }

    [PunRPC]
    public void ReloadRiverAndReactToDiscardedTile(int discardPlayerId, int tileId, bool riichi)
    {
        activeTile = tileId;
        activePlayerId = discardPlayerId;
        playerManager.ReloadRiver(discardPlayerId, tileId, riichi);
        playerManager.isFuro = false;
        playerManager.isRon = false;
        playerManager.input = null;
        if (playerManager.id != discardPlayerId)
        {
            playerManager.ReactToDiscardedTile();
        }
    }

    // ---------------------------------------------------------------------------------------------------------

    [PunRPC]
    public void CountFuroCall(int playerid, float rate = 0)
    {
        if (rate != 0) willFuro++;
        else wontFuro++;
        voiceVolume[playerid] = rate;
        if (willFuro + wontFuro == numberOfPlayers - 1)
        {
            if (willFuro == 0)
            {
                photonView.RPC("EndTurn", RpcTarget.All);

                NewTurn();
            }
            else if (willFuro == 1)
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    if (voiceVolume[i] != 0)
                    {
                        photonView.RPC("PermitFuro", RpcTarget.All, i);
                        Debug.Log(i);
                    }
                }
            }
            else
            {
                photonView.RPC("RecodeVolume", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void PermitFuro(int playerid)
    {
        if (playerid == playerManager.id)
        {
            photonView.RPC("WriteLog", RpcTarget.All, $"PermitFuro Called to {playerid}");
            if (playerManager.isFuro) playerManager.Furo(playerManager.input);
            else playerManager.RonHo(playerManager.input);
        }
    }

    [PunRPC]
    public void RecodeVolume()
    {
        if (playerManager.isFuro || playerManager.isRon)
        {
            playerManager.RecodeVoiceVolume();
            photonView.RPC("WriteVolume", RpcTarget.MasterClient, playerManager.id, playerManager.volume);
        }
    }

    [PunRPC]
    public void WriteLog(string message)
    {
        Debug.Log(message);
    }

    [PunRPC]
    public void WriteVolume(int id, float volume)
    {
        photonView.RPC("WriteLog", RpcTarget.All, $"{volume} from {id}");
        voiceVolume[id] *= (volume * 100 + Random.Range(0, 5f) + 10 + id);
        willFuro--;
        wontFuro++;
        if (willFuro == 0)
        {
            int furoId = 0;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (Mathf.Max(voiceVolume) == voiceVolume[i])
                {
                    furoId = i; break;
                }
            }
            PermitFuro(furoId);
        }
    }


    // Furo from playerManager
    [PunRPC]
    public void EraseAndSaveFuroTiles(int[] pair, int activeTile, int whoDiscard, int whoFuro)
    {
        photonView.RPC("WriteLog", RpcTarget.All, $"{whoFuro} did Furo {activeTile} from {whoDiscard}");
        playerManager.panelManager.EraseAndSaveFuroTiles(new List<int> (pair), activeTile, whoDiscard, whoFuro);
    }


    //[PunRPC]
    //public void Pon(int playerid)
    //{
    //    if (furoWaiting)
    //    {
    //        furoWaiting = false;
    //        photonView.RPC("DidPon", RpcTarget.All, playerid);
    //    }
    //}

    //[PunRPC]
    //public void Chi(int playerid)
    //{
    //    if (furoWaiting)
    //    {
    //        furoWaiting = false;
    //        photonView.RPC("DidChi", RpcTarget.All, playerid);
    //    }
    //}
    //[PunRPC]
    //public void Kan(int playerid)
    //{
    //    if (furoWaiting)
    //    {
    //        furoWaiting = false;
    //        photonView.RPC("DidKan", RpcTarget.All, playerid);
    //    }
    //}

    //[PunRPC]
    //public void Ron(int playerid)
    //{
    //    if (furoWaiting)
    //    {
    //        furoWaiting = false;
    //        photonView.RPC("DidRon", RpcTarget.All, playerid);
    //    }
    //}

    //[PunRPC]
    //public void ShowPointChange(int winPlayerId, int[] maxPoints, string[] yakuNames, int fu, int han)
    //{
    //    playerManager.ShowPointChange(winPlayerId, new List<int> (maxPoints), new List<string>(yakuNames), fu, han);
    //}


    // WRITTEN BELOW ARE FUNCTIONS AFTER   WAITING ACTION
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/

    //[PunRPC]
    //public void Restart()
    //{
    //    playerManager.Restart();
    //}

    //public void GameOver()
    //{
    //    photonView.RPC("GameOver", RpcTarget.All, points);
    //}
}