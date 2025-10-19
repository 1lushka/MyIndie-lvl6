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

    // ▼▼▼ ДОБАВИТЬ ▼▼▼
    [Header("Shields (training)")]
    [SerializeField] private System.Collections.Generic.List<Transform> shields = new System.Collections.Generic.List<Transform>();
    [SerializeField] private float shieldsHideOffsetY = 8f;
    [SerializeField] private float shieldsDropDuration = 0.45f;
    [SerializeField] private Ease shieldsDropEase = Ease.OutQuad;

    private float[] _shieldBaseY;
    private int _shieldsDropped = 0;
    private bool _shieldsInitialized = false;
    // ▲▲▲ ДОБАВИТЬ ▲▲▲


    private bool waitingForAttack = false;
    private bool canAttack = false;
    private bool isBarrierDown = true;
    private bool roundInProgress = false; // ✅ флаг, что раунд идёт
    private int roundCount = 0;
    [SerializeField] private int roundToWin = 15;
    private Vector3 barrierOriginalPosition;

    [SerializeField] private ShieldsRoundDirector shieldsDirector;
    void Start()
    {
        if (barrier != null)
            barrierOriginalPosition = barrier.transform.position;

        UpdateWaveText();
        // ▼▼▼ ДОБАВИТЬ ▼▼▼
        if (shields != null && shields.Count > 0 && !_shieldsInitialized)
        {
            _shieldBaseY = new float[shields.Count];
            for (int i = 0; i < shields.Count; i++)
            {
                if (shields[i] == null) continue;
                _shieldBaseY[i] = shields[i].position.y;
                shields[i].position = new Vector3(
                    shields[i].position.x,
                    _shieldBaseY[i] + shieldsHideOffsetY,
                    shields[i].position.z
                );
            }
            _shieldsInitialized = true;
        }
        // ▲▲▲ ДОБАВИТЬ ▲▲▲
        StartCoroutine(GameLoop());
        if (shieldsDirector != null) shieldsDirector.HideAll();
    }
    // ▼▼▼ ДОБАВИТЬ ▼▼▼
    private void DropNewShieldsForRound(int roundNumber)
    {
        if (!_shieldsInitialized || shields == null || _shieldBaseY == null) return;

        // Сколько щитов должно быть к этому раунду (1 на 1-м, 2 на 2-м, 3 на 3-м и т.д.)
        int planned = Mathf.Clamp(roundNumber, 0, shields.Count);

        // Сколько ещё не уронено
        int toDrop = Mathf.Clamp(planned - _shieldsDropped, 0, shields.Count - _shieldsDropped);

        for (int i = 0; i < toDrop; i++)
        {
            int index = _shieldsDropped + i;
            var tr = shields[index];
            if (tr == null) continue;

            tr.DOKill();
            // гарантируем старт сверху
            tr.position = new Vector3(tr.position.x, _shieldBaseY[index] + shieldsHideOffsetY, tr.position.z);

            tr.DOMoveY(_shieldBaseY[index], shieldsDropDuration)
              .SetEase(shieldsDropEase);
        }

        _shieldsDropped += toDrop;
    }
    // ▲▲▲ ДОБАВИТЬ ▲▲▲

    private IEnumerator GameLoop()
    {
        while (true)
        {
            roundCount++;
            UpdateWaveText();

            if (roundCount == roundToWin)
            {
                Debug.Log("игрок победил");
                SceneManager.LoadScene("Win scene");
            }

            if (barrier != null)
                yield return ShowBarrier();
            // ▼▼▼ ДОБАВИТЬ СТРОГО ПЕРЕД enemyAI.MakeMove(); ▼▼▼
            if (enemyAI != null)
            {
                // 1 нож на 1-2 раундах; со 2-го перехода (т.е. на 3-м раунде) — round-1
                int knivesThisRound = (roundCount < 3) ? 1 : (roundCount - 1);
                // ограничиваем по количеству осей у врага
                knivesThisRound = Mathf.Clamp(knivesThisRound, 1, enemyAI.MaxAxes);
                bool centerThrow = (knivesThisRound == 1);
                enemyAI.ConfigureForRound(knivesThisRound, centerThrow);
            }
            // ▲▲▲ ДОБАВИТЬ ▲▲▲
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
            // 1) Роняем нужное число щитов
            if (shieldsDirector != null)
                shieldsDirector.DropShieldsForRound(roundCount);

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
        int knivesThisRound = Mathf.Clamp(roundCount, 1, enemyAI != null ? enemyAI.MaxAxes : roundCount);
        
        // 2) Готовим врага: он будет бросать N ножей строго по центру
        if (enemyAI != null)
            enemyAI.ConfigureForRound(knivesThisRound, true);
        canAttack = true;
        // ▼▼▼ ДОБАВИТЬ ▼▼▼
        DropNewShieldsForRound(roundCount);
        // ▲▲▲ ДОБАВИТЬ ▲▲▲

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
            waveText.text = $"LEFT: {roundToWin- roundCount+1}";
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
