using UnityEngine;

// ゴールラインに到達したかを判定するトリガー。
// 通過すると GameManager_Training がゴール処理を行う。
public class Tr_GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var info = other.GetComponent<ICarInfo>();
        if (info != null)
        {
            Debug.Log($"[Goal] {info.DriverName} → ゴールトリガー通過");
            Tr_GameManager.Instance.OnAgentReachedGoal(info);
        }
    }
}
