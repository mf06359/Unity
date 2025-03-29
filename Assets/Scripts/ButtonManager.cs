using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject tsumoButton;
    [SerializeField] private GameObject riichiButton;
    [SerializeField] private GameObject ankanButton;
    [SerializeField] private GameObject kakanButton;
    [SerializeField] private GameObject skipToMeButton;

    [SerializeField] private GameObject ponButton;
    [SerializeField] private GameObject chiButton;
    [SerializeField] private GameObject kanButton;
    [SerializeField] private GameObject ronButton;
    [SerializeField] private GameObject skipToOthersButton;

    public Panel buttonPanel;
    public List<GameObject> buttonList = new();

    [SerializeField] float imageSize = 50f;
    [SerializeField] float imageSpacing = 5f;
    [SerializeField] float buttonHeight = 60f;

    private Vector3 canvasPlace, firstButtonPlace, nextButtonPlace, buttonDistance;


    //private void Awake()
    //{
    //    tsumoButton.transform.parent = buttonPanel.transform;
    //    riichiButton.transform.parent = buttonPanel.transform;
    //    ponButton.transform.parent = buttonPanel.transform;
    //    chiButton.transform.parent = buttonPanel.transform;
    //    kanButton.transform.parent = buttonPanel.transform;
    //    ronButton.transform.parent = buttonPanel.transform;
    //    skipButton.transform.parent = buttonPanel.transform;
    //}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasPlace = new(960, 540, 0);
        firstButtonPlace = new(500, -350, 0);
        buttonDistance = new(-200, 0, 0);
    }

    public void EraseActionPatternButtons()
    {
        foreach (GameObject button in buttonList)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
    }

    public void ResetButtonPlace()
    {
        nextButtonPlace = firstButtonPlace;
    }

    public void CreateGroupedImageButtons(Player player, List<List<int>> tileNumberGroups)
    {
        Debug.Log("CreatedGroupedImageButtons called");
        Debug.Log($"imageGroupsCount : {tileNumberGroups.Count}");

        for (int groupIndex = tileNumberGroups.Count - 1; groupIndex >= 0; groupIndex--)
        {
            List<int> tileNumberGroup = tileNumberGroups[groupIndex];

            // ボタンの GameObject（グループ用）
            GameObject buttonGO = new GameObject($"GroupButton_{groupIndex}");
            buttonGO.transform.SetParent(buttonPanel.transform, false);
            buttonList.Add(buttonGO);

            Button button = buttonGO.AddComponent<Button>();
            Image background = buttonGO.AddComponent<Image>();
            background.color = Color.white; // ボタン背景色

            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            float totalWidth = tileNumberGroup.Count * imageSize + (tileNumberGroup.Count - 1) * imageSpacing;
            buttonRT.sizeDelta = new Vector3(totalWidth, buttonHeight, 0);
            buttonRT.transform.position = canvasPlace + firstButtonPlace + (tileNumberGroups.Count - 1 - groupIndex) * buttonDistance;
            // 子画像を横に並べて追加
            for (int i = 0; i < tileNumberGroup.Count; i++)
            {
                string imageName = Library.ToSpriteName[tileNumberGroup[i]];
                Sprite sprite = Resources.Load<Sprite>("Sprites/" + imageName);

                if (sprite == null)
                {
                    Debug.LogWarning($"画像が見つかりません: {imageName}");
                    continue;
                }

                GameObject imageGO = new($"Image_{imageName}");
                imageGO.transform.SetParent(buttonGO.transform, false);

                Image image = imageGO.AddComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;

                RectTransform imageRT = imageGO.GetComponent<RectTransform>();
                imageRT.sizeDelta = new Vector3(imageSize, imageSize, 0);
                imageRT.anchorMin = new Vector3(0, 0.5f, 0);
                imageRT.anchorMax = new Vector3(0, 0.5f, 0);
                imageRT.pivot = new Vector3(0, 0.5f, 0);
                imageRT.anchoredPosition = new Vector3(imageSpacing + i * (imageSize + imageSpacing), 0, 0);
            }

            // ボタンのクリックイベント
            int capturedIndex = groupIndex;
            button.onClick.AddListener(() =>
            {
                PlayerManager.instance.Furo(tileNumberGroup);
            });
        }
    }
    public void CreateKanButtons(Player player, List<int[]> tileNumberGroups)
    {
        for (int groupIndex = tileNumberGroups.Count - 1; groupIndex >= 0; groupIndex--)
        {
            int[] tileNumberGroup = tileNumberGroups[groupIndex];

            GameObject buttonGO = new GameObject($"GroupButton_{groupIndex}");
            buttonGO.transform.SetParent(buttonPanel.transform, false);
            buttonList.Add(buttonGO);

            Button button = buttonGO.AddComponent<Button>();
            Image background = buttonGO.AddComponent<Image>();
            background.color = Color.white; // ボタン背景色

            RectTransform buttonRT = buttonGO.GetComponent<RectTransform>();
            float totalWidth = 2 * imageSize + 1 * imageSpacing;
            buttonRT.sizeDelta = new Vector3(totalWidth, buttonHeight, 0);
            buttonRT.transform.position = canvasPlace + firstButtonPlace + (tileNumberGroups.Count - 1 - groupIndex) * buttonDistance;
            
            string imageName = Library.ToSpriteName[tileNumberGroup[0]];
            int isKakan = tileNumberGroup[1];
            Sprite sprite = Resources.Load<Sprite>("Sprites/" + imageName);

            if (sprite == null)
            {
                Debug.LogWarning($"画像が見つかりません: {imageName}");
                continue;
            }

            GameObject imageGO = new($"Image_{imageName}");
            imageGO.transform.SetParent(buttonGO.transform, false);

            Image image = imageGO.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            RectTransform imageRT = imageGO.GetComponent<RectTransform>();
            imageRT.sizeDelta = new Vector3(imageSize, imageSize, 0);
            imageRT.anchorMin = new Vector3(0, 0.5f, 0);
            imageRT.anchorMax = new Vector3(0, 0.5f, 0);
            imageRT.pivot = new Vector3(0, 0.5f, 0);
            imageRT.anchoredPosition = new Vector3((imageSize + 3 * imageSpacing) / 2f, 0, 0);

            // ボタンのクリックイベント
            int capturedIndex = groupIndex;
            if (isKakan == 0)
            {
                button.onClick.AddListener(() =>
                {
                    PlayerManager.instance.Ankan(player, tileNumberGroup[0]);

                });
            }
            else
            {
                button.onClick.AddListener(() =>
                {
                    PlayerManager.instance.Kakan(player, tileNumberGroup[0]);
                });
            }
        }
    }

    public void DeactivateButtonsToMe() { AnkanOff(); KakanOff(); TsumoOff(); RiichiOff(); SkipToMeOff(); }
    public void DeactivateButtonsToOtherPlayer() { PonOff(); ChiOff(); KanOff(); RonOff(); SkipToOthersOff(); }
    public void ReactToTileOtherPlayerDiscarded(Player player, int tileId)
    {
        int buttonNumber = 1; // for skip button
        ResetButtonPlace();
        if (player.chi[Library.idWithoutRed[tileId]]) buttonNumber++;
        if (player.pon[Library.idWithoutRed[tileId]]) buttonNumber++;
        if (player.kan[Library.idWithoutRed[tileId]]) buttonNumber++;
        if (player.machiTile[Library.idWithoutRed[tileId]]) buttonNumber++;
        if (buttonNumber == 1)
        {
            PlayerManager.instance.TurnStart();
            return;
        }        SkipToOthersOn();
        if (player.machiTile[Library.idWithoutRed[tileId]]) RonOn();
        if (player.kan[Library.idWithoutRed[tileId]]) KanOn();
        if (player.chi[Library.idWithoutRed[tileId]]) ChiOn();
        if (player.pon[Library.idWithoutRed[tileId]]) PonOn();
    }


    public void PonOn() { 
        ponButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        ponButton.SetActive(true);
    }
    public void ChiOn() {
        chiButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        chiButton.SetActive(true);
    }
    public void KanOn()
    {
        kanButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        kanButton.SetActive(true);
    }
    public void KakanOn()
    {
        kakanButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        kakanButton.SetActive(true);
    }
    public void AnkanOn() {
        ankanButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        ankanButton.SetActive(true);
    }
    public void RonOn() {
        ronButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        ronButton.SetActive(true); 
    }
    public void SkipToMeOn()
    {
        skipToMeButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        skipToMeButton.SetActive(true);
    }
    public void SkipToOthersOn()
    {
        skipToOthersButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        skipToOthersButton.SetActive(true);
    }
    public void TsumoOn() {
        tsumoButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        tsumoButton.SetActive(true);
    }
    public void RiichiOn() {
        riichiButton.transform.position = canvasPlace + nextButtonPlace;
        nextButtonPlace += buttonDistance;
        riichiButton.SetActive(true);
    }

    public void PonOff() { ponButton.SetActive(false); }
    public void ChiOff() { chiButton.SetActive(false); }
    public void KanOff() { kanButton.SetActive(false); }
    public void KakanOff() { kakanButton.SetActive(false); }
    public void AnkanOff() { ankanButton.SetActive(false); }
    public void RonOff() { ronButton.SetActive(false); }
    public void SkipToMeOff() { skipToMeButton.SetActive(false); }
    public void SkipToOthersOff() { skipToOthersButton.SetActive(false); }
    public void TsumoOff() { tsumoButton.SetActive(false); }
    public void RiichiOff() { riichiButton.SetActive(false); }
}
