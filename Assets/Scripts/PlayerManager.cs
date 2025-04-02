using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    public int actorNumber; // ÉvÉåÉCÉÑÅ[ID

    [SerializeField] private Tile[] prefabTile;

    public GameObject playerPanel;
    public GameObject gameSetPanel;


    public SoundPlayer audioManager;
    public ButtonManager buttonManager;
    public PanelManager panelManager;
    public StateManager stateManager;
    public Wall wall;

    private const int turnLimit = 52;
    public int riverCount = 0, id;
    public bool pleaseSort = true, pleaseSkip = true, activeTileKanned = false, isComputer = true;
    public Vector3 firstHandPlace, firstRiverTail, riverTail;

    private int[] river;
    private List<Tile> tileAt;
    private List<Vector3> positions;
    public List<int> dora, uraDora;


    public int turnCount = 0;
    public List<int> hand, head;
    public List<List<int>> furoHand;
    public int furoCount = 0;
    public bool[,] machiTiles;
    // from parent, from childs
    // OR
    // from who, point
    public List<string> tempYakuNames, yakuNames;
    public int tempHan = 2, tempFu = 20, han = 2, fu = 20;
    public int[] maxPoints, tempMaxPoints;
    // Absolute
    public int latestTile = 0, riichiTurn = 0;
    public int[] kakan, count;
    public bool[] machiTile, pon, chi, kan;
    public bool[] ryanmen;
    public bool[,] ryanmenIf;
    public int[] seqMeld = new int[37];
    public int[] tripletMeld = new int[37];
    public int[] furoSeqMeld = new int[37];
    public int[] furoTripletMeld = new int[37];
    public int[] kantsu = new int[37];
    public int[] ankantsu = new int[37];
    public int[] tileCount;
    public bool[,] penchanKanchanTankiIf;
    public bool[] penchanKanchanTanki;
    public int[] shantenIf;
    /// (activeTileId, handId1, handId2) -> Can Furo ? 
    public bool[,,] ponPair, chiPair;
    public int shanten, redDoraCount;
    public bool doubleriichiNow = false, kanJustNow = false, canAnkan = false;
    public int activeTile, activePlayerId;

    private void Awake()
    { 
        hand = new List<int>();
        furoHand = new List<List<int>>();
        machiTiles = new bool[37, 37];
        machiTile = new bool[37];
        kakan = new int[37];
        pon = new bool[37];
        chi = new bool[37];
        kan = new bool[37];
        penchanKanchanTanki = new bool[37];
        ryanmen = new bool[37];
        ponPair = new bool[37, 37, 37];
        chiPair = new bool[37, 37, 37];
        penchanKanchanTankiIf = new bool[37, 37];
        ryanmenIf = new bool[37, 37];
        shanten = 13;

        seqMeld = new int[37];
        tripletMeld = new int[37];
        furoSeqMeld = new int[37];
        furoTripletMeld = new int[37];
        kantsu = new int[37];
        ankantsu = new int[37];
        count = new int[37];
        riichiTurn = -1;
        for (int i = 0; i < 37; i++)
        {
            for (int j = 0; j < 37; j++)
            {
                for (int k = 0; k < 37; k++)
                {
                    ponPair[i, j, k] = false;
                    chiPair[i, j, k] = false;
                }
                penchanKanchanTankiIf[i, j] = false;
            }
            kakan[i] = -1;
            furoSeqMeld[i] = 0;
            furoTripletMeld[i] = 0;
            kantsu[i] = 0;
            ankantsu[i] = 0;
            penchanKanchanTanki[i] = false;
        }
        riverTail = new Vector3(6, 5, 0);
        firstRiverTail = new(-1.66f, -2.5f, 0);
        firstHandPlace = new Vector3(-7, -7, 0);
        river = new int[37];
        for (int i = 0; i < 37; i++)
        {
            river[i] = 0;
        }
        riverTail = firstRiverTail;
    }

    public void StartSetting()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                id = i; break;
            }
        }
        panelManager.StartSetting();
        panelManager.id = id;
    }


    public void RequestAction(string action)
    {
        if (PhotonNetwork.IsConnected)
        {
            GameManager.instance.photonView.RPC("ActionFromPlayer_RPC", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, action);
        }
    }

    public void RequestAction(string action, Tile tile)
    {
        if (PhotonNetwork.IsConnected)
        {
            GameManager.instance.photonView.RPC("ActionFromPlayer_RPC", RpcTarget.MasterClient, id, action, tile);
            GameManager.instance.photonView.RPC("ActionFromPlayer_RPC", RpcTarget.MasterClient, id, action, tile);
        }
    }

    public void RequestAction(string action, int num)
    {
        if (PhotonNetwork.IsConnected)
        {
            GameManager.instance.photonView.RPC("ActionFromPlayer_RPC", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, action, num);
        }
    }

    [PunRPC]
    public void DealHand(int playerId, int[] haipai)
    {
        hand = new List<int>(haipai); 
        for (int i = 0; i < 13; i++)
        {
            if (Library.idWithoutRed[hand[i]] != hand[i])
            {
                redDoraCount++;
            }
        }
        hand.Sort();
    }
    public void DisplayFirstHand()
    {
        tileAt = new();
        positions = new();
        for (int i = 0; i < 13; i++)
        {
            Tile newTile = Instantiate(prefabTile[hand[i]], firstHandPlace + new Vector3(i, 0, 0), Quaternion.identity);
            newTile.playerid = id;
            newTile.place = i;
            newTile.canTouch = true;
            tileAt.Add(newTile);
            positions.Add(newTile.transform.position);
        }
    }

    // WRITTEN BELOW ARE FUNCTIONS AFTER         GAME START
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/

    public void StartTurn(int tsumo)
    {
        turnCount++;
        if (Library.idWithoutRed[tsumo] != tsumo)
        {
            redDoraCount++;
        }
        hand.Add(tsumo);
        latestTile = tsumo;
        Tile newTile = Instantiate(prefabTile[tsumo], firstHandPlace + new Vector3(hand.Count, 0, 0), Quaternion.identity);
        newTile.playerid = id;
        newTile.place = hand.Count;
        newTile.canTouch = true;
        newTile.moved = false;
        tileAt.Add(newTile);
        positions.Add(newTile.transform.position);
        // HAVE TO REACT TO TENHO
        Library.CalculateShantenCount(this);
        stateManager.UpdateText(this);
        bool canKakan = false;
        foreach (var tile in hand)
        {
            if (kakan[Library.idWithoutRed[tile]] != -1)
            {
                canKakan = true;
            }
        }
        if (id == GameManager.instance.activePlayerId)
        {
            int cnt = 0;
            if (machiTile[Library.idWithoutRed[tsumo]]) cnt++;
            if (shanten == 0 && riichiTurn == -1 && furoCount == 0) cnt++;
            if (canAnkan || canKakan) cnt++;
            if (cnt > 0)
            {
                buttonManager.ResetButtonPlace();
                buttonManager.SkipToMeOn();
                if (machiTile[Library.idWithoutRed[tsumo]]) buttonManager.TsumoOn();
                if (shanten == 0 && riichiTurn == -1 && furoCount == 0) buttonManager.RiichiOn();
                if (canKakan) buttonManager.KakanOn();
                if (canAnkan) buttonManager.AnkanOn();
            }
        }
    }

    // WRITTEN BELOW ARE FUNCTIONS AFTER         WAITING ACTION
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/


    // Not Done Yet
    public void OnClickTsumoHo()
    {
        dora = wall.Dora();
        uraDora = wall.UraDora();
        Library.CalculatePoints(this);
        panelManager.ShowPoints(this);
        panelManager.ShowUra(wall);
    }

    // Not Done Yet
    public void KanInMyTurn()
    {
        List<int[]> kanList = new();
        for (int i = 0; i + 3 < hand.Count; i++)
        {
            if (Library.idWithoutRed[hand[i]] == Library.idWithoutRed[hand[i + 1]]
            && Library.idWithoutRed[hand[i + 1]] == Library.idWithoutRed[hand[i + 2]]
            && Library.idWithoutRed[hand[i + 2]] == Library.idWithoutRed[hand[i + 3]])
            {
                kanList.Add(new int[2] { Library.idWithoutRed[hand[i]], 0 } );
            }
        }
        if (0 < kanList.Count && kanList.Count < 2) // ANKAN
        {
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                if (Library.idWithoutRed[hand[i]] == kanList[0][0])
                {
                    hand.RemoveAt(i);
                }
                if (Library.idWithoutRed[Library.ToInt(tileAt[i].name)] == kanList[0][0])
                {
                    Tile tempTile = tileAt[i];
                    positions.Remove(tempTile.transform.position);
                    tileAt.RemoveAt(i);
                    Destroy(tempTile.gameObject);
                }
            }
            Debug.Assert(kanList.Count == 1);
            panelManager.SaveAnkanTiles(this, kanList[0][0]);
            ankantsu[Library.idWithoutRed[kanList[0][0]]]++;
            ReloadMenzenDisplayWithoutTsumo();
            //GameManager.instance.furoNow[id] = true;
            RequestAction("AnkanTsumo");
            return;
        }
        for (int i = 0; i < 37; i++)
        {
            if (kakan[i] != -1 && hand.IndexOf(i) != -1)
            {
                kanList.Add(new int[2] { Library.idWithoutRed[i], kakan[i] });
            }
        }
        if (kanList.Count < 2) // KAKAN
        {
            Debug.Assert(kanList.Count == 1);
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                if (Library.idWithoutRed[hand[i]] == kanList[0][0])
                {
                    hand.RemoveAt(i);
                }
                if (Library.idWithoutRed[Library.ToInt(tileAt[i].name)] == kanList[0][0])
                {
                    Tile tempTile = tileAt[i];
                    positions.Remove(tempTile.transform.position);
                    tileAt.RemoveAt(i);
                    Destroy(tempTile.gameObject);
                }
            }
            panelManager.SaveKakanTile(this, kanList[0][0]);
            ReloadMenzenDisplayWithoutTsumo();
            furoTripletMeld[kanList[0][0]]--;
            kantsu[kanList[0][0]]++;
            Debug.Assert(kanList.Count == 1);
            //GameManager.instance.furoNow[id] = true;
            //Tsumo();
            return;
        }
        buttonManager.CreateKanButtons(this, kanList);
    }

    // Not Done Yet
    public void Ankan(PlayerManager player, int ankanTile)
    {
        buttonManager.DeactivateButtonsToMe();
        buttonManager.EraseActionPatternButtons();
        player.ankantsu[Library.idWithoutRed[ankanTile]]++;
        for (int i = player.hand.Count - 1; i >= 0; i--)
        {
            if (Library.idWithoutRed[player.hand[i]] == Library.idWithoutRed[ankanTile])
            {
                player.hand.RemoveAt(i);
            }
            if (Library.idWithoutRed[Library.ToInt(tileAt[i].name)] == Library.idWithoutRed[ankanTile])
            {
                Tile tempTile = tileAt[i];
                positions.Remove(tempTile.transform.position);
                tileAt.RemoveAt(i);
                Destroy(tempTile.gameObject);
            }
        }
        panelManager.SaveAnkanTiles(player, ankanTile);
        ReloadMenzenDisplayWithoutTsumo();
        player.kanJustNow = true;
        panelManager.FlipNewDora(wall);
        buttonManager.DeactivateButtonsToMe();
        //Tsumo();
    }

    // Not Done Yet
    public void Kakan(PlayerManager player, int kakanTile)
    {
        buttonManager.DeactivateButtonsToMe();
        buttonManager.EraseActionPatternButtons();
        player.furoTripletMeld[Library.idWithoutRed[kakanTile]]--;
        player.kantsu[Library.idWithoutRed[kakanTile]]++;
        for (int i = player.hand.Count - 1; i >= 0; i--)
        {
            if (Library.idWithoutRed[player.hand[i]] == Library.idWithoutRed[kakanTile])
            {
                player.hand.Remove(player.hand[i]);
            }
        }
        for (int i = tileAt.Count - 1; i >= 0; i--)
        {
            if (Library.idWithoutRed[Library.ToInt(tileAt[i].name)] == Library.idWithoutRed[kakanTile])
            {
                Tile tempTile = tileAt[i];
                positions.Remove(tempTile.transform.position);
                tileAt.RemoveAt(i);
                Destroy(tempTile.gameObject);
                player.hand.Remove(Library.ToInt(tempTile.name));
            }
        }
        panelManager.SaveKakanTile(player, kakanTile);
        //GameManager.instance.furoNow[id] = true;
        ReloadMenzenDisplayWithoutTsumo();
        player.kanJustNow = true;
        buttonManager.DeactivateButtonsToMe();
        //Tsumo();
    }

    // WRITTEN BELOW ARE FUNCTIONS AFTER         WAITING ACTION
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/

    public void OnClickDiscard(Tile tile)
    {
        DiscardTile(tile);
        GameManager.instance.photonView.RPC("ReactToDiscardedTile", RpcTarget.All, id, Library.ToInt(tile.name));
    }

    public void DiscardTile(Tile tile)
    {

        if (Library.idWithoutRed[Library.ToInt(tile.name)] != Library.ToInt(tile.name))
        {
            redDoraCount--;
        }
        positions.Remove(tile.transform.position);
        hand.Remove(Library.ToInt(tile.name));
        tileAt.Remove(tile);
        river[Library.ToInt(tile.name)]++;
        for (int i = 0; i < 37; i++)
        {
            machiTile[i] = machiTiles[Library.idWithoutRed[Library.ToInt(tile.name)], i];
            penchanKanchanTanki[i] = penchanKanchanTankiIf[Library.idWithoutRed[Library.ToInt(tile.name)], i];
            ryanmen[i] = ryanmenIf[Library.idWithoutRed[Library.ToInt(tile.name)], i];
        }
        ReloadMenzenDisplayWithoutTsumo();
    }

    public void ReactToDiscardedTile(int discardPlayerId, int tileId)
    {
        if (id == discardPlayerId) return;
        int activeButton = 1; //buttonManager.ReactToTileOtherPlayerDiscarded(tile);
        GameManager.instance.photonView.RPC("AddWaitingCount", RpcTarget.MasterClient, activeButton, tileId);
    }

