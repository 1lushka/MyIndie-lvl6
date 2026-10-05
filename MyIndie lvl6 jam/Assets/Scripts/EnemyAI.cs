using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private TapePiece[] tapePieces;
    [SerializeField] private Transform[] axes;

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

    public void MakeMove()
    {
        if (tapePieces == null || tapePieces.Length == 0 || axes == null || axes.Length == 0)
            return;

        List<TapePiece> availablePieces = new List<TapePiece>(tapePieces);

        for (int i = 0; i < axes.Length; i++)
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
    }

    public void StartAttack()
    {
        foreach (Transform axe in axes)
        {
            if (axe == null) continue;

            Axe axeCtrl = axe.GetComponent<Axe>();
            if (axeCtrl != null)
                axeCtrl.Throw();
        }
    }

    // ====================== Туториал (одиночный нож) ======================

    public int AxeCount => axes == null ? 0 : axes.Length;

    public Axe GetAxe(int index)
    {
        if (axes == null || index < 0 || index >= axes.Length || axes[index] == null)
            return null;
        return axes[index].GetComponent<Axe>();
    }

    /// Ближайшая к указанной X верёвка (для ножа "по центру")
    public TapePiece GetTapeClosestToX(float x)
    {
        if (tapePieces == null || tapePieces.Length == 0) return null;

        TapePiece best = null;
        float bestDist = float.MaxValue;

        foreach (var tape in tapePieces)
        {
            if (tape == null) continue;
            float d = Mathf.Abs(tape.transform.position.x - x);
            if (d < bestDist)
            {
                bestDist = d;
                best = tape;
            }
        }

        return best;
    }

    /// Целит только один нож в указанную верёвку
    public void MakeMoveSingle(int axeIndex, TapePiece tape)
    {
        if (axes == null || axeIndex < 0 || axeIndex >= axes.Length) return;
        if (tape == null) return;

        Transform axe = axes[axeIndex];
        if (axe == null) return;

        float startY = startPositions != null && startPositions.Length > axeIndex
            ? startPositions[axeIndex].y : axe.position.y;
        float startZ = startPositions != null && startPositions.Length > axeIndex
            ? startPositions[axeIndex].z : axe.position.z;

        Vector3 targetPos = new Vector3(
            tape.transform.position.x,
            startY,
            startZ
        );

        ObjectMover.MoveTo(axe, targetPos);
    }

    /// Возвращает все ножи на стартовые позиции (без прицеливания)
    public void ReturnAxes()
    {
        if (startPositions == null || axes == null) return;

        for (int i = 0; i < axes.Length && i < startPositions.Length; i++)
        {
            if (axes[i] != null)
                axes[i].position = startPositions[i];
        }
    }

    /// Бросает только один нож
    public void StartAttackSingle(int axeIndex)
    {
        Axe axeCtrl = GetAxe(axeIndex);
        if (axeCtrl != null)
            axeCtrl.Throw();
    }
}