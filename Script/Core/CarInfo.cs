using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ԗ��̊�{���i�h���C�o�[���A�v���C���[���ǂ����j���Ǘ�����N���X�B
// UI�\���⃊�U���g�p�ɎQ�Ƃ����z��B
public class CarInfo : MonoBehaviour
{
    public string driverName; // �\���p�̖��O�i�v���C���[ or AI�j
    public bool isPlayer;     // �v���C���[���삩�ǂ����̃t���O

    private void Start()
    {
        // �v���C���[�̏ꍇ�́A�ۑ��ς݂̖��O��ǂݍ���
        if (isPlayer)
        {
            driverName = PlayerPrefs.GetString("PlayerName");
        }
    }
}
