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
    /// Central mode switch for strategic globe vs tactical battle.
    /// Camera and chunk systems subscribe to OnModeChanged.
    /// </summary>
    public sealed class GameModeController : MonoBehaviour
    {
        public static GameModeController Instance { get; private set; }

        [SerializeField] private GameMode _mode = GameMode.Strategic;
        [SerializeField] private float strategicMinAltitudeMeters = 50_000f;
        [SerializeField] private float tacticalMaxAltitudeMeters = 8_000f;

        public GameMode Mode => _mode;
        public event Action<GameMode, GameMode> OnModeChanged;

        public double FocusLatitude { get; private set; } = 38.8816;
        public double FocusLongitude { get; private set; } = -77.0910; // Arlington, VA default

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetFocus(double latitude, double longitude)
        {
            FocusLatitude = latitude;
            FocusLongitude = longitude;
        }

        public void SetMode(GameMode next)
        {
            if (_mode == next) return;
            var prev = _mode;
            _mode = next;
            OnModeChanged?.Invoke(prev, next);
            Debug.Log($"[Osm4x] Mode {prev} → {next} @ {FocusLatitude:F4},{FocusLongitude:F4}");
        }

        public void SuggestModeFromAltitude(float altitudeMeters)
        {
            if (_mode == GameMode.Transition) return;
            if (altitudeMeters >= strategicMinAltitudeMeters && _mode != GameMode.Strategic)
                SetMode(GameMode.Strategic);
        }

        public void BeginBattleAt(double latitude, double longitude)
        {
            SetFocus(latitude, longitude);
            SetMode(GameMode.Transition);
        }

        public void EnterTactical()
        {
            SetMode(GameMode.Tactical);
        }

        public void ReturnToStrategic()
        {
            SetMode(GameMode.Strategic);
        }
    }
}
