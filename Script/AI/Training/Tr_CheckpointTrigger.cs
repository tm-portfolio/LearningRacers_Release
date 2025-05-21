using UnityEngine;

// �g���[�j���O�V�[���p�̃`�F�b�N�|�C���g�g���K�[�i�ԍ��̓C���X�y�N�^�[�Őݒ�j
public class Tr_CheckpointTrigger : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        var info = other.GetComponent<ICarInfo>();
        if (info != null)
        {
            Debug.Log($"[Checkpoint] {info.DriverName} �� CP{checkpointIndex} �ɐڐG");
            Tr_GameManager.Instance.OnAgentPassedCheckpoint(info, checkpointIndex);
        }
    }
}
