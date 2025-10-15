using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;

    private bool isThrown = false;

    private void Start()
    {
        Throw();
    }
    public void Throw()
    {
        isThrown = true;
    }

    void Update()
    {
        if (isThrown)
        {
            transform.Translate(Vector3.forward * -speed * Time.deltaTime);
        }
    }


    void OnCollisionEnter(Collision col)
    {
        isThrown = false;
        TapePiece tape = col.collider.GetComponent<TapePiece>();
        if (tape == null) tape = col.collider.GetComponentInParent<TapePiece>();
        if (tape)
        {
            tape.TakeDamage(damage);
            Destroy(gameObject); // ”ничтожаем после удара
        }

    }
}