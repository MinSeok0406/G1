using UnityEngine;

namespace ColorPicker.InGame
{
    public class OrigamiTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private OrigamiGame origamiGame;
        
        private void Start()
        {
            if (origamiGame != null)
            {
                origamiGame.Initialize();
                origamiGame.StartGame();
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
        }
        
        private void RestartGame()
        {
            if (origamiGame != null)
            {
                Debug.Log("종이접기 게임 재시작!");
                origamiGame.Initialize();
                origamiGame.StartGame();
            }
        }
    }
}
