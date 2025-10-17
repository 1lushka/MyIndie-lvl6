using UnityEngine;
using UnityEngine.Events;

public class Rope : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onHoverStart;
    public UnityEvent onHoverEnd;
    public UnityEvent onPull;

    [Header("Settings")]
    [SerializeField] private Camera cam;

    private bool isHovered = false;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        bool hitThisFrame = Physics.Raycast(ray, out RaycastHit hit) &&
                            hit.collider != null &&
                            hit.collider.transform == transform;

        if (hitThisFrame && !isHovered)
        {
            isHovered = true;
            onHoverStart?.Invoke();
        }
        else if (!hitThisFrame && isHovered)
        {
            isHovered = false;
            onHoverEnd?.Invoke();
        }

        if (Input.GetMouseButtonDown(0) && isHovered)
        {
            onPull?.Invoke();
        }
    }
}