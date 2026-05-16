using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [Header("Кнопка Начать игру")]
        [SerializeField] private Button _playButton;

        [Header("Кнопка Победителей")]
        [SerializeField] private Button _winnersButton;

        [Header("Кнопка Выход из игры")]
        [SerializeField] private Button _exitButton;

        private ISceneNavigator _navigator;

        private void Start()
        {
            Debug.Log("[MainMenuView] Start");
            if (_playButton  == null) Debug.LogError("[MainMenuView] _playButton == null!");
            if (_winnersButton == null) Debug.LogError("[MainMenuView] _winnersButton == null!");
            if (_exitButton  == null) Debug.LogError("[MainMenuView] _exitButton == null!");

            try { _navigator = GameBootstrapper.Services.Resolve<ISceneNavigator>(); }
            catch (System.Exception e) { Debug.LogError($"[MainMenuView] ОШИБКА Resolve ISceneNavigator: {e.Message}"); return; }

            _playButton.onClick.AddListener(OnPlay);
            _winnersButton.onClick.AddListener(OnWinners);
            _exitButton.onClick.AddListener(OnExit);
            Debug.Log("[MainMenuView] ✓ Start завершён");
        }

        private void OnPlay()  { Debug.Log("[MainMenuView] Нажато: Начать игру"); _navigator.GoToTeamSetup(); }
        private void OnWinners() { Debug.Log("[MainMenuView] Нажато: Победители"); _navigator.GoToWinners(); }
        private void OnExit()  { Debug.Log("[MainMenuView] Нажато: Выход"); Application.Quit(); }

        private void OnDestroy()
        {
            Debug.Log("[MainMenuView] OnDestroy");
            if (_playButton)  _playButton.onClick.RemoveListener(OnPlay);
            if (_winnersButton) _winnersButton.onClick.RemoveListener(OnWinners);
            if (_exitButton)  _exitButton.onClick.RemoveListener(OnExit);
        }
    }
}
