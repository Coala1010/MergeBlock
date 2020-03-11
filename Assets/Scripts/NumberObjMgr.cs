using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberObjMgr : MonoBehaviour
{
    public int number;
    bool bCoin;
    public int nColumn;
    public int nRow;
    public bool isPressing = false;
    bool bMovedToOtherColumn = false;
    bool isMovingDown = false;

    Vector3 vStartPos, vDeltaPos;

    public bool[] bChain = new bool[4]; // 0: right, 1: up, 2: left, 3: down

    int nMovedToOtherColumn = 0;

    GameObject chainObj;

    private void Awake()
    {
        chainObj = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (transform.localPosition.x < GamePlayMgr.nNumberStartPosX)
            transform.localPosition = new Vector3(GamePlayMgr.nNumberStartPosX, transform.localPosition.y, transform.localPosition.z);
        if (transform.localPosition.x > GamePlayMgr.nNumberStartPosX + GamePlayMgr.nNumberSpacing * 5)
            transform.localPosition = new Vector3(GamePlayMgr.nNumberStartPosX + GamePlayMgr.nNumberSpacing * 5, transform.localPosition.y, transform.localPosition.z);
        if (transform.localPosition.y < GamePlayMgr.nNumberStartPosY)
            transform.localPosition = new Vector3(transform.localPosition.x, GamePlayMgr.nNumberStartPosY, transform.localPosition.z);
        if (transform.localPosition.y > GamePlayMgr.nNumberStartPosY + GamePlayMgr.nNumberSpacing * 7)
            transform.localPosition = new Vector3(transform.localPosition.x, GamePlayMgr.nNumberStartPosY + GamePlayMgr.nNumberSpacing * 7, transform.localPosition.z);
        
        if (isPressing)
        {
            vDeltaPos = transform.localPosition - vStartPos;
            GamePlayMgr.Instance.MoveNumbersGroup(vDeltaPos);
            if (this == null)
                return;
            CheckMovedToOtherColumn();
        }
    }

    public void CheckMovedToOtherColumn()
    {
        int nMovingDeltaX = (int)transform.localPosition.x - (GamePlayMgr.nNumberStartPosX + GamePlayMgr.nNumberSpacing * nColumn);
        if (Math.Abs(nMovingDeltaX) >= GamePlayMgr.nNumberSpacing * (Math.Abs(nMovedToOtherColumn) + 1))
        {
            nMovedToOtherColumn = nMovingDeltaX / GamePlayMgr.nNumberSpacing;
            GamePlayMgr.Instance.isAutoMoveDown(nMovedToOtherColumn);
        }
    }

    bool CheckNoMoving()
    {
        if (transform.localPosition.x > (GamePlayMgr.nNumberStartPosX + GamePlayMgr.nNumberSpacing * (nColumn - 0.5f)) &&
            transform.localPosition.x < (GamePlayMgr.nNumberStartPosX + GamePlayMgr.nNumberSpacing * (nColumn + 0.5f)) &&
            transform.localPosition.y > (GamePlayMgr.nNumberStartPosY + GamePlayMgr.nNumberSpacing * (nRow - 0.5f)) &&
            transform.localPosition.y < (GamePlayMgr.nNumberStartPosY + GamePlayMgr.nNumberSpacing * (nRow + 0.5f)))
            return true;
        return false;
    }

    public void Generate(int num, int col)
    {
        nColumn = col;
        nRow = 0;
        number = num;
        SetObjectNameFromColumnAndRow();
        transform.GetComponent<UITexture>().mainTexture = GamePlayMgr.Instance.GetNumberTexture(number);
        SetPosition();
        if (number > 0 && GamePlayMgr.Instance != null && GamePlayMgr.Instance.nGameMaxNumber >= 10)
        {
            bCoin = UnityEngine.Random.Range(1, 100) % 10 == 0;
            gameObject.transform.GetChild(1).gameObject.SetActive(bCoin);
        }
    }

    public void Generate_ForTutorial(int num, int row, int col)
    {
        nColumn = col;
        nRow = row;
        number = num;
        SetObjectNameFromColumnAndRow();
        transform.GetComponent<UITexture>().mainTexture = GamePlayMgr.Instance.GetNumberTexture(number);
        SetPosition();
    }

    public void Generate_ForTest(int num, int row, int col)
    {
        nColumn = col;
        nRow = row;
        number = num;
        SetObjectNameFromColumnAndRow();
        transform.GetComponent<UITexture>().mainTexture = GamePlayMgr.Instance.GetNumberTexture(number);
        SetPosition();
        if (number > 0)
        {
            bCoin = UnityEngine.Random.Range(1, 100) % 10 == 0;
            gameObject.transform.GetChild(1).gameObject.SetActive(bCoin);
        }
    }

    public void AddArrowChain(int chaintype)
    {
        bChain[chaintype] = true;
        if (chaintype < 2)
            chainObj.transform.GetChild(chaintype).gameObject.SetActive(bChain[chaintype]);
    }

    public void RemoveChain()
    {
        for (int i = 0; i < 4; i++)
            RemoveArrowChain(i);
    }

    public void RemoveArrowChain(int nIndex)
    {
        // 0: right, 1: up, 2: left, 3: down
        bChain[nIndex] = false;
        chainObj.transform.GetChild(nIndex).gameObject.SetActive(bChain[nIndex]);
    }

    public void AddNumber()
    {
        GamePlayMgr.Instance.AddGameScore(number);
        number++;
        gameObject.GetComponent<UITexture>().mainTexture = GamePlayMgr.Instance.GetNumberTexture(number);
        SetObjectNameFromColumnAndRow();
        RemoveChain();
        AddCoin();
    }

    public void AddCoin()
    {
        if (!bCoin)
            return;
        bCoin = false;
        gameObject.transform.GetChild(1).gameObject.SetActive(bCoin);
        GamePlayMgr.Instance.AddGameCoin(transform.localPosition);
    }

    public bool hasCoin()
    {
        return bCoin;
    }

    public void onPress()
    {
        if (GamePlayMgr.Instance.bTutorial)
            GamePlayMgr.Instance.HideTutorialGuide();
        if (GamePlayMgr.Instance.bStarHelpBtnClicked)
            return;
        isPressing = true;
        GamePlayMgr.Instance.isNumberMoving = true;

        GamePlayMgr.Instance.HoldNumbers(nRow, nColumn);
        vStartPos = transform.localPosition;
        GamePlayMgr.Instance.onNumberObjClicked();

        nMovedToOtherColumn = 0;
    }

    public void onRelease()
    {
        if (GamePlayMgr.Instance.bTutorial)
            GamePlayMgr.Instance.ShowTutorialGuide();
        if (GamePlayMgr.Instance.bStarHelpBtnClicked)
            return;
        if (isPressing)
        {
            isPressing = false;
            GamePlayMgr.Instance.isNumberMoving = false;

            if (!CheckNoMoving())
                GamePlayMgr.Instance.DropNumbers(gameObject);
            else
                GamePlayMgr.Instance.MoveNumbersToFirstPos();
        }
        else
        {
            transform.GetComponent<UIDragObject>().enabled = true;
        }
    }

    public void onTapClicked()
    {
        if (!GamePlayMgr.Instance.bStarHelpBtnClicked)
            return;
        GamePlayMgr.Instance.PlayStarHelp(nRow, nColumn);
    }

    public int GetRow(GameObject obj = null)
    {
        if (obj == null)
            return nRow;
        else
            return obj.GetComponent<NumberObjMgr>().nRow;
    }

    public int GetColumn(GameObject obj = null)
    {
        if (obj == null)
            return nColumn;
        else
            return obj.GetComponent<NumberObjMgr>().nColumn;
    }

    public void AddRow()
    {
        nRow++;
        SetObjectNameFromColumnAndRow();
        SetPosition();
    }

    public void RemoveRow(int nRemoveRowCount = 1)
    {
        nRow -= nRemoveRowCount;
        SetObjectNameFromColumnAndRow();
    }

    public int GetNumber(GameObject obj = null)
    {
        if (obj == null)
            return number;
        else
            return obj.GetComponent<NumberObjMgr>().number;
    }

    public void SetObjectNameFromColumnAndRow()
    {
        name = (nColumn + 1) + "_" + (nRow + 1);
    }

    public void SetPosition()
    {
        transform.localPosition = new Vector3(GamePlayMgr.nNumberStartPosX + nColumn * GamePlayMgr.nNumberSpacing, GamePlayMgr.nNumberStartPosY + nRow * GamePlayMgr.nNumberSpacing, 0);
    }

    public void SetMovingDown(bool bEnable)
    {
        isMovingDown = bEnable;
    }

    public bool GetMovingDown()
    {
        return isMovingDown;
    }
}
