using UnityEngine;

// ゴールラインにアタッチして、車がゴールしたことを検出するトリガー。
public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 通過したオブジェクトが ICarInfo を持っていればゴール処理を実行
        if (other.TryGetComponent<ICarInfo>(out var car))
        {
            GameManager.Instance?.OnCarReachedGoal(car);
        }
    }
}