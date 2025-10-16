using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private GameObject barrier;

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 3f;  

    private bool waitingForAttack = false;
    private int roundCount = 0;

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            roundCount++;
            if (roundCount == 10) print("игрок победил");
            if (barrier != null)
                barrier.SetActive(true);

            enemyAI.MakeMove();
            waitingForAttack = true;

            yield return new WaitUntil(() =>
                Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame
            );

            waitingForAttack = false;

            enemyAI.StartAttack();

            if (barrier != null)
                barrier.SetActive(false);

            yield return new WaitForSeconds(respawnDelay);
        }
    }
}
