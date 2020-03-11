// #define DEV_TEST_MODE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayMgr : MonoBehaviour
{
    public static GamePlayMgr Instance;

    public GameObject mainPanelObj;
    public GameObject shopPanelObj;
    public GameObject slidePanelObj;

    public GameObject bgObj;
    public GameObject coinObj;
    public GameObject complete20Obj;
    public GameObject scoreObj;
    public GameObject bestScoreObj;
    public GameObject loadingProgressBarObj;
    public GameObject starHelpObj;

    public GameObject congratulationWinObj;
    public GameObject continueWinObj;
    public GameObject doubleCoinWinObj;
    public GameObject tapContinuePanelObj;
    public GameObject quitGameWinObj;
    public GameObject quitGameCoinObj;
    public GameObject quitGameComplete20Obj;
    public GameObject gameResultSlideObj;
    public GameObject gameResultWinObj;
    public GameObject gameResultNewBestObj;
    public GameObject gameResultNewBestScoreObj;
    public GameObject gameResultCoolObj;
    public GameObject gameResultCoolBestScoreObj;
    public GameObject gameResultCoolScoreObj;
    public GameObject gameResultCoinObj;
    public GameObject gameResultGameCoinObj;
    public GameObject gameResultTotalCoinObj;
    public GameObject gameResultComplete20Obj;
    public GameObject gameResultStartLevelObj;
    public GameObject gameResultDoubleVideoBtnObj;
    public GameObject gameResultPlayBtnObj;
    public GameObject gameResultFireworksObj;
    public GameObject starHelpTutorialObj;
    public GameObject tutorialGuideObj;

    public GameObject pauseWinObj;
    public GameObject pauseMusicBtnObj;
    public GameObject pauseSoundBtnObj;

    public GameObject tableBgObj;
    public GameObject tableObj;
    public GameObject numberUpgradeEffObj;
    public GameObject comboEffObj;
    public GameObject starHelpEffObj;
    public GameObject starHelpParticleObj;
    public GameObject tableEffObj;

    GameObject sprayFlowerObj;
    public bool bTutorial;
    bool bFirst15, bFirst20;
    bool bFreeContinueGame, bPaidContinueGame;
    bool isGameOver;

    public int nGameCoin; // Obtained Coin Count while game playing
    int nGameComplete20; // Made 20 Count while game playing
    int nGameScore; // Game Score while game playing
    float nGameSeconds; // Timeframe for new one row generate
    float nGameProgressSeconds; // timeframe to show on progress bar
    public int nGameMaxNumber; // Max number of all numbers while game playing
    int nGameStarHelp; // Star Help Count while game playing
    int nGameTotalRowCount;
    bool isPlaying; // check if game is playing
    public bool isNumberMoving; // check if user is holding a number
    byte nComboCount = 0; // Combo Count while game playing

    int[,] nTable = new int[8, 6];
    bool bGenerateStar = false; // Star Help is generated for 2 rows
    int nStarHelpRow, nStarHelpColumn;
    public bool bAnimationPlaying;
    public bool bStarHelpBtnClicked;

    List<GameObject> aryNumbersGroup = new List<GameObject>();
    List<GameObject> aryObj;
    List<Vector3> aryNumbersStartPos = new List<Vector3>();

    bool bBlockTouch;
    public byte nRewardVideoStatus = 0;  // 1: Double Coin, 2: Continue Game
    public byte nTutorialStep = 0;

    public const int nNumberSize = 130;
    public const int nNumberSpacing = 145;
    public const int nNumberStartPosX = -362;
    public const int nNumberStartPosY = -507;
    const byte ARROW_RIGHT = 0;
    const byte ARROW_UP = 1;
    const byte ARROW_LEFT = 2;
    const byte ARROW_DOWN = 3;
    string sChain;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnApplicationQuit()
    {
        if (bTutorial)
            PlayerPrefs.SetInt("GamePlayStatus", 1);
        else if (isGamePlaying())
        {
            string sTable = string.Empty;
            sChain = string.Empty;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (nTable[i, j] != -1)
                    {
                        sTable += nTable[i, j].ToString("D2");
                        for (int k = 0; k < 4; k++)
                            sChain += GetNumberObj(i, j).GetComponent<NumberObjMgr>().bChain[k] ? "1" : "0";
                    }
                    else
                    {
                        sTable += "20";
                        sChain += "0000";
                    }
                }
            }
            PlayerPrefs.SetString("sTable", sTable);
            PlayerPrefs.SetString("sChain", sChain);
            PlayerPrefs.SetInt("GameComplete20", nGameComplete20);
            PlayerPrefs.SetInt("GameCoin", nGameCoin);
            PlayerPrefs.SetInt("GameScore", nGameScore);
            PlayerPrefs.SetInt("GamePlayStatus", 2);
        }
        else
        {
            PlayerPrefs.SetInt("GamePlayStatus", 0);
        }
    }

    bool CheckGameStatus()
    {
        return isGamePlaying() && !bAnimationPlaying && !isAllNumberObjsMovingDown() && !bStarHelpBtnClicked && !bTutorial;
    }

    // Update is called once per frame
    void Update()
    {
        // Show time progress bar
        if (CheckGameStatus())
        {
            nGameProgressSeconds -= Time.deltaTime;
            if (nGameProgressSeconds <= 0)
            {
                nGameProgressSeconds = 0;
                if (!isNumberMoving)
                {
                    nGameProgressSeconds = nGameSeconds;

                    isGameOver = MoveUp_Table();
                    if (isGameOver)
                    {
                        SetGamePlayStatus(false);
                        if (bFreeContinueGame)
                        {
                            bFreeContinueGame = false;
                            OpenContinueWin(false, nGameMaxNumber);
                        }
                        else if (bPaidContinueGame)
                        {
                            bPaidContinueGame = false;
                            OpenContinueWin(true, nGameMaxNumber);
                        }
                        else
                            StartCoroutine(OpenGameResultWindow());
                        return;
                    }
                    GenerateRow();
                }
            }
            loadingProgressBarObj.GetComponent<UITexture>().fillAmount = nGameProgressSeconds / nGameSeconds;
        }
    }

    private void OnEnable()
    {
        if (MainPanelMgr.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }
        comboEffObj.SetActive(false);

        bTutorial = MainPanelMgr.Instance.isTutorial;
        if (MainPanelMgr.Instance.nGamePlayStatus == 2)
        {
            string sTable = PlayerPrefs.GetString("sTable");
            sChain = PlayerPrefs.GetString("sChain");
            for (int i = 0; i < sTable.Length / 2; i++)
            {
                int n = int.Parse(sTable.Substring(i * 2, 2));
                if (n == 20)
                    nTable[i / 6, i % 6] = -1;
                else
                    nTable[i / 6, i % 6] = n;
            }
            nGameComplete20 = PlayerPrefs.GetInt("GameComplete20", 0);
            nGameCoin = PlayerPrefs.GetInt("GameCoin", 0);
            nGameScore = PlayerPrefs.GetInt("GameScore", 0);
            InitTopBar();
            ResumeTable();
        }
        else
        {
            if (bTutorial)
                StartCoroutine(StartTutorial());
            else
                StartCoroutine(StartGame());
        }
    }

    IEnumerator SwitchToOtherPanel(GameObject targetPanel)
    {
        while (bBlockTouch)
            yield return new WaitForEndOfFrame();
        slidePanelObj.SetActive(true);
        slidePanelObj.GetComponent<Animator>().Play("OpenSlide");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        targetPanel.SetActive(true);
        gameObject.GetComponent<UIPanel>().alpha = 0;
        slidePanelObj.GetComponent<Animator>().Play("CloseSlide");

        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        gameObject.GetComponent<UIPanel>().alpha = 1;
        gameObject.SetActive(false);
        slidePanelObj.SetActive(false);
    }

    private void OnDisable()
    {
        DestroyTableBg();
        ClearTable();
        // ResetGame();
    }

    void DestroyTableBg()
    {
        int nChildCnt = tableBgObj.transform.childCount;
        for (int i = nChildCnt - 1; i >= 0; i--)
            Destroy(tableBgObj.transform.GetChild(i).gameObject);
    }

    void SetTableBg()
    {
        loadingProgressBarObj.transform.parent.gameObject.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/GamePlay/LoadingBarBg" + MainPanelMgr.Instance.nStartLevel);
        bgObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/GamePlay/PlayPanelBg" + MainPanelMgr.Instance.nStartLevel);

        DestroyTableBg();

        GameObject numberBgPrefabObj = Resources.Load("Prefabs/NumberBg") as GameObject;
        numberBgPrefabObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/GamePlay/BlockBg" + MainPanelMgr.Instance.nStartLevel);
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject tmpNumberBgObj = NGUITools.AddChild(tableBgObj, numberBgPrefabObj);
                tmpNumberBgObj.transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * i, nNumberStartPosY + nNumberSpacing * j, 0);
                tmpNumberBgObj.name = (i + 1) + "_" + (j + 1);
                tmpNumberBgObj.SetActive(true);
            }
        }
        starHelpObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/GamePlay/StarHelp" + MainPanelMgr.Instance.nStartLevel);
    }

    IEnumerator OpenWindowPanel(GameObject winObj)
    {
        while (bBlockTouch)
            yield return new WaitForEndOfFrame();
        bBlockTouch = true;
        winObj.SetActive(true);
        winObj.GetComponent<Animator>().Play("OpenWinAnim");
        yield return new WaitForSeconds(0.167f);
        yield return new WaitForEndOfFrame();

        bBlockTouch = false;
    }

    IEnumerator ExitWindow(GameObject winObj)
    {
        bBlockTouch = true;
        winObj.GetComponent<Animator>().Play("CloseWinAnim");
        yield return new WaitForSeconds(0.17f);

        bBlockTouch = false;
        winObj.SetActive(false);
        winObj.transform.localScale = Vector3.one;
    }

    void ShowInterstitialAds(int nAdTiming)
    {
        if (AppManager.Instance != null && AppManager.Instance.bShowAds)
            AdManager.Instance.ShowInterstitial(nAdTiming);
    }

    void ShowRewardedAds(byte nStatus)
    {
        nRewardVideoStatus = nStatus;
        AdManager.Instance.ShowRewarded(nStatus, (nGameCoin + nGameComplete20));
    }

    public void onPauseBtnClicked()
    {
        if (bBlockTouch)
            return;
        ShowInterstitialAds(1);

        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        SetGamePlayStatus(false);
        // pauseWinObj.SetActive(true);
        StartCoroutine(OpenWindowPanel(pauseWinObj));
        PauseInitUI();
    }

    public void onPauseCloseBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        SetGamePlayStatus(true);
        StartCoroutine(ExitWindow(pauseWinObj));
        // pauseWinObj.SetActive(false);
    }

    public void onPauseMainBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        // pauseWinObj.SetActive(false);
        StartCoroutine(ExitWindow(pauseWinObj));
        StartCoroutine(OpenWindowPanel(quitGameWinObj));
        // quitGameWinObj.SetActive(true);
        QuitWindowInitUI(false);
    }

    public void onPauseRestartBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        // pauseWinObj.SetActive(false);
        StartCoroutine(ExitWindow(pauseWinObj));
        StartCoroutine(OpenWindowPanel(quitGameWinObj));
        QuitWindowInitUI(true);
    }

    public void onPauseMusicBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        AppManager.Instance.bMusic = !AppManager.Instance.bMusic;
        SoundManager.Instance.SetBgmEnable(AppManager.Instance.bMusic);
        PlayerPrefs.SetInt("Bgm", AppManager.Instance.bMusic ? 1 : 0);
        PauseInitUI();
    }

    public void onPauseSoundBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        AppManager.Instance.bSound = !AppManager.Instance.bSound;
        PlayerPrefs.SetInt("Sound", AppManager.Instance.bSound ? 1 : 0);
        PauseInitUI();
    }

    void PauseInitUI()
    {
        string str;
        str = AppManager.Instance.bMusic ? "on" : "off";
        pauseMusicBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/Setting/Music_" + str);
        str = AppManager.Instance.bSound ? "on" : "off";
        pauseSoundBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/Setting/Sound_" + str);
    }

    public void onTapContinuePanelClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        tapContinuePanelObj.SetActive(false);
        if (isGameOver)
            SetGamePlayStatus(true);
    }

    void QuitWindowInitUI(bool bRestart)
    {
        if (bRestart)
            quitGameWinObj.transform.GetChild(6).gameObject.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = "Restart";
        else
            quitGameWinObj.transform.GetChild(6).gameObject.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = "Quit";
        quitGameCoinObj.GetComponent<UILabel>().text = nGameCoin.ToString();
        quitGameComplete20Obj.GetComponent<UILabel>().text = nGameComplete20.ToString();
    }

    public void onQuitGame_QuitRestartBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);

        bool bRestart = quitGameWinObj.transform.GetChild(6).gameObject.transform.GetChild(0).gameObject.GetComponent<UILabel>().text.Equals("Restart");
        if (bRestart)
        {
            StartCoroutine(ExitWindow(quitGameWinObj));
            if (bTutorial)
            {
                AppManager.Instance.Record_GameTutorial_AppEvent("Tutorial_restarted");
                StartCoroutine(StartTutorial());
            }
            else
            {
                AppManager.Instance.Record_GameRestart_AppEvent("Game_Restarted", MainPanelMgr.Instance.nStartLevel + 1, MainPanelMgr.Instance.nCoin);
                StartCoroutine(StartGame());
            }
        }
        else
        {
            StartCoroutine(ExitWindow(quitGameWinObj));
            // quitGameWinObj.SetActive(false);
            StartCoroutine(SwitchToOtherPanel(mainPanelObj));
        }
    }

    public void onQuitGamePlayOnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        SetGamePlayStatus(true);
        StartCoroutine(ExitWindow(quitGameWinObj));
        // quitGameWinObj.SetActive(false);
    }

    void OpenCongratulationWin(int number)
    {
        SetGamePlayStatus(false);
        congratulationWinObj.transform.GetChild(2).gameObject.GetComponent<UITexture>().mainTexture = GetNumberTexture(number);
        congratulationWinObj.SetActive(true);
        AppManager.Instance.Record_ReachedBigNumber_AppEvent("Reached", number);
        StartCoroutine(PlaySprayFlowers());
    }

    IEnumerator PlaySprayFlowers()
    {
        yield return new WaitForSeconds(0.5f);
        GameObject sprayFlowerPrefabObj = Resources.Load("Prefabs/SprayFlowers") as GameObject;
        sprayFlowerObj = NGUITools.AddChild(null, sprayFlowerPrefabObj);
        yield return new WaitForSeconds(4.8f);
        if (sprayFlowerObj != null)
            Destroy(sprayFlowerObj);
    }

    public void onCongratulationPlayBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        ShowInterstitialAds(2);

        congratulationWinObj.SetActive(false);
        SetGamePlayStatus(true);
        if (sprayFlowerObj != null)
            Destroy(sprayFlowerObj);
        nGameProgressSeconds = nGameSeconds;
    }

    void OpenContinueWin(bool bPaid, int number)
    {
        SetGamePlayStatus(false);
        continueWinObj.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = MainPanelMgr.Instance.nCoin.ToString(); // Coin
        continueWinObj.transform.GetChild(2).gameObject.GetComponent<UILabel>().text = nGameScore.ToString(); // Game Score
        continueWinObj.transform.GetChild(4).gameObject.GetComponent<UITexture>().mainTexture = GetNumberTexture(number); // Number Texture
        if (bPaid)
        {
            continueWinObj.transform.GetChild(7).gameObject.SetActive(false);
            continueWinObj.transform.GetChild(8).gameObject.SetActive(true);
            continueWinObj.transform.GetChild(9).gameObject.SetActive(true);
        }
        else
        {
            continueWinObj.transform.GetChild(7).gameObject.SetActive(true);
            continueWinObj.transform.GetChild(8).gameObject.SetActive(false);
            continueWinObj.transform.GetChild(9).gameObject.SetActive(false);
        }
        continueWinObj.SetActive(true);
        StartCoroutine(ShowNoThanksBtnAfterDelay());
        StartCoroutine(StartContinueLoadingProgress());
    }

    IEnumerator ShowNoThanksBtnAfterDelay()
    {
        continueWinObj.transform.GetChild(6).gameObject.SetActive(false);
        yield return new WaitForSeconds(3.0f);
        continueWinObj.transform.GetChild(6).gameObject.SetActive(true);
    }

    bool isOtherWindowPanelOpened()
    {
        return tapContinuePanelObj.activeSelf || shopPanelObj.activeSelf;
    }

    IEnumerator StartContinueLoadingProgress()
    {
        bool bForceClosed = false;
        float loadingTime = 10.0f;
        continueWinObj.transform.GetChild(5).gameObject.GetComponent<UITexture>().fillAmount = 1;
        while (loadingTime > 0)
        {
            while (isOtherWindowPanelOpened())
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            loadingTime -= Time.deltaTime;
            continueWinObj.transform.GetChild(5).gameObject.GetComponent<UITexture>().fillAmount = (float)loadingTime / 9;
            if (!continueWinObj.activeSelf)
            {
                bForceClosed = true;
                break;
            }
        }
        if (!bForceClosed)
        {
            continueWinObj.SetActive(false);
            StartCoroutine(OpenGameResultWindow());
        }
    }

    public void onContinueWinFreePlayBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(ExitWindow(continueWinObj));
        // continueWinObj.SetActive(false);
        AppManager.Instance.Record_ResumeGame_AppEvent("Continue_Game", "Free");
        ContinueGameWithRemove4Rows();
    }

    public void onContinueWinPaidPlayBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        if (MainPanelMgr.Instance.nCoin < 180)
        {
            StartCoroutine(OpenWindowPanel(shopPanelObj));
            // shopPanelObj.SetActive(true);
        }
        else
        {
            MainPanelMgr.Instance.nCoin -= 180;
            StartCoroutine(ExitWindow(continueWinObj));
            // continueWinObj.SetActive(false);
            AppManager.Instance.Record_ResumeGame_AppEvent("Continue_Game", "Paied 180");
            ContinueGameWithRemove4Rows();
        }
    }

    public void onContinueWinAdsPlayBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);

        StartCoroutine(ExitWindow(continueWinObj));
