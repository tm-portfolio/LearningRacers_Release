using System.Collections.Generic;
using UnityEngine;

// �e�`�F�b�N�|�C���g�ɃA�^�b�`���āA�Ԃ��ʉ߂������Ƃ����m����g���K�[�B
// checkpointIndex ���g���ĉ��Ԃ̃`�F�b�N�|�C���g�������ʂ���B
public class CheckpointTrigger : MonoBehaviour
{
    public int checkpointIndex; // ���̃g���K�[�����Ԗڂ̃`�F�b�N�|�C���g��

    private void OnTriggerEnter(Collider other)
    {
        // �ʉ߂����I�u�W�F�N�g�� ICarInfo �������Ă���Ώ������s���i�v���C���[ or AI�j
        if (other.TryGetComponent<ICarInfo>(out var car))
        {
            // GameManager �ɒʒm���ă`�F�b�N�|�C���g������i�s
            GameManager.Instance?.OnCarPassedCheckpoint(car, checkpointIndex);
        }
    }

}