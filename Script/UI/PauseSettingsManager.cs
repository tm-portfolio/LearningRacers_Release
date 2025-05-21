using UnityEngine;
using UnityEngine.UI;
using TMPro;

// �|�[�Y���j���[���̊e��p�l���i��������A�ݒ�Ȃǁj�̕\���ؑւ��Ǘ�����N���X
// �� UIManager �Ƃ͖����𕪂��A������̓p�l�������p
public class PauseSettingsManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;            �@�@�@�@�@�@// �|�[�Y���j���[�{�́i���C��UI�j
    public GameObject howToPanel;                �@�@�@�@�@�@// ��������p�l��
    public GameObject settingsPanel;             �@�@�@�@�@�@// �ݒ�i���ʂȂǁj�p�l��
    public VolumeSettingsController volumeController;�@�@�@�@// ���ʃX���C�_�[����p
    [SerializeField] private ReturnToStartPopup returnPopup; // �u�X�^�[�g��ʂ֖߂�v�m�F�|�b�v�A�b�v

    // ��������p�l�����J��
    public void OpenHowTo()
    {
        howToPanel.SetActive(true);                   // ��������\��
        pauseMenuPanel.SetActive(false);              // �|�[�Y��ʔ�\��
        Time.timeScale = 0f;                          // �Q�[����~�͌p��
        GameManager.Instance.uiManager.SetSubPanelOpen(true);
    }

    // ��������p�l������āA�|�[�Y���j���[�ɖ߂�
    public void CloseHowTo()
    {
        howToPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        GameManager.Instance.uiManager.SetSubPanelOpen(false);
    }

    // �u�X�^�[�g��ʂ֖߂�v�{�^���������ꂽ�Ƃ��Ɋm�F�|�b�v�A�b�v��\��
    public void OnReturnButtonPressed()
    {
        returnPopup?.ShowPopup();
    }

    // �ݒ�p�l�����J���i���ʃX���C�_�[�Ȃǁj
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);                // �ݒ�\��
        pauseMenuPanel.SetActive(false);              // �|�[�Y��\��
        Time.timeScale = 0f;                          // �Q�[����~�͌p��
    }

    // �ݒ�p�l������āA�|�[�Y���j���[�ɖ߂�
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
