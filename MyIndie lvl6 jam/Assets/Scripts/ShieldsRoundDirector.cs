using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ShieldsRoundDirector : MonoBehaviour
{
    [SerializeField] private List<Transform> shields = new List<Transform>();

    [Header("������� ����")]
    [SerializeField] private float hideOffsetY = 8f;
    [SerializeField] private float dropDuration = 0.45f;
    [SerializeField] private Ease dropEase = Ease.OutQuad;

    private float[] baseY;

    void Awake()
    {
        baseY = new float[shields.Count];
        for (int i = 0; i < shields.Count; i++)
        {
            if (shields[i] == null) continue;
            baseY[i] = shields[i].position.y;
            // �������� �����
            var tr = shields[i];
            tr.position = new Vector3(tr.position.x, baseY[i] + hideOffsetY, tr.position.z);
        }
    }

    /// �������� ��� ���� ������ (�����������)
    public void HideAll()
    {
        for (int i = 0; i < shields.Count; i++)
        {
            if (shields[i] == null) continue;
            var tr = shields[i];
            tr.DOKill();
            tr.position = new Vector3(tr.position.x, baseY[i] + hideOffsetY, tr.position.z);
        }
    }

    /// ������� N ����� ������ ����
    public void DropShieldsForRound(int roundNumber)
    {
        int count = Mathf.Clamp(roundNumber-1, 0, 1000);
        if (count > shields.Count - 1) return;

        var sc = shields[count];
        var tr = sc.transform;
        tr.DOKill();
        tr.position = new Vector3(tr.position.x, baseY[count] + hideOffsetY, tr.position.z);
        tr.DOMoveY(baseY[count], dropDuration).SetEase(dropEase);
        if (count ==1) DropShieldsForRound(3);
        // ��������� (���� ����) �������� ����������� �������
    }
}
