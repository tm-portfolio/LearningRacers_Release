#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SplinePlacer : MonoBehaviour
{
    [Header("トラック全体の範囲")]
    public Vector3 startPoint = new Vector3(-15f, 0.5f, 20f);
    public Vector3 endPoint = new Vector3(-15f, 0.5f, -20f);

    [Header("追加で配置する数（推奨10~30）")]
    public int pointCount = 20;

    [Header("配置結果を入れる親オブジェクト")]
    public Transform container;

    [Header("ポイント名のプレフィックス")]
    public string pointName = "SplinePoint";

    public void GeneratePoints()
    {
        if (container == null)
        {
            GameObject parent = new GameObject("SplineContainer");
            container = parent.transform;
            container.position = Vector3.zero;
        }

        // すでに存在する最大インデックスを取得
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
            float t = i / (float)(pointCount - 1); // ← 常に 0~1 の間
            Vector3 pos = Vector3.Lerp(startPoint, endPoint, t);

            GameObject point = new GameObject($"{pointName}_{startIndex + i}");
            point.transform.position = pos;
            point.transform.SetParent(container);
        }


        Debug.Log($"{pointCount}個のスプラインポイントを追加生成しました（開始: {startIndex}）");
    }
}

// ↓ CustomEditor 定義も #if UNITY_EDITOR の中に完全に入れる
#if UNITY_EDITOR
[CustomEditor(typeof(SplinePlacer))]
public class SplinePlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplinePlacer placer = (SplinePlacer)target;
        if (GUILayout.Button("スプラインポイント生成"))
        {
            placer.GeneratePoints();
        }
    }
}
#endif