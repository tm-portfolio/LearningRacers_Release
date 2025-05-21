using UnityEngine;

// �A�v���P�[�V�����I�������Ɗm�F�|�b�v�A�b�v�\�����Ǘ�����N���X
public class ExitGame : MonoBehaviour
{
    public GameObject confirmExitPopup;

    // �I���m�F�|�b�v�A�b�v��\��
    public void ShowExitPopup()
    {
        confirmExitPopup.SetActive(true);
    }

    // �I���m�F�|�b�v�A�b�v���\��
    public void HideExitPopup()
    {
        confirmExitPopup.SetActive(false);
    }

    // �A�v���P�[�V�����I��
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
