using UnityEngine;

// �S�[�����C���ɃA�^�b�`���āA�Ԃ��S�[���������Ƃ����o����g���K�[�B
public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // �ʉ߂����I�u�W�F�N�g�� ICarInfo �������Ă���΃S�[�����������s
        if (other.TryGetComponent<ICarInfo>(out var car))
        {
            GameManager.Instance?.OnCarReachedGoal(car);
        }
    }
}