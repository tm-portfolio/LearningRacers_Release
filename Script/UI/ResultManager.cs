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
    public void RegisterFinish(string name, float raceStartTime)
    {
        float finishTime = Time.time - raceStartTime;

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
            RegisterFinish(car.DriverName, Time.time); // 登録時点の時間で計算
        }
    }
}