#if UNITY_EDITOR
        ContinueGameWithRemove4Rows();
#else
        ShowRewardedAds(2); // To continue play game
#endif
    }

    public void onContinueWinNoThanksBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(ExitWindow(continueWinObj));
        // continueWinObj.SetActive(false);
        StartCoroutine(OpenGameResultWindow());
    }

    public void onContinueWinAddCoinBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(OpenWindowPanel(shopPanelObj));
        // shopPanelObj.SetActive(true);
    }

    public void ContinueGameWithRemove4Rows()
    {
        StartCoroutine(_ContinueGameWithRemove4Rows());
    }

    IEnumerator _ContinueGameWithRemove4Rows()
    {
        nGameProgressSeconds = nGameSeconds;
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject numberObj = GetNumberObj(j, i);
                if (numberObj != null)
                {
                    if (j < 4)
                    {
                        Destroy(numberObj);
                        nTable[i, j] = -1;
                    }
                    else
                    {
                        StartCoroutine(MoveNumberDown_N_Rows(numberObj, 4));
                    }
                }
            }
        }
        while (isAllNumberObjsMovingDown())
            yield return new WaitForEndOfFrame();

        for (int i = 0; i < tableObj.transform.childCount; i++)
        {
            tableObj.transform.GetChild(i).gameObject.transform.GetComponent<NumberObjMgr>().RemoveRow(4);
            tableObj.transform.GetChild(i).gameObject.transform.GetComponent<NumberObjMgr>().SetPosition();
        }
        Update_nTable();
        SetGamePlayStatus(true);
    }

    IEnumerator MoveNumberDown_N_Rows(GameObject obj, int nRowCount)
    {
        SetNumberObjMovingDown(obj, true);

        Vector3 pos = obj.transform.localPosition;
        float targetPosY = obj.transform.localPosition.y - nNumberSpacing * nRowCount;
        bool bMoveDown = true;
        while (bMoveDown)
        {
            pos = new Vector3(pos.x, pos.y - 50, pos.z);
            if (pos.y < targetPosY)
            {
                bMoveDown = false;
                pos = new Vector3(pos.x, targetPosY, pos.z);
            }
            obj.transform.localPosition = pos;
            yield return new WaitForEndOfFrame();
        }
            
        SetNumberObjMovingDown(obj, false);
    }

    IEnumerator OpenGameResultWindow()
    {
        gameResultSlideObj.SetActive(true);
        float fAlpha = 0;
        while (fAlpha <= 0.5f)
        {
            fAlpha += Time.deltaTime / 3;
            gameResultSlideObj.GetComponent<UIPanel>().alpha = fAlpha;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(1.0f);
        gameResultSlideObj.SetActive(false);


        gameResultWinObj.SetActive(true);
        if (nGameScore > MainPanelMgr.Instance.nBestScore)
        {
            gameResultNewBestObj.SetActive(true);
            gameResultCoolObj.SetActive(false);
            gameResultNewBestScoreObj.GetComponent<UILabel>().text = nGameScore.ToString("N0");
            MainPanelMgr.Instance.nBestScore = nGameScore;
            PlayerPrefs.SetInt("BestScore", nGameScore);
            gameResultFireworksObj.SetActive(true);
        }
        else
        {
            gameResultNewBestObj.SetActive(false);
            gameResultCoolObj.SetActive(true);
            gameResultCoolBestScoreObj.GetComponent<UILabel>().text = MainPanelMgr.Instance.nBestScore.ToString("N0");
            gameResultCoolScoreObj.GetComponent<UILabel>().text = nGameScore.ToString("N0");
            gameResultFireworksObj.SetActive(false);
        }
        gameResultCoinObj.GetComponent<UILabel>().text = MainPanelMgr.Instance.nCoin.ToString();
        gameResultGameCoinObj.GetComponent<UILabel>().text = nGameCoin.ToString();
        gameResultComplete20Obj.GetComponent<UILabel>().text = nGameComplete20.ToString();
        gameResultTotalCoinObj.GetComponent<UILabel>().text = (nGameCoin + nGameComplete20).ToString();
        gameResultStartLevelObj.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(MainPanelMgr.Instance.nLevelUnlocked < 1);
        gameResultStartLevelObj.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(MainPanelMgr.Instance.nLevelUnlocked < 2);
        if (nGameCoin == 0)
            gameResultDoubleVideoBtnObj.SetActive(false);
        else
            gameResultDoubleVideoBtnObj.SetActive(true);
        GameResult_InitUI();
        AppManager.Instance.Record_GameOver_AppEvent("GameOver", MainPanelMgr.Instance.nStartLevel, nGameComplete20, nGameCoin, nGameScore, MainPanelMgr.Instance.nBestScore);

        yield return new WaitForSeconds(0.167f);
        yield return new WaitForEndOfFrame();
        gameResultWinObj.transform.GetChild(0).gameObject.SetActive(false);
        gameResultWinObj.transform.GetChild(0).gameObject.SetActive(true);
    }

    void GameResult_InitUI()
    {
        for (int i = 0; i < 3; i++)
            gameResultStartLevelObj.transform.GetChild(i).gameObject.transform.GetChild(1).gameObject.SetActive(i == MainPanelMgr.Instance.nStartLevel);
        gameResultPlayBtnObj.transform.GetChild(0).gameObject.SetActive(MainPanelMgr.Instance.nStartLevel > 0);
        gameResultPlayBtnObj.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = (-2 * MainPanelMgr.Instance.nStartLevel).ToString();
        if (MainPanelMgr.Instance.nStartLevel == 0)
            gameResultPlayBtnObj.transform.GetChild(1).gameObject.transform.localPosition = new Vector3(0, 2.7f, 0);
        else
            gameResultPlayBtnObj.transform.GetChild(1).gameObject.transform.localPosition = new Vector3(56.5f, 2.7f, 0);
    }

    public void onGameResultStartLevelBtnClicked(GameObject obj)
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        int nObj = int.Parse(obj.name);
        if (nObj > MainPanelMgr.Instance.nLevelUnlocked)
            return;
        MainPanelMgr.Instance.nStartLevel = nObj;
        GameResult_InitUI();
    }

    public void onGameResultPlayBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        if (MainPanelMgr.Instance.nStartLevel > MainPanelMgr.Instance.nLevelUnlocked)
            return;
        gameResultFireworksObj.SetActive(false);
        int nGameResultCoin = int.Parse(gameResultTotalCoinObj.GetComponent<UILabel>().text);
        MainPanelMgr.Instance.nCoin += nGameResultCoin;
        if (MainPanelMgr.Instance.nCoin < 2 * MainPanelMgr.Instance.nStartLevel)
        {
            Debug.Log("Insufficient Coins");
            shopPanelObj.SetActive(true);
        }
        else
        {
            MainPanelMgr.Instance.nCoin -= 2 * MainPanelMgr.Instance.nStartLevel;
            PlayerPrefs.SetInt("Coin", MainPanelMgr.Instance.nCoin);
            AppManager.Instance.Record_GetCoin_AppEvent("GetCoin", MainPanelMgr.Instance.nCoin, (MainPanelMgr.Instance.nCoin - nGameResultCoin));
            StartCoroutine(PlayGameResultCoinEffect(nGameResultCoin));
        }
    }

    IEnumerator PlayGameResultCoinEffect(int nCount)
    {
        for (int i = 0; i < nCount; i++)
        {
            StartCoroutine(MakeGameResultCoinEffect());
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.7f);
        StartCoroutine(StartGame());
        StartCoroutine(ExitWindow(gameResultWinObj));
        // gameResultWinObj.SetActive(false);
    }

    IEnumerator MakeGameResultCoinEffect()
    {
        GameObject gameResultCoinEffectPrefabObj = Resources.Load("Prefabs/GameResultCoinEffect") as GameObject;
        GameObject obj = NGUITools.AddChild(gameResultWinObj.transform.GetChild(9).gameObject, gameResultCoinEffectPrefabObj);
        SoundManager.Instance.PlaySE(SoundManager.SE_COIN);
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForEndOfFrame();
        Destroy(obj);
    }

    public void onGameResultDoubleVideoBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
