using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private int damage = 1;

    private bool isThrown = false;
    private float currentSpeed;

    public void Throw()
    {
        isThrown = true;
        currentSpeed = startSpeed;
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

        TapePiece tape = collision.gameObject.GetComponent<TapePiece>();
        if (tape == null) tape = collision.gameObject.GetComponentInParent<TapePiece>();
        if (tape != null)
        {
            tape.TakeDamage(damage);
        }
    }
}
