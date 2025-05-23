using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// �Q�[������UI�\���E�X�V���Ǘ�����N���X
// ���b�v�\���E���ʕ\���E�t���x���E�|�[�Y���j���[�Ȃǂ𐧌�
public class UIManager : MonoBehaviour
{
    [Header("UI �\��")]
    public TextMeshProUGUI lapDisplay;           // ���b�v���̕\��
    public TextMeshProUGUI positionDisplay;      // ���ʂ̕\��
    public TextMeshProUGUI reverseWarningText;   // �t���x���e�L�X�g
    public GameObject pauseMenuUI;               // �|�[�Y���j���[UI�̐e�I�u�W�F�N�g
    public TextMeshProUGUI goalText;             // �S�[�����ɕ\�������e�L�X�g

    [Header("�v���C���[�Ɛݒ�")]
    public PlayerCarController player;           // �v���C���[�ԗ��i���擾�p�j
    public int totalCheckpoints = 4;             // �ݒ肳��Ă���`�F�b�N�|�C���g�̐�
    public int targetLaps = 3;                   // �ڕW���b�v���i�S�[�������j

    private bool isSubPanelOpen = false;         // �ݒ�Ȃǂ̃T�u�p�l�����J���Ă��邩�ǂ���
    private bool pauseMenuOpen = false;          // �|�[�Y���j���[���J���Ă��邩�ǂ���

    // ���t���[���Ăяo���āAUI���ŐV�̏�ԂɍX�V����
    public void UpdateRaceUI()
    {
        if (player == null) return;

        int lapDisplayCount = player.CurrentLap;

        // �S�[������̕\���␳�F1�t���[������ +1 �\������iJustFinishedLap�g�p�j
        if (player.JustFinishedLap)
        {
            lapDisplayCount++;
        }

        // ���b�v���� UI �ɕ\���i�ő�\���� targetLaps�j
        lapDisplay.text = $"Lap: {Mathf.Min(lapDisplayCount, targetLaps)}/{targetLaps}";

        // �S�[�������烉�b�v�\�����I�����W�ɕς���
        lapDisplay.color = (lapDisplayCount >= targetLaps)
            ? new Color(1.0f, 0.5f, 0.0f)  // �I�����W
            : Color.white;

        // �v���C���[�̏��ʂ��X�R�A�x�[�X�Ŏ擾���ĕ\��
        int rank = GetPlayerPosition();
        positionDisplay.text = $"Position: {rank}";

        // ���ʂɉ����ĐF�ύX�i1��=���A2��=��A3��=���A����ȊO=�j
        switch (rank)
        {
            case 1: positionDisplay.color = new Color(1.0f, 0.84f, 0.0f); break; // ��
            case 2: positionDisplay.color = new Color(0.75f, 0.75f, 0.75f); break; // ��
            case 3: positionDisplay.color = new Color(0.8f, 0.5f, 0.2f); break; // ��
            default: positionDisplay.color = Color.blue; break;
        }

        // �t������ƕ\��
        CheckReverseWarning();
    }

    // �v���C���[�̌��ݏ��ʂ��v�Z
    // GameManager.GetProgressScore() ���g���Ĕ�r
    private int GetPlayerPosition()
    {
        float playerScore = GameManager.Instance.GetProgressScore(player);
        int rank = 1;

        foreach (var car in GameManager.Instance.allCars)
        {
            float score = GameManager.Instance.GetProgressScore(car);
            Debug.Log($"[���ʃ`�F�b�N] {car.DriverName} | Lap: {car.CurrentLap}, CP: {car.LastCheckpointIndex}, Score: {score:F2}");

            if ((Object)car == (Object)player) continue;

            // �v���C���[���X�R�A�������Ԃ�����΁A���̂Ԃ�v���C���[�̏��ʂ͉�����irank++�j
            if (GameManager.Instance.GetProgressScore(car) > playerScore + 0.001f) rank++;
        }

        return rank;
    }

    // �v���C���[�̌����Ǝ��̃`�F�b�N�|�C���g�̌�������t���𔻒�
    private void CheckReverseWarning()
    {
        int nextIndex = (player.LastCheckpointIndex - 1 + totalCheckpoints) % totalCheckpoints;
        GameObject nextCheckpoint = GameObject.Find($"Checkpoint_{nextIndex}");
        if (nextCheckpoint == null) return;

        Vector3 checkpointForward = nextCheckpoint.transform.forward;
        float angle = Vector3.Angle(checkpointForward, player.transform.forward);
        //Debug.Log($"�p�x�`�F�b�N: {angle}");

        bool isReversing = angle > 130f; // �p�x��130�x�ȏ�Ȃ�t���Ƃ݂Ȃ�
        SetReverseWarning(isReversing);
    }

    // �t���x���̕\���؂�ւ�
    public void SetReverseWarning(bool isActive)
    {
        if (reverseWarningText != null)
            reverseWarningText.gameObject.SetActive(isActive);
    }

    // Tab�L�[�ŌĂ΂��F�|�[�Y���j���[���u�J�������v�ɕύX
    public void TogglePauseMenu()
    {
        if (pauseMenuUI == null) return;

        if (pauseMenuUI.activeSelf) return; // ���łɊJ���Ă��牽�����Ȃ�

        pauseMenuUI.SetActive(true);

        // �J����炷�i����SE�j
        AudioManager.Instance?.PlayCommonSE();

        // �Q�[�����ꎞ��~
        Time.timeScale = 0f;
        pauseMenuOpen = true;
    }

    // �|�[�Y���j���[����ăQ�[���ĊJ
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            pauseMenuOpen = false;
        }
    }

    // �T�u�p�l���i�V�ѕ���ʁA�ݒ��ʂȂǁj�̊J��Ԃ�ݒ�
    public void SetSubPanelOpen(bool isOpen)
    {
        isSubPanelOpen = isOpen;
    }

    // �T�u�p�l�����J���Ă��邩�ǂ������擾
    public bool IsSubPanelOpen()
    {
        return isSubPanelOpen;
    }

    // �|�[�Y���j���[���J���Ă��邩�ǂ������擾
    public bool IsPauseMenuOpen()
    {
        return pauseMenuOpen;
    }
}
