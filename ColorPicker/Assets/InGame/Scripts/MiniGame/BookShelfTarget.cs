using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Collider2D))]
    public class BookShelfTarget : MonoBehaviour
    {
        [Header("Shelf Settings")]
        public int maxBooks = 5;
        
        [Header("UI Settings")]
        public Text countText;
        
        private List<BookItem> placedBooks = new List<BookItem>();
        private DragTriggerSensor triggerSensor;
        
        public System.Action<BookItem> OnBookPlaced;
        public int PlacedBookCount => placedBooks.Count;
        
        private void Awake()
        {
            triggerSensor = GetComponent<DragTriggerSensor>();
            if (triggerSensor == null)
            {
                triggerSensor = gameObject.AddComponent<DragTriggerSensor>();
            }
            
            triggerSensor.OnTriggerEntered += HandleTriggerEntered;
            
            UpdateCountUI();
        }
        
        private void HandleTriggerEntered(Collider2D other)
        {
            var book = other.GetComponent<BookItem>();
            if (book != null && !book.IsPlaced && placedBooks.Count < maxBooks)
            {
                PlaceBook(book);
            }
        }
        
        private void PlaceBook(BookItem book)
        {
            book.SetPlaced(true);
            placedBooks.Add(book);
            
            var draggable = book.GetComponent<DraggableObject>();
            if (draggable != null)
            {
                draggable.enabled = false;
            }
            
            book.gameObject.SetActive(false);
            
            Debug.Log($"책이 배치되었습니다: {book.name} ({placedBooks.Count}/{maxBooks})");
            
            UpdateCountUI();
            
            OnBookPlaced?.Invoke(book);
        }
        
        public bool IsFull()
        {
            return placedBooks.Count >= maxBooks;
        }

        public void ResetShelf()
        {
            placedBooks.Clear();
            UpdateCountUI();
        }
        
        private void UpdateCountUI()
        {
            if (countText != null)
            {
                countText.text = $"{placedBooks.Count}/{maxBooks}";
            }
        }
    }
}
