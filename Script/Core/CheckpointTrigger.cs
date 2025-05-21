using System.Collections.Generic;
using UnityEngine;

// 各チェックポイントにアタッチして、車が通過したことを検知するトリガー。
// checkpointIndex を使って何番のチェックポイントかを識別する。
public class CheckpointTrigger : MonoBehaviour
{
    public int checkpointIndex; // このトリガーが何番目のチェックポイントか

    private void OnTriggerEnter(Collider other)
    {
        // 通過したオブジェクトが ICarInfo を持っていれば処理を行う（プレイヤー or AI）
        if (other.TryGetComponent<ICarInfo>(out var car))
        {
            // GameManager に通知してチェックポイント処理を進行
            GameManager.Instance?.OnCarPassedCheckpoint(car, checkpointIndex);
        }
    }

}