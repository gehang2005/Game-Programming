using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ClassroomAnomalyRandomizer : MonoBehaviour
{
    [Header("Anomalies")]
    [Tooltip("需要参与随机显示的异常对象。可手动指定，或由自动收集逻辑生成。")]
    public List<GameObject> anomalies = new List<GameObject>();

    [Tooltip("开始时自动收集异常对象。建议开启。")]
    public bool autoCollectOnAwake = true;

    [Tooltip("自动收集的根节点。把所有异常 prefab 放在该节点下，后续新增会自动纳入随机池。留空则使用当前物体。")]
    public Transform anomalyRoot;

    [Tooltip("自动收集时是否包含未激活对象。")]
    public bool includeInactive = true;

    [Tooltip("为 true 时仅收集 anomalyRoot 的直接子物体；为 false 时收集所有后代。建议保持为 true。")]
    public bool onlyDirectChildren = true;

    private readonly HashSet<GameObject> shownAnomalies = new HashSet<GameObject>();

    private void OnValidate()
    {
        RemoveNullAndDuplicateEntries();
    }

    private void Awake()
    {
        if (autoCollectOnAwake)
            CollectAnomaliesFromRoot();

        RemoveNullAndDuplicateEntries();

        HideAllAnomalies();
    }

    [ContextMenu("Collect Anomalies From Root")]
    public void CollectAnomaliesFromRoot()
    {
        anomalies.Clear();

        Transform root = anomalyRoot != null ? anomalyRoot : transform;
        if (root == null)
            return;

        if (onlyDirectChildren)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child == null)
                    continue;
                if (!includeInactive && !child.gameObject.activeInHierarchy)
                    continue;

                anomalies.Add(child.gameObject);
            }
            RemoveNullAndDuplicateEntries();
            return;
        }

        Transform[] allTransforms = root.GetComponentsInChildren<Transform>(includeInactive);
        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (t == null || t == root)
                continue;

            anomalies.Add(t.gameObject);
        }

        RemoveNullAndDuplicateEntries();
    }

    public void ActivateRandomAnomaly()
    {
        RemoveNullAndDuplicateEntries();

        if (anomalies.Count == 0)
        {
            Debug.LogWarning("ClassroomAnomalyRandomizer: 未配置异常对象。", this);
            return;
        }

        HideAllAnomalies();

        List<GameObject> candidates = new List<GameObject>();
        for (int i = 0; i < anomalies.Count; i++)
        {
            GameObject anomaly = anomalies[i];
            if (anomaly != null && !shownAnomalies.Contains(anomaly))
                candidates.Add(anomaly);
        }

        if (candidates.Count == 0)
        {
            Debug.Log("ClassroomAnomalyRandomizer: 所有异常都已出现过，不再重复。", this);
            return;
        }

        int index = Random.Range(0, candidates.Count);
        GameObject target = candidates[index];
        target.SetActive(true);
        shownAnomalies.Add(target);
    }

    [ContextMenu("Reset Anomaly History")]
    public void ResetAnomalyHistory()
    {
        shownAnomalies.Clear();
        HideAllAnomalies();
    }

    private void RemoveNullAndDuplicateEntries()
    {
        HashSet<GameObject> seen = new HashSet<GameObject>();
        List<GameObject> clean = new List<GameObject>();
        for (int i = 0; i < anomalies.Count; i++)
        {
            GameObject anomaly = anomalies[i];
            if (anomaly == null)
                continue;
            if (seen.Add(anomaly))
                clean.Add(anomaly);
        }

        anomalies = clean;

        shownAnomalies.RemoveWhere(item => item == null || !seen.Contains(item));
    }

    public void HideAllAnomalies()
    {
        for (int i = 0; i < anomalies.Count; i++)
        {
            if (anomalies[i] != null)
                anomalies[i].SetActive(false);
        }
    }
}