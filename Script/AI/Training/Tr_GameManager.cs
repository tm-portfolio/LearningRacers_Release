using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;
using System.Linq;

// 学習中のAIカー（CarAgent）を管理するクラス。
// タイムアウト処理、チェックポイント通過処理、ゴール処理を担当。
public class Tr_GameManager : MonoBehaviour
{
    public static Tr_GameManager Instance;

    public int totalCheckpoints = 4;      // チェックポイントの総数
    public int targetLaps = 1;            // ゴールするための必要ラップ数
    public float maxEpisodeTime = 30f;    // タイムアウト秒数

    private List<ICarInfo> agents = new();                        // すべてのエージェント
    public Dictionary<ICarInfo, float> agentStartTimes = new();  // エージェントごとの開始時刻

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ICarInfo を実装したオブジェクト（CarAgent）を取得
        agents = FindObjectsOfType<MonoBehaviour>().OfType<ICarInfo>().ToList();

        foreach (var agent in agents)
        {
            agentStartTimes[agent] = Time.time;
        }

        Debug.Log($"[Tr_GameManager] 登録エージェント数: {agents.Count}");
    }

    private void Update()
    {
        foreach (var agent in agents)
        {
            float elapsed = Time.time - agentStartTimes[agent];
            if (elapsed > maxEpisodeTime)
            {
                if (agent is Agent unityAgent)
                {
                    unityAgent.AddReward(-1f); // タイムオーバーのペナルティ
                    unityAgent.EndEpisode();
                    Debug.Log($"[Timeout] {agent.DriverName} → 終了（{maxEpisodeTime}秒超過）");

                    agentStartTimes[agent] = Time.time; // タイマーリセット
                }
            }
        }
    }

    // エージェントがチェックポイントを通過したときに呼び出される
    public void OnAgentPassedCheckpoint(ICarInfo agent, int index)
    {
        if (agent is not Agent unityAgent) return;

        if (agent.PassCheckpoint(index))
        {
            unityAgent.AddReward(0.3f); // 正しい順序で通過
            Debug.Log($"[CP] {agent.DriverName} → CP{index} 正常通過");
        }
        else
        {
            unityAgent.AddReward(-0.2f); // 逆走またはスキップ
            Debug.Log($"[CP] {agent.DriverName} → CP{index} 誤通過（逆走/スキップ）");
        }
    }

    // エージェントがゴールラインを通過したときに呼び出される
    public void OnAgentReachedGoal(ICarInfo agent)
    {
        Debug.Log($"[DEBUG] {agent.DriverName} ゴール処理呼び出し、CP数 = {agent.PassedCheckpoints.Count}");

        if (agent.PassedCheckpoints.Count < totalCheckpoints) return;

        agent.FinishLap();

        if (agent is not Agent unityAgent) return;

        // 各ラップ完了ごとの報酬
        unityAgent.AddReward(0.3f);
        Debug.Log($"[Goal] {agent.DriverName} → Lap {agent.CurrentLap} 完了（+0.3）");        

        // 経過時間に応じたボーナス（早くゴールすると高得点）
        float elapsed = Time.time - agentStartTimes[agent];
        float timeBonus = Mathf.Clamp01(1f - (elapsed / maxEpisodeTime));
        unityAgent.AddReward(timeBonus);
        Debug.Log($"[Goal] {agent.DriverName} 時間ボーナス +{timeBonus:F2}");

        agentStartTimes[agent] = Time.time;

        if (agent.CurrentLap >= targetLaps)
        {
            // 最終ラップ報酬
            unityAgent.AddReward(1.0f);
            unityAgent.EndEpisode();
            Debug.Log($"[Goal] {agent.DriverName} → 完走（+{1.0f}）");

            //// 他のエージェントも同時に終了（公平性のため）
            //foreach (var other in agents)
            //{
            //    if (other != agent && other is Agent otherAgent)
            //    {
            //        otherAgent.EndEpisode();
            //    }
            //}
        }

        agent.PassedCheckpoints.Clear(); // 次のラップに備えて初期化
    }

}
