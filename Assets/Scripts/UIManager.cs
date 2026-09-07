using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text References")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Buttons & Panels")]
    public Button undoButton;
    public GameObject nextLevelPanel;
    public Button nextLevelButton;
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button settingsButton;
    public GameObject mainMenuPanel;
    public Button playButton;

    // Dynamic Overlays Fallbacks
    private GameObject autoGameOverPanel;
    private TextMeshProUGUI autoGameOverScoreText;
    private TextMeshProUGUI autoGameOverBestText;

    private GameObject autoSettingsPanel;
    private TextMeshProUGUI sfxBtnText;
    private TextMeshProUGUI vibBtnText;

    private GameObject autoMainMenuPanel;
    private TextMeshProUGUI autoMainMenuLevelText;
    private TextMeshProUGUI autoMainMenuBestText;

    private Coroutine scorePulseCoroutine;
    private Coroutine panelAnimCoroutine;
    private TMP_FontAsset cachedGameFont;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ApplyFontToExistingUI();

        if (undoButton != null)
        {
            undoButton.onClick.AddListener(OnUndoClicked);
            Sprite undoSprite = Resources.Load<Sprite>("Sprites/btn_undo_icon");
            if (undoSprite != null && undoButton.GetComponent<Image>() != null)
            {
                undoButton.GetComponent<Image>().sprite = undoSprite;
            }
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            Sprite nextSprite = Resources.Load<Sprite>("Sprites/btn_next_level");
            if (nextSprite != null && nextLevelButton.GetComponent<Image>() != null)
            {
                nextLevelButton.GetComponent<Image>().sprite = nextSprite;
            }
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        EnsureSettingsButton();

        ToggleNextLevelPanel(false);
        ToggleGameOverPanel(false);
        ToggleSettingsPanel(false);
    }

    private TMP_FontAsset GetGameFont()
    {
        if (cachedGameFont == null)
        {
            cachedGameFont = Resources.Load<TMP_FontAsset>("Fonts/LilitaOne-Regular SDF");
        }
        return cachedGameFont;
    }

    private void ApplyFont(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        TMP_FontAsset font = GetGameFont();
        if (font != null && tmp.font != font)
        {
            tmp.font = font;
        }
    }

    private void ApplyFontToExistingUI()
    {
        ApplyFont(levelText);
        ApplyFont(scoreText);
        ApplyFont(highScoreText);

        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        TMP_FontAsset font = GetGameFont();
        if (font != null)
        {
            foreach (var txt in allTexts)
            {
                if (txt != null && txt.font != font)
                {
                    txt.font = font;
                }
            }
        }
    }

    public void UpdateLevelUI(int levelNumber)
    {
        if (levelText != null)
        {
            ApplyFont(levelText);
            levelText.text = $"Level {levelNumber}";
        }
    }

    public void UpdateScoreUI(int score)
    {
        int highScore = SaveManager.Instance != null ? SaveManager.Instance.GetHighScore() : 0;
        if (highScoreText != null)
        {
            ApplyFont(scoreText);
            ApplyFont(highScoreText);
            if (scoreText != null) scoreText.text = $"Score: {score}";
            highScoreText.text = $"High Score: {highScore}";
        }
        else
        {
            if (scoreText != null)
            {
                ApplyFont(scoreText);
                scoreText.text = (highScore > 0) ? $"Score: {score} | Best: {highScore}" : $"Score: {score}";
            }
        }

        if (scoreText != null)
        {
            if (scorePulseCoroutine != null) StopCoroutine(scorePulseCoroutine);
            scorePulseCoroutine = StartCoroutine(AnimateScorePulse(scoreText.transform));
        }
    }

    public void UpdateHighScoreUI(int highScore)
    {
        if (highScoreText != null)
        {
            ApplyFont(highScoreText);
            highScoreText.text = $"High Score: {highScore}";
        }
        else if (scoreText != null)
        {
            ApplyFont(scoreText);
            int currentScore = GridManager.Instance != null ? GridManager.Instance.currentScore : 0;
            scoreText.text = (highScore > 0) ? $"Score: {currentScore} | Best: {highScore}" : $"Score: {currentScore}";
        }
    }

    public void ToggleMainMenuPanel(bool show)
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(show);
            if (show)
            {
                if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
                panelAnimCoroutine = StartCoroutine(AnimatePanelElasticBounce(mainMenuPanel.transform));
            }
            return;
        }

        EnsureAutoMainMenuPanel();
        if (autoMainMenuPanel == null) return;

        if (show)
        {
            int levelNum = (SaveManager.Instance != null ? SaveManager.Instance.GetCurrentLevelIndex() : 0) + 1;
            int highScore = SaveManager.Instance != null ? SaveManager.Instance.GetHighScore() : 0;

            if (autoMainMenuLevelText != null) autoMainMenuLevelText.text = $"LEVEL {levelNum}";
            if (autoMainMenuBestText != null) autoMainMenuBestText.text = $"BEST SCORE: {highScore}";

            autoMainMenuPanel.SetActive(true);
            Transform cardTransform = autoMainMenuPanel.transform.Find("MainMenuCard");
            if (cardTransform != null)
            {
                if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
                panelAnimCoroutine = StartCoroutine(AnimatePanelElasticBounce(cardTransform));
            }
        }
        else
        {
            autoMainMenuPanel.SetActive(false);
        }
    }

    public void ToggleNextLevelPanel(bool show)
    {
        if (nextLevelPanel == null) return;

        if (show)
        {
            nextLevelPanel.SetActive(true);
            if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
            panelAnimCoroutine = StartCoroutine(AnimatePanelElasticBounce(nextLevelPanel.transform));
        }
        else
        {
            nextLevelPanel.SetActive(false);
        }
    }

    public void ToggleGameOverPanel(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
            if (show)
            {
                if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
                panelAnimCoroutine = StartCoroutine(AnimatePanelElasticBounce(gameOverPanel.transform));
            }
            return;
        }

        EnsureAutoGameOverPanel();
        if (autoGameOverPanel == null) return;

        if (show)
        {
            int currentScore = GridManager.Instance != null ? GridManager.Instance.currentScore : 0;
            int highScore = SaveManager.Instance != null ? SaveManager.Instance.GetHighScore() : 0;

            if (autoGameOverScoreText != null) autoGameOverScoreText.text = $"Score: {currentScore}";
            if (autoGameOverBestText != null) autoGameOverBestText.text = $"Best Score: {highScore}";

            autoGameOverPanel.SetActive(true);
            Transform cardTransform = autoGameOverPanel.transform.Find("GameOverCard");
            if (cardTransform != null)
            {
                if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
                panelAnimCoroutine = StartCoroutine(AnimatePanelElasticBounce(cardTransform));
            }
        }
        else
        {
            autoGameOverPanel.SetActive(false);
        }
    }

    public void ToggleSettingsPanel(bool show)
    {
        EnsureAutoSettingsPanel();
        if (autoSettingsPanel == null) return;

        if (show)
        {
            UpdateSettingsUI();
            autoSettingsPanel.SetActive(true);
            Transform cardTransform = autoSettingsPanel.transform.Find("SettingsCard");
            if (cardTransform != null)
            {
                if (panelAnimCoroutine != null) StopCoroutine(panelAnimCoroutine);
                panelAnimCoroutine = StartCoroutine(AnimatePanelElasticBounce(cardTransform));
            }
        }
        else
        {
            autoSettingsPanel.SetActive(false);
        }
    }

    private void EnsureSettingsButton()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        GameObject btnObj = new GameObject("Btn_Settings");
        btnObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(30f, -40f);
        rect.sizeDelta = new Vector2(100f, 100f);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

        settingsButton = btnObj.AddComponent<Button>();
        settingsButton.onClick.AddListener(OnSettingsClicked);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(txt);
        txt.text = "OPT";
        txt.fontSize = 28f;
        txt.fontStyle = FontStyles.Bold;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
    }

    private void EnsureAutoMainMenuPanel()
    {
        if (mainMenuPanel != null || autoMainMenuPanel != null) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        autoMainMenuPanel = new GameObject("Auto_Panel_MainMenu");
        autoMainMenuPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = autoMainMenuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bgImage = autoMainMenuPanel.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.1f, 0.16f, 0.97f);

        GameObject card = new GameObject("MainMenuCard");
        card.transform.SetParent(autoMainMenuPanel.transform, false);

        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(800f, 950f);
        cardRect.anchoredPosition = Vector2.zero;

        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.14f, 0.18f, 0.28f, 0.98f);

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(card.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector3(0f, 320f, 0f);
        titleRect.sizeDelta = new Vector2(750f, 120f);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(titleText);
        titleText.text = "BLOCK PUZZLE";
        titleText.fontSize = 68f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.3f, 0.85f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;

        // Current Level
        GameObject levelObj = new GameObject("LevelText");
        levelObj.transform.SetParent(card.transform, false);
        RectTransform levelRect = levelObj.AddComponent<RectTransform>();
        levelRect.anchoredPosition = new Vector3(0f, 180f, 0f);
        levelRect.sizeDelta = new Vector2(700f, 80f);

        autoMainMenuLevelText = levelObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(autoMainMenuLevelText);
        autoMainMenuLevelText.fontSize = 48f;
        autoMainMenuLevelText.fontStyle = FontStyles.Bold;
        autoMainMenuLevelText.color = Color.white;
        autoMainMenuLevelText.alignment = TextAlignmentOptions.Center;

        // Best Score
        GameObject bestObj = new GameObject("BestText");
        bestObj.transform.SetParent(card.transform, false);
        RectTransform bestRect = bestObj.AddComponent<RectTransform>();
        bestRect.anchoredPosition = new Vector3(0f, 90f, 0f);
        bestRect.sizeDelta = new Vector2(700f, 70f);

        autoMainMenuBestText = bestObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(autoMainMenuBestText);
        autoMainMenuBestText.fontSize = 38f;
        autoMainMenuBestText.color = new Color(1f, 0.85f, 0.2f);
        autoMainMenuBestText.alignment = TextAlignmentOptions.Center;

        // PLAY Button
        Sprite playSprite = Resources.Load<Sprite>("Sprites/btn_next_level");
        GameObject playBtnObj = CreateButton(card.transform, "PLAY", new Vector2(0f, -120f), new Vector2(520f, 120f), new Color(0.2f, 0.75f, 0.35f), playSprite);
        Button pBtn = playBtnObj.GetComponent<Button>();
        pBtn.onClick.AddListener(OnPlayClicked);

        TextMeshProUGUI pTxt = playBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (pTxt != null)
        {
            ApplyFont(pTxt);
            pTxt.fontSize = 46f;
        }

        autoMainMenuPanel.SetActive(false);
    }

    private void EnsureAutoSettingsPanel()
    {
        if (autoSettingsPanel != null) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        autoSettingsPanel = new GameObject("Auto_Panel_Settings");
        autoSettingsPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = autoSettingsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bgImage = autoSettingsPanel.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

        GameObject card = new GameObject("SettingsCard");
        card.transform.SetParent(autoSettingsPanel.transform, false);

        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(750f, 850f);
        cardRect.anchoredPosition = Vector2.zero;

        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.14f, 0.17f, 0.25f, 0.96f);

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(card.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector3(0f, 320f, 0f);
        titleRect.sizeDelta = new Vector2(700f, 80f);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(titleText);
        titleText.text = "SETTINGS";
        titleText.fontSize = 54f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;

        // SFX Button
        GameObject sfxBtnObj = CreateButton(card.transform, "SFX SOUNDS: ON", new Vector2(0f, 150f), new Vector2(500f, 100f), new Color(0.2f, 0.6f, 0.4f));
        sfxBtnText = sfxBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        sfxBtnObj.GetComponent<Button>().onClick.AddListener(OnToggleSFXClicked);

        // Vibration Button
        GameObject vibBtnObj = CreateButton(card.transform, "VIBRATION: ON", new Vector2(0f, 20f), new Vector2(500f, 100f), new Color(0.2f, 0.5f, 0.7f));
        vibBtnText = vibBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        vibBtnObj.GetComponent<Button>().onClick.AddListener(OnToggleVibrationClicked);

        // Reset Progress Button
        GameObject resetBtnObj = CreateButton(card.transform, "RESET PROGRESS", new Vector2(0f, -110f), new Vector2(500f, 90f), new Color(0.8f, 0.3f, 0.3f));
        resetBtnObj.GetComponent<Button>().onClick.AddListener(OnResetProgressClicked);

        // Close Button (X)
        GameObject closeBtnObj = CreateButton(card.transform, "CLOSE", new Vector2(0f, -250f), new Vector2(500f, 90f), new Color(0.4f, 0.4f, 0.4f));
        closeBtnObj.GetComponent<Button>().onClick.AddListener(OnCloseSettingsClicked);

        autoSettingsPanel.SetActive(false);
    }

    private void UpdateSettingsUI()
    {
        if (SaveManager.Instance == null) return;

        bool isMuted = SaveManager.Instance.IsSFXMuted();
        if (sfxBtnText != null)
        {
            sfxBtnText.text = isMuted ? "SFX SOUNDS: OFF" : "SFX SOUNDS: ON";
            sfxBtnText.transform.parent.GetComponent<Image>().color = isMuted ? new Color(0.5f, 0.3f, 0.3f) : new Color(0.2f, 0.6f, 0.4f);
        }

        bool vibEnabled = SaveManager.Instance.IsVibrationEnabled();
        if (vibBtnText != null)
        {
            vibBtnText.text = vibEnabled ? "VIBRATION: ON" : "VIBRATION: OFF";
            vibBtnText.transform.parent.GetComponent<Image>().color = vibEnabled ? new Color(0.2f, 0.5f, 0.7f) : new Color(0.4f, 0.4f, 0.4f);
        }
    }

    private void OnPlayClicked()
    {
        ToggleMainMenuPanel(false);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartGameFromMenu();
        }
    }

    private void OnSettingsClicked()
    {
        if (settingsButton != null) StartCoroutine(AnimateButtonClick(settingsButton.transform));
        ToggleSettingsPanel(true);
    }

    private void OnToggleSFXClicked()
    {
        if (SaveManager.Instance != null)
        {
            bool isMuted = SaveManager.Instance.IsSFXMuted();
            SaveManager.Instance.SetSFXMuted(!isMuted);
            UpdateSettingsUI();
        }
    }

    private void OnToggleVibrationClicked()
    {
        if (SaveManager.Instance != null)
        {
            bool vibEnabled = SaveManager.Instance.IsVibrationEnabled();
            SaveManager.Instance.SetVibrationEnabled(!vibEnabled);
            UpdateSettingsUI();
        }
    }

    private void OnResetProgressClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetAllData();
        }
        ToggleSettingsPanel(false);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartCurrentLevel();
        }
        else if (GridManager.Instance != null)
        {
            GridManager.Instance.RestartCurrentLevel();
        }
    }

    private void OnCloseSettingsClicked()
    {
        ToggleSettingsPanel(false);
    }

    private void EnsureAutoGameOverPanel()
    {
        if (gameOverPanel != null || autoGameOverPanel != null) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        autoGameOverPanel = new GameObject("Auto_Panel_GameOver");
        autoGameOverPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = autoGameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bgImage = autoGameOverPanel.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

        GameObject card = new GameObject("GameOverCard");
        card.transform.SetParent(autoGameOverPanel.transform, false);

        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(750f, 850f);
        cardRect.anchoredPosition = Vector2.zero;

        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.12f, 0.15f, 0.22f, 0.96f);

        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(card.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector3(0f, 300f, 0f);
        titleRect.sizeDelta = new Vector2(700f, 100f);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(titleText);
        titleText.text = "GAME OVER";
        titleText.fontSize = 62f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(1f, 0.3f, 0.3f);
        titleText.alignment = TextAlignmentOptions.Center;

        GameObject subTitleObj = new GameObject("SubTitleText");
        subTitleObj.transform.SetParent(card.transform, false);
        RectTransform subRect = subTitleObj.AddComponent<RectTransform>();
        subRect.anchoredPosition = new Vector3(0f, 220f, 0f);
        subRect.sizeDelta = new Vector2(700f, 50f);

        TextMeshProUGUI subText = subTitleObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(subText);
        subText.text = "No More Moves Available!";
        subText.fontSize = 30f;
        subText.color = new Color(0.8f, 0.85f, 0.9f);
        subText.alignment = TextAlignmentOptions.Center;

        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(card.transform, false);
        RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchoredPosition = new Vector3(0f, 110f, 0f);
        scoreRect.sizeDelta = new Vector2(700f, 70f);

        autoGameOverScoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(autoGameOverScoreText);
        autoGameOverScoreText.fontSize = 42f;
        autoGameOverScoreText.color = Color.white;
        autoGameOverScoreText.alignment = TextAlignmentOptions.Center;

        GameObject bestObj = new GameObject("BestText");
        bestObj.transform.SetParent(card.transform, false);
        RectTransform bestRect = bestObj.AddComponent<RectTransform>();
        bestRect.anchoredPosition = new Vector3(0f, 30f, 0f);
        bestRect.sizeDelta = new Vector2(700f, 70f);

        autoGameOverBestText = bestObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(autoGameOverBestText);
        autoGameOverBestText.fontSize = 36f;
        autoGameOverBestText.color = new Color(1f, 0.85f, 0.2f);
        autoGameOverBestText.alignment = TextAlignmentOptions.Center;

        Sprite playSprite = Resources.Load<Sprite>("Sprites/btn_next_level");
        GameObject restartBtnObj = CreateButton(card.transform, "RETRY LEVEL", new Vector2(0f, -120f), new Vector2(480f, 100f), new Color(0.2f, 0.7f, 0.35f), playSprite);
        Button rBtn = restartBtnObj.GetComponent<Button>();
        rBtn.onClick.AddListener(OnRestartClicked);

        Sprite undoSprite = Resources.Load<Sprite>("Sprites/btn_undo_icon");
        GameObject undoBtnObj = CreateButton(card.transform, "UNDO LAST MOVE", new Vector2(0f, -250f), new Vector2(480f, 90f), new Color(0.25f, 0.45f, 0.8f), undoSprite);
        Button uBtn = undoBtnObj.GetComponent<Button>();
        uBtn.onClick.AddListener(OnUndoClicked);

        autoGameOverPanel.SetActive(false);
    }

    private GameObject CreateButton(Transform parent, string label, Vector2 pos, Vector2 size, Color btnColor, Sprite customSprite = null)
    {
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        if (customSprite != null)
        {
            img.sprite = customSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.color = btnColor;
        }

        Button btn = btnObj.AddComponent<Button>();

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        ApplyFont(txt);
        txt.text = label;
        txt.fontSize = 30f;
        txt.fontStyle = FontStyles.Bold;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;

        return btnObj;
    }

    private void OnUndoClicked()
    {
        if (undoButton != null) StartCoroutine(AnimateButtonClick(undoButton.transform));
        ToggleGameOverPanel(false);
        GridManager.Instance.UndoLastMove();
    }

    private void OnNextLevelClicked()
    {
        if (nextLevelButton != null) StartCoroutine(AnimateButtonClick(nextLevelButton.transform));
        ToggleNextLevelPanel(false);
        LevelManager.Instance.OnLevelCompleted();
    }

    private void OnRestartClicked()
    {
        if (restartButton != null) StartCoroutine(AnimateButtonClick(restartButton.transform));
        ToggleGameOverPanel(false);
        GridManager.Instance.RestartCurrentLevel();
    }

    private IEnumerator AnimateScorePulse(Transform targetTransform)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 baseScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t < 0.5f)
                targetTransform.localScale = Vector3.Lerp(baseScale, targetScale, t * 2f);
            else
                targetTransform.localScale = Vector3.Lerp(targetScale, baseScale, (t - 0.5f) * 2f);

            yield return null;
        }

        targetTransform.localScale = baseScale;
    }

    private IEnumerator AnimatePanelElasticBounce(Transform targetTransform)
    {
        float duration = 0.35f;
        float elapsed = 0f;

        targetTransform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Elastic bounce curve: 0 -> 1.15 -> 1.0
            float scale;
            if (t < 0.7f)
            {
                scale = Mathf.Lerp(0f, 1.15f, t / 0.7f);
            }
            else
            {
                scale = Mathf.Lerp(1.15f, 1.0f, (t - 0.7f) / 0.3f);
            }

            targetTransform.localScale = Vector3.one * scale;
            yield return null;
        }

        targetTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimateButtonClick(Transform btnTransform)
    {
        float duration = 0.12f;
        float elapsed = 0f;
        Vector3 originalScale = Vector3.one;
        Vector3 pressedScale = Vector3.one * 0.88f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t < 0.5f)
                btnTransform.localScale = originalScale;
            else
                btnTransform.localScale = Vector3.Lerp(pressedScale, originalScale, (t - 0.5f) * 2f);

            yield return null;
        }

        btnTransform.localScale = originalScale;
    }
}