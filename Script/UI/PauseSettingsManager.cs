using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ポーズメニュー内の各種パネル（操作説明、設定など）の表示切替を管理するクラス
// ※ UIManager とは役割を分け、こちらはパネル制御専用
public class PauseSettingsManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;            　　　　　　// ポーズメニュー本体（メインUI）
    public GameObject howToPanel;                　　　　　　// 操作説明パネル
    public GameObject settingsPanel;             　　　　　　// 設定（音量など）パネル
    public VolumeSettingsController volumeController;　　　　// 音量スライダー制御用
    [SerializeField] private ReturnToStartPopup returnPopup; // 「スタート画面へ戻る」確認ポップアップ

    // 操作説明パネルを開く
    public void OpenHowTo()
    {
        howToPanel.SetActive(true);                   // 操作説明表示
        pauseMenuPanel.SetActive(false);              // ポーズ画面非表示
        Time.timeScale = 0f;                          // ゲーム停止は継続
        GameManager.Instance.uiManager.SetSubPanelOpen(true);
    }

    // 操作説明パネルを閉じて、ポーズメニューに戻る
    public void CloseHowTo()
    {
        howToPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        GameManager.Instance.uiManager.SetSubPanelOpen(false);
    }

    // 「スタート画面へ戻る」ボタンが押されたときに確認ポップアップを表示
    public void OnReturnButtonPressed()
    {
        returnPopup?.ShowPopup();
    }

    // 設定パネルを開く（音量スライダーなど）
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);                // 設定表示
        pauseMenuPanel.SetActive(false);              // ポーズ非表示
        Time.timeScale = 0f;                          // ゲーム停止は継続
    }

    // 設定パネルを閉じて、ポーズメニューに戻る
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
