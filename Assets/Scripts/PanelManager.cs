using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private GameObject riverPanel;
    [SerializeField] private GameObject machiPanel;
    [SerializeField] private DisplayPointPanel displayPointPanel;


    [SerializeField] Tile[] prefabTile;
    [SerializeField] Tile tileBack;
    public List<Transform> assets;
    public List<Transform> river;
    public List<Transform> furo;
    public List<Transform> hand;
    public List<Transform> kakanTiles;

    private Vector3 nextPlace;
    private Vector3 riverPlace;
    private Vector3 nextFuroPlace;
    private Vector3 nextDoraPlace;
    private Vector3 firstDoraPlace;

    private List<Transform> doraSet;
    private List<Transform> backSet;


    public void ShowPoints(Player player)
    {
        displayPointPanel.ShowPoints(player);
    }

    private void Awake()
    {
        assets = new List<Transform>();
        river = new List<Transform>();
        furo = new List<Transform>();
        hand = new List<Transform>();
        doraSet = new List<Transform>();
        backSet = new List<Transform>();
        riverPlace = new Vector3(3, 7, 0);
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

    public void ReloadRiver(int tileId)
    {
        if (riverPlace == Vector3.zero) riverPlace = new Vector3(3, 7, 0);
        Tile newTile = Instantiate(prefabTile[tileId], riverPlace, Quaternion.identity);
        newTile.canTouch = false;
        newTile.transform.localScale = Vector3.one;
        river.Add(newTile.transform);
        riverPlace += new Vector3(0.66f, 0, 0);
        if (riverPlace.x >= 9.5f)
        {
            riverPlace += new Vector3(-6.6f, -0.9f, 0);
        }
    }

    public void SaveKakanTile(Player player, int kakanTile)
    {
        foreach (Transform transform in kakanTiles)
        {
            if (Library.ToInt(transform.GetComponent<Tile>().name) == kakanTile)
            {
                transform.gameObject.SetActive(true);
            }
        }
    }
    public void SaveAnkanTiles(Player player, int ankanTile)
    {
        for (int i = 0; i < 4; i++)
        {
            Tile newTile = Instantiate((i == 0 || i == 3 ? tileBack : prefabTile[ankanTile]), nextFuroPlace, Quaternion.identity);
            newTile.canTouch = false;
            furo.Add(newTile.transform);
            nextFuroPlace += new Vector3(-1, 0, 0);
        }
    }

    public void SaveFuroTiles(Player player, List<int> furoMeld, int furoTile, int whoDiscard = 1)
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

    public void DisplayMachi(Player player, int tileInt) { SaveTilesInPanel(player, tileInt); }
    private void SaveTilesInPanel(Player player, int tileInt)
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
