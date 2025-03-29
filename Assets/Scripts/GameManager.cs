using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    Wall wall;
    PlayerManager[] playerManager;

    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject startingPanel;
    [SerializeField] private GameObject displayPointPanel;
    [SerializeField] private GameObject gameSetPanel;

    private void Start()
    {
        instance = this;
    }

    public void DealHandToPlayers()
    {
        for (int i = 0; i < playerManager.Length; i++)
        {
            playerManager[i].DealHand();
        }
    }
}