//-----------------------------------------------------------------------------------------------------------------------------------


    public void EndTurn()
    {
        buttonManager.TsumoOff();
        buttonManager.RiichiOff();
        buttonManager.DeactivateButtonsToMe();
        Library.ReloadFuroMachi(this);
        kanJustNow = false;
    }

    public void RonHo()
    {
        latestTile = GameManager.instance.activeTile;
        hand.Add(GameManager.instance.activeTile);
        hand.Sort();
        dora = wall.Dora();
        uraDora = wall.UraDora();
        Library.CalculatePoints(this, GameManager.instance.activePlayerId);
        panelManager.ShowPoints(this);
        panelManager.ShowUra(wall);
    }
    public void Chi()
    {
        buttonManager.DeactivateButtonsToOtherPlayer();
        List<List<int>> possiblePair = new();
        for (int i = 0; i < 37; i++)
        {
            for (int j = i + 1; j < 37; j++)
            {
                if (chiPair[GameManager.instance.activeTile, i, j])
                {
                    possiblePair.Add(new List<int> { i, j });
                }
            }
        }
        if (possiblePair.Count < 2)
        {
            Debug.Assert(possiblePair.Count == 1);
            Furo(possiblePair[0]);
            return;
        }
        buttonManager.CreateGroupedImageButtons(this, possiblePair);
    }
    public void Pon()
    {
        buttonManager.DeactivateButtonsToOtherPlayer();
        List<List<int>> possiblePair = new();
        for (int i = 0; i < 37; i++)
        {
            for (int j = i; j < 37; j++)
            {
                if (ponPair[GameManager.instance.activeTile, i, j])
                {   
                    possiblePair.Add(new List<int> { i, j });
                } 
            }
        }
        if (possiblePair.Count < 2)
        {
            Debug.Assert(possiblePair.Count == 1);
            Furo(possiblePair[0]);
            return;
        }
        buttonManager.CreateGroupedImageButtons(this, possiblePair);
    }
    public void Daiminkan()
    {
        List<int> kanList = new();
        for (int i = 0; i < hand.Count; i++)
        {
            if (Library.idWithoutRed[GameManager.instance.activeTile] == Library.idWithoutRed[hand[i]])
            {
                kanList.Add(hand[i]);
            }
        }
        buttonManager.DeactivateButtonsToOtherPlayer();
        Furo(kanList);
        kanJustNow = true;
        //Tsumo();
    }
    public int TileCount()
    {
        return tileAt.Count;
    }
    public void Furo(List<int> partOfHand)
    {
        buttonManager.EraseActionPatternButtons();
        //GameManager.instance.furoNow[this.id] = true;
        List<int> furoMeld = new();
        for (int i = 0; i < partOfHand.Count; i++)
        {
            furoMeld.Add(partOfHand[i]);
        }
        if (partOfHand.Count == 2) // seq or triplet
        {
            List<int> ids = new();
            int newestId = -1;
            for (int i = 0; i < partOfHand.Count; i++)
            {
                for (int j = 0; j < hand.Count; j++)
                {
                    if (j == newestId) continue;
                    if (Library.ToInt(tileAt[j].name) == partOfHand[i])
                    {
                        ids.Add(j);
                        newestId = j;
                        break;
                    }
                }
            }
            if (partOfHand[0] == partOfHand[1])
            {
                furoTripletMeld[Library.idWithoutRed[partOfHand[0]]]++;
            }
            else
            {
                furoSeqMeld[Library.idWithoutRed[Mathf.Max(partOfHand[0], partOfHand[1], GameManager.instance.activeTile)]]++;
            }
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                Tile tileToRemove = tileAt[ids[i]];
                positions.Remove(tileAt[ids[i]].transform.position);
                tileAt.RemoveAt(ids[i]);
                Destroy(tileToRemove.gameObject);
                hand.Remove(partOfHand[i]);
            }
        }
        else if (partOfHand.Count == 3) // Kan 
        {
            List<int> ids = new();
            int newestId = -1;
            for (int i = 0; i < partOfHand.Count; i++)
            {
                for (int j = 0; j < hand.Count; j++)
                {
                    if (j == newestId) continue;
                    if (Library.ToInt(tileAt[j].name) == partOfHand[i])
                    {
                        ids.Add(j);
                        newestId = j;
                        break;
                    }
                }
            }
            kantsu[Library.idWithoutRed[partOfHand[0]]]++;
            kan[Library.idWithoutRed[partOfHand[0]]] = false;
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                Tile tileToRemove = tileAt[ids[i]];
                positions.Remove(tileAt[ids[i]].transform.position);
                tileAt.RemoveAt(ids[i]);
                Destroy(tileToRemove.gameObject);
                hand.Remove(partOfHand[i]);
            }
        }
        int id = Library.idWithoutRed[furoMeld[0]];
        if (id == Library.idWithoutRed[furoMeld[1]])
        {
            kakan[id] = furoHand.Count;
        }
        panelManager.SaveFuroTiles(this, furoMeld, GameManager.instance.activeTile, GameManager.instance.activePlayerId);
        furoCount++;
        ReloadMenzenDisplayWithoutTsumo();
        Library.CalculateShantenCount(this);
    }
    void ReloadMenzenDisplayWithoutTsumo()
    {
        if (pleaseSort)
        {
            tileAt.Sort((a, b) => (Library.ToInt(a.name)).CompareTo(Library.ToInt(b.name)));
        }
        for (int i = 0; i < hand.Count; i++)
        {
            tileAt[i].place = i;
            positions[i] = firstHandPlace + new Vector3(i, 0, 0);
            tileAt[i].MoveTo(positions[i], false);
            if (riichiTurn != -1)
            {
                tileAt[i].canTouch = false;
            }
        }
    }

