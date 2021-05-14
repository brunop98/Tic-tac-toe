using UnityEngine;

public class Player : MonoBehaviour
{
    public Icon icon;
    public bool isAI = false;
    public int timesPlayed = 0;

    public void SetIsAI(bool n)
    {
        isAI = n;
    }
}