#if UNITY_EDITOR
        OpenDoubleCoinWin();
#else
        OpenDoubleCoinWin();
        ShowRewardedAds(1); // to double coin
#endif
    }

    public void OpenDoubleCoinWin()
    {
        if (bBlockTouch)
            return;
        StartCoroutine(OpenWindowPanel(doubleCoinWinObj));
        // doubleCoinWinObj.SetActive(true);
        doubleCoinWinObj.transform.GetChild(3).gameObject.GetComponent<UILabel>().text = "+" + (nGameCoin + nGameComplete20);
        gameResultTotalCoinObj.GetComponent<UILabel>().text = ((nGameCoin + nGameComplete20) * 2).ToString();
        gameResultDoubleVideoBtnObj.SetActive(false);
    }

    public void onGameResultHomeBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        ShowInterstitialAds(3);

        gameResultFireworksObj.SetActive(false);
        MainPanelMgr.Instance.nCoin += int.Parse(gameResultTotalCoinObj.GetComponent<UILabel>().text);
        PlayerPrefs.SetInt("Coin", MainPanelMgr.Instance.nCoin);
        AppManager.Instance.Record_GetCoin_AppEvent("GetCoin", MainPanelMgr.Instance.nCoin, (MainPanelMgr.Instance.nCoin - int.Parse(gameResultTotalCoinObj.GetComponent<UILabel>().text)));
        gameResultWinObj.SetActive(false);
        mainPanelObj.SetActive(true);

        gameObject.SetActive(false);
    }

    public void onGameResultAddCoinBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(OpenWindowPanel(shopPanelObj));
        // shopPanelObj.SetActive(true);
    }

    public void onDoubleCoinReceivedBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(ExitWindow(doubleCoinWinObj));
    }

    public void onStarHelpClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        if (nGameStarHelp < 6)
            return;
        bStarHelpBtnClicked = true;
        int nChildCnt = tableObj.transform.childCount;
        for (int i = 0; i < nChildCnt; i++)
        {
            tableObj.transform.GetChild(i).gameObject.GetComponent<UIDragObject>().enabled = false;
        }
        starHelpTutorialObj.SetActive(true);
        nGameStarHelp -= 6;
        AppManager.Instance.Record_StarHelp_AppEvent("StarHelp", (nGameStarHelp / 6), nGameCoin, nGameComplete20);
        UpdateStarHelpProgress();
    }

    bool isGamePlaying()
    {
        return isPlaying;
    }

    void SetGamePlayStatus(bool bEnable)
    {
        isPlaying = bEnable;
    }

    void ResetGame()
    {
        nGameCoin = 0;
        nGameComplete20 = 0;
        nGameScore = 0;
        nGameStarHelp = 0;
        InitTopBar();
    }

    void InitTopBar()
    {
        coinObj.GetComponent<UILabel>().text = nGameCoin.ToString();
        complete20Obj.GetComponent<UILabel>().text = nGameComplete20.ToString();
        scoreObj.GetComponent<UILabel>().text = nGameScore.ToString("N0");
        UpdateStarHelpProgress();
        bestScoreObj.GetComponent<UILabel>().text = MainPanelMgr.Instance.nBestScore.ToString("N0");
        loadingProgressBarObj.GetComponent<UITexture>().fillAmount = 1;
    }

    IEnumerator StartTutorial()
    {
        MainPanelMgr.Instance.nStartLevel = 0;
        SetTableBg();
        ClearTable();
        ResetGame();
        nGameTotalRowCount = 0;
        bGenerateStar = true;
        bFirst15 = MainPanelMgr.Instance.nStartLevel < 2 ? true : false;
        bFirst20 = true;
        bFreeContinueGame = false; // UnityEngine.Random.Range(1, 100) % 2 == 0 ? true : false;
        bPaidContinueGame = true; // UnityEngine.Random.Range(1, 100) % 2 == 0 ? true : false;

        nGameSeconds = 15;
        nGameProgressSeconds = nGameSeconds;
        loadingProgressBarObj.transform.parent.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        tutorialGuideObj.SetActive(true);

        SetGamePlayStatus(true);
        Init_TutorialTable();

        nTutorialStep = 0;
        tutorialGuideObj.transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(GotoNextTutorialStep());
    }

    IEnumerator GotoNextTutorialStep()
    {
        GameObject guideObj = tutorialGuideObj.transform.GetChild(1).gameObject;

        SetAllNumberObjsClickable(nTutorialStep >= 6);

        GameObject dottedObj = tutorialGuideObj.transform.GetChild(2).gameObject;
        for (int i = 0; i < dottedObj.transform.childCount; i++)
            dottedObj.transform.GetChild(i).gameObject.SetActive(false);
        if (nTutorialStep < 6)
            dottedObj.transform.GetChild(nTutorialStep).gameObject.SetActive(true);

        if (nTutorialStep < 6)
        {
            guideObj.SetActive(true);
            guideObj.transform.GetChild(0).gameObject.GetComponent<Animator>().Play("TutorialStep" + (nTutorialStep + 1));
            if (nTutorialStep == 1)
                guideObj.transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "Drag off";
            else
                guideObj.transform.GetChild(1).gameObject.GetComponent<UILabel>().text = "Drag and Merge" + '\n' + "the same numbers";
        }
        else
            guideObj.SetActive(false);


        yield return new WaitForSeconds(0.5f);
        if (nTutorialStep == 0)
            SetNumberObjClickable(GetNumberObj(0, 0), true);
        else if (nTutorialStep == 1)
            SetNumberObjClickable(GetNumberObj(1, 5), true);
        else if (nTutorialStep == 2)
            SetNumberObjClickable(GetNumberObj(0, 5), true);
        else if (nTutorialStep == 3)
            SetNumberObjClickable(GetNumberObj(0, 2), true);
        else if (nTutorialStep == 4)
            SetNumberObjClickable(GetNumberObj(0, 1), true);
        else if (nTutorialStep == 5)
            SetNumberObjClickable(GetNumberObj(0, 3), true);
        else if (nTutorialStep == 6)
        {
            tutorialGuideObj.transform.GetChild(0).gameObject.SetActive(false);
            tutorialGuideObj.transform.GetChild(3).gameObject.SetActive(true);
            guideObj.SetActive(false);
            GenerateNewRow_ForTutorial();
        }
        else if (nTutorialStep == 7)
        {
            if (Check_NoMove())
                tutorialGuideObj.transform.GetChild(4).gameObject.SetActive(true);
        }

        if (nTutorialStep < 7)
            nTutorialStep++;
    }

    void SetNumberObjClickable(GameObject obj, bool bEnable)
    {
        obj.GetComponent<UIDragObject>().enabled = bEnable;
        obj.GetComponent<BoxCollider>().enabled = bEnable;
    }

    void SetAllNumberObjsClickable(bool bEnable)
    {
        for (int i = 0; i < tableObj.transform.childCount; i++)
            SetNumberObjClickable(tableObj.transform.GetChild(i).gameObject, bEnable);
    }

    public void onTutorialTapContinue()
    {
        tutorialGuideObj.transform.GetChild(3).gameObject.SetActive(false);
    }

    public void onTutorialCongratsClicked()
    {
        tutorialGuideObj.transform.GetChild(4).gameObject.SetActive(false);
        tutorialGuideObj.SetActive(false);

        loadingProgressBarObj.transform.parent.gameObject.SetActive(true);
        loadingProgressBarObj.transform.parent.gameObject.GetComponent<Animator>().enabled = true;
        loadingProgressBarObj.transform.parent.gameObject.GetComponent<Animator>().Play("LoadingProgressBarTutorialAnim");

        StartCoroutine(StartGameFromTutorial());
    }

    IEnumerator StartGameFromTutorial()
    {
        SetAllNumberObjsClickable(false);
        yield return new WaitForSeconds(1.5f);

        GameObject letsgoPrefabObj = Resources.Load("Prefabs/Lets_go") as GameObject;
        GameObject letsgoObj = NGUITools.AddChild(tableEffObj, letsgoPrefabObj);
        letsgoObj.transform.localPosition = new Vector3(0, 210, 0);
        yield return new WaitForSeconds(1.0f);
        Destroy(letsgoObj);

        bTutorial = false;
        tutorialGuideObj.SetActive(false);
        SetAllNumberObjsClickable(true);
    }

    public void GenerateNewRow_ForTutorial()
    {
        MoveUp_Table();

        nTable[0, 0] = 0;
        nTable[0, 1] = 0;
        nTable[0, 2] = 3;
        nTable[0, 3] = 4;
        nTable[0, 4] = 3;
        nTable[0, 5] = 5;

        GameObject numberPrefabObj = Resources.Load("Prefabs/Number") as GameObject;
        for (int j = 0; j < 6; j++)
        {
            if (nTable[0, j] != -1)
            {
                GameObject tempNumberObj = NGUITools.AddChild(tableObj, numberPrefabObj);
                int number = nTable[0, j];
                tempNumberObj.GetComponent<NumberObjMgr>().Generate_ForTutorial(number, 0, j);
            }
        }
    }

    public void HideTutorialGuide()
    {
        tutorialGuideObj.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void ShowTutorialGuide()
    {
        if (nTutorialStep < 7)
        {
            tutorialGuideObj.transform.GetChild(1).gameObject.SetActive(true);
            GameObject guideObj = tutorialGuideObj.transform.GetChild(1).gameObject;
            guideObj.transform.GetChild(0).gameObject.GetComponent<Animator>().Play("TutorialStep" + nTutorialStep);
        }
    }

    IEnumerator StartGame()
    {
        SetTableBg();
        ClearTable();
        ResetGame();
        nGameTotalRowCount = 0;
        bGenerateStar = true;
        bFirst15 = MainPanelMgr.Instance.nStartLevel < 2 ? true : false;
        bFirst20 = true;
        bFreeContinueGame = UnityEngine.Random.Range(1, 100) % 2 == 0 ? true : false;
        bPaidContinueGame = UnityEngine.Random.Range(1, 100) % 2 == 0 ? true : false;
        if (MainPanelMgr.Instance.nStartLevel == 0)
            nGameSeconds = 15;
        else if (MainPanelMgr.Instance.nStartLevel == 1)
            nGameSeconds = 13;
        else if (MainPanelMgr.Instance.nStartLevel == 2)
            nGameSeconds = 11;
        // nGameSeconds = 300;
        nGameProgressSeconds = nGameSeconds;
        loadingProgressBarObj.transform.parent.gameObject.SetActive(true);
        loadingProgressBarObj.transform.parent.gameObject.GetComponent<Animator>().enabled = false;

        yield return new WaitForSeconds(0.8f);
        GameObject letsgoPrefabObj = Resources.Load("Prefabs/Lets_go") as GameObject;
        GameObject letsgoObj = NGUITools.AddChild(tableEffObj, letsgoPrefabObj);
        letsgoObj.transform.localPosition = new Vector3(0, 210, 0);
        yield return new WaitForSeconds(1.0f);
        Destroy(letsgoObj);

        SetGamePlayStatus(true);
        InitTable();
    }

    void InitTable()
    {
        int nStartColumn = MainPanelMgr.Instance.nStartLevel + 2;
        nGameMaxNumber = (MainPanelMgr.Instance.nStartLevel + 1) * 5;
#if DEV_TEST_MODE
        GenerateTable_ForTest();
#else
        for (int j = 0; j < nStartColumn; j++)
        {
            MoveUp_Table();
            GenerateRow();
        }
#endif
    }

    void ResumeTable()
    {
        SetTableBg();
        GameObject numberPrefabObj = Resources.Load("Prefabs/Number") as GameObject;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (nTable[i, j] != -1)
                {
                    GameObject tempNumberObj = NGUITools.AddChild(tableObj, numberPrefabObj);
                    int number = nTable[i, j];
                    tempNumberObj.GetComponent<NumberObjMgr>().Generate_ForTutorial(number, i, j);
                }
            }
        }
        for (int i = 0; i < tableObj.transform.childCount; i++)
        {
            int r = GetRow(tableObj.transform.GetChild(i).gameObject);
            int c = GetColumn(tableObj.transform.GetChild(i).gameObject);

            string subChainStr = sChain.Substring((r * 6 + c) *  4, 4);
            for(int j = 0; j < 4; j++)
            {
                if (subChainStr[j] == '1')
                    tableObj.transform.GetChild(i).gameObject.GetComponent<NumberObjMgr>().AddArrowChain(j);
            }
        }
        UpdateGameSeconds();
        nGameProgressSeconds = nGameSeconds;
        loadingProgressBarObj.transform.parent.gameObject.SetActive(true);
        loadingProgressBarObj.transform.parent.gameObject.GetComponent<Animator>().enabled = false;
        SetGamePlayStatus(true);
    }

    void Init_TutorialTable()
    {
        nTable[0, 0] = 2;
        nTable[0, 1] = 5;
        nTable[0, 2] = 2;
        nTable[0, 4] = 6;
        nTable[0, 5] = 3;
        nTable[1, 5] = 4;

        GameObject numberPrefabObj = Resources.Load("Prefabs/Number") as GameObject;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (nTable[i, j] != -1)
                {
                    GameObject tempNumberObj = NGUITools.AddChild(tableObj, numberPrefabObj);
                    int number = nTable[i, j];
                    tempNumberObj.GetComponent<NumberObjMgr>().Generate_ForTutorial(number, i, j);
                }
            }
        }
    }