// 


    // WRITTEN BELOW ARE FUNCTIONS AFTER GAMESET
    //   |  |
    //   |  |
    // __|  |__
    // \      /
    //  \    / 
    //   \  /
    //    \/

    public void ShowPoints(int winPlayerId, List<int> points, List<string> yakuNames)
    {
        panelManager.ShowPoints(winPlayerId, points, yakuNames);
    }

    public void ReloadRIver(int discardPlayerId, int tileId)
    {
        if (discardPlayerId != id)
            panelManager.ReloadRiver(discardPlayerId, tileId);
    }






    public void GameOver(List<int> points)
    {
        gameSetPanel.SetActive(true);
    }

    public void BackToEnterance()
    {
        Restart();
    }

    public void SettingSkipFuro()
    {
        pleaseSkip = !pleaseSkip;
    }

    public void SortHand()
    {
        if (pleaseSort == true)
        {
            pleaseSort = false;
            return;
        }
        if (id == GameManager.instance.activePlayerId)
        {
            Tile tempTile = tileAt[^1];
            tileAt.Remove(tempTile);
            tileAt.Sort((a, b) => (Library.ToInt(a.name)).CompareTo(Library.ToInt(b.name)));
            tileAt.Add(tempTile);
        }
        else
        {
            tileAt.Sort((a, b) => (Library.ToInt(a.name)).CompareTo(Library.ToInt(b.name)));
        }
        for (int i = 0; i < hand.Count; i++)
        {
            tileAt[i].place = i;
            positions[i] = firstHandPlace + new Vector3(i + (hand.Count == i + 1 && id == GameManager.instance.activePlayerId? 0.5f : 0), 0, 0);
            tileAt[i].MoveTo(positions[i], false);
        }
        pleaseSort = true;
    }
    public void Riichi() 
    { 
        if (turnCount == 1)
        {
            doubleriichiNow = true;
        }
        riichiTurn = turnCount;
    }
    public void DisplayMachi(int tileInt) { panelManager.DisplayMachi(this, tileInt); }
    public void ClearMachiDisplay() { panelManager.ClearMachiDisplay(); }
    public void SwapTiles(int draggedId, int direction)
    {
        int targetId = draggedId + direction;
        if (draggedId < 0 || targetId < 0 || draggedId >= hand.Count || targetId >= hand.Count) return;
        if (!tileAt[draggedId].canTouch) return;
        if (!tileAt[targetId].canTouch) return;

        if (targetId < 0 || targetId >= tileAt.Count) return;

        (tileAt[draggedId], tileAt[targetId]) = (tileAt[targetId], tileAt[draggedId]);
        (tileAt[draggedId].place, tileAt[targetId].place) = (tileAt[targetId].place, tileAt[draggedId].place);

        tileAt[draggedId].MoveTo(positions[draggedId], false);
        tileAt[targetId].MoveTo(positions[targetId], false);
    }
    public Vector3 GetPosition(int index) {
        if (index < 0 || positions.Count <= index) return Vector3.zero;
        return positions[index]; 
    }
    public void SkipToOthers()
    {
        buttonManager.DeactivateButtonsToOtherPlayer();
    }


    public void Restart() { SceneManager.LoadScene("SceneWithNewScript"); }
}
