using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int eliminatedTowerPartsNumber = 0;
    public int TowerPartsNumberToWin = 0;
    //public int completionPercentage = 95;
    public int TowerPartsNumberThatCanBeLeft = 10;
    public int ballsNumberleft = 0;
    public float constructionTime = 5f;
    public float physicalActivationTime = 1f;
    public float failConfirmationTime = 5f;
    public int level = 1;
    bool gameOver = false;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Level"))
        {
            level = PlayerPrefs.GetInt("Level");
        }

        Application.targetFrameRate = 60;
        ballsNumberleft = 10 + Mathf.RoundToInt(level/2f);
        MyEventSystem.instance.Set("BallsNumberleft", ballsNumberleft);
        MyEventSystem.instance.Set("Level",level);
        MyEventSystem.instance.RegisterToEvent("ScoreUpdate",this, ScoreUpdate);
        MyEventSystem.instance.RegisterToEvent("ReduceBallNumber", this, ReduceBallsNumberLeft);
        MyEventSystem.instance.RegisterToEvent("ProgressiveDestruction", this, ProgressiveDestruction);
    }

    #region event reactions

    void ScoreUpdate(string name, GenericDictionary args = null)
    {
        eliminatedTowerPartsNumber++;
        MyEventSystem.instance.Set("CompletionPercentage", Mathf.Floor((float)eliminatedTowerPartsNumber/(float)TowerPartsNumberToWin * 100f));
        if (eliminatedTowerPartsNumber >= TowerPartsNumberToWin && !gameOver)
        {
            gameOver = true;
            StopCoroutine("waitToConfirmFail");
            MyEventSystem.instance.FireEvent("Win");
        }
    }

    void ReduceBallsNumberLeft(string name, GenericDictionary args = null)
    {
        ballsNumberleft--;
        MyEventSystem.instance.Set("BallsNumberleft", ballsNumberleft);
        //UPDATE UI
        if (ballsNumberleft < 0 && !gameOver)
        {
            //wait to validate the loss
            StartCoroutine("waitToConfirmFail");
        }
    }

    void ProgressiveDestruction(string name, GenericDictionary args = null)
    {
        StartCoroutine(ProgressiveDestruction(args.Get("GroupToDestroy")));
    }


    #endregion

    private void Start()
    {
        MyEventSystem.instance.FireEvent("LevelConstruction");
        //TOWER CONSTRUCTION
        //UI SPAWNING
        //TOWER COLORATION
        //SCORE TO REACH

    }

    public void Initialization()
    {
        MyEventSystem.instance.FireEvent("GameStart", new GenericDictionary().Set("ConstructionTime", constructionTime).Set("PhysicalActivationTime",physicalActivationTime));
        //START UI DISACTIVATION
        //GAME UI ACTIVATION
        //CAMERA ANIMATION
        //LOWER BLOCKS in black mode 
        TowerPartsNumberToWin = MyEventSystem.instance.Get("InitialTowerPartsNumber") - TowerPartsNumberThatCanBeLeft;
    }

    IEnumerator waitToConfirmFail()
    {
        //ADD FEEDBACK
        MyEventSystem.instance.FireEvent("FailConfirmation", new GenericDictionary().Set("failConfirmationTime", failConfirmationTime));
        yield return new WaitForSeconds(failConfirmationTime);
        MyEventSystem.instance.FireEvent("Lose");
    }
    
    public void ReloadLevel()
    {
        //reload scene
        SceneManager.LoadScene(0);
    }

    public void LoadNextLevel()
    {
        //set player prefs value in order to create the tower procedurally
        level++;
        PlayerPrefs.SetInt("Level",level);
        //reload scene
        SceneManager.LoadScene(0);
    }

    IEnumerator ProgressiveDestruction(HashSet<TowerPart> groupToDestroy)
    {
        foreach (TowerPart towerPart in groupToDestroy)
        {
            if (towerPart != null)
            {
                towerPart.SolitaryDestruction();
                yield return new WaitForSeconds(0.03f);
            }
        }
    }
}
