using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Управляет появлением/скрытием щитов (в основном для туториала).
/// Вешается на объект "Shields". Если список shields пуст — сам собирает дочерние объекты.
/// </summary>
public class ShieldsRoundDirector : MonoBehaviour
{
    [SerializeField] private List<Transform> shields = new List<Transform>();

    [Header("Hide / Show")]
    [SerializeField] private float hideOffsetY = 8f;
    [SerializeField] private float dropDuration = 0.45f;
    [SerializeField] private Ease dropEase = Ease.OutQuad;
    [SerializeField] private float showStagger = 0.15f;

    [Tooltip("Выключать коллайдеры, пока щит скрыт")]
    [SerializeField] private bool toggleColliders = true;

    private float[] baseY;
    private Collider[][] colliders;
    private bool[] visible;

    public float DropDuration => dropDuration;
    public int Count => shields.Count;

    void Awake()
    {
        if (shields.Count == 0)
        {
            foreach (Transform child in transform)
                shields.Add(child);
        }

        baseY = new float[shields.Count];
        colliders = new Collider[shields.Count][];
        visible = new bool[shields.Count];

        for (int i = 0; i < shields.Count; i++)
        {
            if (shields[i] == null) continue;

            baseY[i] = shields[i].position.y;
            colliders[i] = toggleColliders
                ? shields[i].GetComponentsInChildren<Collider>(true)
                : null;
        }

        HideAll();
    }

    /// Мировая X-позиция щита (индекс 0 — центральный щит)
    public float GetShieldWorldX(int index)
    {
        if (index < 0 || index >= shields.Count || shields[index] == null) return 0f;
        return shields[index].position.x;
    }

    /// Прячет все щиты (поднимает вверх)
    public void HideAll()
    {
        for (int i = 0; i < shields.Count; i++)
            SetHidden(i, true, 0f);
    }

    /// Опускает один щит на место. Возвращает Tween (null — если щит уже видим)
    public Tween Drop(int index, float delay = 0f)
    {
        if (index < 0 || index >= shields.Count || shields[index] == null) return null;
        if (visible[index]) return null;

        return SetHidden(index, false, delay);
    }

    /// Опускает все ещё скрытые щиты с каскадом
    public void ShowAll(float stagger = -1f)
    {
        if (stagger < 0f) stagger = showStagger;

        for (int i = 0; i < shields.Count; i++)
            Drop(i, i * stagger);
    }

    private Tween SetHidden(int index, bool hidden, float delay)
    {
        Transform tr = shields[index];
        tr.DOKill();

        SetColliders(index, !hidden);
        visible[index] = !hidden;

        if (hidden)
        {
            tr.position = new Vector3(tr.position.x, baseY[index] + hideOffsetY, tr.position.z);
            return null;
        }

        // стартовая позиция — сверху, чтобы щит "прилетел"
        tr.position = new Vector3(tr.position.x, baseY[index] + hideOffsetY, tr.position.z);

        return tr.DOMoveY(baseY[index], dropDuration)
            .SetEase(dropEase)
            .SetDelay(delay)
            .SetLink(tr.gameObject, LinkBehaviour.KillOnDestroy);
    }

    private void SetColliders(int index, bool enabled)
    {
        if (!toggleColliders) return;
        if (colliders == null || colliders[index] == null) return;

        foreach (var c in colliders[index])
            if (c != null) c.enabled = enabled;
    }
}
