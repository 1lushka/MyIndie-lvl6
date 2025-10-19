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

    // ДОБАВИТЬ ПОЛЯ
    [Header("Drop (external)")]
    [SerializeField] private float externalHideOffsetY = 8f;
    [SerializeField] private float externalDropDuration = 0.45f;
    [SerializeField] private Ease externalDropEase = Ease.OutQuad;

    private float _baseY;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] stackClip;
    [SerializeField] private AudioClip[] takeClip;


    // ДОБАВИТЬ МЕТОДЫ
    /// Моментально унести щит наверх
    public void HideAbove()
    {
        var tr = transform;
        tr.DOKill();
        tr.position = new Vector3(tr.position.x, _baseY + externalHideOffsetY, tr.position.z);
    }

    /// Кинуть щит сверху вниз на исходную высоту
    public Tween DropFromTop()
    {
        var tr = transform;
        tr.DOKill();
        tr.position = new Vector3(tr.position.x, _baseY + externalHideOffsetY, tr.position.z);
        return tr.DOMoveY(_baseY, externalDropDuration).SetEase(externalDropEase);
    }


    void Start()
    {
        if (cam == null)
            cam = Camera.main;
        // ДОБАВИТЬ В Start()
        _baseY = transform.position.y;
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

                PlayRandomSoundFrom(takeClip);


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
            StartCoroutine( PlayRandomSound(stackClip));
        }
    }
    private System.Collections.IEnumerator PlayRandomSound(AudioClip[] clips)
    {
        if (!(audioSource == null || clips == null || clips.Length == 0))
        {
            yield return new WaitForSeconds(liftDuration);
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayOneShot(clip);
        }
                    
        
    }
    private void PlayRandomSoundFrom(AudioClip[] clips)
    {
        if (!(audioSource == null || clips == null || clips.Length == 0))
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayOneShot(clip);
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
