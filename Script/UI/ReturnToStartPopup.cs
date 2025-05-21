using UnityEngine;
using UnityEngine.SceneManagement;

// �u�X�^�[�g��ʂɖ߂�܂����H�v�̊m�F�|�b�v�A�b�v���Ǘ�����N���X
// GameScene �� ResultScene �Ŏg�p
public class ReturnToStartPopup : MonoBehaviour
{
    [Header("�|�b�v�A�b�vUI")]
    public GameObject confirmPopupUI;

    // �|�b�v�A�b�v��\������
    public void ShowPopup()
    {
        if (confirmPopupUI != null)
            confirmPopupUI.SetActive(true);
    }

    // �|�b�v�A�b�v���\���ɂ���
    public void HidePopup()
    {
        if (confirmPopupUI != null)
            confirmPopupUI.SetActive(false);
    }

    // �X�^�[�g��ʂɑJ�ځi�^�C���X�P�[�����������j
    public void ReturnToStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}
