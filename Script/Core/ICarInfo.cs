using System.Collections.Generic;
using UnityEngine;

// 車両情報の共通インターフェース。
// プレイヤー・AIを問わず「順位計算・ゴール処理」に必要な情報を持たせるための共通仕様。
// GameManager や UIManager がこの型だけを見て共通処理を行える。
public interface ICarInfo
{
    // 最後に通過したチェックポイント番号
    int LastCheckpointIndex { get; }

    // 現在のラップ数
    int CurrentLap { get; }

    // 通過済みのチェックポイント（重複なし）
    HashSet<int> PassedCheckpoints { get; }

    // 車両のTransform（位置・向きなど）← 順位計算などで使用
    Transform transform { get; }

    // ドライバー名（"Player" や "AICar" など）
    string DriverName { get; }

    // 指定されたチェックポイントを通過した処理（順番チェック含む）
    bool PassCheckpoint(int checkpointIndex);

    // ラップ完了処理（1周完了時に呼ばれる）
    void FinishLap();
}
