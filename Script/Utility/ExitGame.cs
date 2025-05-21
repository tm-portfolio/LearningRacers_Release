using UnityEngine;

// アプリケーション終了処理と確認ポップアップ表示を管理するクラス
public class ExitGame : MonoBehaviour
{
    public GameObject confirmExitPopup;

    // 終了確認ポップアップを表示
    public void ShowExitPopup()
    {
        confirmExitPopup.SetActive(true);
    }

    // 終了確認ポップアップを非表示
    public void HideExitPopup()
    {
        confirmExitPopup.SetActive(false);
    }

    // アプリケーション終了
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
