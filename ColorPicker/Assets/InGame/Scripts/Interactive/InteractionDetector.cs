using ColorPicker.InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class InteractionDetector : MonoBehaviour
    {
        private List<IInteractive> nearby = new();
        private PlayerControl player;

        private void Awake()
        {
            player = GetComponent<PlayerControl>();
        }

        public void AddInteractable(IInteractive target)
        {
            if (!nearby.Contains(target))
            {
                nearby.Add(target);
            }

            UpdateClosest();
        }

        public void RemoveInteractable(IInteractive target)
        {
            if (nearby.Contains(target))
            {
                target.ToggleHighlight(false);
                nearby.Remove(target);
            }
            UpdateClosest();
        }

        private void UpdateClosest()
        {
            IInteractive closest = null;
            float minDist = float.MaxValue;

            foreach (var obj in nearby)
            {
                float dist = Vector3.Distance(obj.GetPosition(), transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = obj;
                }
            }

            foreach (var obj in nearby)
            {
                obj.ToggleHighlight(false);
            }

            closest?.ToggleHighlight(true);

            player.currentInteractive = closest;

            if (closest != null)
            {
                UIManager.Instance.ShowInteractionButton(true, () =>
                {
                    player.TryInteract();
                });
            }
            else
            {
                UIManager.Instance.ShowInteractionButton(false);
            }
        }
    }
}