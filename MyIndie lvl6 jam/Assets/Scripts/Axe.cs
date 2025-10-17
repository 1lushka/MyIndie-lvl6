using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private int damage = 1;

    private bool isThrown = false;
    private float currentSpeed;
    private TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null ) trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
            trail.enabled = false; // по умолчанию трейл выключен
    }

    public void Throw()
    {
        isThrown = true;
        currentSpeed = startSpeed;
        if (trail != null)
            trail.enabled = true; // включаем трейл при броске
    }

    void Update()
    {
        if (isThrown)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            Vector3 targetPos = transform.position + transform.forward * -currentSpeed * Time.deltaTime;
            ObjectMover.MoveTo(transform, targetPos);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isThrown = false;

        if (trail != null)
            trail.enabled = false; // выключаем трейл после удара

        TapePiece tape = collision.gameObject.GetComponent<TapePiece>();
        if (tape == null) tape = collision.gameObject.GetComponentInParent<TapePiece>();
        if (tape != null)
        {
            tape.TakeDamage(damage);
        }
    }
}
