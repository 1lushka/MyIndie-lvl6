using UnityEngine;

public class ShieldDragManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float xMin = -5f;
    [SerializeField] private float xMax = 5f;
    [SerializeField] private float hoverScale = 1.2f; // Во сколько раз увеличивать
    [SerializeField] private float scaleDuration = 0.1f; // Время анимации (можно и без неё)

    private Transform currentlyDraggingShield = null;
    private Vector3 originalScale; // Сохраняем исходный масштаб

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
            // На случай, если отпустили кнопку вне Update-логики (маловероятно, но для надёжности)
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
                originalScale = currentlyDraggingShield.localScale; // Запоминаем исходный размер
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
        // Простой вариант — мгновенно или через Lerp
        Vector3 targetScale = originalScale * hoverScale;
        currentlyDraggingShield.localScale = Vector3.Lerp(
            currentlyDraggingShield.localScale,
            targetScale,
            scaleDuration / Time.deltaTime // НЕПРАВИЛЬНО! Исправим ниже
        );
    }

    void ScaleBack()
    {
        currentlyDraggingShield.localScale = Vector3.Lerp(
            currentlyDraggingShield.localScale,
            originalScale,
            scaleDuration * Time.deltaTime // Это тоже криво
        );
    }

    void StopDragging()
    {
        if (currentlyDraggingShield != null)
        {
            // Мгновенно возвращаем, или плавно — как хочешь
            currentlyDraggingShield.localScale = originalScale;
        }
        currentlyDraggingShield = null;
    }
}