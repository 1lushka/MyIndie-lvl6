// CharacterNarrativeSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterNarrative", menuName = "Narrative/Character")]
public class CharacterNarrativeSO : ScriptableObject
{
    [TextArea(3, 6)]
    public string executionReasonText;   // ����� "�� ��� ��� ������"

    [Header("�������� �� ������� �������� (2, 1, 0)")]
    public Sprite spriteAt2;
    public Sprite spriteAt1;
    public Sprite spriteAt0;
}
