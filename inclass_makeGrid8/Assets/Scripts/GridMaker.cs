using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GridMaker : MonoBehaviour
{
    public const int xIndex = 5;
    public const int yIndex = 7;

    public float WIDTH;
    public float HEIGHT;

    float xPos;
    float yPos;

    public GameObject[,] tiles;

    public GameObject tilePrefab;

    float offset;

    GameObject gridHolder;
    //GameObject playerHolder;

    public GameObject circle;
    public GameObject numText;

    int circleIndexX = (xIndex + 1) / 2 - 1;
    int circleIndexY = (yIndex + 1) / 2 - 1;

    public int[,] arrPosX;
    public int[,] arrPosY;

    int takeMove = 0;
    GameObject playerCircle;

    int continuousVerticalColor;
    int originVerticalColor;
    int detectVerticalColor = 0;

    int originHorizontalColor;
    int continuousHorizontalColor;
    int detectHorizontalColor = 0;

    public float lerpSpd;

    public GameObject fogPS;

    public GameObject score;

    public Text circleMoveTimes;
    int nCicleMoveTimes = 6;

    public AudioSource myAudio;
    public GameObject failPanel;
    bool restartTheGame;

    public AudioClip getSameColorAudio;
    public AudioClip failAudio;
    void Start()
    {
        xPos = -2f;
        yPos = -4f;
        offset = Mathf.Max(WIDTH, HEIGHT) / 2f - 0.5f;
        tiles = new GameObject[xIndex, yIndex];

        gridHolder = new GameObject();
        gridHolder.transform.position = new Vector3(-1f, 0.5f, 0);

        arrPosX = new int[xIndex, yIndex];
        arrPosY = new int[xIndex, yIndex];

        for (int x = 0; x < xIndex; x++)
        {
            for (int y = 0; y < yIndex; y++)
            {
                GameObject newTile = Instantiate(tilePrefab);

                newTile.transform.parent = gridHolder.transform;
                arrPosX[x, y] = (int)(xIndex - x * offset + xPos);
                arrPosY[x, y] = (int)(yIndex - y * offset + yPos);
                newTile.transform.localPosition = new Vector2(arrPosX[x, y], arrPosY[x, y]);

                tiles[x, y] = newTile;
            }
        }
        do
        {
            RandomTilesColor();
        } while (checkSameColor());
        

        tiles[circleIndexX, circleIndexY].GetComponent<TileScript>().Clean();
        //playerCircle = Instantiate(circle);
        playerCircle = circle;
        playerCircle.transform.parent = gridHolder.transform;
        setObjPos(playerCircle, circleIndexX, circleIndexY, true);
    }

    void RandomTilesColor()
    {
        for (int x = 0; x < xIndex; x++)
        {
            for (int y = 0; y < yIndex; y++)
            {
                TileScript tileScript = tiles[x, y].GetComponent<TileScript>();
                tileScript.SetColor(Random.Range(0, tileScript.colors.Length));
            }
        }
    }

    // Update is called once per frame
    private int nLastDropFillFrame = 0;
    void Update()
    {
        if (restartTheGame == false)
        {
            UpdateMoving();

            if (bMoving)
            {
                return;
            }

            checkMove();
            if (takeMove > 0)
            {
                swapTokens();
                takeMove = 0;
            }
            //checkVerticalColor();
            //checkHorizontalColor();

            // Check after input.
            checkSameColor(true);

            //
            checkDrop();

            //
            checkFill();

            //
            Debug.Log("circleMoveTime: " + nCicleMoveTimes);
            checkMoveTimes();
            circleMoveTimes.GetComponent<Text>().text = nCicleMoveTimes.ToString();
            //
            AutoInputTest();
        }
        else
        {
            restart();
        }
    }

    private bool bMoving = false;
    void UpdateMoving()
    {
        bMoving = false;
        for(int x = 0; x < xIndex; ++x)
        {
            for(int y = 0; y < yIndex; ++y)
            {
                TileScript tileScript = tiles[x, y].GetComponent<TileScript>();
                Vector2 vDest = tileScript.GetDest();
                if(vDest.x != -999 && vDest.y != -999)
                {
                    Vector2 vNowPos = tiles[x, y].transform.localPosition;
                    if (Mathf.Abs(vDest.x - vNowPos.x) < 0.01 && Mathf.Abs(vDest.y - vNowPos.y) < 0.01)
                    {
                        tileScript.SetDest(-999, -999);
                        continue;
                    }
                    Vector2 vDelta = vDest - vNowPos;
                    float fSpd = tileScript.GetSpeed();
                    vNowPos.x = vNowPos.x + fSpd * vDelta.x;
                    vNowPos.y = vNowPos.y + fSpd * vDelta.y;
                    tiles[x, y].transform.localPosition = vNowPos;
                    bMoving = true;
                }
            }
        }
    }

    private int nLastAutoInputFrame = 0;
    void AutoInputTest()
    {
        if(Time.frameCount > nLastAutoInputFrame + 1)
        {
            nLastAutoInputFrame = Time.frameCount;
            takeMove = Random.Range(1, 5);
        }
    }

    void checkFill()
    {
        for (int x = 0; x < xIndex; ++x)
        {
            int nDeepIndex = -1;
            for (int y = 0; y < yIndex; ++y)
            {
                if(tiles[x, y].GetComponent<TileScript>().IsCleaned() &&
                    (x != circleIndexX || y != circleIndexY) )
                {
                    nDeepIndex = y;
                }
                else
                {
                    break;
                }
            }
            if(nDeepIndex != -1)
            {
                GameObject newTile = Instantiate(tilePrefab);
                newTile.transform.parent = gridHolder.transform;
                newTile.transform.localPosition = new Vector2(arrPosX[x, 0], arrPosY[x, 0]);

                TileScript tileScript = newTile.GetComponent<TileScript>();
                tileScript.SetColor(Random.Range(0, tileScript.colors.Length));
                tiles[x, 0] = newTile;
            }
        }
    }

    void setObjPos(GameObject p, int ix, int iy, bool bImediate)
    {
        if (ix < 0 || ix >= xIndex ||
            iy < 0 || iy >= yIndex)
        {
            return;
        }


        Vector3 tmp = p.transform.localPosition;
        float dst_x = arrPosX[ix, iy];
        float dst_y = arrPosY[ix, iy];
        if (bImediate == false)
        {
            p.GetComponent<TileScript>().SetDest(dst_x, dst_y);
        }
        else
        {
            tmp.x = dst_x;
            tmp.y = dst_y;
        }
        p.transform.localPosition = tmp;
    }

    void checkMove()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
        {
            takeMove = 1;
            nCicleMoveTimes -= 1;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
        {
            takeMove = 2;
            nCicleMoveTimes -= 1;
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
        {
            takeMove = 3;
            nCicleMoveTimes -= 1;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            takeMove = 4;
            nCicleMoveTimes -= 1;
        }
    }

    void swapTokens()
    {
        int new_circleIndexX = circleIndexX;
        int new_circleIndexY = circleIndexY;
        if (takeMove == 1)
        {
            new_circleIndexY = new_circleIndexY - 1;
        }
        else if (takeMove == 2)
        {
            new_circleIndexY = new_circleIndexY + 1;
        }
        else if (takeMove == 3)
        {
            new_circleIndexX = new_circleIndexX - 1;
        }
        else if (takeMove == 4)
        {
            new_circleIndexX = new_circleIndexX + 1;
        }

        if (new_circleIndexX < 0)
        {
            return;
        }
        if (new_circleIndexX > xIndex - 1)
        {
            return;
        }
        if (new_circleIndexY > yIndex - 1)
        {
            return;
        }
        if (new_circleIndexY < 0)
        {
            return;
        }

        GameObject oldCircle = tiles[circleIndexX, circleIndexY];
        GameObject newCircle = tiles[new_circleIndexX, new_circleIndexY];

        setObjPos(playerCircle, new_circleIndexX, new_circleIndexY, true);
        setObjPos(oldCircle, new_circleIndexX, new_circleIndexY, false);
        setObjPos(newCircle, circleIndexX, circleIndexY, false);

        tiles[circleIndexX, circleIndexY] = newCircle;
        tiles[new_circleIndexX, new_circleIndexY] = oldCircle;

        oldCircle.GetComponent<TileScript>().SetAlpha(0);
        newCircle.GetComponent<TileScript>().SetAlpha(1);

        circleIndexY = new_circleIndexY;
        circleIndexX = new_circleIndexX;
    }

    void checkDrop()
    {
        // circle drop
        if(circleIndexY < yIndex - 1)
        {
            int nDropIndexY = -1;
            for(int i = circleIndexY + 1; i < yIndex; ++i)
            {
                if(tiles[circleIndexX, i].GetComponent<TileScript>().IsCleaned())
                {
                    nDropIndexY = i;
                }
                else
                {
                    break;
                }
            }

            if(nDropIndexY != -1)
            {
                setObjPos(playerCircle, circleIndexX, nDropIndexY, true);
                circleIndexY = nDropIndexY;
            }
        }
        
        // titles drop
        for (int x = 0; x < xIndex; ++x)
        {
            for (int y = 0; y < yIndex - 1; ++y)
            {
                if (tiles[x, y].GetComponent<TileScript>().IsCleaned())
                {
                    continue;
                }
                
                int nNextY = y + 1;
                int nNewIndex = -1;
                if (!(x == circleIndexX && nNextY == circleIndexY) &&
                    tiles[x, nNextY].GetComponent<TileScript>().IsCleaned())
                {
                    nNewIndex = nNextY;
                    for (int o = nNextY + 1; o < yIndex; ++o)
                    {
                        if ((x == circleIndexX && o == circleIndexY)
                            || !tiles[x, o].GetComponent<TileScript>().IsCleaned())
                        {
                            break;
                        }
                        else
                        {
                            nNewIndex = o;
                        }
                    }
                }

                if(nNewIndex == -1)
                {
                    continue;
                }

                // drop : swap two circle.
                GameObject pDropTile = tiles[x, y];
                GameObject pCleanedTile = tiles[x, nNewIndex];
                setObjPos(pDropTile, x, nNewIndex, false);
                setObjPos(pCleanedTile, x, y, false);

                tiles[x, nNewIndex] = pDropTile;
                tiles[x, y] = pCleanedTile;
            }
        }
    }

    /*
    bool checkSameColor(bool bClean = false)
    {
        // check line
        int nSameColorCount = 0;
        int nOldColor = -1;
        for (int x = 0; x < xIndex; ++x)
        {
            // Recalc
            nSameColorCount = 0;
            nOldColor = -1;
            for (int y = 0; y < yIndex; ++y)
            {
                TileScript tileScript = tiles[x, y].GetComponent<TileScript>();
                if (tileScript.IsCleaned())
                {
                    nSameColorCount = 0;
                    nOldColor = -1;
                    continue;
                }
                if (x == circleIndexX &&
                    y == circleIndexY)
                {
                    nSameColorCount = 0;
                    nOldColor = -1;
                    continue;
                }
                int nColor = tileScript.returnColor();
                if (nColor == nOldColor)
                {
                    ++nSameColorCount;
                }
                else
                {
                    nSameColorCount = 1;
                    nOldColor = nColor;
                }

                if (nSameColorCount >= 3)
                {
                    // 消除这些连续的色块
                    if(bClean)
                    {
                        nCicleMoveTimes = 6;
                        score.SendMessage("addScore", nSameColorCount);
                        for (int o = 0; o < nSameColorCount; ++o)
                       {
                           tiles[x, y - o].GetComponent<TileScript>().Clean();

                            Vector2 fogPos = new Vector2(arrPosX[x, y-o]-1, arrPosY[x, y-o]);
                            GameObject fog = Instantiate(fogPS);
                            fog.transform.parent = gridHolder.transform;
                            fog.transform.position = fogPos;
                        }
                    }
                    Debug.Log("3 Same Color : x = " + x + ", y = " + y);
                    return true;
                }
            }
        }

        // check row
        nSameColorCount = 0;
        nOldColor = -1;
        for (int y = 0; y < yIndex; ++y)
        {
            // Recalc
            nSameColorCount = 0;
            nOldColor = -1;
            for (int x = 0; x < xIndex; ++x)
            {
                TileScript tileScript = tiles[x, y].GetComponent<TileScript>();
                if(tileScript.IsCleaned())
                {
                    nSameColorCount = 0;
                    nOldColor = -1;
                    continue;
                }
                if(x == circleIndexX &&
                    y == circleIndexY)
                {
                    nSameColorCount = 0;
                    nOldColor = -1;
                    continue;
                }
                int nColor = tileScript.returnColor();
                if (nColor == nOldColor)
                {
                    ++nSameColorCount;
                }
                else
                {
                    nSameColorCount = 1;
                    nOldColor = nColor;
                }

                if (nSameColorCount >= 3)
                {

                    // 消除这些连续的色块
                    if (bClean)
                    {
                        nCicleMoveTimes = 6;
                        score.SendMessage("addScore",nSameColorCount);
                        for (int o = 0; o < nSameColorCount; ++o)
                        {
                            tiles[x - o, y].GetComponent<TileScript>().Clean();
                            Vector2 fogPos = new Vector2(arrPosX[x - o, y]-1, arrPosY[x-o, y]);
                            GameObject fog = Instantiate(fogPS);
                            fog.transform.parent = gridHolder.transform;
                            fog.transform.position = fogPos;
                        }
                    }
                   // Debug.Log("3 Same Color : x = " + x + ", y = " + y);
                    return true;
                }
            }
        }

        return false;
    }
    */
    bool checkSameColor(bool bClean = false)
    {
        // check line
        int nSameColorCount = 0;
        int nOldColor = -1;
        int nStartY = -1;
        for (int x = 0; x < xIndex; ++x)
        {
            // Recalc
            nSameColorCount = 0;
            nOldColor = -1;
            nStartY = -1;
            for (int y = 0; y < yIndex; ++y)
            {
                TileScript tileScript = tiles[x, y].GetComponent<TileScript>();
                if (tileScript.IsCleaned() ||
                    (x == circleIndexX && y == circleIndexY))
                {
                    if (nSameColorCount >= 3)
                    {
                        break;
                    }
                    nSameColorCount = 0;
                    nOldColor = -1;
                    nStartY = -1;
                    continue;
                }

                int nColor = tileScript.returnColor();
                if (nColor == nOldColor)
                {
                    ++nSameColorCount;
                    nStartY = y;
                }
                else
                {
                    if (nSameColorCount >= 3)
                    {
                        break;
                    }
                    nSameColorCount = 1;
                    nOldColor = nColor;
                }
            }

            if (nSameColorCount >= 3)
            {
                // 消除这些连续的色块
                if (bClean)
                {
                    nCicleMoveTimes = 6;
                    score.SendMessage("addScore", nSameColorCount);
                    myAudio.PlayOneShot(getSameColorAudio);
                    for (int o = 0; o < nSameColorCount; ++o)
                    {
                        tiles[x, nStartY - o].GetComponent<TileScript>().Clean();

                        Vector2 fogPos = new Vector2(arrPosX[x, nStartY - o] - 1, arrPosY[x, nStartY - o]);
                        GameObject fog = Instantiate(fogPS);
                        fog.transform.parent = gridHolder.transform;
                        fog.transform.position = fogPos;
                    }
                }
                Debug.Log("3 Same Color : x = " + x + ", y = " + nStartY);
                return true;
            }
        }

        // check row
        nSameColorCount = 0;
        nOldColor = -1;
        int nStartX = -1;
        for (int y = 0; y < yIndex; ++y)
        {
            // Recalc
            nSameColorCount = 0;
            nOldColor = -1;
            nStartX = -1;
            for (int x = 0; x < xIndex; ++x)
            {
                TileScript tileScript = tiles[x, y].GetComponent<TileScript>();
                if (tileScript.IsCleaned() ||
                    (x == circleIndexX && y == circleIndexY))
                {
                    if (nSameColorCount >= 3)
                    {
                        break;
                    }
                    nSameColorCount = 0;
                    nOldColor = -1;
                    nStartX = -1;
                    continue;
                }
                int nColor = tileScript.returnColor();
                if (nColor == nOldColor)
                {
                    ++nSameColorCount;
                    nStartX = x;
                }
                else
                {
                    if (nSameColorCount >= 3)
                    {
                        break;
                    }
                    nSameColorCount = 1;
                    nOldColor = nColor;
                }
            }

            if (nSameColorCount >= 3)
            {

                // 消除这些连续的色块
                if (bClean)
                {
                    nCicleMoveTimes = 6;
                    score.SendMessage("addScore", nSameColorCount);
                    myAudio.PlayOneShot(getSameColorAudio);
                    for (int o = 0; o < nSameColorCount; ++o)
                    {
                        tiles[nStartX - o, y].GetComponent<TileScript>().Clean();
                        Vector2 fogPos = new Vector2(arrPosX[nStartX - o, y] - 1, arrPosY[nStartX - o, y]);
                        GameObject fog = Instantiate(fogPS);
                        fog.transform.parent = gridHolder.transform;
                        fog.transform.position = fogPos;
                    }
                }
                Debug.Log("3 Same Color : x = " + nStartX + ", y = " + y);
                return true;
            }
        }

        return false;
    }

    //如果六次移动之后还没有消除色块，则圆盘上的数字为0
    void checkMoveTimes() {
        if (checkSameColor() ==false && nCicleMoveTimes <= 0) {
            nCicleMoveTimes = 6;
            failPanel.SetActive(true);
            myAudio.PlayOneShot(failAudio);
            restartTheGame = true;
        }
    }

    void restart() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            restartTheGame = false;
        }
    }
}
