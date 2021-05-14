using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;

public class AI_Player : MonoBehaviour
{
    private Player p;

    char enemy => GameManager.instance.player1.icon.GetChar();
    char player => p.icon.GetChar();


    private void Start()
    {
        p = GetComponent<Player>();
        GameManager.onNextTurn += PlayTurn;
    }

    private void OnDestroy()
    {
        GameManager.onNextTurn -= PlayTurn;
    }

    public void PlayTurn()
    {
        Debug.Log("Play turn " + GameManager.instance.WhoseTime().isAI);
        if (!GameManager.instance.WhoseTime().isAI  ) return;

        StartCoroutine(WaitTime());
        
    }

    IEnumerator WaitTime()
    {
        Debug.Log("Started coroutine");
        yield return new WaitForSeconds(1);
        GameManager.instance.Play(GetBestPlay());
    }

    public AI_Result GetBestPlay()
    {
        AI_Result r;
        AI_Result[] resultList;

        r = CheckWinPoint(player);
        if (r) return r;

        r = IsAnyBlock(enemy);
        if (r) return r;

        resultList = GetForks(player);
        Debug.Log("Frok 1 results " + resultList.Length);
        if (resultList.Length > 0)
        {
            //Check if theres enemyFork
            AI_Result[] enemyForks = GetForks(enemy);

            if (enemyForks.Length > 0)
            {
                //Search for positions that block 2 or more forks
                AI_Result[] intersect = enemyForks.GroupBy(p => p).Where(t => t.Count() > 1).Select(g => g.Key).ToArray();

                if (intersect.Length > 0)
                {
                    return intersect[0];
                }
            }

            if (resultList[0]) return resultList[0];
        }


        resultList = GetForks(enemy);
        Debug.Log("Frok 2 results " + resultList.Length);
        if (resultList.Length > 0)
        {
            if (resultList[0]) return resultList[0];
        }

        r = Center(enemy, p.timesPlayed == 0 );
        if (r) return r;

        r = EmptyCorner();
        if (r) return r;

        r = EmptySide();
        return r;
    }

    public GameVictory CheckVictories()
    {
        GameVictory r;

        //Horizontal
        r = CheckLineVictory(GameManager.GetRow(0));
        if (r) return r;

        r = CheckLineVictory(GameManager.GetRow(1));
        if (r) return r;

        r = CheckLineVictory(GameManager.GetRow(2));
        if (r) return r;

        r = CheckLineVictory(GameManager.GetRow(0));
        if (r) return r;

        r = CheckLineVictory(GameManager.GetRow(1));
        if (r) return r;

        r = CheckLineVictory(GameManager.GetRow(2));
        if (r) return r;

        r = CheckLineVictory(new GridPosition[] { GameManager.Get(0, 0), GameManager.Get(1, 1), GameManager.Get(2, 2) });
        if (r) return r;

        r = CheckLineVictory(new GridPosition[] { GameManager.Get(2, 0), GameManager.Get(1, 1), GameManager.Get(0, 2) });
        if (r) return r;

        r.sucess = false;
        return r;
    }

    public GameVictory CheckLineVictory(GridPosition[] line)
    {
        //If there's any blank spce
        if (line.Any(t => t.character == ' ')) return new GameVictory { sucess = false };

        // If all characters of positions are not the same
        if (!line.All(g => g.character == line[0].character)) return new GameVictory { sucess = false };

        return new GameVictory { sucess = true, startPoint = line[0], endPoint = line[2] };
    }

    public AI_Result CheckWinPoint(char playerCharacter)
    {
        if (GameManager.turn < 5)
        {
            return new AI_Result { sucess = false };
        }

        AI_Result ret;
        Debug.Log("Check win point " + playerCharacter);

        ret = WinPositions(GameManager.GetRow(0), playerCharacter);
        if (ret) return ret;

        ret = WinPositions(GameManager.GetRow(1), playerCharacter);
        if (ret) return ret;

        ret = WinPositions(GameManager.GetRow(2), playerCharacter);
        if (ret) return ret;

        ret = WinPositions(GameManager.GetCollum(0), playerCharacter);
        if (ret) return ret;

        ret = WinPositions(GameManager.GetCollum(1), playerCharacter);
        if (ret) return ret;

        ret = WinPositions(GameManager.GetCollum(2), playerCharacter);
        if (ret) return ret;

        ret = WinPositions(GameManager.GetDiagonal1(), playerCharacter);
        if (ret) return ret;

        ret = WinPositions(GameManager.GetDiagonal2(), playerCharacter);
        if (ret) return ret;

        ret.sucess = false;

        return ret;
    }

    public AI_Result WinPositions(GridPosition[] pos, char playerChar)
    {
        //If there's no blankSpace
        if (pos.Count(p => p.character == ' ') != 1) return new AI_Result { sucess = false };

        //If
        if (pos.Count(p => p.character == playerChar) != 2) return new AI_Result { sucess = false };

        GridPosition p = pos.First(p => p.character == ' ');
        return new AI_Result { sucess = true, x = p.x, y = p.y };
    }

    public AI_Result IsAnyBlock(char enemyChar)
    {
        AI_Result g = new AI_Result { sucess = false };

        if (GameManager.instance.player1.timesPlayed < 2)
        {
            return g;
        }

        Debug.Log("IsAnyBlock " + enemyChar);

        //Horizontal
        g = CheckBlock(GameManager.GetRow(0), enemyChar);
        if (g) return g;

        g = CheckBlock(GameManager.GetRow(1), enemyChar);
        if (g) return g;

        g = CheckBlock(GameManager.GetRow(2), enemyChar);
        if (g) return g;

        g = CheckBlock(GameManager.GetCollum(0), enemyChar);
        if (g) return g;

        g = CheckBlock(GameManager.GetCollum(1), enemyChar);
        if (g) return g;

        g = CheckBlock(GameManager.GetCollum(2), enemyChar);
        if (g) return g;

        g = CheckBlock(GameManager.GetDiagonal1(), enemyChar);
        if (g) return g;

        g = CheckBlock(GameManager.GetDiagonal2(), enemyChar);
        if (g) return g;

        g.sucess = false;
        return g;
    }

