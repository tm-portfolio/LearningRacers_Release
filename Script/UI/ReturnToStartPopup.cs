using UnityEngine;
using UnityEngine.SceneManagement;

// 「スタート画面に戻りますか？」の確認ポップアップを管理するクラス
// GameScene や ResultScene で使用
public class ReturnToStartPopup : MonoBehaviour
{
    [Header("ポップアップUI")]
    public GameObject confirmPopupUI;

    // ポップアップを表示する
    public void ShowPopup()
    {
        if (confirmPopupUI != null)
            confirmPopupUI.SetActive(true);
    }

    // ポップアップを非表示にする
    public void HidePopup()
    {
        if (confirmPopupUI != null)
            confirmPopupUI.SetActive(false);
    }

    // スタート画面に遷移（タイムスケールも初期化）
    public void ReturnToStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}
