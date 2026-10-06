using UnityEngine;

public class ExitButton3D : MonoBehaviour
{
    [SerializeField] private StartMenu startMenu;

    private void OnMouseDown()
    {
        if (startMenu != null)
        {
            startMenu.ExitGame();
        }
        else
        {
            Debug.LogWarning("ExitButton3D: не назначен StartMenu");
        }
    }
}