using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager instance;

    [SerializeField] Tile[] prefabTile;
    [SerializeField] Tile tileBack;

    public GameObject panel;

    public List<Transform> assets;
    public List<Transform>[] river;
    public List<Transform> furo;
    public List<Transform> hand;
    public List<Transform> kakanTiles;

    private Vector3 nextPlace;
    private Vector3[] riverPlace;
    private Vector3 nextFuroPlace;
    private Vector3 nextDoraPlace;
    private Vector3 firstDoraPlace;

    private List<Transform> doraSet;
    private List<Transform> backSet;

    [SerializeField] private TMP_Text displayTMPText;
    [SerializeField] private GameObject button;

    public int id = -1;

    readonly Vector3[] dx = {
        new( 1, 0, 0),
        new( 0, 1, 0),
        new(-1, 0, 0),
        new( 0,-1, 0),
    };

    public void ShowPoints(PlayerManager player)
    {
        string result = "";

        result += $"       {player.maxPoints[(player.id + 2) % 4]}        \n";
        result += $"{player.maxPoints[(player.id + 3) % 4]}    {player.maxPoints[(player.id + 1) % 4]}\n";
        result += $"       {player.maxPoints[(player.id + 0) % 4]}        \n\n";

        result += $"{player.fu}•„  {player.han}–|\n";


        foreach (string name in player.yakuNames)
        {
            result += name;
            result += "\n";
        }

        displayTMPText.text = result;
        gameObject.SetActive(true);
        button.SetActive(true);
    }


    public void ShowPoints(int winPlayerId, List<int> points, List<string> yakuNames)
    {
    }

    public void StartSetting()
    {
        assets = new List<Transform>();
        river = new List<Transform>[] { new(), new(), new(), new() };
        furo = new List<Transform>();
        hand = new List<Transform>();
        doraSet = new List<Transform>();
        backSet = new List<Transform>();
        riverPlace = new Vector3[4];
        nextFuroPlace = new Vector3(13, -7);
        firstDoraPlace = new Vector3(-500, -500, 0);
        nextDoraPlace = firstDoraPlace;
    }

    public void ShowWanpai(Wall wall)
    {
        firstDoraPlace = new Vector3(-7, 7, 0);
        nextDoraPlace = firstDoraPlace;
        backSet ??= new List<Transform>();
        doraSet ??= new List<Transform>();
        for (int i = 0; i < 5; i++)
        {
            Tile newTile = Instantiate(prefabTile[wall.wall[^(1 + 2 * i)]], nextDoraPlace, Quaternion.identity);
            newTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
            newTile.canTouch = false;
            newTile.transform.localScale = Vector3.one;
            doraSet.Add(newTile.transform);
            nextDoraPlace += new Vector3(0.66f, 0, 0);
        }
        nextDoraPlace = firstDoraPlace;
        for (int i = 0; i < 5; i++)
        {
            Tile backTile = Instantiate(tileBack, nextDoraPlace, Quaternion.identity);
            backTile.canTouch = false;
            backTile.GetComponent<SpriteRenderer>().sortingOrder = 2;
            backTile.transform.localScale = Vector3.one;
            backSet.Add(backTile.transform);
            nextDoraPlace += new Vector3(0.66f, 0, 0);
        }
        FlipNewDora(wall);
    }
    public void FlipNewDora(Wall wall)
    {
        if (wall.flippedDoraCount == 5) return;
        backSet[wall.flippedDoraCount].gameObject.SetActive(false);
        wall.flippedDoraCount++;
    }

    public void ShowUra(Wall wall)
    {
        nextDoraPlace = firstDoraPlace + new Vector3(0, -1, 0);
        for (int i = 0; i < 5; i++)
        {
            if (i < wall.flippedDoraCount)
            {
                Tile newTile = Instantiate(prefabTile[wall.wall[^(2 + 2 * i)]], nextDoraPlace, Quaternion.identity);
                newTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
                newTile.canTouch = false;
                newTile.transform.localScale = Vector3.one;
                doraSet.Add(newTile.transform);
                nextDoraPlace += new Vector3(0.66f, 0, 0);
            }
            else
            {
                Tile backTile = Instantiate(tileBack, nextDoraPlace, Quaternion.identity);
                backTile.canTouch = false;
                backTile.GetComponent<SpriteRenderer>().sortingOrder = 2;
                backTile.transform.localScale = Vector3.one;
                backSet.Add(backTile.transform);
                nextDoraPlace += new Vector3(0.66f, 0, 0);
            }
        }
    }


    // CHECKED
    public void ReloadRiver(int discardPlayerId, int tileId)
    {
        int relativeId = (discardPlayerId + Rule.numberOfPlayers - id) % 4;
        Debug.Log(relativeId);
        if (riverPlace[discardPlayerId] == Vector3.zero) riverPlace[discardPlayerId] = -1.66f * dx[relativeId] - 2.5f * dx[(relativeId + 1) % 4];
        Tile newTile = Instantiate(prefabTile[tileId], riverPlace[discardPlayerId], Quaternion.Euler(0, 0, 90 * relativeId));
        newTile.canTouch = false;
        newTile.transform.localScale = Vector3.one;
        river[discardPlayerId].Add(newTile.transform);
        riverPlace[discardPlayerId] += 0.66f * dx[relativeId];
        if (river[discardPlayerId].Count % 6 == 0)
        {
            riverPlace[discardPlayerId] = -1.66f * dx[relativeId] + (-2.5f - (river[discardPlayerId].Count / 6) * 0.9f) * dx[(relativeId + 1) % 4];
        }
    }

    public void SaveKakanTile(PlayerManager player, int kakanTile)
    {
        foreach (Transform transform in kakanTiles)
        {
            if (Library.ToInt(transform.GetComponent<Tile>().name) == kakanTile)
            {
                transform.gameObject.SetActive(true);
            }
        }
    }
    public void SaveAnkanTiles(PlayerManager player, int ankanTile)
    {
        for (int i = 0; i < 4; i++)
        {
            Tile newTile = Instantiate((i == 0 || i == 3 ? tileBack : prefabTile[ankanTile]), nextFuroPlace, Quaternion.identity);
            newTile.canTouch = false;
            furo.Add(newTile.transform);
            nextFuroPlace += new Vector3(-1, 0, 0);
        }
    }

    public void SaveFuroTiles(PlayerManager player, List<int> furoMeld, int furoTile, int whoDiscard = 1)
    {
        Debug.Assert(furoMeld.Count > 0);
        if (whoDiscard == 1)
        {
            Tile newTilea = Instantiate(prefabTile[furoTile], nextFuroPlace + new Vector3(-0.175f, -0.175f, 0), Quaternion.Euler(0, 0, 90));
            newTilea.canTouch = false;
            furo.Add(newTilea.transform);
            Tile newTileb = Instantiate(prefabTile[furoTile], nextFuroPlace + new Vector3(-0.175f, -0.175f + 1, 0), Quaternion.Euler(0, 0, 90));
            newTileb.canTouch = false;
            newTileb.gameObject.SetActive(false);
            kakanTiles.Add(newTileb.transform);
            nextFuroPlace += new Vector3(-1.35f, 0, 0);
        }
        Tile newTile = Instantiate(prefabTile[furoMeld[^1]], nextFuroPlace, Quaternion.identity);
        newTile.canTouch = false;
        furo.Add(newTile.transform);
        nextFuroPlace += new Vector3(-1, 0, 0);
        if (whoDiscard == 2)
        {
            Tile newTilea = Instantiate(prefabTile[furoTile], nextFuroPlace + new Vector3(-0.175f, -0.175f, 0), Quaternion.Euler(0, 0, 90));
            newTilea.canTouch = false;
            furo.Add(newTilea.transform);
            Tile newTileb = Instantiate(prefabTile[furoTile], nextFuroPlace + new Vector3(-0.175f, -0.175f + 1, 0), Quaternion.Euler(0, 0, 90));
            newTileb.canTouch = false;
            newTileb.gameObject.SetActive(false);
            kakanTiles.Add(newTileb.transform);
            nextFuroPlace += new Vector3(-1.35f, 0, 0);
        }
        for (int i = furoMeld.Count - 2; i >= 0; i--)
        {
            Tile newTileS = Instantiate(prefabTile[furoMeld[i]], nextFuroPlace, Quaternion.identity);
            newTileS.canTouch = false;
            furo.Add(newTileS.transform);
            nextFuroPlace += new Vector3(-1, 0, 0);
        }
        if (whoDiscard == 3)
        {
            Tile newTilea = Instantiate(prefabTile[furoTile], nextFuroPlace + new Vector3(-0.175f, -0.175f, 0), Quaternion.Euler(0, 0, 90));
            newTilea.canTouch = false;
            furo.Add(newTilea.transform);
            Tile newTileb = Instantiate(prefabTile[furoTile], nextFuroPlace + new Vector3(-0.175f, -0.175f + 1, 0), Quaternion.Euler(0, 0, 90));
            newTileb.canTouch = false;
            newTileb.gameObject.SetActive(false);
            kakanTiles.Add(newTileb.transform);
            nextFuroPlace += new Vector3(-1.35f, 0, 0);
        }
    }

    public void DisplayMachi(PlayerManager player, int tileInt) { SaveTilesInPanel(player, tileInt); }
    private void SaveTilesInPanel(PlayerManager player, int tileInt)
    {
        nextPlace = new Vector3(-10, -2, 0);
        for (int i = 0; i < 37; i++)
        {
            if (player.machiTiles[Library.idWithoutRed[tileInt], i])
            {
                SaveTileInPanel(prefabTile[i]);
            }
        }
    }
    private void SaveTileInPanel(Tile tile)
    {
        Tile newTile = Instantiate(tile, nextPlace, Quaternion.identity);
        newTile.canTouch = false;
        newTile.transform.localScale = Vector3.one;
        assets.Add(newTile.transform);
        nextPlace += new Vector3(0.66f, 0, 0);
    }
    public void ClearMachiDisplay() { ClearTilesInPanel(); }
    private void ClearTilesInPanel()
    {
        foreach (Transform tile in assets)
        {
            Destroy(tile.gameObject);
        }
        assets.Clear();
    }
}
