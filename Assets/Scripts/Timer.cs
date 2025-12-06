
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private int totalTime = 120;
    [SerializeField] private TextMeshProUGUI remainingTime;
    [SerializeField] private TextMeshProUGUI gameEndText;
    [SerializeField] private CharacterMovement player;
    
   
    // if time ends display game over
    private void Update()
    {
        if(Time.time < totalTime)
        {
            remainingTime.text = totalTime - (int)Time.time + "s";
        }
        else
        {
            remainingTime.text = "0s";
            if(player.collectedCount != player.totalCollectableCount)
            {
                gameEndText.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
                
            
        }
    }


}
