using Osm4x.Core;
using UnityEngine;

namespace Osm4x.Strategy
{
    public sealed class HexGridOverlay : MonoBehaviour
    {
        [SerializeField] private bool visibleInStrategicOnly = true;

        private void OnEnable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged += OnMode;
        }

        private void OnDisable()
        {
            if (GameModeController.Instance != null)
                GameModeController.Instance.OnModeChanged -= OnMode;
        }

        private void OnMode(GameMode prev, GameMode next)
        {
            if (!visibleInStrategicOnly) return;
            gameObject.SetActive(next == GameMode.Strategic);
        }
    }
}
