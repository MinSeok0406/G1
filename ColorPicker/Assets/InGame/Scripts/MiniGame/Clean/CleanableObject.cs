using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class CleanableObject : MonoBehaviour
    {
        [SerializeField, Min(1)] private int alphaStepsCount = 3; 
        private List<float> alphaSteps;
        private Image sprite;
        private int currentStep = 0;

        public Action OnCleaned;

        private DragTriggerSensor dragTriggerSensor;

        private void Awake()
        {
            sprite = GetComponent<Image>();
            dragTriggerSensor = GetComponent<DragTriggerSensor>();

            GenerateAlphaSteps();

            Initialized();
        }

        private void OnEnable()
        {
            dragTriggerSensor.OnTriggerEntered += dragTriggerSensor_OnTriggerEntered;
        }
        private void OnDisable()
        {
            dragTriggerSensor.OnTriggerEntered -= dragTriggerSensor_OnTriggerEntered;
        }

        private void dragTriggerSensor_OnTriggerEntered(Collider2D collider)
        {
            Clean();
        }

        public void Initialized()
        {
            SetAlpha(alphaSteps[0]);
            currentStep = 0;
        }

        private void GenerateAlphaSteps()
        {
            alphaSteps = new List<float>(alphaStepsCount);
            alphaSteps.Clear();

            for (int i = 0; i < alphaStepsCount; i++)
            {
                float alpha = 1f - ((float)i / (alphaStepsCount));
                alphaSteps.Add(alpha);
            }

            alphaSteps.Add(0.0f);
        }

        public void Clean()
        {
            currentStep++;
            if (currentStep < alphaSteps.Count - 1)
            {
                UpdateAlpha();
            }
            else
            {
                SetAlpha(0.0f);
                OnCleaned?.Invoke();
                gameObject.SetActive(false);
            }
        }

        private void UpdateAlpha()
        {
            SetAlpha(alphaSteps[Mathf.Min(currentStep, alphaSteps.Count - 1)]);
        }

        public void SetAlpha(float alpha)
        {
            Color color = sprite.color;
            color.a = Mathf.Clamp01(alpha);
            sprite.color = color;
        }
    }
}