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

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
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
