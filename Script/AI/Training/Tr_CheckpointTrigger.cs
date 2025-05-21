using UnityEngine;

// トレーニングシーン用のチェックポイントトリガー（番号はインスペクターで設定）
public class Tr_CheckpointTrigger : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        var info = other.GetComponent<ICarInfo>();
        if (info != null)
        {
            Debug.Log($"[Checkpoint] {info.DriverName} → CP{checkpointIndex} に接触");
            Tr_GameManager.Instance.OnAgentPassedCheckpoint(info, checkpointIndex);
        }
    }
}
