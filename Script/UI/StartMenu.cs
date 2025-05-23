using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// �X�^�[�g��ʂ�UI�����S������N���X
/// </summary>
public class StartMenu : MonoBehaviour
{
    [Header("UI�Q��")]
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

    [Header("AI�Љ�p�l��")]
    public GameObject aiInfoPanel;

    void Start()
    {
        // ======== ��������ɍ폜 ========
        // ��x�������ȃx�X�g�����L���O������������
        // for (int i = 0; i < 5; i++)
        // {
        //     PlayerPrefs.DeleteKey($"BestTime_{i}");
        //     PlayerPrefs.DeleteKey($"BestTimeName_{i}");
        //     PlayerPrefs.DeleteKey($"BestTimeDate_{i}");
        // }
        // PlayerPrefs.DeleteKey("CurrentTime");
        // PlayerPrefs.Save();
           
        // Debug.Log("�y��x�����z���ȃx�X�g�����L���O�����������܂����B");
        // ======== ��L�̃R�[�h���c���Ă���Ɩ���f�[�^�������邽�ߒ��� ========

        AudioManager.Instance?.StopSE(); // �Q�[��SE��~
        nameInput.text = "player";       // ������

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
            Debug.LogWarning("[StartMenu] �v���C���[������ł�");
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
        exitGame?.ShowExitPopup(); // �� �|�b�v�A�b�v��\��
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
