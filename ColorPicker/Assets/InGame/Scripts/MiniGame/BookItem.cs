using UnityEngine;

namespace ColorPicker.InGame
{
    public class BookItem : MonoBehaviour
    {
        [Header("Book Settings")]
        public Color bookColor = Color.white;
        
        private bool isPlaced = false;
        
        public bool IsPlaced => isPlaced;
        
        private void Start()
        {
            var image = GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = bookColor;
            }
        }
        
        public void SetPlaced(bool placed)
        {
            isPlaced = placed;
        }
        
        public void SetColor(Color color)
        {
            bookColor = color;
            var image = GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = bookColor;
            }
        }
    }
}
