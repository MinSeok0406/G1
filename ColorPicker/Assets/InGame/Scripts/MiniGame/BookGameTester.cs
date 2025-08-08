using UnityEngine;

namespace ColorPicker.InGame
{
    public class BookGameTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private BookOrganizingMiniGame bookGame;
        
        private void Start()
        {
            if (bookGame != null)
            {
                bookGame.Initialize();
                
                bookGame.StartGame();
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
        }
        
        private void RestartGame()
        {
            if (bookGame != null)
            {
                Debug.Log("게임 재시작!");
                bookGame.Initialize();
                bookGame.StartGame();
            }
        }
    }
}
