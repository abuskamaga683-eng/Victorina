using UnityEngine;
using Victorina.Core.Interfaces;
using Victorina.Data;
using Victorina.Services;

namespace Victorina.Infrastructure
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Панель Главного меню")]
        [SerializeField] private GameObject _mainMenuPanel;

        [Header("Панель Настройки команд")]
        [SerializeField] private GameObject _teamSetupPanel;

        [Header("Панель Игры (колесо рулетки)")]
        [SerializeField] private GameObject _gamePanel;

        [Header("Панель Администратора")]
        [SerializeField] private GameObject _adminPanel;

        [Header("Панель Победителей")]
        [SerializeField] private GameObject _winnersPanel;

        [Header("Панель Конца игры (победитель)")]
        [SerializeField] private GameObject _gameOverPanel;

        public static ServiceContainer Services { get; private set; }

        private void Awake()
        {
            Debug.Log("[Bootstrapper] Awake — старт инициализации");

            if (_mainMenuPanel  == null) Debug.LogError("[Bootstrapper] _mainMenuPanel  == null!");
            if (_teamSetupPanel == null) Debug.LogError("[Bootstrapper] _teamSetupPanel == null!");
            if (_gamePanel      == null) Debug.LogError("[Bootstrapper] _gamePanel      == null!");
            if (_adminPanel     == null) Debug.LogError("[Bootstrapper] _adminPanel     == null!");
            if (_gameOverPanel  == null) Debug.LogError("[Bootstrapper] _gameOverPanel  == null!");

            Services = new ServiceContainer();

            try
            {
                var db = new DatabaseService();
                db.Initialize();
                Services.Register<IDatabaseService>(db);
                Debug.Log("[Bootstrapper] ✓ DatabaseService инициализирован");
            }
            catch (System.Exception e) { Debug.LogError($"[Bootstrapper] ОШИБКА DatabaseService: {e}"); return; }

            try
            {
                var questionRepo = new QuestionRepository(Services.Resolve<IDatabaseService>());
                Services.Register<IQuestionRepository>(questionRepo);
                Services.Register<ITeamRepository>(new TeamRepository(Services.Resolve<IDatabaseService>()));
                Services.Register<IGameSessionRepository>(new GameSessionRepository(Services.Resolve<IDatabaseService>()));
                Services.Register<IGameResultRepository>(new GameResultRepository(Services.Resolve<IDatabaseService>()));
                Debug.Log("[Bootstrapper] ✓ QuestionRepository зарегистрирован");
            }
            catch (System.Exception e) { Debug.LogError($"[Bootstrapper] ОШИБКА QuestionRepository: {e}"); return; }

            try
            {
                new DataSeeder(Services.Resolve<IQuestionRepository>()).SeedIfEmpty();
                Debug.Log("[Bootstrapper] ✓ DataSeeder выполнен");
            }
            catch (System.Exception e) { Debug.LogError($"[Bootstrapper] ОШИБКА DataSeeder: {e}"); }

            try
            {
                Services.Register<ISessionService>(
                    new SessionService(
                        Services.Resolve<IQuestionRepository>(),
                        Services.Resolve<ITeamRepository>(),
                        Services.Resolve<IGameSessionRepository>(),
                        Services.Resolve<IGameResultRepository>())
                    );
                Debug.Log("[Bootstrapper] ✓ SessionService зарегистрирован");
            }
            catch (System.Exception e) { Debug.LogError($"[Bootstrapper] ОШИБКА SessionService: {e}"); return; }

            try
            {
                Services.Register<IRouletteService>(new RouletteService());
                Debug.Log("[Bootstrapper] ✓ RouletteService зарегистрирован");
            }
            catch (System.Exception e) { Debug.LogError($"[Bootstrapper] ОШИБКА RouletteService: {e}"); return; }

            try
            {
                var navigator = new SceneNavigator(
                    _mainMenuPanel, _teamSetupPanel,
                    _gamePanel, _adminPanel, _winnersPanel, _gameOverPanel);
                Services.Register<ISceneNavigator>(navigator);
                Debug.Log("[Bootstrapper] ✓ SceneNavigator зарегистрирован");

                navigator.GoToMainMenu();
                Debug.Log("[Bootstrapper] ✓ GoToMainMenu вызван");
            }
            catch (System.Exception e) { Debug.LogError($"[Bootstrapper] ОШИБКА Navigator: {e}"); return; }

            Debug.Log("[Bootstrapper] ✓ Инициализация завершена успешно");
        }
    }
}
