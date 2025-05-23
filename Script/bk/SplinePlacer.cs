#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SplinePlacer : MonoBehaviour
{
    [Header("�g���b�N�S�͈̂̔�")]
    public Vector3 startPoint = new Vector3(-15f, 0.5f, 20f);
    public Vector3 endPoint = new Vector3(-15f, 0.5f, -20f);

    [Header("�ǉ��Ŕz�u���鐔�i����10~30�j")]
    public int pointCount = 20;

    [Header("�z�u���ʂ�����e�I�u�W�F�N�g")]
    public Transform container;

    [Header("�|�C���g���̃v���t�B�b�N�X")]
    public string pointName = "SplinePoint";

    public void GeneratePoints()
    {
        if (container == null)
        {
            GameObject parent = new GameObject("SplineContainer");
            container = parent.transform;
            container.position = Vector3.zero;
        }

        // ���łɑ��݂���ő�C���f�b�N�X���擾
        int startIndex = 0;
        foreach (Transform child in container)
        {
            if (child.name.StartsWith(pointName + "_"))
            {
                string numberPart = child.name.Substring((pointName + "_").Length);
                if (int.TryParse(numberPart, out int num))
                {
                    if (num >= startIndex) startIndex = num + 1;
                }
            }
        }

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1); // �� ��� 0~1 �̊�
            Vector3 pos = Vector3.Lerp(startPoint, endPoint, t);

            GameObject point = new GameObject($"{pointName}_{startIndex + i}");
            point.transform.position = pos;
            point.transform.SetParent(container);
        }


        Debug.Log($"{pointCount}�̃X�v���C���|�C���g��ǉ��������܂����i�J�n: {startIndex}�j");
    }
}

// �� CustomEditor ��`�� #if UNITY_EDITOR �̒��Ɋ��S�ɓ����
#if UNITY_EDITOR
[CustomEditor(typeof(SplinePlacer))]
public class SplinePlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplinePlacer placer = (SplinePlacer)target;
        if (GUILayout.Button("�X�v���C���|�C���g����"))
        {
            placer.GeneratePoints();
        }
    }
}
#endif