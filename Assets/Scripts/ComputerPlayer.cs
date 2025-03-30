using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ComputerPlayer : PlayerManager
{
    //public static ComputerPlayer instance;

    //[SerializeField] private Tile[] prefabTile;
    [SerializeField] private Player player;
    //public SoundPlayer audioManager;
    //public ButtonManager buttonManager;
    //public PanelManager panelManager;
    //public StateManager stateManager;
    //public Wall wall;

    //public Vector3 firstHandPlace;
    private List<Tile> tileAt;
    private List<Vector3> positions;
    //public Vector3 firstRiverTail, riverTail;
    //private const int turnLimit = 55;
    //public int riverCount = 0;
    //public bool pleaseSort = true;
    //public int id = 0;
    //public bool activeTileKanned = false;
    //public bool isComputer = true;

    //public List<int> dora;
    //public List<int> uraDora;

    private void Awake()
    {
        //instance = this;
        tileAt = new List<Tile>();
        positions = new List<Vector3>();
        riverTail = new Vector3(6, 5, 0);
        firstRiverTail = new(-1.98f, -1, 0);
        firstHandPlace = new Vector3(-7, -7, 0);
        riverTail = firstRiverTail;
        id = 0;
    }
    private void Start()
    {
        DisplayFirstHand();
        //stateManager.UpdateText(player);

        //panelManager.ShowWanpai(wall);
        if (GameManager.instance.parentId == id) TurnStart();
    }
    public new void TurnStart()
    {
        player.turnCount++;
        Tsumo();
    }
    public new void Tsumo()
    {
        int tsumo = wall.NewTsumo();
        GameManager.instance.activeTile = tsumo;
        if (Library.idWithoutRed[tsumo] != tsumo)
        {
            player.redDoraCount++;
        }
        player.hand.Add(tsumo);
        player.latestTile = tsumo;

        Library.CalculateShantenCount(player);
        for (int i = 0; i < player.hand.Count; i++)
        {
            if (player.shanten == player.shantenIf[i])
            {
                panelManager.ReloadRiver(player.hand[i]);
                player.hand.RemoveAt(i);
            }
        }
        GameManager.instance.NoOneReactToDiscardedTile();
    }

}
