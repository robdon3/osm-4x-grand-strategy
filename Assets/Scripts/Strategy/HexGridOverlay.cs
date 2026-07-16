using Osm4x.Core;
using UnityEngine;

namespace Osm4x.Strategy
{
    /// <summary>
    /// Placeholder for strategic hex/tile overlay. Phase 1 can drive a
    /// procedural grid shader or instanced quads projected on the globe.
    /// </summary>
    public sealed class HexGridOverlay : MonoBehaviour
    {
        [SerializeField] private bool visibleInStrategicOnly = true;
        [SerializeField] private float hexSizeMeters = 50_000f;
        [SerializeField] private Color gridColor = new(0.2f, 0.8f, 1f, 0.25f);

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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gridColor;
            Gizmos.DrawWireSphere(transform.position, hexSizeMeters * 0.001f);
        }
    }
}
