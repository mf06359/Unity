using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [SerializeField] private Tile[] prefabTile;
    [SerializeField] private Player player;
    public SoundPlayer audioManager;
    public ButtonManager buttonManager;
    public PanelManager panelManager;
    public StateManager stateManager;
    public Wall wall;

    public Vector3 firstHandPlace;
    private List<Tile> tileAt;
    private List<Vector3> positions;
    public Vector3 firstRiverTail, riverTail;
    private const int turnLimit = 55;
    private int turns = 0;
    public int riverCount = 0, activeTile = -1, activeTilePlayer = 1, tileCount = 0;
    public bool isParent = true, pleaseSort = true;
    public int riichiedJustNow = 0;
    public int placeWind = 0;
    public bool activeTileKanned = false;
    public int parentId = 0;

    public List<int> dora;
    public List<int> uraDora;

    private void Awake() { instance = this; }
    private void Start()
    {

        tileAt = new List<Tile>();
        positions = new List<Vector3>();
        riverTail = new Vector3(6, 5, 0);
        firstRiverTail = new(-1.98f, -1, 0);
        firstHandPlace = new Vector3(-7, -7, 0);
        riverTail = firstRiverTail;
        DealHand();
        DisplayFirstHand();
        stateManager.UpdateText(player);

        panelManager.ShowWanpai(wall);
        if (isParent == true) TurnStart();
    }

    public void TsumoHo()
    {
        player.hola = activeTile;
        dora = wall.Dora();
        uraDora = wall.UraDora();
        Library.CalculatePoints(player);
        panelManager.ShowPoints(player);
        panelManager.ShowUra(wall);
    }
    public void RonHo()
    {
        player.hola = activeTile;
        player.hand.Add(activeTile);
        player.hand.Sort();
        dora = wall.Dora();
        uraDora = wall.UraDora();
        Library.CalculatePoints(player, activeTilePlayer);
        panelManager.ShowPoints(player);
        panelManager.ShowUra(wall);
    }
    public void OtherPlayerTsumo()
    {
        int tileId = wall.NewTsumo();
        activeTile = tileId;
        activeTilePlayer = 1;
        panelManager.ReloadRiver(tileId);
        buttonManager.ReactToTileOtherPlayerDiscarded(player, tileId);
    }
    public void Chi()
    {
        buttonManager.DeactivateButtonsToOtherPlayer();
        List<List<int>> possiblePair = new();
        for (int i = 0; i < 37; i++)
        {
            for (int j = i + 1; j < 37; j++)
            {
                if (player.chiPair[activeTile, i, j])
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
        buttonManager.CreateGroupedImageButtons(player, possiblePair);
    }
    public void Pon()
    {
        buttonManager.DeactivateButtonsToOtherPlayer();
        List<List<int>> possiblePair = new();
        for (int i = 0; i < 37; i++)
        {
            for (int j = i; j < 37; j++)
            {
                if (player.ponPair[activeTile, i, j])
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
        buttonManager.CreateGroupedImageButtons(player, possiblePair);
    }
    public void Daiminkan()
    {
        List<int> kanList = new();
        for (int i = 0; i < player.hand.Count; i++)
        {
            if (Library.idWithoutRed[activeTile] == Library.idWithoutRed[player.hand[i]])
            {
                kanList.Add(player.hand[i]);
            }
        }
        buttonManager.DeactivateButtonsToOtherPlayer();
        Furo(kanList);
        player.kanJustNow = true;
        Tsumo();
    }

    public void KanInMyTurn()
    {
        List<int[]> kanList = new();
        for (int i = 0; i + 3 < player.hand.Count; i++)
        {
            if (Library.idWithoutRed[player.hand[i]] == Library.idWithoutRed[player.hand[i + 1]]
            && Library.idWithoutRed[player.hand[i + 1]] == Library.idWithoutRed[player.hand[i + 2]]
            && Library.idWithoutRed[player.hand[i + 2]] == Library.idWithoutRed[player.hand[i + 3]])
            {
                kanList.Add(new int[2] { Library.idWithoutRed[player.hand[i]], 0 } );
            }
        }
        if (0 < kanList.Count && kanList.Count < 2)
        {
            for (int i = player.hand.Count - 1; i >= 0; i--)
            {
                if (Library.idWithoutRed[player.hand[i]] == kanList[0][0])
                {
                    player.hand.RemoveAt(i);
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
            panelManager.SaveAnkanTiles(player, kanList[0][0]);
            ReloadMenzenDisplayWithoutTsumo();
            player.furoNow = true;
            Tsumo();
            return;
        }
        for (int i = 0; i < 37; i++)
        {
            if (player.kakan[i] != -1 && player.hand.IndexOf(i) != -1)
            {
                kanList.Add(new int[2] { Library.idWithoutRed[i], player.kakan[i] });
            }
        }
        if (kanList.Count < 2)
        {
            Debug.Assert(kanList.Count == 1);
            for (int i = player.hand.Count - 1; i >= 0; i--)
            {
                if (Library.idWithoutRed[player.hand[i]] == kanList[0][0])
                {
                    player.hand.RemoveAt(i);
                }
                if (Library.idWithoutRed[Library.ToInt(tileAt[i].name)] == kanList[0][0])
                {
                    Tile tempTile = tileAt[i];
                    positions.Remove(tempTile.transform.position);
                    tileAt.RemoveAt(i);
                    Destroy(tempTile.gameObject);
                }
            }
            panelManager.SaveKakanTile(player, kanList[0][0]);
            ReloadMenzenDisplayWithoutTsumo();
            Debug.Assert(kanList.Count == 1);
            player.furoNow = true;
            Tsumo();
            return;
        }
        buttonManager.CreateKanButtons(player, kanList);
    }
    public void Ankan(Player player, int ankanTile)
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
        Tsumo();
    }
    public void Kakan(Player player, int kakanTile)
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
        player.furoNow = true;
        ReloadMenzenDisplayWithoutTsumo();
        player.kanJustNow = true;
        Tsumo();
    }
    public int TileCount()
    {
        return tileAt.Count;
    }
    public bool ItsMyTurn()
    {
        return player.myTurn || player.furoNow;
    }
    // id with Red id
    // delete FuroTiles and reload Display
    public void Furo(List<int> partOfHand)
    {
        buttonManager.EraseActionPatternButtons();
        player.furoNow = true;
        List<int> furoMeld = new();
        for (int i = 0; i < partOfHand.Count; i++)
        {
            furoMeld.Add(partOfHand[i]);
        }
        if (partOfHand.Count == 2) // seq or triplet
        {
            List<int> ids = new List<int>();
            int newestId = -1;
            for (int i = 0; i < partOfHand.Count; i++)
            {
                for (int j = 0; j < player.hand.Count; j++)
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
                player.furoTripletMeld[Library.idWithoutRed[partOfHand[0]]]++;
            }
            else
            {
                player.furoSeqMeld[Mathf.Max(partOfHand[0], partOfHand[1], activeTile)]++;
            }
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                Tile tileToRemove = tileAt[ids[i]];
                positions.Remove(tileAt[ids[i]].transform.position);
                tileAt.RemoveAt(ids[i]);
                Destroy(tileToRemove.gameObject);
                player.hand.Remove(partOfHand[i]);
            }
        }
        else if (partOfHand.Count == 3) // Kan 
        {
            List<int> ids = new();
            int newestId = -1;
            for (int i = 0; i < partOfHand.Count; i++)
            {
                for (int j = 0; j < player.hand.Count; j++)
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
            player.kantsu[Library.idWithoutRed[partOfHand[0]]]++;
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                Tile tileToRemove = tileAt[ids[i]];
                positions.Remove(tileAt[ids[i]].transform.position);
                tileAt.RemoveAt(ids[i]);
                Destroy(tileToRemove.gameObject);
                player.hand.Remove(partOfHand[i]);
            }
        }
        int id = Library.idWithoutRed[furoMeld[0]];
        if (id == Library.idWithoutRed[furoMeld[1]])
        {
            player.kakan[id] = player.furoHand.Count;
        }
        ReloadFuroDisplayWithoutTsumo(furoMeld, activeTile);
        player.furoCount++;
        ReloadMenzenDisplayWithoutTsumo();
        Library.CalculateShantenCount(player);
    }
    public void SkipToMe()
    {
        buttonManager.DeactivateButtonsToMe();
    }
    public void SkipToOthers()
    {
        buttonManager.DeactivateButtonsToOtherPlayer();
        TurnStart();
    }
    public void DealHand()
    {
        for (int i = 0; i < 13; i++)
        {
            player.hand.Add(wall.NewTsumo());
            if (Library.idWithoutRed[player.hand[^1]] != player.hand[^1])
            {
                player.redDoraCount++;
            }
        }
        player.hand.Sort();
    }
    public void DisplayFirstHand()
    {
        for (int i = 0; i < 13; i++)
        {
            Tile newTile = Instantiate(prefabTile[player.hand[i]], firstHandPlace + new Vector3(i, 0, 0), Quaternion.identity);
            newTile.place = i;
            newTile.canTouch = true;
            tileAt.Add(newTile);
            positions.Add(newTile.transform.position);
        }
    }
    public void TurnStart()
    {
        turns++;
        player.myTurn = true;
        Tsumo();
    }
    public void Tsumo()
    {
        int tsumo = wall.NewTsumo();
        activeTile = tsumo;
        activeTilePlayer = 0;
        if (Library.idWithoutRed[tsumo] != tsumo)
        {
            player.redDoraCount++;
            Debug.Log($"turn {turns} : TSUMO RED DORA NOW :  red dora count => {player.redDoraCount}");
        }
        player.hand.Add(tsumo);
        player.tsumo = tsumo;
        Tile newTile = Instantiate(prefabTile[tsumo], firstHandPlace + new Vector3(player.hand.Count, 0, 0), Quaternion.identity);
        newTile.place = player.hand.Count;
        newTile.canTouch = true;
        tileAt.Add(newTile);
        positions.Add(newTile.transform.position);

        Library.CalculateShantenCount(player);
        stateManager.UpdateText(player);
        bool canKakan = false;
        foreach (var tile in player.hand)
        {
            if (player.kakan[Library.idWithoutRed[tile]] != -1)
            {
                canKakan = true;
            }
        }
        if (player.myTurn)
        {
            int cnt = 0;
            if (player.machiTile[Library.idWithoutRed[tsumo]]) cnt++;
            if (player.shanten == 0) cnt++;
            if (player.ankan || canKakan) cnt++;
            if (cnt == 0) return;
            buttonManager.ResetButtonPlace();
            buttonManager.SkipToMeOn();
            if (player.machiTile[Library.idWithoutRed[tsumo]]) buttonManager.TsumoOn();
            if (player.shanten == 0) buttonManager.RiichiOn();
            if (canKakan) buttonManager.KakanOn();
            if (player.ankan) buttonManager.AnkanOn();
        }
    }
    public void DiscardTile(Tile tile)
    {
        Debug.Log($"DiscardTile => tileAt.Count : {tileAt.Count}");
        Debug.Log($"DiscardTile => player.hand.Count : {player.hand.Count}");
        if (Library.idWithoutRed[Library.ToInt(tile.name)] != Library.ToInt(tile.name))
        {
            player.redDoraCount--;
        }
        positions.Remove(tile.transform.position);
        player.hand.Remove(Library.ToInt(tile.name));
        tileAt.Remove(tile);
        for (int i = 0; i < 37; i++)
        {
            player.machiTile[i] = player.machiTiles[Library.idWithoutRed[Library.ToInt(tile.name)], i];
            player.penchanKanchanTanki[i] = player.penchanKanchanTankiIf[Library.idWithoutRed[Library.ToInt(tile.name)], i];
        }
        ReloadMenzenDisplayWithoutTsumo();
        if (turns > turnLimit) Restart();
        TurnEnd();
        OtherPlayerTsumo();
    }
    void ReloadFuroDisplayWithoutTsumo(List<int> handId, int furoTile) { panelManager.SaveFuroTiles(player, handId, furoTile, activeTilePlayer); }
    void ReloadMenzenDisplayWithoutTsumo()
    {
        if (pleaseSort)
        {
            tileAt.Sort((a, b) => (Library.ToInt(a.name)).CompareTo(Library.ToInt(b.name)));
        }
        for (int i = 0; i < player.hand.Count; i++)
        {
            tileAt[i].place = i;
            positions[i] = firstHandPlace + new Vector3(i, 0, 0);
            tileAt[i].MoveTo(positions[i], false);
        }
    }
    public void TurnEnd()
    {
        buttonManager.TsumoOff();
        buttonManager.RiichiOff();
        buttonManager.DeactivateButtonsToMe();
        Library.ReloadFuroMachi(player);
        player.myTurn = false;
        player.furoNow = false;
        player.kanJustNow = false;
    }
    public void SortHand()
    {
        if (pleaseSort == true)
        {
            pleaseSort = false;
            return;
        }
        if (player.myTurn)
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
        for (int i = 0; i < player.hand.Count; i++)
        {
            tileAt[i].place = i;
            positions[i] = firstHandPlace + new Vector3(i + (player.hand.Count == i + 1 && player.myTurn? 0.5f : 0), 0, 0);
            tileAt[i].MoveTo(positions[i], false);
        }
        pleaseSort = true;
    }
    public void DisplayMachi(int tileInt) { panelManager.DisplayMachi(player, tileInt); }
    public void ClearMachiDisplay() { panelManager.ClearMachiDisplay(); }
    public void SwapTiles(int draggedId, int direction)
    {
        int targetId = draggedId + direction;
        if (draggedId < 0 || targetId < 0 || draggedId >= player.hand.Count || targetId >= player.hand.Count) return;
        if (!tileAt[draggedId].canTouch) return;
        if (!tileAt[targetId].canTouch) return;

        if (targetId < 0 || targetId >= tileAt.Count) return;

        (tileAt[draggedId], tileAt[targetId]) = (tileAt[targetId], tileAt[draggedId]);
        (tileAt[draggedId].place, tileAt[targetId].place) = (tileAt[targetId].place, tileAt[draggedId].place);

        tileAt[draggedId].MoveTo(positions[draggedId], false);
        tileAt[targetId].MoveTo(positions[targetId], false);
    }
    public void Riichi() 
    { 
        if (turns == 1)
        {
            player.doubleriichiNow = true;
        }
        player.riichiNow = true;
        riichiedJustNow = 2;
    }
    public Vector3 GetPosition(int index) {
        if (index < 0 || positions.Count <= index) return positions[^1];
        return positions[index]; 
    }
    public void Restart() { SceneManager.LoadScene("SceneWithNewScript"); }
}
