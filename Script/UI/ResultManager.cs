using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// リザルト記録および自己ベスト管理を行うクラス
public class ResultManager : MonoBehaviour
{
    [Header("参照")]
    public PlayerCarController player;
    public GameObject resultButton;

    [Header("設定")]
    public float resultDelay = 1.5f;

    public List<PlayerResult> raceResults = new();
    public static List<PlayerResult> LastRaceResults;

    // リザルト登録
    public void RegisterFinish(string name, float finishTime)
    {
        raceResults.Add(new PlayerResult
        {
            Name = name,
            Time = finishTime,
            Position = raceResults.Count + 1
        });

        if (name == PlayerPrefs.GetString("PlayerName"))
        {
            SaveBestTime(finishTime);
        }

        LastRaceResults = raceResults;
    }

    // ベストタイムを記録（上位5件を保存）
    private void SaveBestTime(float time)
    {
        List<float> bestTimes = new List<float>();
        for (int i = 0; i < 5; i++)
            bestTimes.Add(PlayerPrefs.GetFloat($"BestTime_{i}", float.MaxValue));

        bestTimes.Add(time);
        bestTimes = bestTimes.OrderBy(t => t).Take(5).ToList();

        for (int i = 0; i < bestTimes.Count; i++)
        {
            PlayerPrefs.SetFloat($"BestTime_{i}", bestTimes[i]);
            if (Mathf.Approximately(bestTimes[i], time))
            {
                PlayerPrefs.SetString($"BestTimeName_{i}", PlayerPrefs.GetString("PlayerName", "player"));
                PlayerPrefs.SetString($"BestTimeDate_{i}", System.DateTime.Now.ToString("MM/dd HH:mm"));
            }
        }

        PlayerPrefs.SetFloat("CurrentTime", time);
        PlayerPrefs.Save();
    }

    // ゴール後、一定時間でリザルトボタンを表示
    public IEnumerator ShowResultButtonAfterDelay()
    {
        yield return new WaitForSeconds(resultDelay);
        if (resultButton != null)
            resultButton.SetActive(true);
    }

    // AIカーを順位順に並べてリザルト登録
    public IEnumerator RegisterAIFinishersAfterDelay(List<ICarInfo> allCars, string playerName, System.Func<ICarInfo, float> scoreFunc)
    {
        yield return new WaitForSeconds(1f);

        var ranked = allCars
            .Where(c => c.DriverName != playerName)
            .OrderByDescending(scoreFunc)
            .ToList();

        foreach (var car in ranked)
        {
            if (!GameManager.Instance.carFinishTimes.TryGetValue(car.DriverName, out float finishTime))
            {
                // プレイヤーがゴール済みなら、それより遅い仮タイムを割り当て
                float playerTime = GameManager.Instance.carFinishTimes.TryGetValue(playerName, out float pt) ? pt : 38f;
                finishTime = Mathf.Min(playerTime + Random.Range(3.0f, 7.0f), 45f); // 上限を45秒に制限
                finishTime = (float)System.Math.Round(finishTime, 2); // 小数第2位で丸める

                Debug.LogWarning($"[ResultManager] ゴールしていないAI：{car.DriverName} に仮タイム {finishTime:F2} を割当");
            }

            RegisterFinish(car.DriverName, finishTime);
        }

        //foreach (var car in ranked)
        //{
        //    // AIの正確なゴールタイムを取得
        //    float finishTime = 0f;

        //    if (GameManager.Instance.carFinishTimes.TryGetValue(car.DriverName, out float value))
        //        finishTime = value;
        //    else
        //        finishTime = Time.time - GameManager.Instance.GetRaceStartTime(); // バックアップ（異常時）

        //    RegisterFinish(car.DriverName, finishTime);
        //}
    }
}
