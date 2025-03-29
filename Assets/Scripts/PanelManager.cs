using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private Panel playerPanel;
    [SerializeField] private Panel riverPanel;
    [SerializeField] private Panel machiPanel;
    [SerializeField] private DisplayPointPanel displayPointPanel;


    [SerializeField] Tile[] prefabTile;
    [SerializeField] Tile tileBack;
    public List<Transform> assets;

    public Vector3 nextPlace;
    public Vector3 riverPlace;
    public Vector3 nextFuroPlace;


    public void ShowPoints(Player player)
    {
        displayPointPanel.ShowListWithNumbers(player);
    }


    public void ShowWanpai(Wall wall)
    {
        riverPanel.ShowWanpai(wall);
    }

    public void ShowUra(Wall wall)
    {
        riverPanel.ShowUra(wall);
    }

    public void FlipNewDora(Wall wall)
    {
        riverPanel.FlipNewDora(wall);
    }

    public void ReloadRiver(int tileId) { riverPanel.ReloadRiver(tileId); }

    public void SaveKakanTile(Player player, int kakanTileId) { playerPanel.ShowKakanTile(player, kakanTileId); }
    public void SaveAnkanTiles(Player player, int furoMeldTile) { playerPanel.SaveAnkanTiles(player, furoMeldTile); }
    public void SaveFuroTiles(Player player, List<int> furoMeld, int furoTile, int whoDiscarded) { playerPanel.SaveFuroTiles(player, furoMeld, furoTile, whoDiscarded);}
    public void DisplayMachi(Player player, int tileInt) { machiPanel.ShowMachiTiles(player, tileInt); }
    public void ClearMachiDisplay() { machiPanel.ClearMachiDisplay(); }
}
