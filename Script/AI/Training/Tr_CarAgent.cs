using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// �����w�K�p��AI�J�[�B
// �g���b�N��i�s�����ɉ����đ��s���A�`�F�b�N�|�C���g��ʉ߁E������ڎw���B
public class Tr_CarAgent : Agent, ICarInfo
{
    public float speed = 30f;        // �ő呬�x
    public float torque = 10f;       // ��]��
    public float acceleration = 5f;  // ������

    private Rigidbody rb;
    private Transform _track;            // ���݂̃^�C��
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public bool resetOnCollision = true;  // �ǏՓˎ��ɃG�s�\�[�h�I�����邩
    public bool isTraining = true;        // �g���[�j���O���[�h��

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
        GetTrackIncrement(); // ���������ɑ����̃^�C�������o
    }

    public override void OnEpisodeBegin()
    {
        if (!isTraining) return;

        // �����ʒu�Ƀ��Z�b�g
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
        // �u�i�s�����v�Ɓu�������Ă�����v�̂Ȃ��p�� -180��~+180�� �Ŏ擾
        float angle = Vector3.SignedAngle(_track?.forward ?? transform.forward, transform.forward, Vector3.up);
        sensor.AddObservation(angle / 180f); // �^�C���Ƃ̊p�x�� [-1, 1] �Ŋϑ��i-1.0 ~ +1.0 �ɐ��K���j

        // Ray�ɂ���Q�������ϑ��i6�������[�J�����W�i�ԑ̊�j�j
        // ObserveRay(z, x, angle)
        sensor.AddObservation(ObserveRay(1.5f, 0f, 0f));        // �^����
        sensor.AddObservation(ObserveRay(1.5f, 1.0f, 30f));     // �O�E�΂�
        sensor.AddObservation(ObserveRay(1.5f, -1.0f, -30f));   // �O���΂�
        sensor.AddObservation(ObserveRay(2.5f, 0f, 0f));        // �������̑O��

        float rightNear = ObserveRay(1.5f, 2.0f, 45f);
        float leftNear = ObserveRay(1.5f, -2.0f, -45f);
        sensor.AddObservation(rightNear);                       // �E�O�΂߁i�L�߁j
        sensor.AddObservation(leftNear);                        // ���O�΂߁i�L�߁j

        // �ǂɋ߂Â��������Ƃ��̃y�i���e�B�i���E����v�Z�j
        float proximityPenalty = (Mathf.Clamp01(1f - rightNear) + Mathf.Clamp01(1f - leftNear)) * 0.05f * (speed / 30f);
        AddReward(-proximityPenalty);

        // ���ݑ��x���ϑ�
        sensor.AddObservation(rb.velocity.magnitude / 30f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // ������́i�A���l�j�F[0]�����E, [1]���O�i���
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        // �����̃^�C�����؂�ւ�������V�����Z�i������ +1, �t���� -1�j
        int reward = GetTrackIncrement();
        Vector3 moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, _track.forward);

        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);

        // �����Ȏ��ԃy�i���e�B
        AddReward(-0.001f);

        // ��~�y�i���e�B
        if (isTraining && rb.velocity.magnitude < 1f)
        {
            AddReward(-0.001f);
        }
    }

    // �Ԃ̈ړ�����
    private void MoveCar(float horizontal, float vertical, float dt)
    {
        // �O�i�E��ނ̑��x
        Vector3 desiredVelocity = transform.forward * vertical * speed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * dt);

        // ��]�ݒ�
        rb.angularVelocity = new Vector3(0f, horizontal * torque, 0f);
    }

    // Ray���΂��ď�Q���܂ł̋������擾�i�Ȃ���� -1f�j
    private float ObserveRay(float z, float x, float angle)
    {
        Vector3 raySource = transform.position + Vector3.up / 2f;
        Vector3 position = raySource + transform.forward * z + transform.right * x;
        Vector3 direction = Quaternion.Euler(0, angle, 0f) * transform.forward;

        const float RAY_DIST = 15f;
        Physics.Raycast(position, direction, out var hit, RAY_DIST);

        return hit.distance >= 0 ? hit.distance / RAY_DIST : -1f;
    }

    // �����̃^�C���iTrackTile�j��؂�ւ����Ƃ��ɕ�V��^����
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

        // ���ԂƂ̏Փ˕�V�͍���͎g�p���Ȃ�
    }

    // �`�F�b�N�|�C���g�ʉߏ����i�t���␳�⏉�����܂ށj
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

    // ���b�v���������i���O�o�͂���j
    public void FinishLap()
    {
        CurrentLap++;
        Debug.Log($"[TrainingAgent] {DriverName} Lap {CurrentLap} completed!");
    }

    // �L�[�{�[�h����i�e�X�g�p�j
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }
}
