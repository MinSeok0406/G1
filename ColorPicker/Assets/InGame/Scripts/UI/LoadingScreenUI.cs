using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class LoadingScreenUI : MonoBehaviour
    {
        [SerializeField] private List<Image> playerAssignImage;
        [SerializeField] private GameObject loadingBar;
        [SerializeField, Min(0)] private float fadeDuration = 0.5f;
        [SerializeField, Min(0)] private float showDuration = 2f;

        private Coroutine playing;

        void Awake()
        {
            HideAll();
        }

        public void ShowAssignScene(int sceneNum)
        {
            StartCoroutine(Co_ShowAssignScene(sceneNum));
        }

        private IEnumerator Co_ShowAssignScene(int sceneNum)
        {
            if (!TryGetImage(sceneNum, out var img)) yield break;

            HideAll();
            img.gameObject.SetActive(true);
            SetLoadingBar(false);

            yield return Fade(img, 0f, 1f, fadeDuration);

            yield return new WaitForSeconds(showDuration);

            yield return Fade(img, 1f, 0f, fadeDuration);

            img.gameObject.SetActive(false);
            playing = null;

            gameObject.SetActive(false);
        }

        private IEnumerator Fade(Image img, float from, float to, float duration)
        {
            if (!img) yield break;

            SetAlpha(img, from);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, duration > 0f ? t / duration : 1f);
                SetAlpha(img, alpha);
                yield return null;
            }
            SetAlpha(img, to);
        }

        private void HideAll()
        {
            if (playerAssignImage == null) return;

            for (int i = 0; i < playerAssignImage.Count; i++)
            {
                var img = playerAssignImage[i];
                if (!img) continue;

                var c = img.color;
                c.a = 0f;
                img.color = c;
                img.raycastTarget = false;
                img.gameObject.SetActive(false);
            }
        }

        private void SetAlpha(Image img, float a)
        {
            var c = img.color;
            c.a = a;
            img.color = c;
        }

        private bool TryGetImage(int index, out Image img)
        {
            img = null;
            if (playerAssignImage == null || index < 0 || index >= playerAssignImage.Count) return false;
            img = playerAssignImage[index];
            return img != null;
        }

        private void SetLoadingBar(bool isActive)
        {
            loadingBar.SetActive(isActive);
        }
    }
}
