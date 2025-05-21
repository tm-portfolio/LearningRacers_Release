using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// �񓯊��V�[���J�ڂ��Ǘ����A���[�f�B���O����UI�\���𐧌䂷��N���X
public class SceneLoader : MonoBehaviour
{
    [Header("���[�f�B���OUI")]
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;

    [Header("�ݒ�")]
    [SerializeField] private float postLoadDelay = 1.0f;  // �Ǎ�������̒ǉ��\������
    [SerializeField] private float dotUpdateInterval = 0.5f;  // �h�b�g�X�V�Ԋu

    private float dotTimer = 0f;
    private int dotCount = 0;

    // �w�肳�ꂽ�V�[����񓯊��œǂݍ��݁A���[�f�B���OUI��\��
    public void Load(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        dotTimer = 0f;
        dotCount = 0;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // �V�[���ǂݍ��ݒ��i�ő� progress 0.9�j
        while (asyncLoad.progress < 0.9f)
        {
            UpdateLoadingDots();
            yield return null;
        }

        // �ǂݍ��݊���������������\��
        float wait = postLoadDelay;
        while (wait > 0f)
        {
            wait -= Time.deltaTime;
            UpdateLoadingDots();
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    // Now Loading... �̃h�b�g�A�j���[�V�����X�V
    private void UpdateLoadingDots()
    {
        dotTimer += Time.deltaTime;
        if (dotTimer >= dotUpdateInterval)
        {
            dotTimer = 0f;
            dotCount = (dotCount + 1) % 4;
            if (loadingText != null)
                loadingText.text = "Now Loading" + new string('.', dotCount);
        }
    }
}
