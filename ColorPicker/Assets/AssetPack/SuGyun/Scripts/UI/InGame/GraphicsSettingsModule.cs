using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorPicker.InGame.UI
{
    /// <summary>
    /// 그래픽 설정 모듈 (텍스처, 그림자, 안티앨리어싱, 필터링)
    /// </summary>
    public class GraphicsSettingsModule : MonoBehaviour
    {
        [Header("텍스처 품질 버튼")]
        [SerializeField] private Button textureLowBT;
        [SerializeField] private Button textureMiddleBT;
        [SerializeField] private Button textureHighBT;
        
        [Header("그림자 품질 버튼")]
        [SerializeField] private Button shadowLowBT;
        [SerializeField] private Button shadowMiddleBT;
        [SerializeField] private Button shadowHighBT;
        
        [Header("안티앨리어싱 버튼")]
        [SerializeField] private Button antiAliasingLowBT;
        [SerializeField] private Button antiAliasingMiddleBT;
        [SerializeField] private Button antiAliasingHighBT;
        
        [Header("필터링 버튼")]
        [SerializeField] private Button filteringLowBT;
        [SerializeField] private Button filteringMiddleBT;
        [SerializeField] private Button filteringHighBT;
        
        [Header("버튼 색상")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;
        
        [Header("텍스트 색상")]
        [SerializeField] private Color activeTextColor = Color.black;
        [SerializeField] private Color inactiveTextColor = Color.white;
        
        // 설정값들 (0: Low, 1: Middle, 2: High)
        private int textureQuality = 1;
        private int shadowQuality = 1;
        private int antiAliasingLevel = 1;
        private int filteringLevel = 1;
        
        public void Initialize()
        {
            LoadSettings();
            SetupButtonEvents();
            ApplyAllSettings();
            UpdateAllButtonColors();
        }
        
        public void OnTabActivated()
        {
            UpdateAllButtonColors();
        }
        
        #region 설정 로드/저장
        
        private void LoadSettings()
        {
            textureQuality = PlayerPrefs.GetInt("Graphics_Texture", 1);
            shadowQuality = PlayerPrefs.GetInt("Graphics_Shadow", 1);
            antiAliasingLevel = PlayerPrefs.GetInt("Graphics_AntiAliasing", 1);
            filteringLevel = PlayerPrefs.GetInt("Graphics_Filtering", 1);
        }
        
        public void SaveSettings()
        {
            PlayerPrefs.SetInt("Graphics_Texture", textureQuality);
            PlayerPrefs.SetInt("Graphics_Shadow", shadowQuality);
            PlayerPrefs.SetInt("Graphics_AntiAliasing", antiAliasingLevel);
            PlayerPrefs.SetInt("Graphics_Filtering", filteringLevel);
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region 버튼 이벤트 설정
        
        private void SetupButtonEvents()
        {
            // 텍스처 품질 버튼
            if (textureLowBT) textureLowBT.onClick.AddListener(() => SetTextureQuality(0));
            if (textureMiddleBT) textureMiddleBT.onClick.AddListener(() => SetTextureQuality(1));
            if (textureHighBT) textureHighBT.onClick.AddListener(() => SetTextureQuality(2));
            
            // 그림자 품질 버튼
            if (shadowLowBT) shadowLowBT.onClick.AddListener(() => SetShadowQuality(0));
            if (shadowMiddleBT) shadowMiddleBT.onClick.AddListener(() => SetShadowQuality(1));
            if (shadowHighBT) shadowHighBT.onClick.AddListener(() => SetShadowQuality(2));
            
            // 안티앨리어싱 버튼
            if (antiAliasingLowBT) antiAliasingLowBT.onClick.AddListener(() => SetAntiAliasing(0));
            if (antiAliasingMiddleBT) antiAliasingMiddleBT.onClick.AddListener(() => SetAntiAliasing(1));
            if (antiAliasingHighBT) antiAliasingHighBT.onClick.AddListener(() => SetAntiAliasing(2));
            
            // 필터링 버튼
            if (filteringLowBT) filteringLowBT.onClick.AddListener(() => SetFiltering(0));
            if (filteringMiddleBT) filteringMiddleBT.onClick.AddListener(() => SetFiltering(1));
            if (filteringHighBT) filteringHighBT.onClick.AddListener(() => SetFiltering(2));
        }
        
        #endregion
        
        #region 텍스처 품질 설정
        
        public void SetTextureQuality(int level)
        {
            textureQuality = Mathf.Clamp(level, 0, 2);
            ApplyTextureQuality();
            UpdateTextureButtons();
            SaveSettings();
        }
        
        private void ApplyTextureQuality()
        {
            QualitySettings.globalTextureMipmapLimit = 2 - textureQuality; // 반전 (0=High, 2=Low)
        }
        
        private void UpdateTextureButtons()
        {
            SetButtonStyle(textureLowBT, textureQuality == 0 ? activeColor : inactiveColor, textureQuality == 0 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(textureMiddleBT, textureQuality == 1 ? activeColor : inactiveColor, textureQuality == 1 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(textureHighBT, textureQuality == 2 ? activeColor : inactiveColor, textureQuality == 2 ? activeTextColor : inactiveTextColor);
        }
        
        #endregion
        
        #region 그림자 품질 설정
        
        public void SetShadowQuality(int level)
        {
            shadowQuality = Mathf.Clamp(level, 0, 2);
            ApplyShadowQuality();
            UpdateShadowButtons();
            SaveSettings();
        }
        
        private void ApplyShadowQuality()
        {
            switch (shadowQuality)
            {
                case 0: QualitySettings.shadows = UnityEngine.ShadowQuality.Disable; break;
                case 1: QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly; break;
                case 2: QualitySettings.shadows = UnityEngine.ShadowQuality.All; break;
            }
        }
        
        private void UpdateShadowButtons()
        {
            SetButtonStyle(shadowLowBT, shadowQuality == 0 ? activeColor : inactiveColor, shadowQuality == 0 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(shadowMiddleBT, shadowQuality == 1 ? activeColor : inactiveColor, shadowQuality == 1 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(shadowHighBT, shadowQuality == 2 ? activeColor : inactiveColor, shadowQuality == 2 ? activeTextColor : inactiveTextColor);
        }
        
        #endregion
        
        #region 안티앨리어싱 설정
        
        public void SetAntiAliasing(int level)
        {
            antiAliasingLevel = Mathf.Clamp(level, 0, 2);
            ApplyAntiAliasing();
            UpdateAntiAliasingButtons();
            SaveSettings();
        }
        
        private void ApplyAntiAliasing()
        {
            int[] samples = { 0, 2, 8 }; // Low: 0, Middle: 2x, High: 8x
            QualitySettings.antiAliasing = samples[antiAliasingLevel];
        }
        
        private void UpdateAntiAliasingButtons()
        {
            SetButtonStyle(antiAliasingLowBT, antiAliasingLevel == 0 ? activeColor : inactiveColor, antiAliasingLevel == 0 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(antiAliasingMiddleBT, antiAliasingLevel == 1 ? activeColor : inactiveColor, antiAliasingLevel == 1 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(antiAliasingHighBT, antiAliasingLevel == 2 ? activeColor : inactiveColor, antiAliasingLevel == 2 ? activeTextColor : inactiveTextColor);
        }
        
        #endregion
        
        #region 필터링 설정
        
        public void SetFiltering(int level)
        {
            filteringLevel = Mathf.Clamp(level, 0, 2);
            ApplyFiltering();
            UpdateFilteringButtons();
            SaveSettings();
        }
        
        private void ApplyFiltering()
        {
            switch (filteringLevel)
            {
                case 0: QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable; break;
                case 1: QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable; break;
                case 2: QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable; break;
            }
        }
        
        private void UpdateFilteringButtons()
        {
            SetButtonStyle(filteringLowBT, filteringLevel == 0 ? activeColor : inactiveColor, filteringLevel == 0 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(filteringMiddleBT, filteringLevel == 1 ? activeColor : inactiveColor, filteringLevel == 1 ? activeTextColor : inactiveTextColor);
            SetButtonStyle(filteringHighBT, filteringLevel == 2 ? activeColor : inactiveColor, filteringLevel == 2 ? activeTextColor : inactiveTextColor);
        }
        
        #endregion
        
        #region 공통 메서드
        
        private void ApplyAllSettings()
        {
            ApplyTextureQuality();
            ApplyShadowQuality();
            ApplyAntiAliasing();
            ApplyFiltering();
        }
        
        private void UpdateAllButtonColors()
        {
            UpdateTextureButtons();
            UpdateShadowButtons();
            UpdateAntiAliasingButtons();
            UpdateFilteringButtons();
        }
        
        private void SetButtonStyle(Button button, Color backgroundColor, Color textColor)
        {
            if (button != null)
            {
                // 버튼 배경 색상 변경
                var colors = button.colors;
                colors.normalColor = backgroundColor;
                button.colors = colors;
                
                // 버튼 텍스트 색상 변경
                TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.color = textColor;
                }
            }
        }
        
        #endregion
        
        #region Inspector 연결용 공개 메서드
        
        // 텍스처 품질
        public void OnTextureLowClicked() => SetTextureQuality(0);
        public void OnTextureMiddleClicked() => SetTextureQuality(1);
        public void OnTextureHighClicked() => SetTextureQuality(2);
        
        // 그림자 품질
        public void OnShadowLowClicked() => SetShadowQuality(0);
        public void OnShadowMiddleClicked() => SetShadowQuality(1);
        public void OnShadowHighClicked() => SetShadowQuality(2);
        
        // 안티앨리어싱
        public void OnAntiAliasingLowClicked() => SetAntiAliasing(0);
        public void OnAntiAliasingMiddleClicked() => SetAntiAliasing(1);
        public void OnAntiAliasingHighClicked() => SetAntiAliasing(2);
        
        // 필터링
        public void OnFilteringLowClicked() => SetFiltering(0);
        public void OnFilteringMiddleClicked() => SetFiltering(1);
        public void OnFilteringHighClicked() => SetFiltering(2);
        
        #endregion
        
        #region 프로퍼티
        
        public int TextureQuality => textureQuality;
        public int ShadowQuality => shadowQuality;
        public int AntiAliasingLevel => antiAliasingLevel;
        public int FilteringLevel => filteringLevel;
        
        #endregion
    }
}
