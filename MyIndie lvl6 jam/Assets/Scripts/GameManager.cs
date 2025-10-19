using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private GameObject barrier;
    [SerializeField] private Animator ropeAnimator;
    [SerializeField] private Animator handAnimator;
    [SerializeField] private TextMeshPro waveText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] barrierOpenSounds;   // звуки открытия
    [SerializeField] private AudioClip[] barrierCloseSounds;  // звуки закрытия
    [SerializeField] private AudioClip[] roundStartSounds;    // звуки старта раунда

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float barrierMoveDistance = 2f;
    [SerializeField] private float barrierMoveDuration = 0.5f;
    [SerializeField] private float delayBeforeAttack = 1f;

    private bool waitingForAttack = false;
    private bool canAttack = false;
    private bool isBarrierDown = true;
    private bool roundInProgress = false; // ✅ флаг, что раунд идёт
    private int roundCount = 0;
    private Vector3 barrierOriginalPosition;

    void Start()
    {
        if (barrier != null)
            barrierOriginalPosition = barrier.transform.position;

        UpdateWaveText();
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            roundCount++;
            UpdateWaveText();

            if (roundCount == 10)
            {
                Debug.Log("игрок победил");
                SceneManager.LoadScene("Win scene");
            }

            if (barrier != null)
                yield return ShowBarrier();

            enemyAI.MakeMove();
            waitingForAttack = true;
            

            yield return new WaitUntil(() => canAttack);
            roundInProgress = true; // ✅ начинаем раунд

            waitingForAttack = false;
            canAttack = false;

            yield return new WaitForSeconds(delayBeforeAttack);

            enemyAI.StartAttack();

            if (barrier != null)
                yield return HideBarrier();

            // ✅ Ждём, пока враг закончит атаку (если нужно — можно добавить событие окончания)
            yield return new WaitForSeconds(respawnDelay);

            roundInProgress = false; // ✅ теперь можно снова начать новый раунд
        }
    }

    public void StartRound()
    {
        // ✅ нельзя стартовать, если барьер не опущен или раунд уже идёт
        if (!isBarrierDown)
        {
            Debug.Log("⛔ Нельзя начать раунд — барьер ещё не опущен!");
            return;
        }

        if (roundInProgress)
        {
            Debug.Log("⚠️ Нельзя начать новый раунд — текущий ещё не закончился!");
            return;
        }

        canAttack = true;

        if (ropeAnimator != null)
            ropeAnimator.SetTrigger("StartRound");

        if (handAnimator != null)
            handAnimator.SetTrigger("StartRound");

        PlayRandomSound(roundStartSounds);
    }

    private IEnumerator ShowBarrier()
    {
        isBarrierDown = true;
        PlayRandomSound(barrierCloseSounds);

        Tween t = barrier.transform.DOMoveY(barrierOriginalPosition.y, barrierMoveDuration)
            .SetEase(Ease.InQuad);

        yield return t.WaitForCompletion();
    }

    private IEnumerator HideBarrier()
    {
        isBarrierDown = false;
        PlayRandomSound(barrierOpenSounds);

        Tween t = barrier.transform
            .DOMoveY(barrierOriginalPosition.y + barrierMoveDistance, barrierMoveDuration)
            .SetEase(Ease.OutQuad);

        yield return t.WaitForCompletion();
    }

    private void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"ROUND: {roundCount}";
            waveText.DOFade(1f, 0.3f).From(0f);
            waveText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.5f);
        }
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
