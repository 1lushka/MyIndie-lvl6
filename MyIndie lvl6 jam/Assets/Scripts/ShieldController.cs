using UnityEngine;
using DG.Tweening;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private string targetTag = "Shield";
    [SerializeField] private float xMin = -5f, xMax = 5f;
    [SerializeField] private float scaleMultiplier = 1.2f;  
    [SerializeField] private float scaleDuration = 0.2f; 

    private Transform draggedObject;
    private float zOffset;
    private Vector3 originalScale;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseObject();
        }

        if (draggedObject != null && Input.GetMouseButton(0))
        {
            DragObject();
        }
    }

    private void TryPickObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                draggedObject = hit.transform;
                zOffset = cam.WorldToScreenPoint(draggedObject.position).z;
                originalScale = draggedObject.localScale;
                AnimateScale(draggedObject, originalScale * scaleMultiplier);
            }
        }
    }

    private void ReleaseObject()
    {
        if (draggedObject != null)
        {
            AnimateScale(draggedObject, originalScale);
            draggedObject = null;
        }
    }

    private void DragObject()
    {
        Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        float clampedX = Mathf.Clamp(worldPos.x, xMin, xMax);
        Vector3 targetPos = new Vector3(clampedX, draggedObject.position.y, draggedObject.position.z);

        ObjectMover.MoveTo(draggedObject, targetPos);
    }

    private void AnimateScale(Transform target, Vector3 toScale)
    {
        target.DOScale(toScale, scaleDuration).SetEase(Ease.OutQuad);
    }
}
