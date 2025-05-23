using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// ���_�p��AI�J�[�B�w�K�͍s�킸�A����ƕ�V�v�Z�̂ݒS���B
// GameManager���璼�ڐ��䂳��A���[�X�V�[���Ŏg�p�����B
public class CarAgent : Agent, ICarInfo
{
    public float speed = 30f;            // �ő呬�x
    // public float torque = 10f;           // ��]�́i���g�p�j
    public float acceleration = 5f;      // ������

    private Rigidbody rb;
    private Vector3 targetVelocity;
    private Transform _track;            // ���݂̃^�C��
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    // ���ʌv�Z�E�S�[������p
    public int LastCheckpointIndex { get; private set; } = -1;
    public int CurrentLap { get; private set; } = 0;
    public HashSet<int> PassedCheckpoints { get; private set; } = new();
    public string DriverName { get; private set; } = "AICar";

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        GetTrackIncrement(); // ���������ɑ����̃^�C�������o
    }

    public override void OnEpisodeBegin()
    {
        // �w�K�p�ł͎g�����A���_���͉������Ȃ�
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // trackForward�F�����̃^�C���iTrackTile�j�́u�i�s�����v        
        Vector3 trackForward = transform.forward;

        // _track���j���ς݂��ǂ����m�F���Ă���g��
        if (_track != null && _track.gameObject != null)
        {
            trackForward = _track.forward;
        }

        // �u�i�s�����v�Ɓu�������Ă�����v�̂Ȃ��p�� -180��~+180�� �Ŏ擾
        float angle = Vector3.SignedAngle(trackForward, transform.forward, Vector3.up);
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
        if (!GameManager.Instance.IsRaceStarted()) return;

        // ������́i�A���l�j�F[0]�����E, [1]���O�i���
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        // �����̃^�C�����؂�ւ�������V�����Z�i������ +1, �t���� -1�j
        int reward = GetTrackIncrement();

        Vector3 moveVec = transform.position - lastPos;
        float angle = (_track != null && _track.gameObject != null)
            ? Vector3.Angle(moveVec, _track.forward)
            : 0f;

        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus + reward);

        // �����Ȏ��ԃy�i���e�B
           AddReward(-0.001f); 
    }

    // �Ԃ̈ړ�����
    private void MoveCar(float horizontal, float vertical, float dt)
    {
        Vector3 desiredVelocity = transform.forward * vertical * speed;
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * dt);
        rb.angularVelocity = new Vector3(0f, horizontal * 10f, 0f);
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

            // ������ newHit �͖���
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
        //    SetReward(-1f); // �ǏՓˎ��̃y�i���e�B�i�G�s�\�[�h�I�����Ȃ��j
        //}
    }

    // �`�F�b�N�|�C���g�ʉߏ����i�t���␳�⏉�����܂ށj
    public bool PassCheckpoint(int checkpointIndex)
    {
        int total = GameManager.Instance.totalCheckpoints;
        int expected = (LastCheckpointIndex == -1)
            ? 3
            : (LastCheckpointIndex - 1 + total) % total;

        // �ʉߍς݂Ȃ�X�L�b�v
        if (PassedCheckpoints.Contains(checkpointIndex)) return true;

        // �X�^�[�g����̓���
        if (LastCheckpointIndex == -1 && checkpointIndex == 3)
        {
            LastCheckpointIndex = 3;
            PassedCheckpoints.Add(3);
            return true;
        }

        // �t������̕��A�����i�v���C���[�Ɠ��������j
        if (
            LastCheckpointIndex != 0 &&
            checkpointIndex == 0 &&
            !PassedCheckpoints.SetEquals(new HashSet<int> { 3, 2, 1 })
        )
        {
            Debug.Log($"[CarAgent] {DriverName} �� Checkpoint�C���F�t�����Checkpoint 0���ăX�^�[�g");
            LastCheckpointIndex = 0;
            PassedCheckpoints.Clear();
            PassedCheckpoints.Add(0);
            return true;
        }

        // ���������Ԃł̒ʉ�
        if (checkpointIndex == expected)
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
        Debug.Log($"[AICar] {DriverName} Lap {CurrentLap} ����");

        if (CurrentLap >= GameManager.Instance.targetLaps)
        {
            Debug.Log($"[AICar] {DriverName} �� �S�[��");

            // �S�[���������L�^
            float finishTime = Time.time - GameManager.Instance.GetRaceStartTime(); // raceStartTime�擾�p�Ƀ��\�b�h��p��
            GameManager.Instance.carFinishTimes[DriverName] = finishTime;
        }
    }

    // �L�[�{�[�h����i�e�X�g�p�j
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }
}
