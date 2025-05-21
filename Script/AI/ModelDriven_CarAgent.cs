using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// 強化学習用のAIカーレーサー（バランス型）。
/// トラックを進行方向に沿って走行し、チェックポイントを通過・完走を目指す。
public class ModelDriven_CarAgent : Agent //, ICarInfo
{
    public float speed = 30f;        // 最大速度
    public float torque = 10f;       // 回転の鋭さ
    public float acceleration = 5f;  // 加速力

    private Rigidbody rb;
    private Transform _track;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public bool resetOnCollision = true;  // 壁衝突時にエピソード終了するか
    public bool isTraining = true;        // トレーニングモードか（falseなら推論用）

    //public int LastCheckpointIndex { get; private set; } = -1;
    //public int CurrentLap { get; private set; } = 0;
    //public HashSet<int> PassedCheckpoints { get; private set; } = new HashSet<int>();
    public string DriverName { get; private set; } = "AICar";

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // ←追加！
        //_startPosition = transform.localPosition;
        //_startRotation = transform.localRotation;

        GetTrackIncrement(); // 初期トラック設定
    }

    //public override void OnEpisodeBegin()
    //{
    //    if (!isTraining) return;

    //    // 初期位置にリセット
    //    transform.localPosition = _startPosition;
    //    transform.localRotation = _startRotation;
    //    rb.velocity = Vector3.zero;
    //    rb.angularVelocity = Vector3.zero;

    //    CurrentLap = 0;
    //    LastCheckpointIndex = -1;
    //    PassedCheckpoints.Clear();

    //    Tr_GameManager.Instance.agentStartTimes[this] = Time.time;
    //}

    public override void CollectObservations(VectorSensor sensor)
    {
        // 進行方向との角度（正面なら0）
        Vector3 trackForward = transform.forward;

        if (_track != null && _track.gameObject != null)
        {
            trackForward = _track.forward;
        }

        float angle = Vector3.SignedAngle(trackForward, transform.forward, Vector3.up);
        sensor.AddObservation(angle / 180f);

        // 距離センサー（前方・斜め）
        sensor.AddObservation(ObserveRay(1.5f, 0f, 0f));         // 前
        sensor.AddObservation(ObserveRay(1.5f, 1.0f, 30f));      // 右前
        sensor.AddObservation(ObserveRay(1.5f, -1.0f, -30f));    // 左前
        sensor.AddObservation(ObserveRay(2.5f, 0f, 0f));         // 長距離前

        float rightNear = ObserveRay(1.5f, 2.0f, 45f);
        float leftNear = ObserveRay(1.5f, -2.0f, -45f);
        sensor.AddObservation(rightNear);
        sensor.AddObservation(leftNear);

        // 壁接近時のペナルティ（微小）
        float proximityPenalty = (Mathf.Clamp01(1f - rightNear) + Mathf.Clamp01(1f - leftNear)) * 0.05f * (speed / 30f);
        AddReward(-proximityPenalty);

        // 現在の速度
        sensor.AddObservation(rb.velocity.magnitude / 30f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        // 報酬計算：進行方向に沿って進んだか
        int reward = GetTrackIncrement();
        Vector3 moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, _track.forward);

        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);
        AddReward(-0.001f); // 時間ペナルティ

        // 停止ペナルティ
        if (isTraining && rb.velocity.magnitude < 1f)
        {
            AddReward(-0.001f);
        }
    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        // 前進・後退の速度設定
        Vector3 desiredVelocity = transform.forward * vertical * speed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * dt);

        // 回転設定（固定値で簡略化）
        rb.angularVelocity = new Vector3(0f, horizontal * torque, 0f);
    }

    private float ObserveRay(float z, float x, float angle)
    {
        Vector3 raySource = transform.position + Vector3.up / 2f;
        Vector3 position = raySource + transform.forward * z + transform.right * x;
        Vector3 direction = Quaternion.Euler(0, angle, 0f) * transform.forward;

        const float RAY_DIST = 15f;
        Physics.Raycast(position, direction, out var hit, RAY_DIST);

        return hit.distance >= 0 ? hit.distance / RAY_DIST : -1f;
    }

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
        //if (other.gameObject.CompareTag("wall"))
        //{
        //    SetReward(-1f);
        //    //if (resetOnCollision) EndEpisode();
        //}

        // 他車との衝突報酬は使用しない
    }

    //public bool PassCheckpoint(int checkpointIndex)
    //{
    //    int total = GameManager.Instance.totalCheckpoints;

    //    if ((LastCheckpointIndex + 1) % total == checkpointIndex)
    //    {
    //        LastCheckpointIndex = checkpointIndex;
    //        PassedCheckpoints.Add(checkpointIndex);
    //        return true;
    //    }
    //    return false;
    //}


    //public void FinishLap()
    //{
    //    CurrentLap++;
    //    Debug.Log($"[TrainingAgent] {DriverName} Lap {CurrentLap} completed!");
    //}

    //public override void Heuristic(in ActionBuffers actionsOut)
    //{
    //    var ca = actionsOut.ContinuousActions;
    //    ca[0] = Input.GetAxis("Horizontal");
    //    ca[1] = Input.GetAxis("Vertical");
    //}
}
