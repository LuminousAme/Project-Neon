using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    [SerializeField] private TMP_Text score;
    private int scoreNum;

    // Start is called before the first frame update
    void Start()
    {
        score.text = "Score: 0";
        scoreNum = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddScore(100); 
        }   
    }

    public void AddScore(int amount)
    {
        scoreNum = scoreNum + amount;
        score.text = "Score: "+ scoreNum.ToString();
    }


}
