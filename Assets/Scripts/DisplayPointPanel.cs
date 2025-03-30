using System.Collections.Generic;
using UnityEngine;
using TMPro; // �� TextMeshPro �p

public class DisplayPointPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text displayTMPText; // TextMeshPro�p�̎Q��
    [SerializeField] private GameObject button;

    public void ShowPoints(Player player)
    {
        string result = "";

        result += $"       {player.maxPoints[2]}        \n";
        result += $"{player.maxPoints[3]}    {player.maxPoints[1]}\n";
        result += $"       {player.maxPoints[0]}        \n\n";

        result += $"{player.fu}��  {player.han}�|\n";


        foreach (string name in player.yakuNames)
        {
            result += name;
            result += "\n";
        }

        displayTMPText.text = result;
        gameObject.SetActive(true);
        button.SetActive(true);
    }
}
