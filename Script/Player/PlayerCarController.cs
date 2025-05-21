using System.Collections.Generic;
using UnityEngine;

// �v���C���[����̎ԗ��R���g���[��
// ICarInfo���������āA���ʌv�Z��`�F�b�N�|�C���g�ʉߏ����ɑΉ��B
public class PlayerCarController : MonoBehaviour, ICarInfo
{
    [Header("��{�ݒ�")]
    [SerializeField] private float moveSpeed = 50f;        // �ړ����x
    [SerializeField] private float turnSpeed = 10f;        // ��]�̑����i���E�j
    [SerializeField] private float acceleration = 5f;      // �����̊��炩��

    [Header("�G�t�F�N�g�E�T�E���h")]
    [SerializeField] private GameObject sparkEffectPrefab; // �ΉԃG�t�F�N�g�̃v���n�u
    [SerializeField] private float hornCooldown = 2.0f;    // �N���N�V�����̘A���Đ��Ԋu
    [SerializeField] private float hornChance = 0.6f;      // �N���N�V��������m��
    [SerializeField] private float crashCooldown = 1.5f;   // �Փ�SE�̍Đ��Ԋu
    [SerializeField] private float sparkCooldown = 0.8f;   // �ΉԂ̍Đ����Ԋu

    private Rigidbody rb;
    private Vector3 targetVelocity = Vector3.zero;
    private float lapTime = 0f;
    private bool raceStarted = false;

    // �G�t�F�N�g�֘A�̃^�C���Ǘ�
    private float lastHornTime = -10f;
    private float lastCrashSoundTime = -10f;
    private float lastSparkTime = -10f;

    // GameManager/���ʕ\���̂��߂ɎQ�Ƃ����v���p�e�B
    public string DriverName { get; private set; }
    public int LastCheckpointIndex { get; private set; } = -1; // ���ԂȂ��A�d��NG�̔z�� 
    public int CurrentLap { get; private set; } = 0;
    public HashSet<int> PassedCheckpoints { get; private set; } = new();

    // �S�[����ɕ\�����ꎞ�␳���邽�߂̃t���O
    public bool JustFinishedLap { get; set; } = false;
    private int lapFinishFrame = -1;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // ������Ԃ͓������Ȃ�

