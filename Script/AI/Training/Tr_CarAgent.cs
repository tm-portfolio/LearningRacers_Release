using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// 強化学習用のAIカー。
// トラックを進行方向に沿って走行し、チェックポイントを通過・完走を目指す。
public class Tr_CarAgent : Agent, ICarInfo
{
    public float speed = 30f;        // 最大速度
    public float torque = 10f;       // 回転力
    public float acceleration = 5f;  // 加速力

    private Rigidbody rb;
    private Transform _track;            // 現在のタイル
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public bool resetOnCollision = true;  // 壁衝突時にエピソード終了するか
    public bool isTraining = true;        // トレーニングモードか

    public int LastCheckpointIndex { get; private set; } = -1;
    public int CurrentLap { get; private set; } = 0;
    public HashSet<int> PassedCheckpoints { get; private set; } = new HashSet<int>();
    public string DriverName { get; private set; } = "AICar";

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        GetTrackIncrement(); // 初期化時に足元のタイルを検出
    }

    public override void OnEpisodeBegin()
    {
        if (!isTraining) return;

        // 初期位置にリセット
        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        CurrentLap = 0;
        LastCheckpointIndex = -1;
        PassedCheckpoints.Clear();

        Tr_GameManager.Instance.agentStartTimes[this] = Time.time;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 「進行方向」と「今向いてる方向」のなす角を -180°~+180° で取得
        float angle = Vector3.SignedAngle(_track?.forward ?? transform.forward, transform.forward, Vector3.up);
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
        // 操作入力（連続値）：[0]＝左右, [1]＝前進後退
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        // 足元のタイルが切り替わったら報酬を加算（順方向 +1, 逆方向 -1）
        int reward = GetTrackIncrement();
        Vector3 moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, _track.forward);

        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);

        // 微小な時間ペナルティ
        AddReward(-0.001f);

        // 停止ペナルティ
        if (isTraining && rb.velocity.magnitude < 1f)
        {
            AddReward(-0.001f);
        }
    }

    // 車の移動処理
    private void MoveCar(float horizontal, float vertical, float dt)
    {
        // 前進・後退の速度
        Vector3 desiredVelocity = transform.forward * vertical * speed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * dt);

        // 回転設定
        rb.angularVelocity = new Vector3(0f, horizontal * torque, 0f);
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
        if (other.gameObject.CompareTag("wall"))
        {
            SetReward(-1f);
            if (resetOnCollision) EndEpisode();
        }

        // 他車との衝突報酬は今回は使用しない
    }

    // チェックポイント通過処理（逆走補正や初期化含む）
    public bool PassCheckpoint(int checkpointIndex)
    {
        int total = Tr_GameManager.Instance.totalCheckpoints;

        if ((LastCheckpointIndex + 1) % total == checkpointIndex)
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
        Debug.Log($"[TrainingAgent] {DriverName} Lap {CurrentLap} completed!");
    }

    // キーボード操作（テスト用）
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }
}
