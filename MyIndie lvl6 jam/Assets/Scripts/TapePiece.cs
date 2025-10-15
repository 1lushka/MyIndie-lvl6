using UnityEngine;

public class TapePiece : MonoBehaviour
{
    [SerializeField] private int health = 3;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Detach();
        }
    }

    void Detach()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; 
            rb.AddForce(Vector3.up * 3f, ForceMode.Impulse); 
        }

        Destroy(gameObject, 5f);

    }
}