using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        PLAYING,
        GAME_OVER,
        MENU
    }

    [Header("Main")]
    public GameObject ticTacToe;
    public GameState state;
    public static int turn;
    public Row[] rows;


    [Header("Players")]
    public Player player1;
    public Player player2;

    public static GameManager instance;
    public static UnityAction onNextTurn;
    public static UnityAction onStartGame;



    [Header("Turn")]
    public UnityEvent onPlay;

    [Header("Victory")]
    public LineRenderer victoryLine;
    public TextMeshProUGUI victoryText;
    public UnityEvent onGameover;
    public UnityEvent onVictory; 
    public UnityEvent onDraw; 


    public static char[][] board;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        ticTacToe.SetActive(false);
    }

    public void StartGame()
    {
        board = new char[][]{
                 new char[] { ' ', ' ', ' ' },
                 new char[] { ' ', ' ', ' ' },
                 new char[] { ' ', ' ', ' ' },
        };

        state = GameState.PLAYING;
        victoryLine.enabled = false;
        turn = -1;
        ticTacToe.SetActive(true);
        onStartGame?.Invoke();
        NextTurn();

    }

    public GameVictory CheckVictories()
    {
        GameVictory r;

        //Horizontal
        r = CheckLineVictory(GetRow(0));
        if (r) return r;

        r = CheckLineVictory(GetRow(1));
        if (r) return r;

        r = CheckLineVictory(GetRow(2));
        if (r) return r;

        r = CheckLineVictory(GetCollum(0));
        if (r) return r;

        r = CheckLineVictory(GetCollum(1));
        if (r) return r;

        r = CheckLineVictory(GetCollum(2));
        if (r) return r;

        r = CheckLineVictory(new GridPosition[] { Get(0, 0), Get(1, 1), Get(2, 2) });
        if (r) return r;

        r = CheckLineVictory(new GridPosition[] { Get(2, 0), Get(1, 1), Get(0, 2) });
        if (r) return r;

        r.sucess = false;
        return r;
    }

    public static GameVictory CheckLineVictory(GridPosition[] line)
    {
        //If there's any blank spce
        if (line.Any(t => t.character == ' ')) return new GameVictory { sucess = false };

        // If all characters of positions are not the same
        if (!line.All(g => g.character == line[0].character)) return new GameVictory { sucess = false };

        return new GameVictory { sucess = true, startPoint = line[0], endPoint = line[2] };
    }

    public static GridPosition[] GetDiagonal1()
    {
        return new GridPosition[] { Get(0, 0), Get(1, 1), Get(2, 2) };
    }

    public static GridPosition[] GetDiagonal2()
    {
        return new GridPosition[] { Get(2, 0), Get(1, 1), Get(0, 2) };
    }

    private bool IsPositionValid(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x > 2) return false;
        if (pos.y < 0 || pos.y > 2) return false;

        return true;
    }

    private bool IsSameIcon(Vector2Int pos, Icon c)
    {
        //if (rows[pos.y].Collum[pos.x].placedIcon == null) return false;
        // if (rows[pos.y].Collum[pos.x].placedIcon != c) return false;

        return true;
    }

    public static GridPosition[] GetRow(int r)
    {
        //    Debug.Log("Get Row " + r);

        return new GridPosition[] {
                new GridPosition{ character = GameManager.board[0][r], x = 0, y = r },
                new GridPosition{ character = GameManager.board[1][r], x = 1, y = r },
                new GridPosition{ character = GameManager.board[2][r], x = 2, y = r },
            };
    }

    public static GridPosition[] GetCollum(int c)
    {
        //       Debug.Log("GetCollum " + c);

        return new GridPosition[] {
                new GridPosition{ character = GameManager.board[c][0], x = c, y = 0 },
                new GridPosition{ character = GameManager.board[c][1], x = c, y = 1 },
                new GridPosition{ character = GameManager.board[c][2], x = c, y = 2 },
            };
    }

    public BoardPosition GetPosition(int x, int y)
    {
        Debug.Log($"{x} {y}  {rows[y].Collum[x].gameObject.name}");
        return rows[y].Collum[x];
    }

    public Player WhoseTime()
    {
        return turn % 2 == 0 ? player1 : player2;
    }

    public static void UpdateBoard(Vector2Int pos, char c)
    {

        board[pos.x][pos.y] = c;
        ShowGrid();
    }

#if UNITY_EDITOR

    public static void ShowGrid()
    {
        Debug.Log($" {board[0][0]}  |  {board[1][0]}  |  {board[2][0]} ");
        Debug.Log($" {board[0][1]}  |  {board[1][1]}  |  {board[2][1]} ");
        Debug.Log($" {board[0][2]}  |  {board[1][2]}  |  {board[2][2]} ");
        Debug.Log($" ");
    }

#endif

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextTurn()
    {
        if (state != GameState.PLAYING) return;

        onPlay.Invoke();

        GameVictory victory = CheckVictories();
        if (victory)
        {
            GameOverVictory(victory);
            return;
        }

        turn++;

        if (turn < 9)
        {
            onNextTurn?.Invoke();
        } else
        {
            GameOverDraw();
        }
    }

    void GameOverVictory( GameVictory v)
    {
        Vector3[] pos = new Vector3[2];

        pos[0] = rows[v.startPoint.y].Collum[v.startPoint.x].transform.position ;
        pos[1] = rows[v.endPoint.y].Collum[v.endPoint.x].transform.position ;


        /*
        Vector3 m = (pos[0] + pos[1]) / 2;

        pos[0] = (m - pos[0]) * 2;
        pos[1] = (m - pos[1]) * 2;*/

        victoryLine.enabled = true;
        victoryLine.useWorldSpace = true;
        victoryLine.SetPositions(pos);

        Player winner = WhoseTime();
        victoryText.text = winner.icon.c + " victory!";
        victoryText.color = winner.icon.color;

        state = GameState.GAME_OVER;

        onGameover.Invoke();
        onVictory.Invoke();


    }

    void GameOverDraw()
    {
        victoryText.text = "Draw!";
        victoryText.color =  Color.black;
        onGameover.Invoke();
        onDraw.Invoke();
    }

    public void Play(AI_Result play)
    {
        Debug.Log(play);
        GetPosition(play.x, play.y).OnClick();
    }

    public static int GetCurrentTurn()
    {
        return turn;
    }

    public static GridPosition Get(int posX, int posY)
    {
        return new GridPosition { character = board[posX][posY], x = posX, y = posY };
    }

    public static Vector2Int Get(GridPosition p)
    {
        return new Vector2Int(p.x, p.y);
    }

    
}

[System.Serializable]
public struct Row
{
    public BoardPosition[] Collum;
}

public struct GridPosition
{
    public char character;
    public int x;
    public int y;

    public override string ToString()
    {
        return $"{character.ToString().ToUpper()} (x:{x}, y:{y})";
    }
}

public struct GameVictory
{
    public bool sucess;
    public GridPosition startPoint;
    public GridPosition endPoint;

    public override string ToString()
    {
        return $"Game victory -> {sucess} starting: {startPoint} | end:{endPoint} ";
    }

    public static implicit operator bool(GameVictory foo)
    {
        return foo.sucess;
    }
}