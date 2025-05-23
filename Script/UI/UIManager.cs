using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ゲーム中のUI表示・更新を管理するクラス
// ラップ表示・順位表示・逆走警告・ポーズメニューなどを制御
public class UIManager : MonoBehaviour
{
    [Header("UI 表示")]
    public TextMeshProUGUI lapDisplay;           // ラップ数の表示
    public TextMeshProUGUI positionDisplay;      // 順位の表示
    public TextMeshProUGUI reverseWarningText;   // 逆走警告テキスト
    public GameObject pauseMenuUI;               // ポーズメニューUIの親オブジェクト
    public TextMeshProUGUI goalText;             // ゴール時に表示されるテキスト

    [Header("プレイヤーと設定")]
    public PlayerCarController player;           // プレイヤー車両（情報取得用）
    public int totalCheckpoints = 4;             // 設定されているチェックポイントの数
    public int targetLaps = 3;                   // 目標ラップ数（ゴール条件）

    private bool isSubPanelOpen = false;         // 設定などのサブパネルが開いているかどうか
    private bool pauseMenuOpen = false;          // ポーズメニューが開いているかどうか

    // 毎フレーム呼び出して、UIを最新の状態に更新する
    public void UpdateRaceUI()
    {
        if (player == null) return;

        int lapDisplayCount = player.CurrentLap;

        // ゴール直後の表示補正：1フレームだけ +1 表示する（JustFinishedLap使用）
        if (player.JustFinishedLap)
        {
            lapDisplayCount++;
        }

        // ラップ数を UI に表示（最大表示は targetLaps）
        lapDisplay.text = $"Lap: {Mathf.Min(lapDisplayCount, targetLaps)}/{targetLaps}";

        // ゴールしたらラップ表示をオレンジに変える
        lapDisplay.color = (lapDisplayCount >= targetLaps)
            ? new Color(1.0f, 0.5f, 0.0f)  // オレンジ
            : Color.white;

        // プレイヤーの順位をスコアベースで取得して表示
        int rank = GetPlayerPosition();
        positionDisplay.text = $"Position: {rank}";

        // 順位に応じて色変更（1位=金、2位=銀、3位=銅、それ以外=青）
        switch (rank)
        {
            case 1: positionDisplay.color = new Color(1.0f, 0.84f, 0.0f); break; // 金
            case 2: positionDisplay.color = new Color(0.75f, 0.75f, 0.75f); break; // 銀
            case 3: positionDisplay.color = new Color(0.8f, 0.5f, 0.2f); break; // 銅
            default: positionDisplay.color = Color.blue; break;
        }

        // 逆走判定と表示
        CheckReverseWarning();
    }

    // プレイヤーの現在順位を計算
    // GameManager.GetProgressScore() を使って比較
    private int GetPlayerPosition()
    {
        float playerScore = GameManager.Instance.GetProgressScore(player);
        int rank = 1;

        foreach (var car in GameManager.Instance.allCars)
        {
            float score = GameManager.Instance.GetProgressScore(car);
            Debug.Log($"[順位チェック] {car.DriverName} | Lap: {car.CurrentLap}, CP: {car.LastCheckpointIndex}, Score: {score:F2}");

            if ((Object)car == (Object)player) continue;

            // プレイヤーよりスコアが高い車がいれば、そのぶんプレイヤーの順位は下がる（rank++）
            if (GameManager.Instance.GetProgressScore(car) > playerScore + 0.001f) rank++;
        }

        return rank;
    }

    // プレイヤーの向きと次のチェックポイントの向きから逆走を判定
    private void CheckReverseWarning()
    {
        int nextIndex = (player.LastCheckpointIndex - 1 + totalCheckpoints) % totalCheckpoints;
        GameObject nextCheckpoint = GameObject.Find($"Checkpoint_{nextIndex}");
        if (nextCheckpoint == null) return;

        Vector3 checkpointForward = nextCheckpoint.transform.forward;
        float angle = Vector3.Angle(checkpointForward, player.transform.forward);
        //Debug.Log($"角度チェック: {angle}");

        bool isReversing = angle > 130f; // 角度が130度以上なら逆走とみなす
        SetReverseWarning(isReversing);
    }

    // 逆走警告の表示切り替え
    public void SetReverseWarning(bool isActive)
    {
        if (reverseWarningText != null)
            reverseWarningText.gameObject.SetActive(isActive);
    }

    // Tabキーで呼ばれる：ポーズメニューを「開くだけ」に変更
    public void TogglePauseMenu()
    {
        if (pauseMenuUI == null) return;

        if (pauseMenuUI.activeSelf) return; // すでに開いてたら何もしない

        pauseMenuUI.SetActive(true);

        // 開音を鳴らす（共通SE）
        AudioManager.Instance?.PlayCommonSE();

        // ゲームを一時停止
        Time.timeScale = 0f;
        pauseMenuOpen = true;
    }

    // ポーズメニューを閉じてゲーム再開
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            pauseMenuOpen = false;
        }
    }

    // サブパネル（遊び方画面、設定画面など）の開閉状態を設定
    public void SetSubPanelOpen(bool isOpen)
    {
        isSubPanelOpen = isOpen;
    }

    // サブパネルが開いているかどうかを取得
    public bool IsSubPanelOpen()
    {
        return isSubPanelOpen;
    }

    // ポーズメニューが開いているかどうかを取得
    public bool IsPauseMenuOpen()
    {
        return pauseMenuOpen;
    }
}
