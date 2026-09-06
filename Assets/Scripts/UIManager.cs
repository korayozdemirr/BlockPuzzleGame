using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text References")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;

    [Header("Buttons & Panels")]
    public Button undoButton;
    public GameObject nextLevelPanel;
    public Button nextLevelButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (undoButton != null)
        {
            undoButton.onClick.AddListener(OnUndoClicked);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

        ToggleNextLevelPanel(false);
    }

    public void UpdateLevelUI(int levelNumber)
    {
        if (levelText != null) levelText.text = $"Level {levelNumber}";
    }

    public void UpdateScoreUI(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    public void ToggleNextLevelPanel(bool show)
    {
        if (nextLevelPanel != null)
        {
            nextLevelPanel.SetActive(show);
        }
    }

    private void OnUndoClicked()
    {
        GridManager.Instance.UndoLastMove();
    }

    private void OnNextLevelClicked()
    {
        ToggleNextLevelPanel(false);
        LevelManager.Instance.OnLevelCompleted();
    }
}