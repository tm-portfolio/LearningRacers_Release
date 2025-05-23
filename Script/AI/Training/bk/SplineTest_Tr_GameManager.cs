using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �w�K����AI�J�[�iCarAgent�j���Ǘ�����N���X�B
/// �^�C���A�E�g�����A�`�F�b�N�|�C���g�ʉߏ����A�S�[��������S���B
/// </summary>
public class SplineTest_Tr_GameManager : MonoBehaviour
{
    public static SplineTest_Tr_GameManager Instance;

    public int totalCheckpoints = 4;      // �`�F�b�N�|�C���g�̑���
    public int targetLaps = 1;            // �S�[�����邽�߂̕K�v���b�v��
    public float maxEpisodeTime = 50f;    // �^�C���A�E�g�b��

    private List<ICarInfo> agents = new();                        // ���ׂẴG�[�W�F���g
    public Dictionary<ICarInfo, float> agentStartTimes = new();  // �G�[�W�F���g���Ƃ̊J�n����

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ICarInfo �����������I�u�W�F�N�g�iCarAgent�j���擾
        agents = FindObjectsOfType<MonoBehaviour>().OfType<ICarInfo>().ToList();

        foreach (var agent in agents)
        {
            agentStartTimes[agent] = Time.time;
        }

        Debug.Log($"[SplineTest_Tr_GameManager] �o�^�G�[�W�F���g��: {agents.Count}");
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
                    unityAgent.AddReward(-1f); // �^�C���I�[�o�[�̃y�i���e�B
                    unityAgent.EndEpisode();
                    Debug.Log($"[Timeout] {agent.DriverName} �� �I���i{maxEpisodeTime}�b���߁j");

                    agentStartTimes[agent] = Time.time; // �^�C�}�[���Z�b�g
                }
            }
        }
    }

    ///// <summary>
    ///// �G�[�W�F���g���`�F�b�N�|�C���g��ʉ߂����Ƃ��ɌĂяo�����
    ///// </summary>
    //public void OnAgentPassedCheckpoint(ICarInfo agent, int index)
    //{
    //    if (agent is not Agent unityAgent) return;

    //    if (agent.PassCheckpoint(index))
    //    {
    //        unityAgent.AddReward(0.3f); // �����������Œʉ�
    //        Debug.Log($"[CP] {agent.DriverName} �� CP{index} ����ʉ�");
    //    }
    //    else
    //    {
    //        unityAgent.AddReward(-0.2f); // �t���܂��̓X�L�b�v
    //        Debug.Log($"[CP] {agent.DriverName} �� CP{index} ��ʉ߁i�t��/�X�L�b�v�j");
    //    }
    //}

    /// <summary>
    /// �G�[�W�F���g���S�[�����C����ʉ߂����Ƃ��ɌĂяo�����
    /// </summary>
    public void OnAgentReachedGoal(ICarInfo agent)
    {
        //if (agent.PassedCheckpoints.Count < totalCheckpoints) return;

        agent.FinishLap();

        if (agent is not Agent unityAgent) return;

        // �e���b�v�������Ƃ̕�V
        unityAgent.AddReward(0.3f);
        Debug.Log($"[Goal] {agent.DriverName} �� Lap {agent.CurrentLap} �����i+0.3�j");

        // �o�ߎ��Ԃɉ������{�[�i�X�i�����S�[������ƍ����_�j
        float elapsed = Time.time - agentStartTimes[agent];
        float timeBonus = Mathf.Clamp01(1f - (elapsed / maxEpisodeTime));
        unityAgent.AddReward(timeBonus);
        Debug.Log($"[Goal] {agent.DriverName} ���ԃ{�[�i�X +{timeBonus:F2}");

        agentStartTimes[agent] = Time.time;

        if (agent.CurrentLap >= targetLaps)
        {
            // �ŏI���b�v��V
            unityAgent.AddReward(1.0f);
            unityAgent.EndEpisode();
            Debug.Log($"[Goal] {agent.DriverName} �� �����i+{1.0f}�j");

            //// ���̃G�[�W�F���g�������ɏI���i�������̂��߁j
            //foreach (var other in agents)
            //{
            //    if (other != agent && other is Agent otherAgent)
            //    {
            //        otherAgent.EndEpisode();
            //    }
            //}
        }

        //agent.PassedCheckpoints.Clear(); // ���̃��b�v�ɔ����ď�����
    }

}