#if DEV_TEST_MODE
    void GenerateTable_ForTest()
    {
        /*
        nTable[4, 0] = 7;
        nTable[3, 0] = 6;
        nTable[2, 0] = 5;
        nTable[1, 0] = 6;
        nTable[0, 0] = 7;*/
        /*
        nTable[4, 0] = 19;
        nTable[3, 0] = 18;
        nTable[2, 0] = 17;
        nTable[1, 0] = 18;
        nTable[0, 0] = 19;*/
        /*
        nTable[0, 2] = 8;
        nTable[0, 4] = 9;
        nTable[0, 5] = 10;
        nTable[1, 5] = 5;
        nTable[2, 5] = 10;
        nTable[2, 4] = 8;
        nTable[3, 4] = 5;*/
        nTable[0, 3] = -1;
        nTable[0, 4] = 5;
        nTable[0, 5] = 12;
        nTable[1, 3] = 5;
        nTable[1, 4] = 12;
        nTable[1, 5] = -1;

        GameObject numberPrefabObj = Resources.Load("Prefabs/Number") as GameObject;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (nTable[i, j] != -1)
                {
                    GameObject tempNumberObj = NGUITools.AddChild(tableObj, numberPrefabObj);
                    int number = nTable[i, j];
                    tempNumberObj.GetComponent<NumberObjMgr>().Generate_ForTest(number, i, j);
                }
            }
        }
    }
