using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.MLAgents;


// レース全体を管理。
// プレイヤー・AIの初期化や、レース進行、ゴール処理を担当。
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("設定")]
    public int targetLaps = 3;       // ゴールするための必要ラップ数
    public int totalCheckpoints = 4; // コース上に設置されているチェックポイントの数

    private int playerLaps = 0;         // プレイヤーが完了したラップ数
    private float raceStartTime = 0f;   // レース開始時刻（Time.time）
    private float raceTimer = 0f;       // レース経過時間（秒）
    private bool raceStarted = false;   // レースが開始されたかどうかのフラグ

    [Header("車両")]
    public PlayerCarController player;         // プレイヤーの車両（操作対象）
    public List<Agent> aiCarObjects;           // Unityエディタ上で登録するAI車両（Agent）のリスト
    private List<ICarInfo> aiCars = new();     // ゲーム内で有効化されたAI車（ICarInfoとして扱う）
    public List<ICarInfo> allCars = new();     // プレイヤー＋AIを含む全車両のリスト（順位判定用）

    [Header("演出")]
    public TextMeshProUGUI countdownText;       // カウントダウンのテキスト表示
    public TextMeshProUGUI timerText;           // タイマーのテキスト表示
    public GameObject starEffectPrefab;         // ラップ完了時に出す星エフェクトのプレハブ
    public GameObject goalFireworkPrefab;       // ゴール時に出す火花エフェクトのプレハブ
    private bool allowDriveLoop = false;        // DriveLoop音を鳴らすかどうか（START後に有効）
    private bool forceDisableDriveLoop = false; // ゴール後などでDriveLoopを強制停止するかどうか

    [Header("UI / 外部参照")]
    public UIManager uiManager;                // UI制御用のマネージャー
    public ResultManager resultManager;        // リザルト処理用のマネージャー

    public bool JustFinishedLap { get; set; } = false; // ゴール直後に一時的に表示補正を入れるためのフラグ

    public Dictionary<string, float> carFinishTimes = new(); // リザルト画面用
    public float GetRaceStartTime() => raceStartTime;        // リザルト画面用

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // レース初期化処理（ゲーム開始時に1度だけ実行される）
    void Start()
    {
        carFinishTimes.Clear(); // 0523

        playerLaps = 0;
        AudioManager.Instance?.StopSE();                                            // 念のためSEを停止
        AudioManager.Instance?.PlayBGM(Resources.Load<AudioClip>("BGM_GameScene")); // BGM再生

        allCars.Add(player); // プレイヤー車をallCarsに登録

        // 最初はAIを無効化（物理挙動を止める）
        foreach (var car in aiCarObjects)
            car.enabled = false;

        // 少し待ってからAI有効化＆カウントダウン開始
        StartCoroutine(InitializeAICarsAfterDelay(0.5f));
        StartCoroutine(DelayedStartCountdown());
    }

    // 指定時間待ってからAI車両を有効化する
    private IEnumerator InitializeAICarsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var car in aiCarObjects)
        {
            car.enabled = true;

            if (car is ICarInfo aiCar)
            {
                aiCars.Add(aiCar);
                allCars.Add(aiCar);
            }
        }
    }

    // 少し遅れてカウントダウンを開始する
    private IEnumerator DelayedStartCountdown()
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(StartCountdown());
    }

    // 「3→2→1→START!!」のカウントダウン演出と、レース開始処理
    private IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        string[] counts = { "3", "2", "1", "START!!" };

        foreach (var count in counts)
        {
            countdownText.text = count;
            AudioClip clip = Resources.Load<AudioClip>(count == "START!!" ? "SE_Start" : "SE_Countdown");
            AudioManager.Instance?.PlaySE(clip);

            yield return new WaitForSeconds(1f);

            if (count == "START!!")
            {
                countdownText.gameObject.SetActive(false);
                AudioManager.Instance?.StopSE();
                player.RestartRace();
                raceStartTime = Time.time;
                raceStarted = true;
                allowDriveLoop = true;
                forceDisableDriveLoop = false;
            }
        }
    }

    // 毎フレーム処理（ポーズやBGMの制御、DriveLoop切替など）
    void Update()
    {
        AudioManager.Instance?.ApplyVolumeSettings(); // 音量反映

        // Tabキーでのポーズメニュー開閉処理
        if (Input.GetKeyDown(KeyCode.Tab) && !uiManager.IsSubPanelOpen())
        {
            uiManager.TogglePauseMenu();
        }

        // ゴール後の停止処理
        if (forceDisableDriveLoop)
        {
            AudioManager.Instance?.StopSE();
            return;
        }

        // ポーズ中はDriveLoop停止
        if (uiManager.IsPauseMenuOpen())
        {
            // DriveLoopは止めるが、ボタンSEは止めない
            if (AudioManager.Instance?.seSource != null && AudioManager.Instance.seSource.loop)
            {
                AudioManager.Instance?.StopDriveLoop();
            }
            return;
        }

        // 上キー入力を取得（DriveLoop制御用）
        bool accelInput = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);

        // カウントダウン中はIdleLoopのみ再生（今回は使用しないため、無効化中）
        if (!allowDriveLoop && raceStarted)
        {
            if (!uiManager.IsPauseMenuOpen())
            {
                //if (AudioManager.Instance != null && AudioManager.Instance.IsNotPlaying("SE_IdleLoop"))
                //{
                //    AudioManager.Instance?.HandleDriveLoop(false);
                //}
            }
            return;
        }

        // 通常走行中はDriveLoop再生
        if (allowDriveLoop)
        {
            AudioManager.Instance?.HandleDriveLoop(accelInput);
        }
    }

    // 毎フレーム後（UI表示更新・タイマー処理）
    void LateUpdate()
    {
        if (!raceStarted) return;

        raceTimer += Time.deltaTime;
        timerText.text = raceTimer.ToString("F2");

        uiManager.UpdateRaceUI(); // Lap/順位/逆走のUI更新
    }

    // レース開始判定用
    public bool IsRaceStarted() => raceStarted;

    // プレイヤー or AI がチェックポイントを通過したときに呼ばれる
    public void OnCarPassedCheckpoint(ICarInfo car, int checkpointIndex)
    {
        if (!raceStarted) return;

        bool isCorrect = car.PassCheckpoint(checkpointIndex);

        // プレイヤーでかつ正しい通過だった場合のみSE再生
        if (!isCorrect || car is not PlayerCarController) return;

        // SE再生条件：0番と1,2番は鳴らすが、3番は鳴らさない
        if (checkpointIndex == 0 || (checkpointIndex != 3 && checkpointIndex != 0))
        {
            AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_CheckpointPass"), 1.5f);
        }
    }

    // プレイヤー or AI がゴールラインを通過したときに呼ばれる
    public void OnCarReachedGoal(ICarInfo car)
    {
        if (!raceStarted || car.PassedCheckpoints.Count < totalCheckpoints) return;

        // ゴール時刻を記録（全車共通）
        if (!carFinishTimes.ContainsKey(car.DriverName)
    && car.CurrentLap >= targetLaps
    && car.PassedCheckpoints.Count >= totalCheckpoints)
        {
            float finishTime = Time.time - raceStartTime;
            carFinishTimes[car.DriverName] = finishTime;
            Debug.Log($"[記録] {car.DriverName} ゴールタイム: {finishTime:F2}秒");
        }

        //if (!carFinishTimes.ContainsKey(car.DriverName))
        //{
        //    float finishTime = Time.time - raceStartTime;
        //    carFinishTimes[car.DriverName] = finishTime;
        //    Debug.Log($"[記録] {car.DriverName} ゴールタイム: {finishTime:F2}秒");
        //}

        car.FinishLap();                // ラップ数を加算
        car.PassedCheckpoints.Clear();  // 次のラップに備えて初期化

        if (car is PlayerCarController)
        {
            playerLaps++;
            Debug.Log($"[Goal] playerLaps = {playerLaps} / targetLaps = {targetLaps}");

            if (playerLaps >= targetLaps)
            {
                FinishRace(); // ゴール処理へ
            }
            else
            {
                AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_LapComplete"), 1.5f);
                PlayStarEffect(); // 演出

                StartCoroutine(ClearLapFinishFlag());
            }

            if (playerLaps == targetLaps - 1)
                StartCoroutine(PlayFinalLapVoiceAfterDelay(0.7f)); // 最終ラップ告知
        }
    }

    // ゴール処理：リザルト登録や演出再生
    private void FinishRace()
    {
        raceStarted = false;

        if (uiManager != null && uiManager.goalText != null)
        {
            uiManager.goalText.text = "GOAL!!";
            uiManager.goalText.gameObject.SetActive(true);
        }

        AudioManager.Instance?.StopDriveLoop();

        AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_Goal"), 2.0f);
        StartCoroutine(PlayCheerAfterDelay(1.0f));
        SpawnGoalFireworks();

        // プレイヤーの正しいゴールタイムを記録してから登録
        float playerFinishTime = Time.time - raceStartTime;
        carFinishTimes[player.DriverName] = playerFinishTime;

        resultManager.RegisterFinish(player.DriverName, playerFinishTime);

        StartCoroutine(resultManager.RegisterAIFinishersAfterDelay(allCars, player.DriverName, GetProgressScore));
        StartCoroutine(resultManager.ShowResultButtonAfterDelay());

        StartCoroutine(ClearLapFinishFlag());
        StartCoroutine(UpdateUIAfterFinalLap());
    }

    // ゴール直後、フラグをリセットするための遅延処理
    private IEnumerator ClearLapFinishFlag()
    {
        yield return new WaitForSeconds(0.1f);
        if (player is PlayerCarController p)
        {
            p.JustFinishedLap = false;
        }
    }

    // 表示UIを1フレーム後に更新する（ズレ防止）
    private IEnumerator UpdateUIAfterFinalLap()
    {
        yield return null; // 1フレーム待って
        uiManager?.UpdateRaceUI(); // 明示的に表示補正状態で更新
    }

    // 星エフェクトをプレイヤーの頭上に再生
    private void PlayStarEffect()
    {
        if (starEffectPrefab != null && player != null)
        {
            GameObject fx = Instantiate(starEffectPrefab, player.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            fx.transform.SetParent(player.transform);
            Destroy(fx, 1.5f);
        }
    }

    // ゴール時の火花の演出
    private void SpawnGoalFireworks()
    {
        if (goalFireworkPrefab != null && player != null)
        {
            GameObject fx = Instantiate(goalFireworkPrefab, player.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            fx.transform.SetParent(player.transform);
            Destroy(fx, 2.0f);
        }
    }

    // 最終ラップに入ったときのSE再生
    private IEnumerator PlayFinalLapVoiceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_FinalLap"), 1.5f);
    }

    // ゴール時の歓声SE
    private IEnumerator PlayCheerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_Cheer"), 1.5f);
    }

    // 順位計算のためのスコア算出（ラップ数、通過チェックポイント、次のCPまでの距離）
    public float GetProgressScore(ICarInfo car)
    {
        // 各要素の重みを定義（ラップ > チェックポイント > 距離）
        float lapWeight = 100000f;
        float checkpointWeight = 100f;
        float distanceWeight = 1f;

        int total = totalCheckpoints;
        int lap = car.CurrentLap;
        int adjustedIndex = car.LastCheckpointIndex; // 最後に通過したチェックポイント番号

        // ゴール直後は「Lapが1加算されている & CPが0」になるため、
        // 表示上のズレ（1つ前のチェックポイント扱い）を補正する。
        if (car is PlayerCarController p && p.JustFinishedLap && adjustedIndex == 0)
        {
            lap = Mathf.Max(0, lap - 1);　// ラップ数を一時的に1つ減らす
        }

        int progressIndex = (total + 3 - adjustedIndex) % total;　// 通過したチェックポイントの進み具合
        int nextIndex = (adjustedIndex - 1 + total) % total;　　　// 次に向かうチェックポイント

        // 次のチェックポイントのオブジェクトを取得
        GameObject nextCheckpoint = GameObject.Find($"Checkpoint_{nextIndex}");
        if (nextCheckpoint == null) return 0f;

        // 自車と次チェックポイントとの距離を算出（近いほどスコアが高い）
        float distanceToNext = Vector3.Distance(car.transform.position, nextCheckpoint.transform.position);

        // スコア計算
        return (lap * lapWeight) + (progressIndex * checkpointWeight) - (distanceToNext * distanceWeight);
    }

    // リザルト画面への遷移処理
    public void GoToResultScene()
    {
        forceDisableDriveLoop = true;
        AudioManager.Instance?.StopSE();           // 通常SE停止
        AudioManager.Instance?.StopDriveLoop();    // DriveLoop停止
        SceneManager.LoadScene("ResultScene");
    }
}