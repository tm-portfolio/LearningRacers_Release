using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// 推論用のAIカー。学習は行わず、動作と報酬計算のみ担当。
// GameManagerから直接制御され、レースシーンで使用される。
public class CarAgent : Agent, ICarInfo
{
    public float speed = 30f;            // 最大速度
    // public float torque = 10f;           // 回転力（未使用）
    public float acceleration = 5f;      // 加速力

    private Rigidbody rb;
    private Vector3 targetVelocity;
    private Transform _track;            // 現在のタイル
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    // 順位計算・ゴール判定用
    public int LastCheckpointIndex { get; private set; } = -1;
    public int CurrentLap { get; private set; } = 0;
    public HashSet<int> PassedCheckpoints { get; private set; } = new();
    public string DriverName { get; private set; } = "AICar";

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        GetTrackIncrement(); // 初期化時に足元のタイルを検出
    }

    public override void OnEpisodeBegin()
    {
        // 学習用では使うが、推論時は何もしない
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // trackForward：足元のタイル（TrackTile）の「進行方向」        
        Vector3 trackForward = transform.forward;

        // _trackが破棄済みかどうか確認してから使う
        if (_track != null && _track.gameObject != null)
        {
            trackForward = _track.forward;
        }

        // 「進行方向」と「今向いてる方向」のなす角を -180°~+180° で取得
        float angle = Vector3.SignedAngle(trackForward, transform.forward, Vector3.up);
        sensor.AddObservation(angle / 180f); // タイルとの角度を [-1, 1] で観測（-1.0 ~ +1.0 に正規化）

        // Rayによる障害物距離観測（6方向ローカル座標（車体基準））
        // ObserveRay(z, x, angle)
        sensor.AddObservation(ObserveRay(1.5f, 0f, 0f));        // 真正面
        sensor.AddObservation(ObserveRay(1.5f, 1.0f, 30f));     // 前右斜め
        sensor.AddObservation(ObserveRay(1.5f, -1.0f, -30f));   // 前左斜め
        sensor.AddObservation(ObserveRay(2.5f, 0f, 0f));        // 長距離の前方

        float rightNear = ObserveRay(1.5f, 2.0f, 45f);
        float leftNear = ObserveRay(1.5f, -2.0f, -45f);
        sensor.AddObservation(rightNear);                       // 右前斜め（広め）
        sensor.AddObservation(leftNear);                        // 左前斜め（広め）

        // 壁に近づきすぎたときのペナルティ（左右から計算）
        float proximityPenalty = (Mathf.Clamp01(1f - rightNear) + Mathf.Clamp01(1f - leftNear)) * 0.05f * (speed / 30f);
        AddReward(-proximityPenalty);

        // 現在速度を観測
        sensor.AddObservation(rb.velocity.magnitude / 30f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!GameManager.Instance.IsRaceStarted()) return;

        // 操作入力（連続値）：[0]＝左右, [1]＝前進後退
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        // 足元のタイルが切り替わったら報酬を加算（順方向 +1, 逆方向 -1）
        int reward = GetTrackIncrement();

        Vector3 moveVec = transform.position - lastPos;
        float angle = (_track != null && _track.gameObject != null)
            ? Vector3.Angle(moveVec, _track.forward)
            : 0f;

        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);

        // 微小な時間ペナルティ
           AddReward(-0.001f); 
    }

    // 車の移動処理
    private void MoveCar(float horizontal, float vertical, float dt)
    {
        Vector3 desiredVelocity = transform.forward * vertical * speed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * dt);
        rb.angularVelocity = new Vector3(0f, horizontal * 10f, 0f);
    }

    // Rayを飛ばして障害物までの距離を取得（なければ -1f）
    private float ObserveRay(float z, float x, float angle)
    {
        Vector3 raySource = transform.position + Vector3.up / 2f;
        Vector3 position = raySource + transform.forward * z + transform.right * x;
        Vector3 direction = Quaternion.Euler(0, angle, 0f) * transform.forward;

        const float RAY_DIST = 15f;
        Physics.Raycast(position, direction, out var hit, RAY_DIST);

        return hit.distance >= 0 ? hit.distance / RAY_DIST : -1f;
    }

    // 足元のタイル（TrackTile）を切り替えたときに報酬を与える
    private int GetTrackIncrement()
    {
        int reward = 0;
        var center = transform.position + Vector3.up;

        if (Physics.Raycast(center, Vector3.down, out var hit, 2f))
        {
            var newHit = hit.transform;

            // 無効な newHit は無視
            if (newHit == null || newHit.gameObject == null)
                return 0;

            if (_track != null && newHit != _track)
            {
                float angle = Vector3.Angle(_track.forward, newHit.position - _track.position);
                reward = (angle < 90f) ? 1 : -1;
            }

            _track = newHit;
        }

        return reward;
    }

    private void OnCollisionEnter(Collision other)
    {
        //if (other.gameObject.CompareTag("wall"))
        //{
        //    SetReward(-1f); // 壁衝突時のペナルティ（エピソード終了しない）
        //}
    }

    // チェックポイント通過処理（逆走補正や初期化含む）
    public bool PassCheckpoint(int checkpointIndex)
    {
        int total = GameManager.Instance.totalCheckpoints;
        int expected = (LastCheckpointIndex == -1)
            ? 3
            : (LastCheckpointIndex - 1 + total) % total;

        // 通過済みならスキップ
        if (PassedCheckpoints.Contains(checkpointIndex)) return true;

        // スタート直後の特例
        if (LastCheckpointIndex == -1 && checkpointIndex == 3)
        {
            LastCheckpointIndex = 3;
            PassedCheckpoints.Add(3);
            return true;
        }

        // 逆走からの復帰処理（プレイヤーと同じ条件）
        if (
            LastCheckpointIndex != 0 &&
            checkpointIndex == 0 &&
            !PassedCheckpoints.SetEquals(new HashSet<int> { 3, 2, 1 })
        )
        {
            Debug.Log($"[CarAgent] {DriverName} → Checkpoint修正：逆走後にCheckpoint 0を再スタート");
            LastCheckpointIndex = 0;
            PassedCheckpoints.Clear();
            PassedCheckpoints.Add(0);
            return true;
        }

        // 正しい順番での通過
        if (checkpointIndex == expected)
        {
            LastCheckpointIndex = checkpointIndex;
            PassedCheckpoints.Add(checkpointIndex);
            return true;
        }

        return false;
    }

    // ラップ完了処理（ログ出力あり）
    public void FinishLap()
    {
        CurrentLap++;
        Debug.Log($"[AICar] {DriverName} Lap {CurrentLap} 完了");

        if (CurrentLap >= GameManager.Instance.targetLaps)
        {
            Debug.Log($"[AICar] {DriverName} → ゴール");

            // ゴール時刻を記録
            float finishTime = Time.time - GameManager.Instance.GetRaceStartTime(); // raceStartTime取得用にメソッドを用意
            GameManager.Instance.carFinishTimes[DriverName] = finishTime;
        }
    }

    // キーボード操作（テスト用）
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }
}
