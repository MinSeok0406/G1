using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public interface IHighlighter
    {
        void Apply(PhotonView view);
        void Clear();
    }

    public sealed class OutlineHighlighter : IHighlighter
    {
        private readonly Material _outlineMat;
        private PhotonView _currentView;
        private OutlineMarker _currentMarker;

        public OutlineHighlighter(Material outlineMat)
        {
            _outlineMat = outlineMat;
        }

        public void Apply(PhotonView view)
        {
            if (_outlineMat == null || !view || view == _currentView) return;

            Clear();

            _currentView = view;
            _currentMarker = OutlineMarker.GetOrAdd(view.gameObject);
            _currentMarker?.Apply(_outlineMat);
        }

        public void Clear()
        {
            _currentMarker?.Clear();
            _currentMarker = null;
            _currentView = null;
        }
    }
}
