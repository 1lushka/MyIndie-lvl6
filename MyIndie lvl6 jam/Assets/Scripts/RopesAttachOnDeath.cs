// RopesAttachOnDeath.cs
using UnityEngine;

public class RopesAttachOnDeath : MonoBehaviour
{
    [Header("�������� �������� ������")]
    [SerializeField] private RopesHealthManager manager;

    [Header("���� �����������")]
    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform rightAnchor;

    [Header("��� ����� ������")]
    [Tooltip("���� ����� � ������ 'ropes' �� �����, ������ ���� TapePiece � �����.")]
    [SerializeField] private Transform ropesRoot;
    [SerializeField] private TapePiece[] ropes;

    [Header("��������� ������������")]
    [Tooltip("��������� ������� �������/�������/������� ��� SetParent.")]
    [SerializeField] private bool worldPositionStays = true;
    [Tooltip("���� X ����� ����� X ����������� � ���������� ������� (����� ������).")]
    [SerializeField] private bool equalGoesRight = false;

    private bool _done;

    [SerializeField] private Vector2 postZeroDelayRange = new Vector2(0.4f, 1.2f); // ��������� ��������
    [SerializeField] private float leftMoveSpeed = 2.5f;                            // �������� ����� (�/�)
    [SerializeField] private Animator deathAnimator;                                // ��������, ������� ������
    [SerializeField] private string deathTrigger = "Death";                         // ��� ��������
    private bool _moveLeftActive;                                                   // ���� ��������



    private void Awake()
    {
        if (manager == null) manager = FindFirstObjectByType<RopesHealthManager>();

        if ((ropes == null || ropes.Length == 0))
        {
            if (ropesRoot != null)
                ropes = ropesRoot.GetComponentsInChildren<TapePiece>(includeInactive: false);
            else
                ropes = FindObjectsByType<TapePiece>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }
    }

    private void OnEnable()
    {
        if (manager != null)
            manager.OnFirstHit0 += HandleFirstZero;
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.OnFirstHit0 -= HandleFirstZero;
    }

    private void HandleFirstZero(TapePiece destroyed)
    {
        if (_done || destroyed == null || leftAnchor == null || rightAnchor == null) return;
        _done = true;

        float pivotX = destroyed.transform.position.x;

        foreach (var r in ropes)
        {
            if (r == null || r == destroyed) continue;

            float x = r.transform.position.x;

            // ���������� �������  
            bool goRight = x > pivotX || (Mathf.Approximately(x, pivotX) && equalGoesRight);
            var targetParent = goRight ? rightAnchor : leftAnchor;

            // ������ ����������� (��� ��������)  
            r.transform.SetParent(targetParent, worldPositionStays);
        }
        StartCoroutine(PostZeroSequence(destroyed));
    }

    private System.Collections.IEnumerator PostZeroSequence(TapePiece destroyed)
    {
        // ��������� ��������
        float delay = Random.Range(postZeroDelayRange.x, postZeroDelayRange.y);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // 1) ���������� ����� destroyed
        if (destroyed != null) Destroy(destroyed.gameObject);

        // 2) �������� �������� ������ ���������� �����
        _moveLeftActive = true;

        // 3) ��������� ��������
        if (deathAnimator != null && !string.IsNullOrEmpty(deathTrigger))
            deathAnimator.SetTrigger(deathTrigger);
    }

    private void Update()
    {
        if (_moveLeftActive && leftAnchor != null && leftMoveSpeed > 0f)
            leftAnchor.Translate(Vector3.left * leftMoveSpeed * Time.deltaTime, Space.World);
    }
}
