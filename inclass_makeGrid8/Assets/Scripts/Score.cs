using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Score : MonoBehaviour
{
    public Text scoreText;
int score;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("score: " + score);
        scoreText.GetComponent<Text>().text = "Score: " + score;
    }
    public void addScore(int nScore) {
        //Debug.Log(nScore);
        score += nScore;
    }
}
