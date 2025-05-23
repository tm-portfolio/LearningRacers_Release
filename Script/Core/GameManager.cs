using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.MLAgents;


// ���[�X�S�̂��Ǘ��B
// �v���C���[�EAI�̏�������A���[�X�i�s�A�S�[��������S���B
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("�ݒ�")]
    public int targetLaps = 3;       // �S�[�����邽�߂̕K�v���b�v��
    public int totalCheckpoints = 4; // �R�[�X��ɐݒu����Ă���`�F�b�N�|�C���g�̐�

    private int playerLaps = 0;         // �v���C���[�������������b�v��
    private float raceStartTime = 0f;   // ���[�X�J�n�����iTime.time�j
    private float raceTimer = 0f;       // ���[�X�o�ߎ��ԁi�b�j
    private bool raceStarted = false;   // ���[�X���J�n���ꂽ���ǂ����̃t���O

    [Header("�ԗ�")]
    public PlayerCarController player;         // �v���C���[�̎ԗ��i����Ώہj
    public List<Agent> aiCarObjects;           // Unity�G�f�B�^��œo�^����AI�ԗ��iAgent�j�̃��X�g
    private List<ICarInfo> aiCars = new();     // �Q�[�����ŗL�������ꂽAI�ԁiICarInfo�Ƃ��Ĉ����j
    public List<ICarInfo> allCars = new();     // �v���C���[�{AI���܂ޑS�ԗ��̃��X�g�i���ʔ���p�j

    [Header("���o")]
    public TextMeshProUGUI countdownText;       // �J�E���g�_�E���̃e�L�X�g�\��
    public TextMeshProUGUI timerText;           // �^�C�}�[�̃e�L�X�g�\��
    public GameObject starEffectPrefab;         // ���b�v�������ɏo�����G�t�F�N�g�̃v���n�u
    public GameObject goalFireworkPrefab;       // �S�[�����ɏo���ΉԃG�t�F�N�g�̃v���n�u
    private bool allowDriveLoop = false;        // DriveLoop����炷���ǂ����iSTART��ɗL���j
    private bool forceDisableDriveLoop = false; // �S�[����Ȃǂ�DriveLoop��������~���邩�ǂ���

    [Header("UI / �O���Q��")]
    public UIManager uiManager;                // UI����p�̃}�l�[�W���[
    public ResultManager resultManager;        // ���U���g�����p�̃}�l�[�W���[

    public bool JustFinishedLap { get; set; } = false; // �S�[������Ɉꎞ�I�ɕ\���␳�����邽�߂̃t���O

    public Dictionary<string, float> carFinishTimes = new(); // ���U���g��ʗp
    public float GetRaceStartTime() => raceStartTime;        // ���U���g��ʗp

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ���[�X�����������i�Q�[���J�n����1�x�������s�����j
    void Start()
    {
        carFinishTimes.Clear(); // 0523

        playerLaps = 0;
        AudioManager.Instance?.StopSE();                                            // �O�̂���SE���~
        AudioManager.Instance?.PlayBGM(Resources.Load<AudioClip>("BGM_GameScene")); // BGM�Đ�

        allCars.Add(player); // �v���C���[�Ԃ�allCars�ɓo�^

        // �ŏ���AI�𖳌����i�����������~�߂�j
        foreach (var car in aiCarObjects)
            car.enabled = false;

        // �����҂��Ă���AI�L�������J�E���g�_�E���J�n
        StartCoroutine(InitializeAICarsAfterDelay(0.5f));
        StartCoroutine(DelayedStartCountdown());
    }

    // �w�莞�ԑ҂��Ă���AI�ԗ���L��������
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

    // �����x��ăJ�E���g�_�E�����J�n����
    private IEnumerator DelayedStartCountdown()
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(StartCountdown());
    }

    // �u3��2��1��START!!�v�̃J�E���g�_�E�����o�ƁA���[�X�J�n����
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

    // ���t���[�������i�|�[�Y��BGM�̐���ADriveLoop�ؑւȂǁj
    void Update()
    {
        AudioManager.Instance?.ApplyVolumeSettings(); // ���ʔ��f

        // Tab�L�[�ł̃|�[�Y���j���[�J����
        if (Input.GetKeyDown(KeyCode.Tab) && !uiManager.IsSubPanelOpen())
        {
            uiManager.TogglePauseMenu();
        }

        // �S�[����̒�~����
        if (forceDisableDriveLoop)
        {
            AudioManager.Instance?.StopSE();
            return;
        }

        // �|�[�Y����DriveLoop��~
        if (uiManager.IsPauseMenuOpen())
        {
            // DriveLoop�͎~�߂邪�A�{�^��SE�͎~�߂Ȃ�
            if (AudioManager.Instance?.seSource != null && AudioManager.Instance.seSource.loop)
            {
                AudioManager.Instance?.StopDriveLoop();
            }
            return;
        }

        // ��L�[���͂��擾�iDriveLoop����p�j
        bool accelInput = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);

        // �J�E���g�_�E������IdleLoop�̂ݍĐ��i����͎g�p���Ȃ����߁A���������j
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

        // �ʏ푖�s����DriveLoop�Đ�
        if (allowDriveLoop)
        {
            AudioManager.Instance?.HandleDriveLoop(accelInput);
        }
    }

    // ���t���[����iUI�\���X�V�E�^�C�}�[�����j
    void LateUpdate()
    {
        if (!raceStarted) return;

        raceTimer += Time.deltaTime;
        timerText.text = raceTimer.ToString("F2");

        uiManager.UpdateRaceUI(); // Lap/����/�t����UI�X�V
    }

    // ���[�X�J�n����p
    public bool IsRaceStarted() => raceStarted;

    // �v���C���[ or AI ���`�F�b�N�|�C���g��ʉ߂����Ƃ��ɌĂ΂��
    public void OnCarPassedCheckpoint(ICarInfo car, int checkpointIndex)
    {
        if (!raceStarted) return;

        bool isCorrect = car.PassCheckpoint(checkpointIndex);

        // �v���C���[�ł��������ʉ߂������ꍇ�̂�SE�Đ�
        if (!isCorrect || car is not PlayerCarController) return;

        // SE�Đ������F0�Ԃ�1,2�Ԃ͖炷���A3�Ԃ͖炳�Ȃ�
        if (checkpointIndex == 0 || (checkpointIndex != 3 && checkpointIndex != 0))
        {
            AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_CheckpointPass"), 1.5f);
        }
    }

    // �v���C���[ or AI ���S�[�����C����ʉ߂����Ƃ��ɌĂ΂��
    public void OnCarReachedGoal(ICarInfo car)
    {
        if (!raceStarted || car.PassedCheckpoints.Count < totalCheckpoints) return;

        // �S�[���������L�^�i�S�ԋ��ʁj
        if (!carFinishTimes.ContainsKey(car.DriverName)
    && car.CurrentLap >= targetLaps
    && car.PassedCheckpoints.Count >= totalCheckpoints)
        {
            float finishTime = Time.time - raceStartTime;
            carFinishTimes[car.DriverName] = finishTime;
            Debug.Log($"[�L�^] {car.DriverName} �S�[���^�C��: {finishTime:F2}�b");
        }

        //if (!carFinishTimes.ContainsKey(car.DriverName))
        //{
        //    float finishTime = Time.time - raceStartTime;
        //    carFinishTimes[car.DriverName] = finishTime;
        //    Debug.Log($"[�L�^] {car.DriverName} �S�[���^�C��: {finishTime:F2}�b");
        //}

        car.FinishLap();                // ���b�v�������Z
        car.PassedCheckpoints.Clear();  // ���̃��b�v�ɔ����ď�����

        if (car is PlayerCarController)
        {
            playerLaps++;
            Debug.Log($"[Goal] playerLaps = {playerLaps} / targetLaps = {targetLaps}");

            if (playerLaps >= targetLaps)
            {
                FinishRace(); // �S�[��������
            }
            else
            {
                AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_LapComplete"), 1.5f);
                PlayStarEffect(); // ���o

                StartCoroutine(ClearLapFinishFlag());
            }

            if (playerLaps == targetLaps - 1)
                StartCoroutine(PlayFinalLapVoiceAfterDelay(0.7f)); // �ŏI���b�v���m
        }
    }

    // �S�[�������F���U���g�o�^�≉�o�Đ�
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

        // �v���C���[�̐������S�[���^�C�����L�^���Ă���o�^
        float playerFinishTime = Time.time - raceStartTime;
        carFinishTimes[player.DriverName] = playerFinishTime;

        resultManager.RegisterFinish(player.DriverName, playerFinishTime);

        StartCoroutine(resultManager.RegisterAIFinishersAfterDelay(allCars, player.DriverName, GetProgressScore));
        StartCoroutine(resultManager.ShowResultButtonAfterDelay());

        StartCoroutine(ClearLapFinishFlag());
        StartCoroutine(UpdateUIAfterFinalLap());
    }

    // �S�[������A�t���O�����Z�b�g���邽�߂̒x������
    private IEnumerator ClearLapFinishFlag()
    {
        yield return new WaitForSeconds(0.1f);
        if (player is PlayerCarController p)
        {
            p.JustFinishedLap = false;
        }
    }

    // �\��UI��1�t���[����ɍX�V����i�Y���h�~�j
    private IEnumerator UpdateUIAfterFinalLap()
    {
        yield return null; // 1�t���[���҂���
        uiManager?.UpdateRaceUI(); // �����I�ɕ\���␳��ԂōX�V
    }

    // ���G�t�F�N�g���v���C���[�̓���ɍĐ�
    private void PlayStarEffect()
    {
        if (starEffectPrefab != null && player != null)
        {
            GameObject fx = Instantiate(starEffectPrefab, player.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            fx.transform.SetParent(player.transform);
            Destroy(fx, 1.5f);
        }
    }

    // �S�[�����̉ΉԂ̉��o
    private void SpawnGoalFireworks()
    {
        if (goalFireworkPrefab != null && player != null)
        {
            GameObject fx = Instantiate(goalFireworkPrefab, player.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            fx.transform.SetParent(player.transform);
            Destroy(fx, 2.0f);
        }
    }

    // �ŏI���b�v�ɓ������Ƃ���SE�Đ�
    private IEnumerator PlayFinalLapVoiceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_FinalLap"), 1.5f);
    }

    // �S�[�����̊���SE
    private IEnumerator PlayCheerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance?.PlaySE(Resources.Load<AudioClip>("SE_Cheer"), 1.5f);
    }

    // ���ʌv�Z�̂��߂̃X�R�A�Z�o�i���b�v���A�ʉ߃`�F�b�N�|�C���g�A����CP�܂ł̋����j
    public float GetProgressScore(ICarInfo car)
    {
        // �e�v�f�̏d�݂��`�i���b�v > �`�F�b�N�|�C���g > �����j
        float lapWeight = 100000f;
        float checkpointWeight = 100f;
        float distanceWeight = 1f;

        int total = totalCheckpoints;
        int lap = car.CurrentLap;
        int adjustedIndex = car.LastCheckpointIndex; // �Ō�ɒʉ߂����`�F�b�N�|�C���g�ԍ�

        // �S�[������́uLap��1���Z����Ă��� & CP��0�v�ɂȂ邽�߁A
        // �\����̃Y���i1�O�̃`�F�b�N�|�C���g�����j��␳����B
        if (car is PlayerCarController p && p.JustFinishedLap && adjustedIndex == 0)
        {
            lap = Mathf.Max(0, lap - 1);�@// ���b�v�����ꎞ�I��1���炷
        }

        int progressIndex = (total + 3 - adjustedIndex) % total;�@// �ʉ߂����`�F�b�N�|�C���g�̐i�݋
        int nextIndex = (adjustedIndex - 1 + total) % total;�@�@�@// ���Ɍ������`�F�b�N�|�C���g

        // ���̃`�F�b�N�|�C���g�̃I�u�W�F�N�g���擾
        GameObject nextCheckpoint = GameObject.Find($"Checkpoint_{nextIndex}");
        if (nextCheckpoint == null) return 0f;

        // ���ԂƎ��`�F�b�N�|�C���g�Ƃ̋������Z�o�i�߂��قǃX�R�A�������j
        float distanceToNext = Vector3.Distance(car.transform.position, nextCheckpoint.transform.position);

        // �X�R�A�v�Z
        return (lap * lapWeight) + (progressIndex * checkpointWeight) - (distanceToNext * distanceWeight);
    }

    // ���U���g��ʂւ̑J�ڏ���
    public void GoToResultScene()
    {
        forceDisableDriveLoop = true;
        AudioManager.Instance?.StopSE();           // �ʏ�SE��~
        AudioManager.Instance?.StopDriveLoop();    // DriveLoop��~
        SceneManager.LoadScene("ResultScene");
    }
}