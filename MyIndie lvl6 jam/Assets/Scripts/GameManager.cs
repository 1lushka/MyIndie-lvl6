using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro; 

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private GameObject barrier;
    [SerializeField] private Animator ropeAnimator;
    [SerializeField] private Animator handAnimator;
    [SerializeField] private TextMeshPro waveText;

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float barrierMoveDistance = 2f;
    [SerializeField] private float barrierMoveDuration = 0.5f;
    [SerializeField] private float delayBeforeAttack = 1f;

    private bool waitingForAttack = false;
    private bool canAttack = false;
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
                print("игрок победил");

            if (barrier != null)
                ShowBarrier();

            enemyAI.MakeMove();
            waitingForAttack = true;

            yield return new WaitUntil(() => canAttack);

            waitingForAttack = false;
            canAttack = false;

            yield return new WaitForSeconds(delayBeforeAttack);

            enemyAI.StartAttack();

            if (barrier != null)
                HideBarrier();

            yield return new WaitForSeconds(respawnDelay);
        }
    }

    public void StartRound()
    {
        canAttack = true;

        if (ropeAnimator != null)
            ropeAnimator.SetTrigger("StartRound");

        if (handAnimator != null)
            handAnimator.SetTrigger("StartRound");
    }

    private void ShowBarrier()
    {
        barrier.transform.DOMoveY(barrierOriginalPosition.y, barrierMoveDuration)
            .SetEase(Ease.InQuad);
    }

    private void HideBarrier()
    {
        barrier.transform.DOMoveY(barrierOriginalPosition.y + barrierMoveDistance, barrierMoveDuration)
            .SetEase(Ease.OutQuad);
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
}
