using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// �����w�K�p��AI�J�[���[�T�[�i�o�����X�^�j�B
/// �g���b�N��i�s�����ɉ����đ��s���A�`�F�b�N�|�C���g��ʉ߁E������ڎw���B
public class ModelDriven_CarAgent : Agent //, ICarInfo
{
    public float speed = 30f;        // �ő呬�x
    public float torque = 10f;       // ��]�̉s��
    public float acceleration = 5f;  // ������

    private Rigidbody rb;
    private Transform _track;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public bool resetOnCollision = true;  // �ǏՓˎ��ɃG�s�\�[�h�I�����邩
    public bool isTraining = true;        // �g���[�j���O���[�h���ifalse�Ȃ琄�_�p�j

    //public int LastCheckpointIndex { get; private set; } = -1;
    //public int CurrentLap { get; private set; } = 0;
    //public HashSet<int> PassedCheckpoints { get; private set; } = new HashSet<int>();
    public string DriverName { get; private set; } = "AICar";

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // ���ǉ��I
        //_startPosition = transform.localPosition;
        //_startRotation = transform.localRotation;

        GetTrackIncrement(); // �����g���b�N�ݒ�
    }

    //public override void OnEpisodeBegin()
    //{
    //    if (!isTraining) return;

    //    // �����ʒu�Ƀ��Z�b�g
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
        // �i�s�����Ƃ̊p�x�i���ʂȂ�0�j
        Vector3 trackForward = transform.forward;

        if (_track != null && _track.gameObject != null)
        {
            trackForward = _track.forward;
        }

        float angle = Vector3.SignedAngle(trackForward, transform.forward, Vector3.up);
        sensor.AddObservation(angle / 180f);

        // �����Z���T�[�i�O���E�΂߁j
        sensor.AddObservation(ObserveRay(1.5f, 0f, 0f));         // �O
        sensor.AddObservation(ObserveRay(1.5f, 1.0f, 30f));      // �E�O
        sensor.AddObservation(ObserveRay(1.5f, -1.0f, -30f));    // ���O
        sensor.AddObservation(ObserveRay(2.5f, 0f, 0f));         // �������O

        float rightNear = ObserveRay(1.5f, 2.0f, 45f);
        float leftNear = ObserveRay(1.5f, -2.0f, -45f);
        sensor.AddObservation(rightNear);
        sensor.AddObservation(leftNear);

        // �ǐڋߎ��̃y�i���e�B�i�����j
        float proximityPenalty = (Mathf.Clamp01(1f - rightNear) + Mathf.Clamp01(1f - leftNear)) * 0.05f * (speed / 30f);
        AddReward(-proximityPenalty);

        // ���݂̑��x
        sensor.AddObservation(rb.velocity.magnitude / 30f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        // ��V�v�Z�F�i�s�����ɉ����Đi�񂾂�
        int reward = GetTrackIncrement();
        Vector3 moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, _track.forward);

        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);
        AddReward(-0.001f); // ���ԃy�i���e�B

        // ��~�y�i���e�B
        if (isTraining && rb.velocity.magnitude < 1f)
        {
            AddReward(-0.001f);
        }
    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        // �O�i�E��ނ̑��x�ݒ�
        Vector3 desiredVelocity = transform.forward * vertical * speed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * dt);

        // ��]�ݒ�i�Œ�l�Ŋȗ����j
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

        // ���ԂƂ̏Փ˕�V�͎g�p���Ȃ�
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
