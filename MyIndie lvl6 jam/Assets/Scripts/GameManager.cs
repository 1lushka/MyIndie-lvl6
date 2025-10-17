using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private GameObject barrier;

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float barrierScaleDuration = 0.3f;

    private bool waitingForAttack = false;
    private bool canAttack = false;
    private int roundCount = 0;
    private Vector3 barrierOriginalScale;

    void Start()
    {
        if (barrier != null)
        {
            barrierOriginalScale = barrier.transform.localScale;
            barrier.transform.localScale = Vector3.zero;
            barrier.SetActive(false);
        }

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {

            roundCount++;
            if (roundCount == 10) print("игрок победил");

            if (barrier != null)
                ShowBarrier();

            enemyAI.MakeMove();
            waitingForAttack = true;

            yield return new WaitUntil(() => canAttack);

            waitingForAttack = false;
            canAttack = false;

            enemyAI.StartAttack();

            if (barrier != null)
                HideBarrier();

            yield return new WaitForSeconds(respawnDelay);
        }
    }

    public void StartRound()
    {
        canAttack = true;
    }

    private void ShowBarrier()
    {
        barrier.SetActive(true);
        barrier.transform.localScale = Vector3.zero;
        barrier.transform.DOScale(barrierOriginalScale, barrierScaleDuration).SetEase(Ease.OutBack);
    }

    private void HideBarrier()
    {
        barrier.transform.DOScale(Vector3.zero, barrierScaleDuration).SetEase(Ease.InBack)
            .OnComplete(() => barrier.SetActive(false));
    }
}
