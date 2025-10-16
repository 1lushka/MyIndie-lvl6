using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement; // обязательно! для DOShakePosition и т.д.

public class TapePiece : MonoBehaviour
{
    [SerializeField] private int health = 3;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;

    private Tween shakeTween;

    private void OnCollisionEnter(Collision collision)
    {
        //Shake();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        Shake();

        if (health <= 0)
            Detach();
    }

    void Shake()
    {
        shakeTween?.Kill();
        shakeTween = transform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: shakeVibrato,
            randomness: shakeRandomness
        ).SetEase(Ease.OutQuad);
    }

    void Detach()
    {
        shakeTween?.Kill();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    rb.isKinematic = false;
        //    rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
        //}

        //Destroy(gameObject, 5f);
    }
}