#endif

    int GenerateNumber(int index)
    {
        int bottomNumber = 1;
        if (nGameMaxNumber == 5)
            bottomNumber = 1;
        else if (nGameMaxNumber == 6)
            bottomNumber = 2;
        else if (nGameMaxNumber > 13)
            bottomNumber = nGameMaxNumber - 10;
        else
            bottomNumber = 3;
        int number = UnityEngine.Random.Range(bottomNumber, nGameMaxNumber - 1);
        while (number == nTable[0, index])
        {
            number = UnityEngine.Random.Range(bottomNumber, nGameMaxNumber - 1);
        }
        return number;
    }

    int GetChainCount(int nMaxNumber)
    {
        if (nMaxNumber >= 14)
            return UnityEngine.Random.Range(0, 100) % 3 + 1;
        else if (nMaxNumber >= 12)
            return UnityEngine.Random.Range(0, 100) % 2 + 1;
        else if (nMaxNumber >= 10)
            return 1;
        else
            return 0;
    }

    void GenerateRow()
    {
        if (bGenerateStar)
        {
            nStarHelpRow = UnityEngine.Random.Range(0, 100) % 2;
            nStarHelpColumn = UnityEngine.Random.Range(0, 100) % 6;
            bGenerateStar = false;
        }
        nGameTotalRowCount++;

        GameObject numberPrefabObj = Resources.Load("Prefabs/Number") as GameObject;
        for (int i = 0; i < 6; i++)
        {
            GameObject tempNumberObj = NGUITools.AddChild(tableObj, numberPrefabObj);
            int number = GenerateNumber(i);
            if (nStarHelpRow == nGameTotalRowCount % 2 && nStarHelpColumn == i && 0 != nTable[0, nStarHelpColumn])
                number = 0;
            tempNumberObj.GetComponent<NumberObjMgr>().Generate(number, i);
            nTable[0, i] = number;
        }

        // Generate & Set Chain Index and Type
        int nChainCount = GetChainCount(nGameMaxNumber);
        int[] nChainIndex = new int[nChainCount];
        int[] nChainType = new int[nChainCount];
        for (int i = 0; i < nChainCount; i++)
        {
            nChainIndex[i] = UnityEngine.Random.Range(0, 100) % 5;
            nChainType[i] = nGameMaxNumber > 15 ? UnityEngine.Random.Range(0, 100) % 2 : 0; // If the max number is over 15, up chian is available to attach
            if (nChainType[i] == 1 && GetNumber(GetNumberObj(1, nChainIndex[i])) == -1)
                nChainType[i] = ARROW_RIGHT;
            if (nChainType[i] == ARROW_RIGHT) // Right chain
            {
                while (GetNumber(GetNumberObj(0, nChainIndex[i])) == GetNumber(GetNumberObj(0, (nChainIndex[i] + 1))))
                    nChainIndex[i] = UnityEngine.Random.Range(0, 100) % 5;
                GetNumberObj(0, nChainIndex[i]).GetComponent<NumberObjMgr>().AddArrowChain(ARROW_RIGHT);
                GetNumberObj(0, nChainIndex[i] + 1).GetComponent<NumberObjMgr>().AddArrowChain(ARROW_LEFT);
            }
            else if (nChainType[i] == ARROW_UP) // Up chain
            {
                while (GetNumber(GetNumberObj(0, nChainIndex[i])) == GetNumber(GetNumberObj(1, nChainIndex[i])) ||
                    GetNumber(GetNumberObj(1, nChainIndex[i])) == -1)
                {
                    nChainIndex[i] = UnityEngine.Random.Range(0, 100) % 5;
                }
                GetNumberObj(0, nChainIndex[i]).GetComponent<NumberObjMgr>().AddArrowChain(ARROW_UP);
                GetNumberObj(1, nChainIndex[i]).GetComponent<NumberObjMgr>().AddArrowChain(ARROW_DOWN);
            }
        }

        if (nGameTotalRowCount % 2 == 0)
            bGenerateStar = true;
    }

    int GetNumber(GameObject obj)
    {
        if (obj == null)
            return -1;
        return obj.GetComponent<NumberObjMgr>().GetNumber();
    }

    GameObject GetNumberObj(int row, int col)
    {
        return GetChildWithName(tableObj, (col + 1) + "_" + (row + 1));
    }

    bool MoveUp_Table()
    {
        int i, j;
        for (i = 0; i < 6; i++)
        {
            if (nTable[7, i] != -1)
                return true; // Game is Over
        }
        for (i = 6; i >= 0; i--)
        {
            for (j = 0; j < 6; j++)
                nTable[i + 1, j] = nTable[i, j];
        }

        int nTableChildCnt = tableObj.transform.childCount;
        for (i = 0; i < nTableChildCnt; i++)
        {
            tableObj.transform.GetChild(i).gameObject.GetComponent<NumberObjMgr>().AddRow();
        }
        return false;
    }

    GameObject GetChildWithName(GameObject obj, string name)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
            return childTrans.gameObject;
        else
            return null;
    }

    void ClearTable()
    {
        int i, j;
        int nChildCnt = tableObj.transform.childCount;
        for (i = nChildCnt - 1; i >= 0; i--)
            Destroy(tableObj.transform.GetChild(i).gameObject);
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 6; j++)
                nTable[i, j] = -1;
        }
    }

    bool Check_NoMove()
    {
        int i;
        int[] nTableArray = new int[20];
        int childCnt = tableObj.transform.childCount;
        for (i = 0; i < childCnt; i++)
        {
            GameObject obj = tableObj.transform.GetChild(i).gameObject;
            if (GetNumber(obj) < 20)
                nTableArray[GetNumber(obj)]++;
        }
        for (i = 0; i < 20; i++)
        {
            if (nTableArray[i] > 1)
                return false;
        }

        return true;
    }

    void GenerateRowIfNoMove()
    {
        if (Check_NoMove())
        {
            //Debug.Log("GenerateRowIfNoMove");
            //yield return new WaitForSeconds(0.3f);
            nGameProgressSeconds = 0;
        }
    }

    void UpdateGameSeconds()
    {
        nGameSeconds = 15 - (0.4f * nGameMaxNumber) - 0.25f * nGameComplete20;
    }

    public void AddGameScore(int score)
    {
        UpdateGameSeconds();
        //Debug.Log("nGameSeconds Update   " + nGameSeconds);

        if (bFirst15 && score == 14)
        {
            bFirst15 = false;
            OpenCongratulationWin(15);
        }
        else if (bFirst20 && score == 19)
        {
            bFirst20 = false;
            OpenCongratulationWin(20);
        }
        if (score == 19)
            StartCoroutine(AddComplete20());
        nGameScore += score * nComboCount;
        scoreObj.GetComponent<UILabel>().text = nGameScore.ToString("N0");

        if (bTutorial)
        {
            if (nTutorialStep == 1 && score == 2)
                StartCoroutine(GotoNextTutorialStep());
            else if (nTutorialStep == 3 && score == 3)
                StartCoroutine(GotoNextTutorialStep());
            else if (nTutorialStep == 4 && score == 4)
                StartCoroutine(GotoNextTutorialStep());
            else if (nTutorialStep == 5 && score == 5)
                StartCoroutine(GotoNextTutorialStep());
            else if (nTutorialStep == 6 && score == 6)
                StartCoroutine(GotoNextTutorialStep());
            else if (nTutorialStep == 7)
                StartCoroutine(GotoNextTutorialStep());
        }
    }

    IEnumerator AddComplete20()
    {
        yield return new WaitForSeconds(1.0f);
        nGameComplete20++;
        complete20Obj.GetComponent<UILabel>().text = nGameComplete20.ToString();
    }

    IEnumerator AddStarHelp()
    {
        nGameScore += 40;
        scoreObj.GetComponent<UILabel>().text = nGameScore.ToString("N0");

        yield return new WaitForSeconds(1.2f);
        nGameStarHelp++;
        if (fRaiseAmount <= 0)
        {
            fRaiseAmount += (float)1 / 6;
            StartCoroutine(PlayStarHelpRaiseAnim());
        }
        else
            fRaiseAmount += (float)1 / 6;
        //UpdateStarHelpProgress();
        isNumberMoving = false;

        if (bTutorial && nTutorialStep == 7)
            StartCoroutine(GotoNextTutorialStep());
    }

    public void AddGameCoin(Vector3 pos)
    {
        nGameCoin++;
        coinObj.GetComponent<UILabel>().text = nGameCoin.ToString();
        SoundManager.Instance.PlaySE(SoundManager.SE_COIN);
        StartCoroutine(PlayCoinEffect(pos));
    }

    IEnumerator PlayCoinEffect(Vector3 pos)
    {
        GameObject coinEffObj = Resources.Load("Prefabs/CoinEffect") as GameObject;
        GameObject tmpCoinEffObj = NGUITools.AddChild(tableEffObj, coinEffObj);
        tmpCoinEffObj.transform.localPosition = new Vector3(pos.x + 64, pos.y + 64, pos.z);
        yield return new WaitForSeconds(0.6f);
        Destroy(tmpCoinEffObj);
    }

    List<GameObject> GetGroupNumbers(int row, int col)
    {
        aryObj = new List<GameObject>();
        AddNumbersToGroup(row, col);
        return aryObj;
    }

    void AddNumbersToGroup(int row, int col)
    {
        GameObject obj = GetNumberObj(row, col);
        if (obj == null)
            return;
        if (aryObj.Contains(obj))
            return;
        aryObj.Add(obj);
        
        if (isHaveArrowChain(GetNumberObj(row, col), ARROW_RIGHT) && GetNumberObj(row, col + 1) != null)
            AddNumbersToGroup(row, col + 1);
        if (isHaveArrowChain(GetNumberObj(row, col), ARROW_UP) && GetNumberObj(row + 1, col) != null)
            AddNumbersToGroup(row + 1, col);
        if (GetNumberObj(row, col - 1) != null && isHaveArrowChain(GetNumberObj(row, col - 1), ARROW_RIGHT))
            AddNumbersToGroup(row, col - 1);
        if (GetNumberObj(row - 1, col) != null && isHaveArrowChain(GetNumberObj(row - 1, col), ARROW_UP))
            AddNumbersToGroup(row - 1, col);
    }

    public void HoldNumbers(int row, int col)
    {
        aryNumbersGroup = new List<GameObject>();
        aryNumbersGroup = GetGroupNumbers(row, col);
        prevRows = new int[aryNumbersGroup.Count];
        prevCols = new int[aryNumbersGroup.Count];
        SavePrevRowsColumns_WhileMoving();
        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            aryNumbersGroup[i].name = aryNumbersGroup[i].name + "_Moving";
        }

        nComboCount = 0;
    }

    public void onNumberObjClicked()
    {
        aryNumbersStartPos = new List<Vector3>();
        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            aryNumbersStartPos.Add(aryNumbersGroup[i].transform.localPosition);
        }
    }

    int GetNumberPositionX(int col)
    {
        return nNumberStartPosX + col * nNumberSpacing;
    }

    int GetNumberPositionY(int row)
    {
        return nNumberStartPosY + row * nNumberSpacing;
    }

    public int GetRow_WhileMoving(GameObject obj)
    {
        return (int)(obj.transform.localPosition.y - nNumberStartPosY + (nNumberSpacing / 2)) / nNumberSpacing;
    }

    public int GetColumn_WhileMoving(GameObject obj)
    {
        return (int)(obj.transform.localPosition.x - nNumberStartPosX + (nNumberSpacing / 2)) / nNumberSpacing;
    }

    public int GetRow_WhileMoving(Vector3 pos)
    {
        return (int)(pos.y - nNumberStartPosY + (nNumberSpacing / 2)) / nNumberSpacing;
    }

    public int GetColumn_WhileMoving(Vector3 pos)
    {
        return (int)(pos.x - nNumberStartPosX + (nNumberSpacing / 2)) / nNumberSpacing;
    }

    int GetRow(GameObject obj)
    {
        return obj.GetComponent<NumberObjMgr>().GetRow();
    }

    int GetColumn(GameObject obj)
    {
        return obj.GetComponent<NumberObjMgr>().GetColumn();
    }

    void SetNumberRowColumn(GameObject obj, int row, int col)
    {
        obj.GetComponent<NumberObjMgr>().nRow = row;
        obj.GetComponent<NumberObjMgr>().nColumn = col;
        obj.GetComponent<NumberObjMgr>().SetObjectNameFromColumnAndRow();
    }

    bool isHolding(int row, int col)
    {
        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            if (row == GetRow_WhileMoving(aryNumbersGroup[i]) && col == GetColumn_WhileMoving(aryNumbersGroup[i]))
                return true;
        }
        return false;
    }

    void Update_nTable()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 6; j++)
                nTable[i, j] = -1;
        }
        for (int i = 0; i < tableObj.transform.childCount; i++)
        {
            GameObject obj = tableObj.transform.GetChild(i).gameObject;
            nTable[GetRow_WhileMoving(obj), GetColumn_WhileMoving(obj)] = GetNumber(obj);
        }
    }

    bool isHaveArrowChain(GameObject obj, int nIndex)
    {
        return obj.GetComponent<NumberObjMgr>().bChain[nIndex];
    }

    Vector3 RestrictNumbers_ForBoard_WhileMoving(Vector3 vDeltaPos)
    {
        float fNewDeltaPosX = 0, fNewDeltaPosY = 0;

        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            Vector3 newPos = aryNumbersStartPos[i] + vDeltaPos;
            if (newPos.y > nNumberStartPosY + nNumberSpacing * 7)
            {
                fNewDeltaPosY = nNumberStartPosY + nNumberSpacing * 7 - aryNumbersStartPos[i].y;
                vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
            }
            if (newPos.y < nNumberStartPosY)
            {
                fNewDeltaPosY = nNumberStartPosY - aryNumbersStartPos[i].y;
                vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
            }
            if (newPos.x < nNumberStartPosX)
            {
                fNewDeltaPosX = nNumberStartPosX - aryNumbersStartPos[i].x;
                vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
            }
            if (newPos.x > nNumberStartPosX + nNumberSpacing * 5)
            {
                fNewDeltaPosX = nNumberStartPosX + nNumberSpacing * 5 - aryNumbersStartPos[i].x;
                vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
            }
        }

        return vDeltaPos;
    }
    
    public int[] prevRows, prevCols;
    public void MoveNumbersGroup(Vector3 vDeltaPos)
    {
        int i;
        float fNewDeltaPosX = 0, fNewDeltaPosY = 0;

        UpdateNumbersPos_WhileMoving(vDeltaPos);

        vDeltaPos = RestrictNumbers_ForBoard_WhileMoving(vDeltaPos);

        UpdateNumbersPos_WhileMoving(vDeltaPos);

#region Restrict move numbers inside numbers when moving fast

        int curRow = GetRow_WhileMoving(aryNumbersGroup[0]);
        int curCol = GetColumn_WhileMoving(aryNumbersGroup[0]);
        
        if (curCol - prevCols[0] > 0) // Move numbers to right in fast
        {
            fNewDeltaPosX = 0;
            for (i = prevCols[0]; i < curCol; i++)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + ((curRow - prevRows[0]) / (curCol -i));
                    int c = prevCols[j] + (i - prevCols[0]);
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_RIGHT) == false && nTable[r, c] != nTable[r, c + 1] && nTable[r, c + 1] != -1 && fNewDeltaPosX == 0)
                    {
                        fNewDeltaPosX = GetNumberPositionX(c) - aryNumbersStartPos[j].x + (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosX != 0)
                vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
        }
        if (curCol - prevCols[0] < 0) // Move numbers to left in fast
        {
            fNewDeltaPosX = 0;
            for (i = prevCols[0]; i > curCol; i--)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + ((curRow - prevRows[0]) / (i - curCol));
                    int c = prevCols[j] + (i - prevCols[0]);
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_LEFT) == false && nTable[r, c] != nTable[r, c - 1] && nTable[r, c - 1] != -1 && fNewDeltaPosX == 0)
                    {
                        fNewDeltaPosX = GetNumberPositionX(c) - aryNumbersStartPos[j].x - (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosX != 0)
                vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
        }
        if (curRow - prevRows[0] > 0) // Move numbers to top in fast
        {
            fNewDeltaPosY = 0;
            for (i = prevRows[0]; i < curRow; i++)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + (i - prevRows[0]);
                    int c = prevCols[j] + ((curCol - prevCols[0]) / (curRow - i));
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_UP) == false && nTable[r, c] != nTable[r + 1, c] && nTable[r + 1, c] != -1 && fNewDeltaPosY == 0)
                    {
                        fNewDeltaPosY = GetNumberPositionY(r) - aryNumbersStartPos[j].y + (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosY != 0)
                vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
        }
        if (curRow - prevRows[0] < 0) // Move numbers to bottom in fast
        {
            fNewDeltaPosY = 0;
            for (i = prevRows[0]; i > curRow; i--)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + (i - prevRows[0]);
                    int c = prevCols[j] + ((curCol - prevCols[0]) / (i - curRow));
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_DOWN) == false && nTable[r, c] != nTable[r - 1, c] && nTable[r - 1, c] != -1 && fNewDeltaPosY == 0)
                    {
                        fNewDeltaPosY = GetNumberPositionY(r) - aryNumbersStartPos[j].y - (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosY != 0)
                vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
        }
        // Debug.Log(vDeltaPos);

        UpdateNumbersPos_WhileMoving(vDeltaPos);





        curRow = GetRow_WhileMoving(aryNumbersGroup[0]);
        curCol = GetColumn_WhileMoving(aryNumbersGroup[0]);
        if (curCol - prevCols[0] > 0) // Move numbers to right in fast
        {
            fNewDeltaPosX = 0;
            for (i = prevCols[0]; i < curCol; i++)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + ((curRow - prevRows[0]) / (curCol - i));
                    int c = prevCols[j] + (i - prevCols[0]);
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_RIGHT) == false && nTable[r, c] != nTable[r, c + 1] && nTable[r, c + 1] != -1 && fNewDeltaPosX == 0)
                    {
                        fNewDeltaPosX = GetNumberPositionX(c) - aryNumbersStartPos[j].x + (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosX != 0)
                vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
        }
        if (curCol - prevCols[0] < 0) // Move numbers to left in fast
        {
            fNewDeltaPosX = 0;
            for (i = prevCols[0]; i > curCol; i--)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + ((curRow - prevRows[0]) / (i - curCol));
                    int c = prevCols[j] + (i - prevCols[0]);
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_LEFT) == false && nTable[r, c] != nTable[r, c - 1] && nTable[r, c - 1] != -1 && fNewDeltaPosX == 0)
                    {
                        fNewDeltaPosX = GetNumberPositionX(c) - aryNumbersStartPos[j].x - (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosX != 0)
                vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
        }
        if (curRow - prevRows[0] > 0) // Move numbers to top in fast
        {
            fNewDeltaPosY = 0;
            for (i = prevRows[0]; i < curRow; i++)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + (i - prevRows[0]);
                    int c = prevCols[j] + ((curCol - prevCols[0]) / (curRow - i));
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_UP) == false && nTable[r, c] != nTable[r + 1, c] && nTable[r + 1, c] != -1 && fNewDeltaPosY == 0)
                    {
                        fNewDeltaPosY = GetNumberPositionY(r) - aryNumbersStartPos[j].y + (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosY != 0)
                vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
        }
        if (curRow - prevRows[0] < 0) // Move numbers to bottom in fast
        {
            fNewDeltaPosY = 0;
            for (i = prevRows[0]; i > curRow; i--)
            {
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    int r = prevRows[j] + (i - prevRows[0]);
                    int c = prevCols[j] + ((curCol - prevCols[0]) / (i - curRow));
                    if (isHaveArrowChain(aryNumbersGroup[j], ARROW_DOWN) == false && nTable[r, c] != nTable[r - 1, c] && nTable[r - 1, c] != -1 && fNewDeltaPosY == 0)
                    {
                        fNewDeltaPosY = GetNumberPositionY(r) - aryNumbersStartPos[j].y - (nNumberSpacing - nNumberSize);
                        break;
                    }
                }
            }
            if (fNewDeltaPosY != 0)
                vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
        }

        UpdateNumbersPos_WhileMoving(vDeltaPos);







        SavePrevRowsColumns_WhileMoving();

        #endregion

        Update_nTable();

