using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private int damage = 1;

    [Header("Particles")]
    [SerializeField] private ParticleSystem ropeHitParticles;
    [SerializeField] private ParticleSystem defaultHitParticles;

    [Header("Settings")]
    [SerializeField] private float hitCooldown = 0.3f;

    private bool isThrown = false;
    private float currentSpeed;
    private TrailRenderer trail;
    private float lastHitTime = -999f;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null) trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
            trail.enabled = false;
    }

    public void Throw()
    {
        isThrown = true;
        currentSpeed = startSpeed;
        if (trail != null)
            trail.enabled = true;
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
        if (Time.time - lastHitTime < hitCooldown)
            return; 

        lastHitTime = Time.time;
        isThrown = false;

        if (trail != null)
            trail.enabled = false;

        TapePiece tape = collision.gameObject.GetComponent<TapePiece>();
        if (tape == null) tape = collision.gameObject.GetComponentInParent<TapePiece>();
        if (tape != null)
        {
            tape.TakeDamage(damage);
            if (ropeHitParticles != null)
                Instantiate(ropeHitParticles, collision.contacts[0].point, Quaternion.identity);
        }
        else
        {
            if (defaultHitParticles != null)
                Instantiate(defaultHitParticles, collision.contacts[0].point, Quaternion.identity);
        }
    }
}
