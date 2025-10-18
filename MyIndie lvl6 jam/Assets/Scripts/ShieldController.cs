using UnityEngine;
using DG.Tweening;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private string targetTag = "Shield";

    [Header("X Clamp (drag)")]
    [SerializeField] private float xMin = -5f, xMax = 5f;

    [Header("Hover (while holding)")]
    [Tooltip("Насколько поднять объект по Y во время удержания")]
    [SerializeField] private float hoverHeight = 1.0f;
    [SerializeField] private float liftDuration = 0.2f;
    [SerializeField] private Ease liftEase = Ease.OutQuad;

    [Header("Wobble (around Z while holding)")]
    [Tooltip("Амплитуда покачивания в градусах (±по Z)")]
    [SerializeField] private float wobbleAngle = 7f;
    [Tooltip("Полный период одной «качалки» (вверх-вниз)")]
    [SerializeField] private float wobblePeriod = 0.8f;
    [SerializeField] private Ease wobbleEase = Ease.InOutSine;

    private Transform draggedObject;
    private float zOffset;

    private float originalY;
    private Vector3 originalLocalEuler;

    private Tween liftTween;
    private Tween wobbleTween;

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

                originalY = draggedObject.position.y;
                originalLocalEuler = draggedObject.localEulerAngles;

                DOTween.Kill(draggedObject, complete: false);

                float targetY = originalY + hoverHeight;
                liftTween?.Kill();
                liftTween = draggedObject
                    .DOMoveY(targetY, liftDuration)
                    .SetEase(liftEase)
                    .SetLink(draggedObject.gameObject, LinkBehaviour.KillOnDestroy);

                wobbleTween?.Kill();
                float half = Mathf.Max(0.01f, wobblePeriod * 0.5f);
                var baseEuler = originalLocalEuler;

                wobbleTween = DOTween.Sequence()
                    .Append(draggedObject.DOLocalRotate(new Vector3(baseEuler.x + wobbleAngle, baseEuler.y, baseEuler.z), half)
                        .SetEase(wobbleEase))
                    .Append(draggedObject.DOLocalRotate(new Vector3(baseEuler.x - wobbleAngle, baseEuler.y, baseEuler.z), half)
                        .SetEase(wobbleEase))
                    .Append(draggedObject.DOLocalRotate(new Vector3(baseEuler.x , baseEuler.y, baseEuler.z), half)
                        .SetEase(wobbleEase))
                    .SetLoops(-1, LoopType.Restart)
                    .SetLink(draggedObject.gameObject, LinkBehaviour.KillOnDestroy);
            }
        }
    }

    private void ReleaseObject()
    {
        if (draggedObject != null)
        {
            wobbleTween?.Kill();
            liftTween?.Kill();

            draggedObject
                .DOMoveY(originalY, liftDuration)
                .SetEase(Ease.InQuad)
                .SetLink(draggedObject.gameObject, LinkBehaviour.KillOnDestroy);

            draggedObject
                .DOLocalRotate(originalLocalEuler, liftDuration)
                .SetEase(Ease.InOutSine)
                .SetLink(draggedObject.gameObject, LinkBehaviour.KillOnDestroy);

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

    private void OnDisable()
    {
        wobbleTween?.Kill();
        liftTween?.Kill();
        if (draggedObject) DOTween.Kill(draggedObject, complete: false);
    }
}
