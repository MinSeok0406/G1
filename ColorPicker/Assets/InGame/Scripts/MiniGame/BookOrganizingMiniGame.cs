using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(MiniGameTag))]
    public class BookOrganizingMiniGame : MiniGameBase
    {
        [Header("Game Settings")]
        [SerializeField] private BookItem[] books;
        [SerializeField] private BookShelfTarget bookShelf;
        [SerializeField] private int targetBooks = 5;
        
        private int playerId;
        private MiniGameTag miniGameTag;
        
        private void Awake()
        {
            miniGameTag = GetComponent<MiniGameTag>();
        }
        
        public override void Initialize()
        {
            // 책장 리셋
            if (bookShelf != null)
            {
                bookShelf.ResetShelf();
                bookShelf.OnBookPlaced += OnBookPlaced;
            }
            
            // 책들 초기화
            if (books != null)
            {
                foreach (var book in books)
                {
                    book.SetPlaced(false);
                    book.gameObject.SetActive(true);
                    
                    var draggable = book.GetComponent<DraggableObject>();
                    if (draggable != null)
                    {
                        draggable.enabled = true;
                    }
                }
            }
        }
        
        public override void StartGame()
        {
            Debug.Log("책 정리 미니게임 시작!");
        }
        
        private void OnBookPlaced(BookItem book)
        {
            Debug.Log($"책 배치됨: {book.name}");
            
            // 목표 달성 확인
            if (bookShelf != null && bookShelf.PlacedBookCount >= targetBooks)
            {
                CompleteGame(true);
            }
        }
        
        private void CompleteGame(bool success)
        {
            Debug.Log($"게임 완료: {(success ? "성공" : "실패")}");
            
            var report = new MiniGameReport
            {
                playerId = playerId,
                miniGameType = MiniGameType.BookOrganizing,
                success = success
            };
            
            onComplete?.Invoke(report);
        }
        
        public void SetPlayerId(int id)
        {
            playerId = id;
        }
        
        private void OnDestroy()
        {
            if (bookShelf != null)
            {
                bookShelf.OnBookPlaced -= OnBookPlaced;
            }
        }
    }
}
