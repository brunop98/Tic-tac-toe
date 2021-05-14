using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class BoardPosition : MonoBehaviour
{
    public Icon placedIcon;
    public int posNumber;
    private SpriteRenderer renderer;

    bool canPlace => placedIcon == null;

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = null;
    }

    public void OnClick()
    {
        if (!canPlace)
        {
            Debug.LogError("Tentou colocar aonde não pode | " + GetPos());
            return;
        }

        Player p = GameManager.instance.WhoseTime();

        p.timesPlayed++;

        placedIcon = p.icon;
        renderer.sprite = placedIcon.sprite;
        renderer.color = placedIcon.color;

        // GameManager.instance.GetPosition(GetPos().x, GetPos().y);
        GameManager.UpdateBoard(GetPos(), placedIcon.GetChar());
        GameManager.instance.NextTurn();
    }

    private void OnMouseDown()
    {
        if (GameManager.instance.WhoseTime().isAI) return;
        OnClick();
    }

    private Vector2Int GetPos()
    {
        return new Vector2Int((posNumber - 1) % 3, posNumber / 3 + (posNumber % 3 == 0 ? -1 : 0));
    }

    private void OnValidate()
    {
        posNumber = transform.GetSiblingIndex() + 1;
        gameObject.name = "Position " + posNumber + " " + GetPos();
    }
}