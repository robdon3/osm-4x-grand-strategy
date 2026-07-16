using System;
using UnityEngine;

namespace Osm4x.Core
{
    public enum GameMode
    {
        Strategic,
        Transition,
        Tactical
    }

    /// <summary>
    /// Strategic map vs tactical battle. Focus is world XZ (not GPS).
    /// </summary>
    public sealed class GameModeController : MonoBehaviour
    {
        public static GameModeController Instance { get; private set; }

        [SerializeField] private GameMode _mode = GameMode.Strategic;

        public GameMode Mode => _mode;
        public event Action<GameMode, GameMode> OnModeChanged;

        public Vector3 FocusWorld { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            FocusWorld = Vector3.zero;
        }

        public void SetFocus(Vector3 worldPosition)
        {
            FocusWorld = worldPosition;
        }

        public void SetMode(GameMode next)
        {
            if (_mode == next) return;
            var prev = _mode;
            _mode = next;
            OnModeChanged?.Invoke(prev, next);
            Debug.Log($"[Proc4x] Mode {prev} → {next} @ {FocusWorld}");
        }

        public void BeginBattleAt(Vector3 worldPosition)
        {
            SetFocus(worldPosition);
            SetMode(GameMode.Transition);
        }

        public void EnterTactical() => SetMode(GameMode.Tactical);
        public void ReturnToStrategic() => SetMode(GameMode.Strategic);
    }
}
