using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ���U���g�L�^����ю��ȃx�X�g�Ǘ����s���N���X
public class ResultManager : MonoBehaviour
{
    [Header("�Q��")]
    public PlayerCarController player;
    public GameObject resultButton;

    [Header("�ݒ�")]
    public float resultDelay = 1.5f;

    public List<PlayerResult> raceResults = new();
    public static List<PlayerResult> LastRaceResults;

    // ���U���g�o�^
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

    // �x�X�g�^�C�����L�^�i���5����ۑ��j
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

    // �S�[����A��莞�ԂŃ��U���g�{�^����\��
    public IEnumerator ShowResultButtonAfterDelay()
    {
        yield return new WaitForSeconds(resultDelay);
        if (resultButton != null)
            resultButton.SetActive(true);
    }

    // AI�J�[�����ʏ��ɕ��ׂă��U���g�o�^
    public IEnumerator RegisterAIFinishersAfterDelay(List<ICarInfo> allCars, string playerName, System.Func<ICarInfo, float> scoreFunc)
    {
        yield return new WaitForSeconds(1f);

        var ranked = allCars
            .Where(c => c.DriverName != playerName)
            .OrderByDescending(scoreFunc)
            .ToList();

        foreach (var car in ranked)
        {
            RegisterFinish(car.DriverName, Time.time); // �o�^���_�̎��ԂŌv�Z
        }
    }
}
