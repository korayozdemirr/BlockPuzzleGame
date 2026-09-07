using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;

    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SaveManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private const string KEY_CURRENT_LEVEL_INDEX = "Saved_CurrentLevelIndex";
    private const string KEY_HIGH_SCORE = "Saved_HighScore";
    private const string KEY_SFX_MUTED = "Saved_SFXMuted";
    private const string KEY_VIBRATION_ENABLED = "Saved_VibrationEnabled";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int GetCurrentLevelIndex()
    {
        return PlayerPrefs.GetInt(KEY_CURRENT_LEVEL_INDEX, 0);
    }

    public void SaveCurrentLevelIndex(int levelIndex)
    {
        PlayerPrefs.SetInt(KEY_CURRENT_LEVEL_INDEX, Mathf.Max(0, levelIndex));
        PlayerPrefs.Save();
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
    }

    public bool TryUpdateHighScore(int newScore)
    {
        int currentHighScore = GetHighScore();
        if (newScore > currentHighScore)
        {
            PlayerPrefs.SetInt(KEY_HIGH_SCORE, newScore);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    public bool IsSFXMuted()
    {
        return PlayerPrefs.GetInt(KEY_SFX_MUTED, 0) == 1;
    }

    public void SetSFXMuted(bool mute)
    {
        PlayerPrefs.SetInt(KEY_SFX_MUTED, mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt(KEY_VIBRATION_ENABLED, 1) == 1;
    }

    public void SetVibrationEnabled(bool enable)
    {
        PlayerPrefs.SetInt(KEY_VIBRATION_ENABLED, enable ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteKey(KEY_CURRENT_LEVEL_INDEX);
        PlayerPrefs.DeleteKey(KEY_HIGH_SCORE);
        PlayerPrefs.Save();
    }
}