        DriverName = PlayerPrefs.GetString("PlayerName", "player");
        Debug.Log($"[PlayerCarController] �h���C�o�[���ݒ�: {DriverName}");
    }

    void Update()
    {
        if (raceStarted)
        {
            lapTime += Time.deltaTime;

            // �f�o�b�O�F���ɒʉ߂��ׂ��`�F�b�N�|�C���g�̔ԍ���\��
            int total = GameManager.Instance.totalCheckpoints;
            int expected = (LastCheckpointIndex - 1 + total) % total;
            Debug.Log($"[PlayerCar] ���̃`�F�b�N�|�C���gIndex = {expected}");
        }
    }

    // �Œ�X�V�i���������̏����j���������Z�ɓK�����^�C�~���O
    void FixedUpdate()
    {
        // ���삪�����i���[�X�O or ��~��ԁj�Ȃ牽�����Ȃ�
        if (rb.isKinematic) return;

        if (raceStarted)
        {
            float moveInput = Input.GetAxis("Vertical");   // �c�����̓���
            float turnInput = Input.GetAxis("Horizontal"); // �c�����̓���

            // ��]���� �� Rigidbody �ɉ�]���x�𒼐ډ�����
            rb.angularVelocity = new Vector3(0f, turnInput * turnSpeed, 0f);

            // �O�����ւ̑��x��ݒ�
            targetVelocity = transform.forward * moveInput * moveSpeed;

            // ���݂̑��x�ƖڕW���x���ԁi���������炩�ɂ���j
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // ���[�X��~���͊��S�ɐÎ~������
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // ���̃I�u�W�F�N�g�ƏՓ˂����Ƃ��ɌĂ΂�鏈��
    void OnCollisionEnter(Collision collision)
    {
        // ���[�X���n�܂��Ă��Ȃ���Ώ������Ȃ�
        if (!raceStarted) return;

        // �O�i���͒������`�F�b�N�i���o�����Ɏg�p�j
        bool accelInput = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);

        // �Փ˂����ŏ��̐ڐG�_�i�G�t�F�N�g�p�j
        ContactPoint contact = collision.contacts[0];

        // �ǂƂ̏Փ� �� �Փ�SE�{�Ή�
        if (collision.gameObject.CompareTag("wall"))
        {
            TryPlayCrashEffect(contact.point, contact.normal, accelInput);
        }
        // ���̎ԂƂ̏ՓˁiICarInfo���������Ă���I�u�W�F�N�g�j
        else if (collision.gameObject.GetComponent<ICarInfo>() is ICarInfo otherCar && (Object)otherCar != this)
        {
            // �Փˑ��肪�����ȊO�̎Ԃ̏ꍇ �� �N���N�V���� or �Ή�
            // �󋵂ɉ����ăN���N�V�����E�ΉԂ̂����ꂩ�A�܂��͗������Đ�
            TryPlayHornAndSpark(contact.point, contact.normal, accelInput);
        }
    }

    // ���ԂɂԂ������Ƃ��̉��o�i�N���N�V�����E�Ήԁj
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

    // �ǂɂԂ������Ƃ��̉��o�iSE�{�Ήԁj
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

    // �S�[�����̃��b�v���Z�����iUI����1�t���[���␳����j
    public void FinishLap()
    {
        CurrentLap++;
        JustFinishedLap = true; // �� UI��1�t���[������Lap��+1�\�����邽��
        lapFinishFrame = Time.frameCount; //�ǉ�
        Debug.Log($"Lap {CurrentLap} completed!");
    }

    // GameManager �� UIManager �� �v���C���[���� LateUpdate ���œ���
    // UI�X�V��A�␳�t���O��1�t���[����ɃI�t
    public void LateUpdate()
    {
        // Lap��������̃t���[�������\���␳ �� ���̎��̃t���[����OFF
        if (JustFinishedLap && Time.frameCount > lapFinishFrame)
        {
            JustFinishedLap = false;
        }
    }

    // ���݂̃��b�v�^�C�����擾�iUI�⃊�U���g�\���Ŏg�p�j
    public float GetLapTime() => lapTime;

    // ���[�X��~�����i�Q�[���I����|�[�Y���Ɏg�p�j
    // �����������~�߂邽�߂� isKinematic �� true ��
    public void StopRace()
    {
        raceStarted = false;
        rb.isKinematic = true;
    }

    // ���[�X�ĊJ�����i�Q�[���J�n�⃊�X�^�[�g���Ɏg�p�j
    // �^�C�}�[�����������A����������L����
    public void RestartRace()
    {
        rb.isKinematic = false;
        lapTime = 0f;
        raceStarted = true;
    }

    // �`�F�b�N�|�C���g�ʉߏ����i���ԃ`�F�b�N�A�t���␳����j
    public bool PassCheckpoint(int checkpointIndex)
    {
        int total = GameManager.Instance.totalCheckpoints;
        int expected = (LastCheckpointIndex == -1)
    ? 3 // �X�^�[�g�����3�ԁi���S�[����O�j��z��
    : (LastCheckpointIndex - 1 + total) % total;

        // �ʉߍς݂̃`�F�b�N�|�C���g�̏ꍇ�A�����X�V����true��Ԃ�
        if (PassedCheckpoints.Contains(checkpointIndex)) return true;

        // �X�^�[�g����̓��Ꮘ��
        // �� LastCheckpointIndex �� -1�i���ʉ߁j�ŁA�ŏ���3�Ԓʉ߂Ȃ�OK
        if (LastCheckpointIndex == -1 && checkpointIndex == 3)
        {
            LastCheckpointIndex = 3;
            PassedCheckpoints.Add(3); //�u3�Ԃ�ʉ߂����v�ƋL�^
            return true;
        }

        // �t������̕��A����
        // �Ō��0�Ԃ�ʉ߂��Ă��Ȃ���Ԃ�0�Ԃɖ߂��Ă����ꍇ�A
        // ���������ԂŒʂ��ĂȂ��Ȃ�t���Ƃ݂Ȃ��ă��Z�b�g
        if (
            LastCheckpointIndex != 0 &&
            checkpointIndex == 0 &&
            !PassedCheckpoints.SetEquals(new HashSet<int> { 3, 2, 1 }) // ���K��1�����ł͂Ȃ�
        )
        {
            Debug.Log("Checkpoint�C���F�t�����Checkpoint 0���ăX�^�[�g");
            LastCheckpointIndex = 0;
            PassedCheckpoints.Clear();
            PassedCheckpoints.Add(0);
            return true;
        }

        // ���������ԂŒʉ߂��Ă���ꍇ�̂݁A�`�F�b�N�|�C���g�L�^
        if (checkpointIndex == expected)
        {
            LastCheckpointIndex = checkpointIndex;
            PassedCheckpoints.Add(checkpointIndex);
            return true;
        }

        // �Ԉ�������ԂŒʉ߂����ꍇ�͖���
        return false;
    }

}
