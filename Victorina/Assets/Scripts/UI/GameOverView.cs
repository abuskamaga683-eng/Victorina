using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class GameOverView : MonoBehaviour
    {
        [Header("Текст с именем победителя")]
        [SerializeField] private TMP_Text _winnerText;

        [Header("Текст с итоговыми очками всех команд")]
        [SerializeField] private TMP_Text _scoresText;

        [Header("Кнопка В главное меню")]
        [SerializeField] private Button _mainMenuButton;

        private ISessionService _session;
        private ISceneNavigator _navigator;
        private bool            _ready;

        private void Start()
        {
            Debug.Log("[GameOverView] Start");
            if (_winnerText     == null) Debug.LogError("[GameOverView] _winnerText == null!");
            if (_scoresText     == null) Debug.LogError("[GameOverView] _scoresText == null!");
            if (_mainMenuButton == null) Debug.LogError("[GameOverView] _mainMenuButton == null!");

            try { _session   = GameBootstrapper.Services.Resolve<ISessionService>(); }
            catch (System.Exception e) { Debug.LogError($"[GameOverView] ОШИБКА Resolve ISessionService: {e.Message}"); }
            try { _navigator = GameBootstrapper.Services.Resolve<ISceneNavigator>(); }
            catch (System.Exception e) { Debug.LogError($"[GameOverView] ОШИБКА Resolve ISceneNavigator: {e.Message}"); }

            _mainMenuButton.onClick.AddListener(OnMainMenu);
            Victorina.Services.SceneNavigator.OnGameOverShown += Refresh;
            _ready = true;
            Debug.Log("[GameOverView] ✓ Start завершён");
            // НЕ вызываем Refresh() в Start — сессия ещё не начата
        }

        private void OnEnable()
        {
            Debug.Log("[GameOverView] OnEnable");
            if (_ready) Refresh();
        }

        private void Refresh()
        {
            Debug.Log("[GameOverView] Refresh");

            if (_session == null)
            {
                Debug.LogWarning("[GameOverView] Refresh: _session == null, пропускаем");
                return;
            }

            if (_session.Teams == null || _session.Teams.Count == 0)
            {
                Debug.LogWarning("[GameOverView] Refresh: команд нет, пропускаем");
                _winnerText.text = "";
                _scoresText.text = "";
                return;
            }

            try
            {
                var winner = _session.GetWinner();
                _winnerText.text = winner != null ? $"Победитель: {winner.Name}!" : "Ничья!";
                _scoresText.text = string.Join("\n",
                    _session.Teams
                        .OrderByDescending(t => t.Score)
                        .Select(t => $"{t.Name}  —  {t.Score} очков"));
                Debug.Log($"[GameOverView] ✓ Победитель: {winner?.Name}, команд: {_session.Teams.Count}");
            }
            catch (System.Exception e) { Debug.LogError($"[GameOverView] ОШИБКА Refresh: {e.Message}"); }
        }

        private void OnMainMenu()
        {
            Debug.Log("[GameOverView] OnMainMenu → Reset + GoToMainMenu");
            _session.Reset();
            _navigator.GoToMainMenu();
        }

        private void OnDestroy()
        {
            Debug.Log("[GameOverView] OnDestroy");
            if (_mainMenuButton) _mainMenuButton.onClick.RemoveListener(OnMainMenu);
            Victorina.Services.SceneNavigator.OnGameOverShown -= Refresh;
        }
    }
}
