using System.Collections.Generic;
using UnityEngine;

// プレイヤー操作の車両コントローラ
// ICarInfoを実装して、順位計算やチェックポイント通過処理に対応。
public class PlayerCarController : MonoBehaviour, ICarInfo
{
    [Header("基本設定")]
    [SerializeField] private float moveSpeed = 50f;        // 移動速度
    [SerializeField] private float turnSpeed = 10f;        // 回転の速さ（左右）
    [SerializeField] private float acceleration = 5f;      // 加速の滑らかさ

    [Header("エフェクト・サウンド")]
    [SerializeField] private GameObject sparkEffectPrefab; // 火花エフェクトのプレハブ
    [SerializeField] private float hornCooldown = 2.0f;    // クラクションの連続再生間隔
    [SerializeField] private float hornChance = 0.6f;      // クラクションが鳴る確率
    [SerializeField] private float crashCooldown = 1.5f;   // 衝突SEの再生間隔
    [SerializeField] private float sparkCooldown = 0.8f;   // 火花の再生成間隔

    private Rigidbody rb;
    private Vector3 targetVelocity = Vector3.zero;
    private float lapTime = 0f;
    private bool raceStarted = false;

    // エフェクト関連のタイム管理
    private float lastHornTime = -10f;
    private float lastCrashSoundTime = -10f;
    private float lastSparkTime = -10f;

    // GameManager/順位表示のために参照されるプロパティ
    public string DriverName { get; private set; }
    public int LastCheckpointIndex { get; private set; } = -1; // 順番なし、重複NGの配列 
    public int CurrentLap { get; private set; } = 0;
    public HashSet<int> PassedCheckpoints { get; private set; } = new();

    // ゴール後に表示を一時補正するためのフラグ
    public bool JustFinishedLap { get; set; } = false;
    private int lapFinishFrame = -1;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // 初期状態は動かさない

