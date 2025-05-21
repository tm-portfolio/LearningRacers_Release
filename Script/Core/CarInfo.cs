using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 車両の基本情報（ドライバー名、プレイヤーかどうか）を管理するクラス。
// UI表示やリザルト用に参照される想定。
public class CarInfo : MonoBehaviour
{
    public string driverName; // 表示用の名前（プレイヤー or AI）
    public bool isPlayer;     // プレイヤー操作かどうかのフラグ

    private void Start()
    {
        // プレイヤーの場合は、保存済みの名前を読み込む
        if (isPlayer)
        {
            driverName = PlayerPrefs.GetString("PlayerName");
        }
    }
}
