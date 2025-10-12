using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame.UI
{
    public class GameplaySettingsModule : MonoBehaviour
    {
        [Header("게임플레이 스위치 버튼")]
        [SerializeField] private Button bloodPhobiaSwitch;
        [SerializeField] private Button organPhobiaSwitch;
        [SerializeField] private Button colorTextModeSwitch;
        
        [Header("스위치 이미지들")]
        [SerializeField] private Image bloodPhobiaSwitchImage;
        [SerializeField] private Image organPhobiaSwitchImage;
        [SerializeField] private Image colorTextModeSwitchImage;
        
        [Header("스위치 색상")]
        [SerializeField] private Color switchOnColor = Color.green;
        [SerializeField] private Color switchOffColor = Color.gray;
        
        private bool bloodPhobiaEnabled = false;
        private bool organPhobiaEnabled = false;
        private bool colorTextModeEnabled = false;
        
        public void Initialize()
        {
            LoadSettings();
            SetupSwitchEvents();
            ApplyAllSettings();
        }
        
        public void OnTabActivated() { }
        
        private void LoadSettings()
        {
            bloodPhobiaEnabled = PlayerPrefs.GetInt("Gameplay_BloodPhobia", 0) == 1;
            organPhobiaEnabled = PlayerPrefs.GetInt("Gameplay_OrganPhobia", 0) == 1;
            colorTextModeEnabled = PlayerPrefs.GetInt("Gameplay_ColorTextMode", 0) == 1;
            
            UpdateSwitchVisual(bloodPhobiaSwitchImage, bloodPhobiaEnabled);
            UpdateSwitchVisual(organPhobiaSwitchImage, organPhobiaEnabled);
            UpdateSwitchVisual(colorTextModeSwitchImage, colorTextModeEnabled);
        }
        
        public void SaveSettings()
        {
            PlayerPrefs.SetInt("Gameplay_BloodPhobia", bloodPhobiaEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Gameplay_OrganPhobia", organPhobiaEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Gameplay_ColorTextMode", colorTextModeEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void SetupSwitchEvents()
        {
            if (bloodPhobiaSwitch)
                bloodPhobiaSwitch.onClick.AddListener(() => OnBloodPhobiaSwitchClicked());
            if (organPhobiaSwitch)
                organPhobiaSwitch.onClick.AddListener(() => OnOrganPhobiaSwitchClicked());
            if (colorTextModeSwitch)
                colorTextModeSwitch.onClick.AddListener(() => OnColorTextModeSwitchClicked());
        }
        
        private void UpdateSwitchVisual(Image switchImage, bool isOn)
        {
            if (switchImage != null)
                switchImage.color = isOn ? switchOnColor : switchOffColor;
        }
        
        public void OnBloodPhobiaSwitchClicked()
        {
            bloodPhobiaEnabled = !bloodPhobiaEnabled;
            UpdateSwitchVisual(bloodPhobiaSwitchImage, bloodPhobiaEnabled);
            ApplyBloodPhobiaSetting();
            SaveSettings();
        }
        
        public void OnOrganPhobiaSwitchClicked()
        {
            organPhobiaEnabled = !organPhobiaEnabled;
            UpdateSwitchVisual(organPhobiaSwitchImage, organPhobiaEnabled);
            ApplyOrganPhobiaSetting();
            SaveSettings();
        }
        
        public void OnColorTextModeSwitchClicked()
        {
            colorTextModeEnabled = !colorTextModeEnabled;
            UpdateSwitchVisual(colorTextModeSwitchImage, colorTextModeEnabled);
            ApplyColorTextModeSetting();
            SaveSettings();
        }
        
        private void ApplyAllSettings()
        {
            ApplyBloodPhobiaSetting();
            ApplyOrganPhobiaSetting();
            ApplyColorTextModeSetting();
        }
        
        private void ApplyBloodPhobiaSetting()
        {
            // 피공포증 설정 적용 - 게임 로직에서 처리
        }
        
        private void ApplyOrganPhobiaSetting()
        {
            // 장기 공포증 설정 적용 - 게임 로직에서 처리
        }
        
        private void ApplyColorTextModeSetting()
        {
            // 색 글자 표시 모드 적용 - 게임 로직에서 처리
        }
        
        public void SetBloodPhobiaMode(bool enabled)
        {
            bloodPhobiaEnabled = enabled;
            UpdateSwitchVisual(bloodPhobiaSwitchImage, enabled);
            ApplyBloodPhobiaSetting();
            SaveSettings();
        }
        
        public void SetOrganPhobiaMode(bool enabled)
        {
            organPhobiaEnabled = enabled;
            UpdateSwitchVisual(organPhobiaSwitchImage, enabled);
            ApplyOrganPhobiaSetting();
            SaveSettings();
        }
        
        public void SetColorTextMode(bool enabled)
        {
            colorTextModeEnabled = enabled;
            UpdateSwitchVisual(colorTextModeSwitchImage, enabled);
            ApplyColorTextModeSetting();
            SaveSettings();
        }
        
        // Inspector 연결용
        public void OnBloodPhobiaSwitchClickedPublic() => OnBloodPhobiaSwitchClicked();
        public void OnOrganPhobiaSwitchClickedPublic() => OnOrganPhobiaSwitchClicked();
        public void OnColorTextModeSwitchClickedPublic() => OnColorTextModeSwitchClicked();
        
        // 프로퍼티
        public bool BloodPhobiaEnabled => bloodPhobiaEnabled;
        public bool OrganPhobiaEnabled => organPhobiaEnabled;
        public bool ColorTextModeEnabled => colorTextModeEnabled;
    }
}
