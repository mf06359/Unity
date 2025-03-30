using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Tile[] prefabTile;
    [SerializeField] private Player player;

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

    private void Awake()
    { 
        riverTail = new Vector3(6, 5, 0);
        firstRiverTail = new(-1.98f, -1, 0);
        firstHandPlace = new Vector3(-7, -7, 0);
        river = new int[37];
        for (int i = 0; i < 37; i++)
        {
            river[i] = 0;
        }
        riverTail = firstRiverTail;
    }
    public void TurnStart()
    {
        player.turnCount++;
        Debug.Log($"playerid {id}");
        Debug.Log($"playerisComputer {isComputer}");
        if (isComputer)
        {
            int tileId = wall.NewTsumo();
            GameManager.instance.activeTile = tileId;
            panelManager.ReloadRiver(tileId);
            if (GameManager.instance.NoOneReactToDiscardedTile())
            {
                GameManager.instance.NextTurn();
            }
        }
        else
        {
            Tsumo();
        }
    }

    public void Tsumo()
    {
        int tsumo = wall.NewTsumo();
        GameManager.instance.activeTile = tsumo;
        if (Library.idWithoutRed[tsumo] != tsumo)
        {
            player.redDoraCount++;
        }
        player.hand.Add(tsumo);
        player.latestTile = tsumo;
        Tile newTile = Instantiate(prefabTile[tsumo], firstHandPlace + new Vector3(player.hand.Count, 0, 0), Quaternion.identity);
        newTile.playerid = id;
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
        if (id == GameManager.instance.activePlayerId)
        {
            int cnt = 0;
            if (player.machiTile[Library.idWithoutRed[tsumo]]) cnt++;
            if (player.shanten == 0 && player.riichiTurn == -1 && player.furoCount == 0) cnt++;
            if (player.canAnkan || canKakan) cnt++;
            if (cnt > 0)
            {
                buttonManager.ResetButtonPlace();
                buttonManager.SkipToMeOn();
                if (player.machiTile[Library.idWithoutRed[tsumo]]) buttonManager.TsumoOn();
                if (player.shanten == 0 && player.riichiTurn == -1 && player.furoCount == 0) buttonManager.RiichiOn();
                if (canKakan) buttonManager.KakanOn();
                if (player.canAnkan) buttonManager.AnkanOn();
            }
        }
    }
    public void DiscardTile(Tile tile)
    {
        if (Library.idWithoutRed[Library.ToInt(tile.name)] != Library.ToInt(tile.name))
        {
            player.redDoraCount--;
        }
        positions.Remove(tile.transform.position);
        player.hand.Remove(Library.ToInt(tile.name));
        tileAt.Remove(tile);
        river[Library.ToInt(tile.name)]++;
        for (int i = 0; i < 37; i++)
        {
            player.machiTile[i] = player.machiTiles[Library.idWithoutRed[Library.ToInt(tile.name)], i];
            player.penchanKanchanTanki[i] = player.penchanKanchanTankiIf[Library.idWithoutRed[Library.ToInt(tile.name)], i];
            player.ryanmen[i] = player.ryanmenIf[Library.idWithoutRed[Library.ToInt(tile.name)], i];
        }
        ReloadMenzenDisplayWithoutTsumo();
        if (player.turnCount > turnLimit) GameManager.instance.Restart();
        TurnEnd();
    }
    public void TurnEnd()
    {
        buttonManager.TsumoOff();
        buttonManager.RiichiOff();
        buttonManager.DeactivateButtonsToMe();
        Library.ReloadFuroMachi(player);
        GameManager.instance.furoNow[id] = false;
        player.kanJustNow = false;
        if (GameManager.instance.NoOneReactToDiscardedTile()) GameManager.instance.NextTurn();
    }

    public void TsumoHo()
    {
        player.hola = GameManager.instance.activeTile;
        dora = wall.Dora();
        uraDora = wall.UraDora();
        Library.CalculatePoints(player);
        panelManager.ShowPoints(player);
        panelManager.ShowUra(wall);
    }
    public void RonHo()
    {
        player.hola = GameManager.instance.activeTile;
        player.hand.Add(GameManager.instance.activeTile);
        player.hand.Sort();
        dora = wall.Dora();
        uraDora = wall.UraDora();
        Library.CalculatePoints(player, GameManager.instance.activePlayerId);
        panelManager.ShowPoints(player);
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
                if (player.chiPair[GameManager.instance.activeTile, i, j])
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
                if (player.ponPair[GameManager.instance.activeTile, i, j])
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
            if (Library.idWithoutRed[GameManager.instance.activeTile] == Library.idWithoutRed[player.hand[i]])
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
        if (0 < kanList.Count && kanList.Count < 2) // ANKAN
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
            player.ankantsu[Library.idWithoutRed[kanList[0][0]]]++;
            ReloadMenzenDisplayWithoutTsumo();
            GameManager.instance.furoNow[id] = true;
            //player.furoNow = true;
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
        if (kanList.Count < 2) // KAKAN
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
            player.furoTripletMeld[kanList[0][0]]--;
            player.kantsu[kanList[0][0]]++;
            Debug.Assert(kanList.Count == 1);
            GameManager.instance.furoNow[id] = true;
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
        panelManager.FlipNewDora(wall);
        buttonManager.DeactivateButtonsToMe();
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
        GameManager.instance.furoNow[id] = true;
        ReloadMenzenDisplayWithoutTsumo();
        player.kanJustNow = true;
        buttonManager.DeactivateButtonsToMe();
        Tsumo();
    }
    public int TileCount()
    {
        return tileAt.Count;
    }
    public void Furo(List<int> partOfHand)
    {
        buttonManager.EraseActionPatternButtons();
        GameManager.instance.furoNow[this.id] = true;
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
                player.furoSeqMeld[Library.idWithoutRed[Mathf.Max(partOfHand[0], partOfHand[1], GameManager.instance.activeTile)]]++;
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
            player.kan[Library.idWithoutRed[partOfHand[0]]] = false;
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
        panelManager.SaveFuroTiles(player, furoMeld, GameManager.instance.activeTile, GameManager.instance.activePlayerId);
        player.furoCount++;
        ReloadMenzenDisplayWithoutTsumo();
        Library.CalculateShantenCount(player);
    }
    public void DisplayFirstHand()
    {
        tileAt = new();
        positions = new();
        for (int i = 0; i < 13; i++)
        {
            Tile newTile = Instantiate(prefabTile[player.hand[i]], firstHandPlace + new Vector3(i, 0, 0), Quaternion.identity);
            newTile.playerid = id;
            newTile.place = i;
            newTile.canTouch = true;
            tileAt.Add(newTile);
            positions.Add(newTile.transform.position);
        }
    }
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
            if (player.riichiTurn != -1)
            {
                tileAt[i].canTouch = false;
            }
        }
    }




    // WRITTEN BELOW IS CALLED IN THE ANOTHER FUNCTION
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
        for (int i = 0; i < player.hand.Count; i++)
        {
            tileAt[i].place = i;
            positions[i] = firstHandPlace + new Vector3(i + (player.hand.Count == i + 1 && id == GameManager.instance.activePlayerId? 0.5f : 0), 0, 0);
            tileAt[i].MoveTo(positions[i], false);
        }
        pleaseSort = true;
    }
    public void Riichi() 
    { 
        if (player.turnCount == 1)
        {
            player.doubleriichiNow = true;
        }
        player.riichiTurn = player.turnCount;
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
    public Vector3 GetPosition(int index) {
        if (index < 0 || positions.Count <= index)
            //return positions[^1];
            return Vector3.zero;
        return positions[index]; 
    }
    public void SkipToOthers()
    {
        buttonManager.DeactivateButtonsToOtherPlayer();
        GameManager.instance.CountWaiting();
    }
    public Player Player()
    {
        return player;
    }
}
