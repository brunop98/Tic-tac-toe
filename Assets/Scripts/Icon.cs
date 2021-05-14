using UnityEngine;

[CreateAssetMenu(menuName = "Tic Tac Toe/Icon")]
public class Icon : ScriptableObject
{
    public string c;
    public Sprite sprite;
    public Color color;

    public char GetChar()
    {
        return c[0];
    }
}