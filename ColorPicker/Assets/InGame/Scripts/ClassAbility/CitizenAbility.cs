using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class CitizenAbility : MonoBehaviour
    {
        [Header("Activate-only UI (optional)")]
        [SerializeField] private Button interactiveButton;
        [SerializeField] private Button paintButton;


        public void EnableAbility()
        {
            gameObject.SetActive(true);

            if (interactiveButton) interactiveButton.gameObject.SetActive(true);
            if (paintButton) paintButton.gameObject.SetActive(true);

        }

        public void DisableAbility()
        {

            if (interactiveButton) interactiveButton.gameObject.SetActive(false);
            if (paintButton) paintButton.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }
    }
}
