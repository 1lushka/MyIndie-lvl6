using UnityEngine;

public class ShieldDragManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float xMin = -5f;
    [SerializeField] private float xMax = 5f;
    [SerializeField] private float hoverScale = 1.2f; // �� ������� ��� �����������
    [SerializeField] private float scaleDuration = 0.1f; // ����� �������� (����� � ��� ��)

    private Transform currentlyDraggingShield = null;
    private Vector3 originalScale; // ��������� �������� �������

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectShield();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }

        if (Input.GetMouseButton(0) && currentlyDraggingShield != null)
        {
            DragShield();
            ScaleUp();
        }
        else if (currentlyDraggingShield != null)
        {
            // �� ������, ���� ��������� ������ ��� Update-������ (������������, �� ��� ���������)
            ScaleBack();
        }
    }

    void TrySelectShield()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Shield"))
            {
                currentlyDraggingShield = hit.transform;
                originalScale = currentlyDraggingShield.localScale; // ���������� �������� ������
            }
        }
    }

    void DragShield()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(currentlyDraggingShield.position);
        screenPos.x = Input.mousePosition.x;
        screenPos.y = Input.mousePosition.y;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        float clampedX = Mathf.Clamp(worldPos.x, xMin, xMax);

        currentlyDraggingShield.position = new Vector3(
            clampedX,
            currentlyDraggingShield.position.y,
            currentlyDraggingShield.position.z
        );
    }

    void ScaleUp()
    {
        // ������� ������� � ��������� ��� ����� Lerp
        Vector3 targetScale = originalScale * hoverScale;
        currentlyDraggingShield.localScale = Vector3.Lerp(
            currentlyDraggingShield.localScale,
            targetScale,
            scaleDuration / Time.deltaTime // �����������! �������� ����
        );
    }

    void ScaleBack()
    {
        currentlyDraggingShield.localScale = Vector3.Lerp(
            currentlyDraggingShield.localScale,
            originalScale,
            scaleDuration * Time.deltaTime // ��� ���� �����
        );
    }

    void StopDragging()
    {
        if (currentlyDraggingShield != null)
        {
            // ��������� ����������, ��� ������ � ��� ������
            currentlyDraggingShield.localScale = originalScale;
        }
        currentlyDraggingShield = null;
    }
}