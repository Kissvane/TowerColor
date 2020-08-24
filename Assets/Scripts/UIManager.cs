using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public Text level;

    public GameObject startMenu;

    public GameObject gameMenu;
    public Slider progression;
    public Text textProgression;
    public Text ballsNumberLeft;
    public Image failConfirmationImage;

    public GameObject gameOverMenu;
    public Text endGameFeedback;
    public GameObject nextLevelButton;
    public GameObject retryButton;
    //edngamefeedback tween settings
    public float scaleInitialSize = 0.01f;
    public float scaleDuration = 1f;

    public Transform ballSpawn;

    public void Awake()
    {
        MyEventSystem.instance.RegisterToEvent("LevelConstruction", this, LevelConstruction);
        MyEventSystem.instance.RegisterToEvent("GameStart", this, GameStart);
        MyEventSystem.instance.RegisterToEvent("TowerActivated", this, TowerActivated);
        MyEventSystem.instance.RegisterToEvent("Win", this, Win);
        MyEventSystem.instance.RegisterToEvent("Lose", this, Lose);
        MyEventSystem.instance.RegisterToEvent("FailConfirmation", this, FailConfirmation);
        MyEventSystem.instance.Register("CompletionPercentage", this, UpdateProgressionUI);
        MyEventSystem.instance.Register("BallsNumberleft", this, UpdateBallsNumberLeftUI);
    }

    private void Start()
    {
        level.text = string.Concat("Level "+MyEventSystem.instance.Get("Level"));
    }

    #region eventReactions
    void LevelConstruction(string name, GenericDictionary args)
    {
        ActivateStartMenu();
    }

    void GameStart(string name, GenericDictionary args)
    {
        DisactivateStartMenu();
        ActivateGameMenu();
    }

    void TowerActivated(string name, GenericDictionary args)
    {
        ballsNumberLeft.gameObject.SetActive(true);
    }

    void Win(string name, GenericDictionary args)
    {
        DisactivateGameMenu();
        ActivateGameOverMenu(true);
    }

    void Lose(string name, GenericDictionary args)
    {
        DisactivateGameMenu();
        ActivateGameOverMenu(false);
    }

    void UpdateProgressionUI(dynamic completion)
    {
        progression.value = completion;
        textProgression.text = string.Concat(((int)Mathf.Min(completion, 100f)).ToString(), " %");
    }

    void UpdateBallsNumberLeftUI(dynamic number)
    {
        ballsNumberLeft.text = ((int)number).ToString();
    }

    void FailConfirmation(string name, GenericDictionary args)
    {
        float failConfirmationTime = args.Get("failConfirmationTime");
        failConfirmationImage.transform.parent.gameObject.SetActive(true);
        failConfirmationImage.DOFillAmount(1f, failConfirmationTime);
    }
    #endregion

    void ActivateStartMenu()
    {
        startMenu.SetActive(true);
    }

    void DisactivateStartMenu()
    {
        startMenu.SetActive(false);
    }

    void ActivateGameMenu()
    {
        gameMenu.SetActive(true);
    }

    void DisactivateGameMenu()
    {
        gameMenu.SetActive(false);
    }


    IEnumerator ScoreUpdateCo()
    {
        yield return null;
    }

    void ActivateGameOverMenu(bool win)
    {
        endGameFeedback.transform.localScale = Vector3.one * 0.01f;
        gameOverMenu.SetActive(true);
        if (win)
        {
            endGameFeedback.text = "Tu as réussi";
            nextLevelButton.SetActive(true);
        }
        else
        {
            endGameFeedback.text = "Réessaye";
            retryButton.SetActive(true);
        }
        endGameFeedback.rectTransform.DOScale(1f, scaleDuration).SetEase(Ease.OutBounce);
    }

    void DisactivateGameOverMenu()
    {
        gameOverMenu.SetActive(false);
    }
  
}
