using System;
using UnityEngine;
using Victorina.Core.Interfaces;

namespace Victorina.Services
{
    public class SceneNavigator : ISceneNavigator
    {
        private readonly GameObject _mainMenu;
        private readonly GameObject _teamSetup;
        private readonly GameObject _game;
        private readonly GameObject _admin;
        private readonly GameObject _winners;
        private readonly GameObject _gameOver;

        /// <summary>
        /// Статические события — скрипты на отдельных объектах подписываются в Start().
        /// Вызываются каждый раз при переходе на соответствующий экран.
        /// </summary>
        public static event Action OnMainMenuShown;
        public static event Action OnTeamSetupShown;
        public static event Action OnGameShown;
        public static event Action OnAdminShown;
        public static event Action OnGameOverShown;
        public static event Action OnWinnersShown;

        public SceneNavigator(GameObject mainMenu, GameObject teamSetup, 
            GameObject game, GameObject admin, GameObject winners, GameObject gameOver)
        {
            _mainMenu  = mainMenu;
            _teamSetup = teamSetup;
            _game      = game;
            _admin     = admin;
            _winners   = winners;
            _gameOver  = gameOver;
            Debug.Log($"[Navigator] Создан. mainMenu={mainMenu?.name} teamSetup={teamSetup?.name} game={game?.name} admin={admin?.name} gameOver={gameOver?.name}");
        }

        public void GoToMainMenu()  { Debug.Log("[Navigator] → GoToMainMenu");  Show(_mainMenu,  OnMainMenuShown); }
        public void GoToTeamSetup() { Debug.Log("[Navigator] → GoToTeamSetup"); Show(_teamSetup, OnTeamSetupShown); }
        public void GoToGame()      { Debug.Log("[Navigator] → GoToGame");      Show(_game,      OnGameShown); }
        public void GoToAdmin()     { Debug.Log("[Navigator] → GoToAdmin");     Show(_admin,     OnAdminShown); }
        public void GoToWinners()   { Debug.Log("[Navigator] → GoToWinners");   Show(_winners, OnWinnersShown); }
        public void GoToGameOver()  { Debug.Log("[Navigator] → GoToGameOver");  Show(_gameOver,  OnGameOverShown); }

        private void Show(GameObject target, Action onShown)
        {
            if (target == null) { Debug.LogError("[Navigator] ОШИБКА: target == null в Show()"); return; }
            Debug.Log($"[Navigator] Show({target.name}) — скрываю остальные");

            // Активируем цель (на случай если панель стартовала неактивной)
            target.SetActive(true);

            SetVisible(_mainMenu,  _mainMenu  == target);
            SetVisible(_teamSetup, _teamSetup == target);
            SetVisible(_game,      _game      == target);
            SetVisible(_admin,     _admin     == target);
            SetVisible(_winners,   _winners     == target);
            SetVisible(_gameOver,  _gameOver  == target);

            // Уведомляем скрипты-контроллеры (они на отдельных объектах, OnEnable им не помогает)
            onShown?.Invoke();
            Debug.Log($"[Navigator] ✓ Активна панель: {target.name}");
        }

        /// <summary>
        /// Показывает/скрывает панель через CanvasGroup.
        /// SetActive НЕ вызывается — скрипты на объекте остаются живыми.
        /// </summary>
        private static void SetVisible(GameObject go, bool visible)
        {
            if (go == null) return;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.alpha          = visible ? 1f : 0f;
            cg.interactable   = visible;
            cg.blocksRaycasts = visible;
        }
    }
}
