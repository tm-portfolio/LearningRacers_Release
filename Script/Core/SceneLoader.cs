using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// 非同期シーン遷移を管理し、ローディング中のUI表示を制御するクラス
public class SceneLoader : MonoBehaviour
{
    [Header("ローディングUI")]
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;

    [Header("設定")]
    [SerializeField] private float postLoadDelay = 1.0f;  // 読込完了後の追加表示時間
    [SerializeField] private float dotUpdateInterval = 0.5f;  // ドット更新間隔

    private float dotTimer = 0f;
    private int dotCount = 0;

    // 指定されたシーンを非同期で読み込み、ローディングUIを表示
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

        // シーン読み込み中（最大 progress 0.9）
        while (asyncLoad.progress < 0.9f)
        {
            UpdateLoadingDots();
            yield return null;
        }

        // 読み込み完了後も少しだけ表示
        float wait = postLoadDelay;
        while (wait > 0f)
        {
            wait -= Time.deltaTime;
            UpdateLoadingDots();
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    // Now Loading... のドットアニメーション更新
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
