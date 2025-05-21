using UnityEngine;

// �S�[�����C���ɓ��B�������𔻒肷��g���K�[�B
// �ʉ߂���� GameManager_Training ���S�[���������s���B
public class Tr_GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var info = other.GetComponent<ICarInfo>();
        if (info != null)
        {
            Debug.Log($"[Goal] {info.DriverName} �� �S�[���g���K�[�ʉ�");
            Tr_GameManager.Instance.OnAgentReachedGoal(info);
        }
    }
}
