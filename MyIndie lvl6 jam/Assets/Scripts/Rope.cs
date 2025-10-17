using UnityEngine;
using UnityEngine.Events;

public class Rope : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPull;

    [Header("Settings")]
    [SerializeField] private Camera cam;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.transform == transform)
                {
                    TriggerEvent();
                }
            }
        }
    }

    private void TriggerEvent()
    {
        onPull?.Invoke();
    }
}
