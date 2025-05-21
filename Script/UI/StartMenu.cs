using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// スタート画面のUI操作を担当するクラス
/// </summary>
public class StartMenu : MonoBehaviour
{
    [Header("UI参照")]
    public TMP_InputField nameInput;
    public Button settingsButton;
    public Button aiInfoButton;
    public GameObject howToPanel;
    public SceneLoader sceneLoader;
    public SettingsManager settingsManager;

    [Header("SE")]
    public AudioClip startButtonClip;
    public AudioClip settingsButtonClip;
    public AudioClip aiInfoButtonClip;

    [SerializeField] private ExitGame exitGame;

    [Header("AI紹介パネル")]
    public GameObject aiInfoPanel;

    void Start()
    {
        AudioManager.Instance?.StopSE(); // ゲームSE停止
        nameInput.text = "player";       // 初期名

        AudioManager.Instance?.PlayBGM(Resources.Load<AudioClip>("BGM_StartScene"));
    }

    public void OnStartButtonClicked()
    {
        PlaySE(startButtonClip);

        string playerName = nameInput.text.Trim();

        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.SetInt("TrainingMode", 0);
            PlayerPrefs.Save();

            sceneLoader?.Load("GameScene");
        }
        else
        {
            Debug.LogWarning("[StartMenu] プレイヤー名が空です");
        }
    }

    public void OnSettingsButtonClicked()
    {
        PlaySE(settingsButtonClip);
        settingsManager?.OpenSettings();
    }

    public void OnHowToButtonClicked()
    {
        PlayCommonSE();
        howToPanel.SetActive(true);
    }

    public void OnCloseHowToPanel()
    {
        howToPanel.SetActive(false);
    }

    public void OnAIInfoButtonClicked()
    {
        PlaySE(aiInfoButtonClip);
        aiInfoPanel?.SetActive(true);
    }

    public void OnCloseAIInfoPanel()
    {
        aiInfoPanel?.SetActive(false);
    }

    public void OnExitButtonClicked()
    {
        PlayCommonSE();
        exitGame?.ShowExitPopup(); // ← ポップアップを表示
    }

    private void PlayCommonSE()
    {
        AudioManager.Instance?.PlayCommonSE();
    }

    private void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.Instance?.PlaySE(clip);
        }
    }
}
