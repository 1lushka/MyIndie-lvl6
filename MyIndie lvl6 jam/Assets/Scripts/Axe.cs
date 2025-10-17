using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    private bool isThrown = false;

    private void Start()
    {
        //Throw();
    }
    public void Throw()
    {
        isThrown = true;
    }

    void Update()
    {
        if (isThrown)
        {
            Vector3 targetPos = transform.position + transform.forward * -speed * Time.deltaTime;

            ObjectMover.MoveTo(transform, targetPos);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isThrown = false;
        TapePiece tape = collision.gameObject.GetComponent<TapePiece>();
        if(tape == null) tape = collision.gameObject.GetComponentInParent<TapePiece>();
        if(tape != null)
        {
            tape.TakeDamage(damage);
        }
    }
}