    public AI_Result CheckBlock(GridPosition[] positions, char enemyChar)
    {
        //If the blank space count is greater than 1
        if (positions.Count(p => p.character == ' ') != 1) return new AI_Result { sucess = false };

        //Check if there's 2 spaces with the enemy char
        if (positions.Count(p => p.character == enemyChar) != 2) return new AI_Result { sucess = false };

        //Search for the blank space
        GridPosition result = positions.First(p => p.character == ' ');

        Debug.Log("check block result: " + result);
        return new AI_Result { sucess = true, x = result.x, y = result.y };
    }

    public AI_Result[] GetFork(char player)
    {
        AI_Result[] aux;

        aux = IsAnyForks(GameManager.GetRow(0), player);
        if (aux.Length > 0) return aux;

        aux = IsAnyForks(GameManager.GetRow(1), player);
        if (aux.Length > 0) return aux;

        aux = IsAnyForks(GameManager.GetRow(2), player);
        if (aux.Length > 0) return aux;

        aux = IsAnyForks(GameManager.GetRow(0), player);
        if (aux.Length > 0) return aux;

        aux = IsAnyForks(GameManager.GetRow(1), player);
        if (aux.Length > 0) return aux;

        aux = IsAnyForks(GameManager.GetRow(2), player);
        if (aux.Length > 0) return aux;

        aux = IsAnyForks(GameManager.GetDiagonal1(), player);
        if (aux.Length > 0) return aux;

        aux = IsAnyForks(GameManager.GetDiagonal2(), player);
        if (aux.Length > 0) return aux;

        aux = new AI_Result[0];

        return aux;
    }

    public AI_Result[] GetForks(char player)
    {
        Debug.Log("Get forks " + player);
        List<AI_Result> results = new List<AI_Result>();
        AI_Result[] aux;

        aux = IsAnyForks(GameManager.GetRow(0), player);
        if (aux.Length > 0) results.AddRange(aux);

        aux = IsAnyForks(GameManager.GetRow(1), player);
        if (aux.Length > 0) results.AddRange(aux);

        aux = IsAnyForks(GameManager.GetRow(2), player);
        if (aux.Length > 0) results.AddRange(aux);

        aux = IsAnyForks(GameManager.GetRow(0), player);
        if (aux.Length > 0) results.AddRange(aux);

        aux = IsAnyForks(GameManager.GetRow(1), player);
        if (aux.Length > 0) results.AddRange(aux);

        aux = IsAnyForks(GameManager.GetRow(2), player);
        if (aux.Length > 0) results.AddRange(aux);

        aux = IsAnyForks(GameManager.GetDiagonal1(), player);
        if (aux.Length > 0) results.AddRange(aux);

        aux = IsAnyForks(GameManager.GetDiagonal2(), player);
        if (aux.Length > 0) results.AddRange(aux);

        return results.ToArray();
    }

    public AI_Result[] IsAnyForks(GridPosition[] positions, char player)
    {
        AI_Result[] results;

        if (positions.Count(p => p.character == ' ') == 2 && positions.Count(p => p.character == player) == 1)
        {
            GridPosition[] f = positions.Where(p => p.character == ' ').ToArray();
            results = new AI_Result[f.Length];
            for (int i = 0; i < f.Length; i++)
            {
                results[i] = new AI_Result
                {
                    sucess = true,
                    x = f[i].x,
                    y = f[i].y
                };
            }

            return results;
        }

        results = new AI_Result[0];

        return results;
    }

    public AI_Result Center(char enemyChar, bool firstMove)
    {
        Debug.Log("center " + firstMove);
        if (GameManager.Get(1, 1).character == enemyChar && firstMove)
        {
            return new AI_Result
            {
                sucess = true,
                x = 0,
                y = 0
            };
        }
        Debug.Log("Center " );
        return new AI_Result { sucess = false };
    }

    public AI_Result EmptyCorner()
    {
        if (GameManager.Get(2, 2).character == ' ') return new AI_Result { sucess = true, x = 2, y = 2 };
        if (GameManager.Get(0, 2).character == ' ') return new AI_Result { sucess = true, x = 0, y = 2 };

        if (GameManager.Get(2, 0).character == ' ') return new AI_Result { sucess = true, x = 2, y = 0 };
        if (GameManager.Get(0, 0).character == ' ') return new AI_Result { sucess = true, x = 0, y = 0 };

        return new AI_Result { sucess = false };
    }

    public AI_Result EmptySide()
    {
        if (GameManager.Get(0, 1).character == ' ') return new AI_Result { sucess = true, x = 0, y = 1 };
        if (GameManager.Get(1, 0).character == ' ') return new AI_Result { sucess = true, x = 1, y = 0 };

        if (GameManager.Get(2, 1).character == ' ') return new AI_Result { sucess = true, x = 1, y = 1 };
        if (GameManager.Get(1, 2).character == ' ') return new AI_Result { sucess = true, x = 2, y = 2 };

        return new AI_Result { sucess = false };
    }
}

public struct AI_Result
{
    public bool sucess;
    public int x;
    public int y;

    public override string ToString()
    {
        return $"AI result -> {sucess} x: {x} y:{y} ";
    }

    public static implicit operator bool(AI_Result foo)
    {
        return foo.sucess;
    }
}