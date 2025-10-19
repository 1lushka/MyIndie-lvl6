using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private TapePiece[] tapePieces;
    [SerializeField] private Transform[] axes;
    // ▼▼▼ ДОБАВИТЬ ПОЛЯ ВНУТРИ КЛАССА ▼▼▼
    [SerializeField] private float centerX = 0f;

    private int _activeKnives = 1;
    private bool _centerMode = true;

    public int MaxAxes => axes != null ? axes.Length : 0;
    // ▲▲▲ ДОБАВИТЬ ▲▲▲

    private Vector3[] startPositions;

    void Awake()
    {
        if (axes != null && axes.Length > 0)
        {
            startPositions = new Vector3[axes.Length];
            for (int i = 0; i < axes.Length; i++)
            {
                if (axes[i] != null)
                    startPositions[i] = axes[i].position;
            }
        }
    }
    // ▼▼▼ ДОБАВИТЬ ▼▼▼
    public void ConfigureForRound(int knivesToUse, bool centerThrow)
    {
        _centerMode = centerThrow;
        _activeKnives = Mathf.Clamp(knivesToUse, 0, MaxAxes);

        // Включаем первые N осей, остальные выключаем
        for (int i = 0; i < MaxAxes; i++)
        {
            if (axes[i] == null) continue;
            axes[i].gameObject.SetActive(i < _activeKnives);
        }
    }
    // ▲▲▲ ДОБАВИТЬ ▲▲▲

    public void MakeMove()
    {
        // ▼▼▼ ДОБАВИТЬ В НАЧАЛО MakeMove() ПОСЛЕ проверок tapePieces/axes ▼▼▼
        if (_centerMode)
        {
            // один нож строго по центру
            for (int i = 0; i < _activeKnives; i++)
            {
                Transform axe = axes[i];
                if (axe == null) continue;

                float startY = (startPositions != null && startPositions.Length > i) ? startPositions[i].y : axe.position.y;
                float startZ = (startPositions != null && startPositions.Length > i) ? startPositions[i].z : axe.position.z;

                Vector3 targetPos = new Vector3(centerX, startY, startZ);
                ObjectMover.MoveTo(axe, targetPos);
            }
            return; // не делаем рандом, если центр-режим
        }
        // ▲▲▲ ДОБАВИТЬ ▲▲▲

       

        if (tapePieces == null || tapePieces.Length == 0 || axes == null || axes.Length == 0)
            return;

        List<TapePiece> availablePieces = new List<TapePiece>(tapePieces);
        // ▼▼▼ ЗАМЕНИТЬ исходный цикл for на: ▼▼▼
        for (int i = 0; i < _activeKnives; i++)
        {
            Transform axe = axes[i];
            if (axe == null) continue;
            if (availablePieces.Count == 0)
            {
                Debug.LogWarning("Не хватает TapePiece для всех топоров!");
                break;
            }

            int randIndex = Random.Range(0, availablePieces.Count);
            TapePiece chosenPiece = availablePieces[randIndex];
            availablePieces.RemoveAt(randIndex);

            float startY = startPositions != null && startPositions.Length > i ? startPositions[i].y : axe.position.y;
            float startZ = startPositions != null && startPositions.Length > i ? startPositions[i].z : axe.position.z;

            Vector3 targetPos = new Vector3(
                chosenPiece.transform.position.x,
                startY,
                startZ
            );

            ObjectMover.MoveTo(axe, targetPos);
        }
        // ▲▲▲ ЗАМЕНА ▲▲▲
    }

    public void StartAttack()
    {
        // ▼▼▼ ЗАМЕНИТЬ foreach на: ▼▼▼
        for (int i = 0; i < _activeKnives; i++)
        {
            var axe = axes[i];
            if (axe == null) continue;

            Axe axeCtrl = axe.GetComponent<Axe>();
            if (axeCtrl != null)
                axeCtrl.Throw();
        }
        // ▲▲▲ ЗАМЕНА ▲▲▲

    }







}
