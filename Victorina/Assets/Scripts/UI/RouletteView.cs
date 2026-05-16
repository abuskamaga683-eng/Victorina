using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class RouletteView : MonoBehaviour
    {
        [Header("Компонент колеса (анимация + шейдер)")]
        [SerializeField] private RouletteWheelView _wheelView;

        [Header("Контроллер игрового флоу (вопросы, ответы, ходы)")]
        [SerializeField] private GameFlowController _flowController;

        [Header("Кнопка Крутить колесо")]
        [SerializeField] private Button _spinButton;

        [Header("Кнопка Завершить игру")]
        [SerializeField] private Button _endGameButton;

        [Header("Текст ведущего")]
        [SerializeField] private TMP_Text _hostText;

        [Header("Табло с очками")]
        [SerializeField] private ScoreboardView _scoreboard;

        private ISessionService  _session;
        private IRouletteService _roulette;
        private ISceneNavigator  _navigator;

        private bool _isSpinning;
        private bool _ready;

        private void Start()
        {
            Debug.Log("[RouletteView] Start");
            if (_wheelView      == null) Debug.LogError("[RouletteView] Колесо не назначено!");
            if (_flowController == null) Debug.LogError("[RouletteView] Контроллер флоу не назначен!");
            if (_spinButton     == null) Debug.LogError("[RouletteView] Кнопка крутить не назначена!");
            if (_hostText       == null) Debug.LogError("[RouletteView] Текст ведущего не назначен!");
            if (_scoreboard     == null) Debug.LogError("[RouletteView] Табло не назначено!");

            try { _session  = GameBootstrapper.Services.Resolve<ISessionService>(); }
            catch (System.Exception e) { Debug.LogError($"[RouletteView] Resolve ISessionService: {e.Message}"); }
            try { _roulette = GameBootstrapper.Services.Resolve<IRouletteService>(); }
            catch (System.Exception e) { Debug.LogError($"[RouletteView] Resolve IRouletteService: {e.Message}"); }
            try { _navigator = GameBootstrapper.Services.Resolve<ISceneNavigator>(); }
            catch (System.Exception e) { Debug.LogError($"[RouletteView] Resolve ISceneNavigator: {e.Message}"); }

            _spinButton.onClick.AddListener(OnSpin);
            if (_endGameButton) _endGameButton.onClick.AddListener(OnEndGame);
            Victorina.Services.SceneNavigator.OnGameShown += Activate;
            _ready = true;
            Debug.Log("[RouletteView] ✓ Start завершён");
            Activate();
        }

        private void OnEnable()
        {
            Debug.Log("[RouletteView] OnEnable");
            if (_ready) Activate();
        }

        private void Activate()
        {
            Debug.Log("[RouletteView] Activate");
            _isSpinning              = false;
            _spinButton.interactable = true;
            _wheelView.RefreshWheel();
            _scoreboard.Refresh(_session.Teams);
            Debug.Log("[RouletteView] ✓ Activate завершён");
        }

        private void OnSpin()
        {
            Debug.Log("[RouletteView] OnSpin нажат");

            if (_isSpinning)
            {
                Debug.Log("[RouletteView] Уже крутится");
                return;
            }

            if (_session.IsGameOver)
            {
                Debug.Log("[RouletteView] Игра завершена");

                _session.SaveResults();

                _navigator.GoToGameOver();
                return;
            }

            // Маппинг:
            // обычный вопрос = 1 сектор
            // блиц = всегда 1 сектор
            var sectorPool = new List<int>();

            for (int pool = 0; pool < PoolInfo.Count; pool++)
            {
                int cnt =
                    _session.GetAvailableQuestionCount(pool);

                if (cnt <= 0)
                    continue;

                // Блиц = один сектор
                if (PoolInfo.IsBlitz(pool))
                {
                    sectorPool.Add(pool);
                }
                else
                {
                    // Обычные вопросы:
                    // 1 вопрос = 1 сектор
                    for (int i = 0; i < cnt; i++)
                    {
                        sectorPool.Add(pool);
                    }
                }
            }

            int totalSectors = sectorPool.Count;

            Debug.Log(
                $"[RouletteView] Всего секторов: {totalSectors}"
            );

            if (totalSectors == 0)
            {
                Debug.Log("[RouletteView] Нет доступных секторов");

                _session.SaveResults();

                _navigator.GoToGameOver();
                return;
            }

            int targetSectorIndex =
                UnityEngine.Random.Range(0, totalSectors);

            int poolIndex =
                sectorPool[targetSectorIndex];

            Debug.Log(
                $"[RouletteView] Сектор {targetSectorIndex} " +
                $"→ категория {poolIndex} " +
                $"({PoolInfo.Names[poolIndex]})"
            );

            _isSpinning = true;

            _spinButton.interactable = false;

            _wheelView.SpinTo(
                targetSectorIndex,
                totalSectors,
                () =>
                {
                    Debug.Log(
                        $"[RouletteView] Спин завершён → " +
                        $"StartFlow категория={poolIndex}"
                    );

                    _flowController.StartFlow(
                        poolIndex,
                        OnFlowComplete,
                        OnRespin
                    );
                }
            );
        }

        private void OnFlowComplete()
        {
            Debug.Log("[RouletteView] OnFlowComplete — разблокируем крутилку");
            _isSpinning              = false;
            _spinButton.interactable = true;
        }

        private void OnRespin()
        {
            Debug.Log("[RouletteView] OnRespin — вопрос уже был, автоматическая прокрутка");
            _isSpinning = false;
            OnSpin();
        }

        private void OnEndGame()
        {
            Debug.Log("[RouletteView] OnEndGame");

            _session.SaveResults();

            _navigator.GoToGameOver();
        }

        private void OnDestroy()
        {
            Debug.Log("[RouletteView] OnDestroy");
            if (_spinButton)    _spinButton.onClick.RemoveAllListeners();
            if (_endGameButton) _endGameButton.onClick.RemoveAllListeners();
            Victorina.Services.SceneNavigator.OnGameShown -= Activate;
        }
    }
}
