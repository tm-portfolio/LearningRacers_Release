using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// スプライン対応AIカーレーサー
public class Tr_CarAgent_spline : Agent, ICarInfo
{
    public float speed = 30f;
    public float torque = 10f;
    public float acceleration = 5f;

    private Rigidbody rb;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public Transform[] splinePoints; // スプライン制御点
    private int nearestIndex = 0;
    private float progress = 0f;

    public int LastCheckpointIndex { get; private set; } = -1;
    public int CurrentLap { get; private set; } = 0;
    public HashSet<int> PassedCheckpoints { get; private set; } = new();
    public string DriverName { get; private set; } = "SplineAgent";

    // 必要ならダミーでもOK
    public bool PassCheckpoint(int checkpointIndex) => false;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        progress = 0f;
        nearestIndex = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 closestDir = GetSplineDirection();
        float angle = Vector3.SignedAngle(closestDir, transform.forward, Vector3.up);
        sensor.AddObservation(angle / 180f);
        sensor.AddObservation(rb.velocity.magnitude / 30f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float h = actions.ContinuousActions[0];
        float v = actions.ContinuousActions[1];
        MoveCar(h, v, Time.fixedDeltaTime);

        float reward = UpdateProgressAndReward();
        AddReward(reward);
        AddReward(-0.001f); // 時間ペナルティ
        Debug.Log($"[Progress] {progress:F2}");

    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        Vector3 desiredVelocity = transform.forward * vertical * speed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * dt);
        rb.angularVelocity = new Vector3(0f, horizontal * torque, 0f);
    }

    private Vector3 GetSplineDirection()
    {
        if (splinePoints.Length < 2) return transform.forward;
        nearestIndex = FindNearestSplineIndex();
        int next = (nearestIndex + 1) % splinePoints.Length;
        return (splinePoints[next].position - splinePoints[nearestIndex].position).normalized;
    }

    private int FindNearestSplineIndex()
    {
        float minDist = float.MaxValue;
        int result = 0;
        for (int i = 0; i < splinePoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, splinePoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                result = i;
            }
        }
        return result;
    }

    private float UpdateProgressAndReward()
    {
        Vector3 current = splinePoints[nearestIndex].position;
        Vector3 next = splinePoints[(nearestIndex + 1) % splinePoints.Length].position;
        Vector3 toAgent = transform.position - current;
        Vector3 segment = next - current;
        float projected = Vector3.Dot(toAgent, segment.normalized);
        float clamped = Mathf.Clamp01(projected / segment.magnitude);

        float newProgress = nearestIndex + clamped;
        float delta = newProgress - progress;
        progress = Mathf.Max(progress, newProgress);
        return delta * 0.1f; // スケール調整された報酬
    }

    public void FinishLap()
    {
        CurrentLap++;
        Debug.Log($"[SplineAgent] Lap {CurrentLap} completed!");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }
}
