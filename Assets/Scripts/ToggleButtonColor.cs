using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonColor : MonoBehaviour
{
    public Button targetButton;
    private Image buttonImage;

    private Color originalColor = Color.green;
    private Color toggledColor = Color.red; // âüÇ≥ÇÍÇΩÇ∆Ç´ÇÃêF

    private bool isToggled = false;

    void Start()
    {
        buttonImage = targetButton.GetComponent<Image>();
        buttonImage.color = originalColor;
        targetButton.onClick.AddListener(ToggleColor);
    }

    void ToggleColor()
    {
        isToggled = !isToggled;

        if (isToggled)
        {
            buttonImage.color = toggledColor;
        }
        else
        {
            buttonImage.color = originalColor;
        }
    }
}