#region Merge_numbers while dragging
        
        GameObject mainHoldingObj = aryNumbersGroup[0];
        bool bMerged = false;
        for (i = 0; i < aryNumbersGroup.Count; i++)
        {
            int row = GetRow_WhileMoving(aryNumbersGroup[i]);
            int col = GetColumn_WhileMoving(aryNumbersGroup[i]);
            GameObject obj = GetNumberObj(row, col);
            List<GameObject> gonnaDestroyObjAry = GetGroupNumbers(row, col);
            if (obj != null && aryNumbersGroup[i] != obj && GetNumber(obj) == GetNumber(aryNumbersGroup[i]))
            {
                GameObject otherObj = aryNumbersGroup[i];
                Vector3 deltaBetweenTwoObjs = obj.transform.localPosition - otherObj.transform.localPosition;
                for (int j = 0; j < aryNumbersGroup.Count; j++)
                {
                    aryNumbersGroup[j].transform.localPosition += deltaBetweenTwoObjs;
                }
                if (bMerged == false)
                {
                    mainHoldingObj.GetComponent<NumberObjMgr>().CheckMovedToOtherColumn();
                }
                bMerged = true;
                int objColumn, otherObjColumn;
                objColumn = GetColumn(obj);
                otherObjColumn = GetColumn(otherObj);

                RemoveChain(obj);
                RemoveChain(otherObj);
                if (hasCoin(otherObj))
                    AddGameCoin(otherObj.transform.localPosition);

                if (GetNumber(obj) == 0 || GetNumber(obj) == 19)
                {
                    if (GetNumber(obj) == 0)
                    {
                        StartCoroutine(PlayPlusStarAnim(obj));
                        StartCoroutine(AddStarHelp());
                    }
                    else if (GetNumber(obj) == 19)
                    {
                        StartCoroutine(PlayComplete20Anim(obj));
                        AddNumber(obj);
                    }
                    aryNumbersGroup.RemoveAt(i);

                    gonnaDestroyObjAry.Remove(obj);
                    //Debug.Log("3333333");
                    //Debug.Log(obj.name + "   " + GetNumber(obj));
                    //Debug.Log(otherObj.name + "   " + GetNumber(otherObj));
                    DestroyImmediate(obj);
                    DestroyImmediate(otherObj);

                    Update_nTable();
                    CheckColumnByGravity(7, otherObjColumn);
                    CheckColumnByGravity(7, objColumn);
                }
                else
                {
                    AddNumber(obj);
                    otherObjColumn = GetColumn(otherObj);
                    aryNumbersGroup.RemoveAt(i);

                    //Debug.Log("4444444");
                    //Debug.Log(otherObj.name + "   " + GetNumber(otherObj));
                    DestroyImmediate(otherObj);

                    Update_nTable();
                    CheckColumnByGravity(7, otherObjColumn);
                    CheckColumnByGravity(7, objColumn);
                }

                for (int j = 0; j < gonnaDestroyObjAry.Count; j++)
                {
                    CheckColumnByGravity(7, GetColumn(gonnaDestroyObjAry[j]));
                }
            }
        }

        if (bMerged)
        {
            if (mainHoldingObj != null)
            {
                ForceReleaseNumberObj(mainHoldingObj);
            }

            for (i = 0; i < aryNumbersGroup.Count; i++)
            {
                int nDropColumnIndex = GetColumn_WhileMoving(aryNumbersGroup[i]);
                int nDropRowIndex = GetRow_WhileMoving(aryNumbersGroup[i]);
                SetNumberRowColumn(aryNumbersGroup[i], nDropRowIndex, nDropColumnIndex);
                SetNumberObjPosition(aryNumbersGroup[i]);
            }
            Update_nTable();
            for (i = 0; i < aryNumbersGroup.Count; i++)
            {
                CheckColumnByGravity(7, GetColumn(aryNumbersGroup[i]));
            }
            return;
        }

        #endregion

        #region Restrict move numbers inside numbers when moving slow
        for (i = 0; i < aryNumbersGroup.Count; i++)
        {
            int row = GetRow_WhileMoving(aryNumbersGroup[i]);
            int col = GetColumn_WhileMoving(aryNumbersGroup[i]);
            if (row > 0 && nTable[row, col] != nTable[row - 1, col] && nTable[row - 1, col] != -1 && !isHolding(row - 1, col))
            {
                if (aryNumbersGroup[i].transform.localPosition.y < GetNumberPositionY(row - 1) + nNumberSize)
                {
                    fNewDeltaPosY = GetNumberPositionY(row - 1) + nNumberSize - aryNumbersStartPos[i].y;
                    vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
                }
            }
            if (row < 7 && nTable[row, col] != nTable[row + 1, col] && nTable[row + 1, col] != -1 && !isHolding(row + 1, col))
            {
                if (aryNumbersGroup[i].transform.localPosition.y > GetNumberPositionY(row + 1) - nNumberSize)
                {
                    fNewDeltaPosY = GetNumberPositionY(row + 1) - nNumberSize - aryNumbersStartPos[i].y;
                    vDeltaPos = new Vector3(vDeltaPos.x, fNewDeltaPosY, vDeltaPos.z);
                }
            }
            if (col > 0 && nTable[row, col] != nTable[row, col - 1] && nTable[row, col - 1] != -1 && !isHolding(row, col - 1))
            {
                if (aryNumbersGroup[i].transform.localPosition.x < GetNumberPositionX(col - 1) + nNumberSize)
                {
                    fNewDeltaPosX = GetNumberPositionX(col - 1) + nNumberSize - aryNumbersStartPos[i].x;
                    vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
                }
            }
            if (col < 5 && nTable[row, col] != nTable[row, col + 1] && nTable[row, col + 1] != -1 && !isHolding(row, col + 1))
            {
                if (aryNumbersGroup[i].transform.localPosition.x > GetNumberPositionX(col + 1) - nNumberSize)
                {
                    fNewDeltaPosX = GetNumberPositionX(col + 1) - nNumberSize - aryNumbersStartPos[i].x;
                    vDeltaPos = new Vector3(fNewDeltaPosX, vDeltaPos.y, vDeltaPos.z);
                }
            }
        }

        vDeltaPos = RestrictNumbers_ForBoard_WhileMoving(vDeltaPos);

        #endregion

        UpdateNumbersPos_WhileMoving(vDeltaPos);
        SavePrevRowsColumns_WhileMoving();
    }

    void UpdateNumbersPos_WhileMoving(Vector3 vDeltaPos)
    {
        for (int i = 0; i < aryNumbersGroup.Count; i++)
            aryNumbersGroup[i].transform.localPosition = aryNumbersStartPos[i] + vDeltaPos;
    }

    void SavePrevRowsColumns_WhileMoving()
    {
        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            prevRows[i] = GetRow_WhileMoving(aryNumbersGroup[i]);
            prevCols[i] = GetColumn_WhileMoving(aryNumbersGroup[i]);
        }
    }

    void SetNumberObjPosition(GameObject obj)
    {
        obj.GetComponent<NumberObjMgr>().SetPosition();
    }

    void AddNumber(GameObject obj)
    {
        if (nComboCount > 0)
            PlayComboEffect();
        int num = GetNumber(obj);
        if (num + 1 > nGameMaxNumber)
            nGameMaxNumber = num + 1;
        nComboCount++;
        CheckUnlockLevel();
        obj.GetComponent<NumberObjMgr>().AddNumber();

        isNumberMoving = false;
        GenerateRowIfNoMove();
    }

    void CheckUnlockLevel()
    {
        if (nGameMaxNumber >= 15 && MainPanelMgr.Instance.nLevelUnlocked < 2)
        {
            MainPanelMgr.Instance.nLevelUnlocked = 2;
            PlayerPrefs.SetInt("UnlockLevel", MainPanelMgr.Instance.nLevelUnlocked);
        }
        else if (nGameMaxNumber >= 10 && MainPanelMgr.Instance.nLevelUnlocked < 1)
        {
            MainPanelMgr.Instance.nLevelUnlocked = 1;
            PlayerPrefs.SetInt("UnlockLevel", MainPanelMgr.Instance.nLevelUnlocked);
        }
    }

    public void RemoveChain(GameObject obj)
    {
        // 0: right, 1: up, 2: left, 3: down
        if (obj != null)
            obj.GetComponent<NumberObjMgr>().RemoveChain();
        GameObject leftObj = GetNumberObjFromRowAndColumn(GetRow(obj), GetColumn(obj) - 1);
        if (leftObj != null)
            leftObj.GetComponent<NumberObjMgr>().RemoveArrowChain(ARROW_RIGHT);
        GameObject rightObj = GetNumberObjFromRowAndColumn(GetRow(obj), GetColumn(obj) + 1);
        if (rightObj != null)
            rightObj.GetComponent<NumberObjMgr>().RemoveArrowChain(ARROW_LEFT);
        GameObject bottomObj = GetNumberObjFromRowAndColumn(GetRow(obj) - 1, GetColumn(obj));
        if (bottomObj != null)
            bottomObj.GetComponent<NumberObjMgr>().RemoveArrowChain(ARROW_UP);
        GameObject topObj = GetNumberObjFromRowAndColumn(GetRow(obj) + 1, GetColumn(obj));
        if (topObj != null)
            topObj.GetComponent<NumberObjMgr>().RemoveArrowChain(ARROW_DOWN);
    }

    GameObject GetNumberObjFromRowAndColumn(int row, int col)
    {
        int nChildCnt = tableObj.transform.childCount;
        for (int i = 0; i < nChildCnt; i++)
        {
            GameObject obj = tableObj.transform.GetChild(i).gameObject;
            if (GetRow(obj) == row && GetColumn(obj) == col)
                return obj;
        }
        return null;
    }

    public void MoveNumbersToFirstPos()
    {
        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            aryNumbersGroup[i].transform.localPosition = aryNumbersStartPos[i];
            aryNumbersGroup[i].GetComponent<NumberObjMgr>().SetObjectNameFromColumnAndRow();
        }
    }

    public void DropNumbers(GameObject obj)
    {
        if (bTutorial)
        {
            if (nTutorialStep == 1)
            {
                if (GetColumn_WhileMoving(obj) != 2)
                    obj.transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * 0, obj.transform.localPosition.y, obj.transform.localPosition.z);
            }
            else if (nTutorialStep == 2)
            {
                if (GetColumn_WhileMoving(obj) != 3)
                    obj.transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * 5, obj.transform.localPosition.y, obj.transform.localPosition.z);
            }
            else if (nTutorialStep == 3)
            {
                if (GetColumn_WhileMoving(obj) != 2)
                    obj.transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * 5, obj.transform.localPosition.y, obj.transform.localPosition.z);
            }
            else if (nTutorialStep == 4)
            {
                if (GetColumn_WhileMoving(obj) != 3)
                    obj.transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * 2, obj.transform.localPosition.y, obj.transform.localPosition.z);
            }
            else if (nTutorialStep == 5)
            {
                if (GetColumn_WhileMoving(obj) != 3)
                    obj.transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * 1, obj.transform.localPosition.y, obj.transform.localPosition.z);
            }
            else if (nTutorialStep == 6)
            {
                if (GetColumn_WhileMoving(obj) != 4)
                    obj.transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * 3, obj.transform.localPosition.y, obj.transform.localPosition.z);
            }
        }

        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            int nDropColumnIndex = GetColumn_WhileMoving(aryNumbersGroup[i]);
            aryNumbersGroup[i].transform.localPosition = new Vector3(nNumberStartPosX + nNumberSpacing * nDropColumnIndex, aryNumbersGroup[i].transform.localPosition.y, aryNumbersGroup[i].transform.localPosition.z);
        }
        obj.GetComponent<NumberObjMgr>().CheckMovedToOtherColumn();
        StartCoroutine(GotoPositionOfTable());
    }

    IEnumerator GotoPositionOfTable()
    {
        int i;
        Update_nTable();
        int vDeltaMoveRow = 7;
        for (i = 0; i < aryNumbersGroup.Count; i++)
        {
            int nDropColumnIndex = GetColumn_WhileMoving(aryNumbersGroup[i]);
            int nDropRowIndex = GetRow_WhileMoving(aryNumbersGroup[i]);
            if (vDeltaMoveRow > nDropRowIndex)
                vDeltaMoveRow = nDropRowIndex;
            for (int j = nDropRowIndex - 1; j >= 0; j--)
            {
                if (nTable[j, nDropColumnIndex] != -1 && isGroupHasTheNumber(aryNumbersGroup, j, nDropColumnIndex) == false)
                {
                    if (nDropRowIndex - j - 1 < vDeltaMoveRow)
                        vDeltaMoveRow = nDropRowIndex - j - 1;
                }
            }
        }
        for (i = 0; i < aryNumbersGroup.Count; i++)
        {
            int nDropColumnIndex = GetColumn_WhileMoving(aryNumbersGroup[i]);
            int nDropRowIndex = GetRow_WhileMoving(aryNumbersGroup[i]);
            int nTargetRow = nDropRowIndex - vDeltaMoveRow;
            int nTargetPosY = GetNumberPositionY(nTargetRow);

            StartCoroutine(MoveNumberObjByGravity(aryNumbersGroup[i], nTargetPosY));
            SetNumberRowColumn(aryNumbersGroup[i], nTargetRow, nDropColumnIndex);
        }

        while (bAnimationPlaying)
            yield return new WaitForEndOfFrame();

        Update_nTable();
        for (i = 0; i < aryNumbersGroup.Count; i++)
        {
            CheckColumnByGravity(7, GetColumn(aryNumbersGroup[i]));
        }

        if (bTutorial)
        {
            if (nTutorialStep == 2 && GetColumn_WhileMoving(aryNumbersGroup[0]) == 3)
                StartCoroutine(GotoNextTutorialStep());
        }
    }

    IEnumerator MoveNumberObjByGravity(GameObject obj, int nTargetPosY)
    {
        bAnimationPlaying = true;

        bool bNumberMoving = true;
        while (bNumberMoving)
        {
            float fOldY = obj.transform.localPosition.y;
            fOldY -= 145;
            if (fOldY < nTargetPosY)
            {
                fOldY = nTargetPosY;
                bNumberMoving = false;
            }
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, fOldY, obj.transform.localPosition.z);
            yield return new WaitForEndOfFrame();
        }

        bAnimationPlaying = false;
    }

    public bool isAutoMoveDown(int nMovedToOtherColumn)
    {
        // Debug.Log("isAutoMoveDown " + nMovedToOtherColumn);
        Update_nTable();
        for (int i = 0; i < aryNumbersGroup.Count; i++)
        {
            int k = GetColumn_WhileMoving(aryNumbersGroup[i]) - nMovedToOtherColumn;
            CheckColumnByGravity(7, k);
        }
        return false;
    }

    bool isGroupHasTheNumber(List<GameObject> groupNumber, int row, int col)
    {
        for (int i = 0; i < groupNumber.Count; i++)
        {
            if (GetRow_WhileMoving(groupNumber[i]) == row && GetColumn_WhileMoving(groupNumber[i]) == col)
                return true;
        }
        return false;
    }

    bool isMoveNumbersDown(List<GameObject> grpNumber)
    {
        for (int i = 0; i < grpNumber.Count; i++)
        {
            int row = GetRow_WhileMoving(grpNumber[i]);
            int col = GetColumn_WhileMoving(grpNumber[i]);
            if (row > 0 && nTable[row, col] != nTable[row - 1, col] && nTable[row - 1, col] != -1 && isGroupHasTheNumber(grpNumber, row - 1, col) == false)
                return false;
        }
        return true;
    }

    IEnumerator CheckColumnAgain(int row, int col)
    {
        bool isNumberMovingDown = isColumnNumberObjsMovingDown(col);
        while (isColumnNumberObjsMovingDown(col))
            yield return new WaitForEndOfFrame();
        if (isNumberMovingDown)
        {
            Update_nTable();
            CheckColumnByGravity(row, col);
        }
    }

    void CheckColumnByGravity(int row, int col)
    {
        // Debug.Log("CheckColumnByGravity " + col);
        if (row == 0)
        {
            if (isAllNumberObjsMovingDown() == false)
                GenerateRowIfNoMove();
            else
                StartCoroutine(CheckColumnAgain(7, col));
        }
        if (row > 0)
        {
            if ((nTable[row, col] != -1 && nTable[row - 1, col] == -1) || (nTable[row, col] != -1 && nTable[row, col] == nTable[row - 1, col]))
            {
                List<GameObject> numbersOfMoveDown = new List<GameObject>();
                for (int j = row; j < 8; j++)
                {
                    if (GetNumberObj(j, col) != null && !numbersOfMoveDown.Contains(GetNumberObj(j, col)))
                    {
                        List<GameObject> gNumbers = GetGroupNumbers(j, col);
                        bool bMoveNumberDown = isMoveNumbersDown(gNumbers);
                        if (bMoveNumberDown)
                        {
                            for (int i = 0; i < gNumbers.Count; i++)
                            {
                                if (nTable[GetRow(gNumbers[i]), GetColumn(gNumbers[i])] == nTable[GetRow(gNumbers[i]) - 1, GetColumn(gNumbers[i])])
                                {
                                    if (nTable[GetRow(gNumbers[i]) - 1, GetColumn(gNumbers[i])] == 0)
                                        nTable[GetRow(gNumbers[i]) - 1, GetColumn(gNumbers[i])] = -1;
                                    else
                                        nTable[GetRow(gNumbers[i]) - 1, GetColumn(gNumbers[i])]++;
                                    nTable[GetRow(gNumbers[i]), GetColumn(gNumbers[i])] = -1;
                                }
                                else
                                {
                                    nTable[GetRow(gNumbers[i]) - 1, GetColumn(gNumbers[i])] = nTable[GetRow(gNumbers[i]), GetColumn(gNumbers[i])];
                                    nTable[GetRow(gNumbers[i]), GetColumn(gNumbers[i])] = -1;
                                }

                                numbersOfMoveDown.Add(gNumbers[i]);
                            }
                        }
                        else
                            break;
                    }
                }

                if (numbersOfMoveDown.Count > 0)
                {
                    List<int> columnsOfMoveDown = new List<int>();
                    for (int j = 0; j < numbersOfMoveDown.Count; j++)
                    {
                        if (!columnsOfMoveDown.Contains(GetColumn(numbersOfMoveDown[j])))
                            columnsOfMoveDown.Add(GetColumn(numbersOfMoveDown[j]));
                    }

                    for (int j = 0; j < numbersOfMoveDown.Count; j++)
                        StartCoroutine(MoveNumberDown(numbersOfMoveDown[j]));
                    for (int j = 0; j < columnsOfMoveDown.Count; j++)
                        CheckColumnByGravity(0, columnsOfMoveDown[j]);
                }
                else
                    CheckColumnByGravity(row - 1, col);
            }
            else
                CheckColumnByGravity(row - 1, col);
        }
    }

    IEnumerator MoveNumberDown(GameObject obj)
    {
        if (!GetNumberObjMovingDown(obj))
        {
            SetNumberObjMovingDown(obj, true);

            Vector3 pos = obj.transform.localPosition;
            float targetPosY = obj.transform.localPosition.y - nNumberSpacing;
            bool bMoveDown = true;
            while (bMoveDown)
            {
                pos = new Vector3(pos.x, pos.y - 50, pos.z);
                if (pos.y < targetPosY)
                {
                    bMoveDown = false;
                    pos = new Vector3(pos.x, targetPosY, pos.z);
                }
                obj.transform.localPosition = pos;
                yield return new WaitForEndOfFrame();
            }

            bool bMerged = false;
            if (obj != null)
            {
                bMerged = true;
                GameObject otherObj = GetNumberObj(GetRow_WhileMoving(obj), GetColumn_WhileMoving(obj));
                if (otherObj != null && GetNumber(obj) == GetNumber(otherObj))
                {
                    List<GameObject> newNumbersGroup = GetGroupNumbers(GetRow(otherObj), GetColumn(otherObj));

                    RemoveChain(obj);
                    RemoveChain(otherObj);
                    if (hasCoin(otherObj))
                        AddGameCoin(otherObj.transform.localPosition);

                    if (GetNumber(obj) == 0 || GetNumber(obj) == 19)
                    {
                        if (GetNumber(obj) == 0)
                        {
                            StartCoroutine(PlayPlusStarAnim(obj));
                            StartCoroutine(AddStarHelp());
                        }
                        else if (GetNumber(obj) == 19)
                        {
                            StartCoroutine(PlayComplete20Anim(obj));
                            AddNumber(obj);
                        }
                        //Debug.Log("11111111");
                        //Debug.Log(obj.name + "   " + GetNumber(obj));
                        //Debug.Log(otherObj.name + "   " + GetNumber(otherObj));
                        DestroyImmediate(obj);
                        newNumbersGroup.Remove(otherObj);
                        DestroyImmediate(otherObj);
                    }
                    else
                    {
                        AddNumber(obj);
                        newNumbersGroup.Remove(otherObj);
                        //Debug.Log("222222222");
                        //Debug.Log(otherObj.name + "   " + GetNumber(otherObj));
                        DestroyImmediate(otherObj);
                    }

                    for (int j = 0; j < newNumbersGroup.Count; j++)
                    {
                        CheckColumnByGravity(7, GetColumn(newNumbersGroup[j]));
                    }

                    if (bMerged)
                        yield return new WaitForSeconds(0.2f);
                }
            }

            if (obj != null)
            {
                obj.transform.GetComponent<NumberObjMgr>().RemoveRow();
                obj.transform.GetComponent<NumberObjMgr>().SetPosition();

                SetNumberObjMovingDown(obj, false);
            }
        }
    }

    bool hasCoin(GameObject obj)
    {
        return obj.GetComponent<NumberObjMgr>().hasCoin();
    }

    void SetNumberObjMovingDown(GameObject obj, bool bEnable)
    {
        obj.transform.GetComponent<NumberObjMgr>().SetMovingDown(bEnable);
    }

    bool GetNumberObjMovingDown(GameObject obj)
    {
        return obj.transform.GetComponent<NumberObjMgr>().GetMovingDown();
    }

    bool isAllNumberObjsMovingDown()
    {
        int nChildCnt = tableObj.transform.childCount;
        for (int i = 0; i < nChildCnt; i++)
        {
            if (GetNumberObjMovingDown(tableObj.transform.GetChild(i).gameObject))
                return true;
        }
        return false;
    }

    bool isColumnNumberObjsMovingDown(int col)
    {
        for (int i = 0; i < 8; i++)
        {
            if (GetNumberObj(i, col) != null && GetNumberObjMovingDown(GetNumberObj(i, col)))
                return true;
        }
        return false;
    }

    void ForceReleaseNumberObj(GameObject obj)
    {
        obj.GetComponent<UIDragObject>().enabled = false;
        obj.GetComponent<NumberObjMgr>().isPressing = false;
        isNumberMoving = false;
    }

    void PlayComboEffect()
    {
        comboEffObj.SetActive(true);
        comboEffObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/GamePlay/combo_" + nComboCount);
        comboEffObj.GetComponent<Animator>().Play(0);
    }

    IEnumerator PlayComplete20Anim(GameObject obj)
    {
        GameObject complete20EffObj = Resources.Load("Prefabs/Complete20Anim") as GameObject;
        GameObject tmpComplete20EffObj = NGUITools.AddChild(tableEffObj, complete20EffObj);
        tmpComplete20EffObj.SetActive(true);
        tmpComplete20EffObj.GetComponent<TweenPosition>().enabled = true;
        tmpComplete20EffObj.GetComponent<TweenPosition>().ResetToBeginning();
        tmpComplete20EffObj.GetComponent<TweenPosition>().from = obj.transform.localPosition;
        tmpComplete20EffObj.GetComponent<TweenPosition>().PlayForward();
        tmpComplete20EffObj.GetComponent<TweenScale>().enabled = true;
        tmpComplete20EffObj.GetComponent<TweenScale>().ResetToBeginning();
        tmpComplete20EffObj.GetComponent<TweenScale>().duration = 1.0f;
        tmpComplete20EffObj.GetComponent<TweenScale>().PlayForward();

        yield return new WaitForSeconds(1.0f);
        Destroy(tmpComplete20EffObj);
    }

    public void PlayStarHelp(int row, int col)
    {
        int i;
        for (i = 0; i < 6; i++)
        {
            if (i != col)
            {
                GameObject obj = GetNumberObj(row, i);
                if (obj != null)
                {
                    RemoveChain(obj);
                    if (GetNumber(obj) == 0)
                    {
                        StartCoroutine(PlayPlusStarAnim(obj));
                        StartCoroutine(AddStarHelp());
                    }
                    Destroy(obj);
                    nTable[row, i] = -1;
                }
            }
        }
        for (i = 0; i < 8; i++)
        {
            GameObject obj = GetNumberObj(i, col);
            if (obj != null)
            {
                RemoveChain(obj);
                if (GetNumber(obj) == 0)
                {
                    StartCoroutine(PlayPlusStarAnim(obj));
                    StartCoroutine(AddStarHelp());
                }
                Destroy(obj);
                nTable[i, col] = -1;
            }
        }
        starHelpTutorialObj.SetActive(false);
        StartCoroutine(PlayStarHelpAnim(row, col));
    }

    IEnumerator PlayStarHelpAnim(int row, int col)
    {
        for (int i = 0; i < 4; i++)
        {
            starHelpEffObj.transform.GetChild(i).gameObject.GetComponent<TweenScale>().enabled = false;
            starHelpEffObj.transform.GetChild(i).gameObject.transform.localPosition = Vector3.one;
        }
        GameObject obj = GetNumberObj(row, col);
        starHelpEffObj.SetActive(true);

        if (col > 0)
        {
            starHelpEffObj.transform.GetChild(0).gameObject.SetActive(true);
            starHelpEffObj.transform.GetChild(0).gameObject.transform.localScale = Vector3.one;
            starHelpEffObj.transform.GetChild(0).gameObject.GetComponent<TweenPosition>().enabled = true;
            starHelpEffObj.transform.GetChild(0).gameObject.GetComponent<TweenPosition>().ResetToBeginning();
            starHelpEffObj.transform.GetChild(0).gameObject.GetComponent<TweenPosition>().from = obj.transform.localPosition;
            starHelpEffObj.transform.GetChild(0).gameObject.GetComponent<TweenPosition>().to = new Vector3(nNumberStartPosX, obj.transform.localPosition.y, obj.transform.localPosition.z);
            starHelpEffObj.transform.GetChild(0).gameObject.GetComponent<TweenPosition>().PlayForward();
        }
        else
            starHelpEffObj.transform.GetChild(0).gameObject.SetActive(false);

        if (col < 5)
        {
            starHelpEffObj.transform.GetChild(1).gameObject.SetActive(true);
            starHelpEffObj.transform.GetChild(1).gameObject.transform.localScale = Vector3.one;
            starHelpEffObj.transform.GetChild(1).gameObject.GetComponent<TweenPosition>().enabled = true;
            starHelpEffObj.transform.GetChild(1).gameObject.GetComponent<TweenPosition>().ResetToBeginning();
            starHelpEffObj.transform.GetChild(1).gameObject.GetComponent<TweenPosition>().from = obj.transform.localPosition;
            starHelpEffObj.transform.GetChild(1).gameObject.GetComponent<TweenPosition>().to = new Vector3(nNumberStartPosX + nNumberSpacing * 5, obj.transform.localPosition.y, obj.transform.localPosition.z);
            starHelpEffObj.transform.GetChild(1).gameObject.GetComponent<TweenPosition>().PlayForward();
        }
        else
            starHelpEffObj.transform.GetChild(1).gameObject.SetActive(false);

        if (row > 0)
        {
            starHelpEffObj.transform.GetChild(2).gameObject.SetActive(true);
            starHelpEffObj.transform.GetChild(2).gameObject.transform.localScale = Vector3.one;
            starHelpEffObj.transform.GetChild(2).gameObject.GetComponent<TweenPosition>().enabled = true;
            starHelpEffObj.transform.GetChild(2).gameObject.GetComponent<TweenPosition>().ResetToBeginning();
            starHelpEffObj.transform.GetChild(2).gameObject.GetComponent<TweenPosition>().from = obj.transform.localPosition;
            starHelpEffObj.transform.GetChild(2).gameObject.GetComponent<TweenPosition>().to = new Vector3(obj.transform.localPosition.x, nNumberStartPosY, obj.transform.localPosition.z);
            starHelpEffObj.transform.GetChild(2).gameObject.GetComponent<TweenPosition>().PlayForward();
        }
        else
            starHelpEffObj.transform.GetChild(2).gameObject.SetActive(false);

        if (row < 7)
        {
            starHelpEffObj.transform.GetChild(3).gameObject.SetActive(true);
            starHelpEffObj.transform.GetChild(3).gameObject.transform.localScale = Vector3.one;
            starHelpEffObj.transform.GetChild(3).gameObject.GetComponent<TweenPosition>().enabled = true;
            starHelpEffObj.transform.GetChild(3).gameObject.GetComponent<TweenPosition>().ResetToBeginning();
            starHelpEffObj.transform.GetChild(3).gameObject.GetComponent<TweenPosition>().from = obj.transform.localPosition;
            starHelpEffObj.transform.GetChild(3).gameObject.GetComponent<TweenPosition>().to = new Vector3(obj.transform.localPosition.x, nNumberStartPosY + nNumberSpacing * 7, obj.transform.localPosition.z);
            starHelpEffObj.transform.GetChild(3).gameObject.GetComponent<TweenPosition>().PlayForward();
        }
        else
            starHelpEffObj.transform.GetChild(3).gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < 4; i++)
        {
            starHelpEffObj.transform.GetChild(i).gameObject.GetComponent<TweenScale>().enabled = true;
            starHelpEffObj.transform.GetChild(i).gameObject.GetComponent<TweenScale>().ResetToBeginning();
            starHelpEffObj.transform.GetChild(i).gameObject.GetComponent<TweenScale>().duration = 0.3f;
            starHelpEffObj.transform.GetChild(i).gameObject.GetComponent<TweenScale>().PlayForward();
        }
        yield return new WaitForSeconds(0.5f);

        starHelpEffObj.SetActive(false);

        bStarHelpBtnClicked = false;

        for (int i = 0; i < 6; i++)
        {
            if (i != col)
            {
                for (int j = row + 1; j < 8; j++)
                {
                    GameObject numberObj = GetNumberObj(j, i);
                    if (numberObj != null)
                        StartCoroutine(MoveNumberDown_N_Rows(numberObj, 1));
                }
            }
        }
        while (isAllNumberObjsMovingDown())
            yield return new WaitForEndOfFrame();

        for (int i = 0; i < 6; i++)
        {
            if (i != col)
            {
                for (int j = row + 1; j < 8; j++)
                {
                    GameObject numberObj = GetNumberObj(j, i);
                    if (numberObj != null)
                    {
                        numberObj.transform.GetComponent<NumberObjMgr>().RemoveRow(1);
                        numberObj.transform.GetComponent<NumberObjMgr>().SetPosition();
                    }
                }
            }
        }
        Update_nTable();

        for (int i = 0; i < 6; i++)
        {
            if (i != col)
                CheckColumnByGravity(7, i);
        }
        int nChildCnt = tableObj.transform.childCount;
        for (int i = 0; i < nChildCnt; i++)
            tableObj.transform.GetChild(i).gameObject.GetComponent<UIDragObject>().enabled = true;
    }

    float fRaiseAmount = 0;
    IEnumerator PlayStarHelpRaiseAnim()
    {
        while (fRaiseAmount > 0)
        {
            float fDeltaTimeVal = Time.deltaTime / 3;
            fRaiseAmount -= fDeltaTimeVal;
            if (fRaiseAmount < 0)
                fRaiseAmount = 0;
            yield return new WaitForEndOfFrame();
            float fVal = starHelpObj.transform.GetChild(0).gameObject.GetComponent<UITexture>().fillAmount;
            fVal += fDeltaTimeVal;
            if (fVal > 1)
                fVal--;
            starHelpObj.transform.GetChild(0).gameObject.GetComponent<UITexture>().fillAmount = fVal;
            starHelpObj.transform.GetChild(1).gameObject.GetComponent<UITexture>().fillAmount = fVal;
        }
        UpdateStarHelpProgress();
    }

    void UpdateStarHelpProgress()
    {
        starHelpObj.transform.GetChild(0).gameObject.GetComponent<UITexture>().fillAmount = (float)(nGameStarHelp % 6) / 6;
        starHelpObj.transform.GetChild(1).gameObject.GetComponent<UITexture>().fillAmount = (float)(nGameStarHelp % 6) / 6;
        starHelpObj.transform.GetChild(2).gameObject.SetActive(nGameStarHelp >= 6);
        starHelpObj.transform.GetChild(3).gameObject.SetActive(nGameStarHelp >= 6);
        starHelpObj.transform.GetChild(3).gameObject.GetComponent<UILabel>().text = (nGameStarHelp / 6).ToString();
        starHelpObj.transform.GetChild(4).gameObject.SetActive(nGameStarHelp >= 6);
        starHelpObj.transform.GetChild(5).gameObject.SetActive(nGameStarHelp >= 6);
    }

    IEnumerator PlayPlusStarAnim(GameObject obj)
    {
        GameObject plusStarHelpAnimPrefabObj = Resources.Load("Prefabs/PlusStarEffect") as GameObject;
        GameObject plusStarHelpAnimObj = NGUITools.AddChild(tableEffObj, plusStarHelpAnimPrefabObj);
        plusStarHelpAnimObj.SetActive(true);
        plusStarHelpAnimObj.GetComponent<TweenPosition>().ResetToBeginning();
        plusStarHelpAnimObj.GetComponent<TweenPosition>().from = obj.transform.localPosition;
        plusStarHelpAnimObj.GetComponent<TweenPosition>().PlayForward();

        yield return new WaitForSeconds(0.7f);
        Destroy(plusStarHelpAnimObj);

        starHelpParticleObj.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        starHelpParticleObj.SetActive(false);
    }

    public Texture GetNumberTexture(int number)
    {
        return Resources.Load<Texture>("Images/GamePlay/Numbers/" + number);
    }
}
