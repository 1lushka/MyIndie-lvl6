using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private int damage = 1;
    [SerializeField] private Transform target;     // цель
    [SerializeField] private float arcHeight = 2f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem ropeHitParticles;
    [SerializeField] private ParticleSystem defaultHitParticles;

    [Header("Settings")]
    [SerializeField] private float hitCooldown = 0.3f;

    private bool isThrown = false;
    private Animator anim;
    private float currentSpeed;
    private TrailRenderer trail;
    private float lastHitTime = -999f;

    // максимальная высота дуги над стартовой высотой
    private Vector3 arcStart;

    private void Awake()
    {
        arcStart=transform.position;
        anim = GetComponent<Animator>();
        trail = GetComponent<TrailRenderer>();
        if (trail == null) trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
            trail.enabled = false;
    }

    public void Throw()
    {
        if (anim) anim.SetBool("Flying", true);

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
            targetPos = ApplyArcHeight(targetPos);
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
            if (anim) anim.SetBool("Flying",false);
            if (anim) anim.SetBool("InShield", false);

        }
        else
        {
            if (defaultHitParticles != null)
                Instantiate(defaultHitParticles, collision.contacts[0].point, Quaternion.identity);
            if (anim) anim.SetBool("InShield", true);
            if (anim) anim.SetBool("Flying", false);

        }
    }
    private Vector3 ApplyArcHeight(Vector3 currentPos)
    {
        if (target == null) return currentPos;

        // Линия движения по земле/пространству: от старта до цели (цель — на высоте старта)
        Vector3 a = arcStart;
        Vector3 b = target.position;
        b.y = a.y;

        Vector3 ab = b - a;
        float abLenSqr = ab.sqrMagnitude;
        if (abLenSqr < 1e-6f) return currentPos;

        // Нормализованный прогресс вдоль линии старта->цели (0..1) исходя из текущего положения топора
        float s = Mathf.Clamp01(Vector3.Dot(currentPos - a, ab) / abLenSqr);

        // Параболическая добавка по высоте: пик в середине (s=0.5), 0 на концах
        float yOffset = 4f * arcHeight * s * (1f - s);

        // Применяем высоту: базовый уровень — высота старта
        currentPos.y = a.y + yOffset;
        return currentPos;
    }
}
