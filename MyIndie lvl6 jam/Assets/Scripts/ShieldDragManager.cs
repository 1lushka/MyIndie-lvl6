using UnityEngine;

public class ShieldDragManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float xMin = -5f;
    [SerializeField] private float xMax = 5f;

    private Transform currentlyDraggingShield = null;

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
            currentlyDraggingShield = null;
        }

        if (Input.GetMouseButton(0) && currentlyDraggingShield != null)
        {
            DragShield();
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
}