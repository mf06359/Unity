using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [SerializeField] Wall wall;
    
    [SerializeField] PlayerManager[] playerManager;

    public List<int> points;

    public int numberOfPlayers;
    public int turnCount = 0;
    public int placeWind = 0;
    public int honbaCount = 0;
    public int parentId = 0;
    public int activePlayerId = 0;
    public int activeTile = 0;
    public bool[] furoNow;
    int skipFuroCount = 0;

    //[SerializeField] private GameObject titlePanel;
    //[SerializeField] private GameObject startingPanel;
    //[SerializeField] private GameObject displayPointPanel;
    //[SerializeField] private GameObject gameSetPanel;


    private void Awake()
    {
        instance = this;
        furoNow = new bool[4];
    }

    private void Start()
    {
        numberOfPlayers = Rule.numberOfPlayers;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            points.Add(Rule.startPoint);
            playerManager[i].isComputer = (i != 0);
            playerManager[i].id = i;
        }
        DealHandToPlayers();
        NextTurn(true);
    }

    public void DealHandToPlayers()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            playerManager[i].DealHand();
            if (!playerManager[i].isComputer)
            {
                playerManager[i].DisplayFirstHand();
                playerManager[i].panelManager.ShowWanpai(wall);
                playerManager[i].stateManager.UpdateText(playerManager[i].Player());
            }
        }
    }

    public void CountWaiting()
    {
        skipFuroCount++;
        if (skipFuroCount == numberOfPlayers - 1)
        {
            skipFuroCount = 0;
            NextTurn();
        }
    }

    public Player ActivePlayer()
    {
        return playerManager[activePlayerId].Player();
    }

    public PlayerManager Player(int id)
    {
        return playerManager[id];
    }

    public void NextTurn(bool isFirstTurn = false)
    {
        if (isFirstTurn) activePlayerId = 0;
        else activePlayerId = (activePlayerId + 1) % numberOfPlayers;
        playerManager[activePlayerId].TurnStart();
    }

    public bool NoOneReactToDiscardedTile()
    {
        skipFuroCount = 0;
        Debug.Log($"ReactToDiscardedTile in GM Called");
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (i != activePlayerId)
            {
                skipFuroCount += playerManager[i].buttonManager.ReactToTileOtherPlayerDiscarded(playerManager[i].Player());
            }
        }
        return skipFuroCount == numberOfPlayers - 1;
    }
    public void Restart() { SceneManager.LoadScene("SceneWithNewScript"); }

}
