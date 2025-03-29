using UnityEngine;
using TMPro;

public class StateManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    private int shanten;
    private void Awake()
    {
        shanten = 14;
    }
    public void UpdateText(Player player)
    {
        shanten = player.shanten;
        text.text = "Shanten Count : " + shanten;
    }
}
