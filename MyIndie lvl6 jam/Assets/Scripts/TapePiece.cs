using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class TapePiece : MonoBehaviour
{
    [SerializeField] private int health = 3;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.3f;

    private Tween shakeTween;
    private float lastDamageTime = -999f;

    public void TakeDamage(int damage)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;
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
    }
}