        DriverName = PlayerPrefs.GetString("PlayerName", "player");
        Debug.Log($"[PlayerCarController] ドライバー名設定: {DriverName}");
    }

    void Update()
    {
        if (raceStarted)
        {
            lapTime += Time.deltaTime;

            // デバッグ：次に通過すべきチェックポイントの番号を表示
            int total = GameManager.Instance.totalCheckpoints;
            int expected = (LastCheckpointIndex - 1 + total) % total;
            Debug.Log($"[PlayerCar] 次のチェックポイントIndex = {expected}");
        }
    }

    // 固定更新（物理挙動の処理）※物理演算に適したタイミング
    void FixedUpdate()
    {
        // 動作が無効（レース前 or 停止状態）なら何もしない
        if (rb.isKinematic) return;

        if (raceStarted)
        {
            float moveInput = Input.GetAxis("Vertical");   // 縦方向の入力
            float turnInput = Input.GetAxis("Horizontal"); // 縦方向の入力

            // 回転処理 → Rigidbody に回転速度を直接加える
            rb.angularVelocity = new Vector3(0f, turnInput * turnSpeed, 0f);

            // 前方向への速度を設定
            targetVelocity = transform.forward * moveInput * moveSpeed;

            // 現在の速度と目標速度を補間（加速を滑らかにする）
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // レース停止時は完全に静止させる
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 他のオブジェクトと衝突したときに呼ばれる処理
    void OnCollisionEnter(Collision collision)
    {
        // レースが始まっていなければ処理しない
        if (!raceStarted) return;

        // 前進入力中かをチェック（演出条件に使用）
        bool accelInput = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);

        // 衝突した最初の接触点（エフェクト用）
        ContactPoint contact = collision.contacts[0];

        // 壁との衝突 → 衝突SE＋火花
        if (collision.gameObject.CompareTag("wall"))
        {
            TryPlayCrashEffect(contact.point, contact.normal, accelInput);
        }
        // 他の車との衝突（ICarInfoを実装しているオブジェクト）
        else if (collision.gameObject.GetComponent<ICarInfo>() is ICarInfo otherCar && (Object)otherCar != this)
        {
            // 衝突相手が自分以外の車の場合 → クラクション or 火花
            // 状況に応じてクラクション・火花のいずれか、または両方を再生
            TryPlayHornAndSpark(contact.point, contact.normal, accelInput);
        }
    }

    // 他車にぶつかったときの演出（クラクション・火花）
    private void TryPlayHornAndSpark(Vector3 position, Vector3 normal, bool accelInput)
    {
        if (Time.time - lastHornTime >= hornCooldown && Random.value < hornChance)
        {
            AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_Horn"));
            lastHornTime = Time.time;
        }

        if (accelInput && Time.time - lastSparkTime >= sparkCooldown && sparkEffectPrefab != null)
        {
            Instantiate(sparkEffectPrefab, position, Quaternion.LookRotation(normal));
            lastSparkTime = Time.time;
        }
    }

    // 壁にぶつかったときの演出（SE＋火花）
    private void TryPlayCrashEffect(Vector3 position, Vector3 normal, bool accelInput)
    {
        if (accelInput && Time.time - lastCrashSoundTime >= crashCooldown)
        {
            AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_Crash"), 1.5f);
            lastCrashSoundTime = Time.time;

            if (Time.time - lastSparkTime >= sparkCooldown && sparkEffectPrefab != null)
            {
                Instantiate(sparkEffectPrefab, position, Quaternion.LookRotation(normal));
                lastSparkTime = Time.time;
            }
        }
    }

    // ゴール時のラップ加算処理（UI側で1フレーム補正あり）
    public void FinishLap()
    {
        CurrentLap++;
        JustFinishedLap = true; // ← UIで1フレームだけLap数+1表示するため
        lapFinishFrame = Time.frameCount; //追加
        Debug.Log($"Lap {CurrentLap} completed!");
    }

    // GameManager → UIManager → プレイヤー側の LateUpdate 順で動く
    // UI更新後、補正フラグを1フレーム後にオフ
    public void LateUpdate()
    {
        // Lap完了直後のフレームだけ表示補正 → その次のフレームでOFF
        if (JustFinishedLap && Time.frameCount > lapFinishFrame)
        {
            JustFinishedLap = false;
        }
    }

    // 現在のラップタイムを取得（UIやリザルト表示で使用）
    public float GetLapTime() => lapTime;

    // レース停止処理（ゲーム終了やポーズ時に使用）
    // 物理挙動を止めるために isKinematic を true に
    public void StopRace()
    {
        raceStarted = false;
        rb.isKinematic = true;
    }

    // レース再開処理（ゲーム開始やリスタート時に使用）
    // タイマーを初期化し、物理挙動を有効化
    public void RestartRace()
    {
        rb.isKinematic = false;
        lapTime = 0f;
        raceStarted = true;
    }

    // チェックポイント通過処理（順番チェック、逆走補正あり）
    public bool PassCheckpoint(int checkpointIndex)
    {
        int total = GameManager.Instance.totalCheckpoints;
        int expected = (LastCheckpointIndex == -1)
    ? 3 // スタート直後は3番（＝ゴール手前）を想定
    : (LastCheckpointIndex - 1 + total) % total;

        // 通過済みのチェックポイントの場合、何も更新せずtrueを返す
        if (PassedCheckpoints.Contains(checkpointIndex)) return true;

        // スタート直後の特例処理
        // → LastCheckpointIndex が -1（未通過）で、最初に3番通過ならOK
        if (LastCheckpointIndex == -1 && checkpointIndex == 3)
        {
            LastCheckpointIndex = 3;
            PassedCheckpoints.Add(3); //「3番を通過した」と記録
            return true;
        }

        // 逆走からの復帰処理
        // 最後に0番を通過していない状態で0番に戻ってきた場合、
        // 正しい順番で通ってないなら逆走とみなしてリセット
        if (
            LastCheckpointIndex != 0 &&
            checkpointIndex == 0 &&
            !PassedCheckpoints.SetEquals(new HashSet<int> { 3, 2, 1 }) // 正規の1周順ではない
        )
        {
            Debug.Log("Checkpoint修正：逆走後にCheckpoint 0を再スタート");
            LastCheckpointIndex = 0;
            PassedCheckpoints.Clear();
            PassedCheckpoints.Add(0);
            return true;
        }

        // 正しい順番で通過している場合のみ、チェックポイント記録
        if (checkpointIndex == expected)
        {
            LastCheckpointIndex = checkpointIndex;
            PassedCheckpoints.Add(checkpointIndex);
            return true;
        }

        // 間違った順番で通過した場合は無効
        return false;
    }

}
