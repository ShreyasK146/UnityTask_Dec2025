using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private int totalTime = 10;
    [SerializeField] private TextMeshProUGUI remainingTime;
    [SerializeField] private TextMeshProUGUI gameEndText;
    [SerializeField] private CharacterMovement player;
   

    private void Update()
    {
        if(Time.time < totalTime)
        {
            remainingTime.text = totalTime - (int)Time.time + "s";
        }
        else
        {
            remainingTime.text = "0s";
            if(player.totalCollectableCount != 0)
            {
                gameEndText.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
                
            
        }
    }


}
