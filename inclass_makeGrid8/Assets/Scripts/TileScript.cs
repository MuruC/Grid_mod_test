using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int type;
    public Color[] colors;
    private bool bCleaned;
    private float fSpeed = 0.5f;
    private Vector2 vecDest;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(colors.Length);
        vecDest = new Vector2(-999, -999);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetSpeed()
    {
        return fSpeed;
    }

    public Vector2 GetDest()
    {
        return vecDest;
    }

    public void SetDest(float x, float y)
    {
        vecDest.x = x;
        vecDest.y = y;
    }

    public void SetColor(int rand) {
        if (bCleaned)
        {
            return;
        }
        type = rand;
        Color tmp = colors[type];
        GetComponent<SpriteRenderer>().color = tmp;
    }
    public void SetAlpha(int alpha)
    {
        if(bCleaned)
        {
            return;
        }
        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp.a = alpha;
        GetComponent<SpriteRenderer>().color = tmp;
    }
    public int returnColor() {
        return type;
    }
    public int returnAlpha()
    {
        Color tmp = GetComponent<SpriteRenderer>().color;
        return (int)tmp.a;
    }
    public void Clean()
    {
        SetAlpha(0);
        bCleaned = true;
    }
    public bool IsCleaned()
    {
        return bCleaned;
    }
}
