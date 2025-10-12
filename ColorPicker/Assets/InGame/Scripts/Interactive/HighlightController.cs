using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// SpriteRenderer면 sharedMaterial 스왑,
    /// Mesh/SkinnedMesh면 OutlineMarker로 전 슬롯 outline 적용.
    /// </summary>
    [DisallowMultipleComponent]
    public class HighlightController : MonoBehaviour
    {
        [Header("Optional override (비우면 GameResources 폴백)")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Material defaultSpriteMaterial;

        private SpriteRenderer _sprite;
        private OutlineMarker _outline;
        private Material _cachedDefaultSpriteMat;
        private bool _applied;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _outline = OutlineMarker.GetOrAdd(gameObject);

            if (_sprite != null)
                _cachedDefaultSpriteMat = _sprite.sharedMaterial; // 인스턴스 생성 방지
        }

        public void SetHighlighted(bool active)
        {
            if (_applied == active) return;

            var outlineMat = outlineMaterial ?? GameResources.Instance?.outlineMaterial;
            var defaultMat = defaultSpriteMaterial ?? _cachedDefaultSpriteMat ?? GameResources.Instance?.interactiveMaterial;

            if (TrySpriteMode(outlineMat, defaultMat, active))
            {
                _applied = active;
                return;
            }

            if (_outline != null)
            {
                if (active) _outline.Apply(outlineMat);
                else _outline.Clear();

                _applied = active;
            }
        }

        private bool TrySpriteMode(Material outlineMat, Material defaultMat, bool active)
        {
            if (_sprite == null) return false;

            if (active)
            {
                if (outlineMat != null) _sprite.sharedMaterial = outlineMat;
            }
            else
            {
                if (defaultMat != null) _sprite.sharedMaterial = defaultMat;
            }
            return true;
        }
    }
}
