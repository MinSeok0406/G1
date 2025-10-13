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

        public OutlineHighlighter(Material outlineMat)
        {
            _outlineMat = outlineMat;
        }

        public void Apply(PhotonView view)
        {
            if (_outlineMat == null || !view || view == _currentView) return;

            Clear();

            _currentView = view;
        }

        public void Clear()
        {
            _currentView = null;
        }
    }
}
