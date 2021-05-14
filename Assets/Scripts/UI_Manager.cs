using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public GameObject mainMenu;
    public Text whoseTurn;

    private void Start()
    {
        GameManager.onNextTurn += UpdateUI;
        GameManager.onStartGame += StartGame;

        mainMenu.SetActive(true);
    }

    private void OnDestroy()
    {
        GameManager.onNextTurn -= UpdateUI;
        GameManager.onStartGame -= StartGame;
    }

    private void StartGame()
    {
        mainMenu.SetActive(false);
    }

    private void UpdateUI()
    {
        whoseTurn.text = GameManager.instance.WhoseTime().icon.GetChar() + " turn!";
    }
}