using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private int totalTime = 120;
    [SerializeField] private TextMeshProUGUI remainingTime;
    [SerializeField] private TextMeshProUGUI gameEndText;


    private void Update()
    {
        if(Time.time < totalTime)
        {
            remainingTime.text = totalTime - (int)Time.time + "s";
        }
        else
        {
            remainingTime.text = "0s";
            gameEndText.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }
    }


}
