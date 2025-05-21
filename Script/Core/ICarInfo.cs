using System.Collections.Generic;
using UnityEngine;

// �ԗ����̋��ʃC���^�[�t�F�[�X�B
// �v���C���[�EAI���킸�u���ʌv�Z�E�S�[�������v�ɕK�v�ȏ����������邽�߂̋��ʎd�l�B
// GameManager �� UIManager �����̌^���������ċ��ʏ������s����B
public interface ICarInfo
{
    // �Ō�ɒʉ߂����`�F�b�N�|�C���g�ԍ�
    int LastCheckpointIndex { get; }

    // ���݂̃��b�v��
    int CurrentLap { get; }

    // �ʉߍς݂̃`�F�b�N�|�C���g�i�d���Ȃ��j
    HashSet<int> PassedCheckpoints { get; }

    // �ԗ���Transform�i�ʒu�E�����Ȃǁj�� ���ʌv�Z�ȂǂŎg�p
    Transform transform { get; }

    // �h���C�o�[���i"Player" �� "AICar" �Ȃǁj
    string DriverName { get; }

    // �w�肳�ꂽ�`�F�b�N�|�C���g��ʉ߂��������i���ԃ`�F�b�N�܂ށj
    bool PassCheckpoint(int checkpointIndex);

    // ���b�v���������i1���������ɌĂ΂��j
    void FinishLap();
}
