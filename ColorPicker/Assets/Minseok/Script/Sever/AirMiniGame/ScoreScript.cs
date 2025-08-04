using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minseok
{
    public class ScoreScript : MonoBehaviour
    {
        public enum Score
        {
            AiScore, PlayerScore
        }

        public TMP_Text AiScoreTxt, PlayerScoreTxt;
        private int aiScore, playerScore;

        public void Increment(Score whichScore)
        {
            if (whichScore == Score.AiScore)
                AiScoreTxt.text = (++aiScore).ToString();
            else
                PlayerScoreTxt.text = (++playerScore).ToString();
        }

        public void GameOver()
        {
            if (AiScoreTxt.text == "3")
            {
                Debug.Log("AI ½Â¸®!!");
            }
            else if (PlayerScoreTxt.text == "3")
            {
                Debug.Log("ÇÃ·¹ÀÌ¾î ½Â¸®!!");
            }
        }
    }
}