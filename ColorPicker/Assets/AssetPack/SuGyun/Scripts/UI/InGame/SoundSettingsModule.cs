using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorPicker.InGame.UI
{
    /// <summary>
    /// 사운드 설정 모듈 (전체 볼륨, 배경음, 효과음)
    /// </summary>
    public class SoundSettingsModule : MonoBehaviour
    {
        [Header("볼륨 슬라이더")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        
        [Header("볼륨 텍스트 (슬라이더 연동)")]
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private TextMeshProUGUI bgmVolumeText;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        
        // 볼륨 값들 (0.0f ~ 1.0f)
        private float masterVolume = 1.0f;
        private float bgmVolume = 1.0f;
        private float sfxVolume = 1.0f;
        
        // 오디오 소스 참조 (필요시 연결)
        [Header("오디오 소스 참조 (선택사항)")]
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioSource[] sfxAudioSources;
        
        public void Initialize()
        {
            LoadSettings();
            SetupSliderEvents();
            ApplyAllVolumes();
            UpdateAllTexts();
        }
        
        public void OnTabActivated()
        {
            UpdateAllTexts();
        }
        
        #region 설정 로드/저장
        
        private void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1.0f);
            bgmVolume = PlayerPrefs.GetFloat("Audio_BGMVolume", 1.0f);
            sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1.0f);
            
            // 슬라이더에 값 적용
            if (masterVolumeSlider) masterVolumeSlider.value = masterVolume;
            if (bgmVolumeSlider) bgmVolumeSlider.value = bgmVolume;
            if (sfxVolumeSlider) sfxVolumeSlider.value = sfxVolume;
        }
        
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("Audio_MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("Audio_BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("Audio_SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region 슬라이더 이벤트 설정
        
        private void SetupSliderEvents()
        {
            if (masterVolumeSlider) 
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (bgmVolumeSlider) 
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            if (sfxVolumeSlider) 
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        #endregion
        
        #region 볼륨 변경 이벤트
        
        public void OnMasterVolumeChanged(float value)
        {
            masterVolume = value;
            ApplyMasterVolume();
            UpdateMasterVolumeText();
            SaveSettings();
        }
        
        public void OnBGMVolumeChanged(float value)
        {
            bgmVolume = value;
            ApplyBGMVolume();
            UpdateBGMVolumeText();
            SaveSettings();
        }
        
        public void OnSFXVolumeChanged(float value)
        {
            sfxVolume = value;
            ApplySFXVolume();
            UpdateSFXVolumeText();
            SaveSettings();
        }
        
        #endregion
        
        #region 볼륨 적용
        
        private void ApplyAllVolumes()
        {
            ApplyMasterVolume();
            ApplyBGMVolume();
            ApplySFXVolume();
        }
        
        private void ApplyMasterVolume()
        {
            // Unity 마스터 볼륨 적용
            AudioListener.volume = masterVolume;
        }
        
        private void ApplyBGMVolume()
        {
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = bgmVolume * masterVolume;
            }
        }
        
        private void ApplySFXVolume()
        {
            if (sfxAudioSources != null)
            {
                foreach (var sfxSource in sfxAudioSources)
                {
                    if (sfxSource != null)
                    {
                        sfxSource.volume = sfxVolume * masterVolume;
                    }
                }
            }
        }
        
        #endregion
        
        #region 텍스트 업데이트
        
        private void UpdateAllTexts()
        {
            UpdateMasterVolumeText();
            UpdateBGMVolumeText();
            UpdateSFXVolumeText();
        }
        
        private void UpdateMasterVolumeText()
        {
            if (masterVolumeText)
                masterVolumeText.text = $"{(int)(masterVolume * 100)}%";
        }
        
        private void UpdateBGMVolumeText()
        {
            if (bgmVolumeText)
                bgmVolumeText.text = $"{(int)(bgmVolume * 100)}%";
        }
        
        private void UpdateSFXVolumeText()
        {
            if (sfxVolumeText)
                sfxVolumeText.text = $"{(int)(sfxVolume * 100)}%";
        }
        
        #endregion
        
        #region 공개 메서드 (코드에서 볼륨 조절용)
        
        /// <summary>
        /// 마스터 볼륨 설정 (0.0f ~ 1.0f)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            if (masterVolumeSlider) masterVolumeSlider.value = masterVolume;
            ApplyMasterVolume();
            UpdateMasterVolumeText();
            SaveSettings();
        }
        
        /// <summary>
        /// BGM 볼륨 설정 (0.0f ~ 1.0f)
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmVolumeSlider) bgmVolumeSlider.value = bgmVolume;
            ApplyBGMVolume();
            UpdateBGMVolumeText();
            SaveSettings();
        }
        
        /// <summary>
        /// SFX 볼륨 설정 (0.0f ~ 1.0f)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxVolumeSlider) sfxVolumeSlider.value = sfxVolume;
            ApplySFXVolume();
            UpdateSFXVolumeText();
            SaveSettings();
        }
        
        /// <summary>
        /// BGM 오디오 소스 설정
        /// </summary>
        public void SetBGMAudioSource(AudioSource audioSource)
        {
            bgmAudioSource = audioSource;
            ApplyBGMVolume();
        }
        
        /// <summary>
        /// SFX 오디오 소스들 설정
        /// </summary>
        public void SetSFXAudioSources(AudioSource[] audioSources)
        {
            sfxAudioSources = audioSources;
            ApplySFXVolume();
        }
        
        #endregion
        
        #region 프로퍼티
        
        public float MasterVolume => masterVolume;
        public float BGMVolume => bgmVolume;
        public float SFXVolume => sfxVolume;
        
        /// <summary>
        /// 실제 BGM 볼륨 (마스터 볼륨 적용됨)
        /// </summary>
        public float EffectiveBGMVolume => bgmVolume * masterVolume;
        
        /// <summary>
        /// 실제 SFX 볼륨 (마스터 볼륨 적용됨)
        /// </summary>
        public float EffectiveSFXVolume => sfxVolume * masterVolume;
        
        #endregion
    }
}
