using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class TeamSetupView : MonoBehaviour
    {
        [Header("Контейнер куда спавнятся поля команд")]
        [SerializeField] private Transform _container;

        [Header("Префаб поля ввода имени команды")]
        [SerializeField] private TMP_InputField _inputPrefab;

        [Header("Кнопка Добавить команду")]
        [SerializeField] private Button _addTeamButton;

        [Header("Кнопка Убрать команду")]
        [SerializeField] private Button _removeTeamButton;

        [Header("Кнопка Начать игру")]
        [SerializeField] private Button _startButton;

        [Header("Кнопка Назад в главное меню")]
        [SerializeField] private Button _backButton;

        [Header("Текст ошибки валидации")]
        [SerializeField] private TMP_Text _errorText;

        private const int MinTeams = 2;
        private const int MaxTeams = 4;

        private readonly List<TMP_InputField> _inputs = new();
        private ISessionService _session;
        private ISceneNavigator _navigator;

        private void Start()
        {
            Debug.Log("[TeamSetupView] Start");
            if (_container    == null) Debug.LogError("[TeamSetupView] _container не назначен!");
            if (_inputPrefab  == null) Debug.LogError("[TeamSetupView] _inputPrefab не назначен!");
            if (_addTeamButton    == null) Debug.LogError("[TeamSetupView] _addTeamButton == null!");
            if (_removeTeamButton == null) Debug.LogError("[TeamSetupView] _removeTeamButton == null!");
            if (_startButton      == null) Debug.LogError("[TeamSetupView] _startButton == null!");
            if (_backButton       == null) Debug.LogError("[TeamSetupView] _backButton == null!");
            if (_errorText        == null) Debug.LogError("[TeamSetupView] _errorText == null!");

            try { _session   = GameBootstrapper.Services.Resolve<ISessionService>(); }
            catch (System.Exception e) { Debug.LogError($"[TeamSetupView] Resolve ISessionService: {e.Message}"); }
            try { _navigator = GameBootstrapper.Services.Resolve<ISceneNavigator>(); }
            catch (System.Exception e) { Debug.LogError($"[TeamSetupView] Resolve ISceneNavigator: {e.Message}"); }

            _addTeamButton.onClick.AddListener(OnAddTeam);
            _removeTeamButton.onClick.AddListener(OnRemoveTeam);
            _startButton.onClick.AddListener(OnStart);
            _backButton.onClick.AddListener(OnBack);

            // Спавним 2 команды по умолчанию
            SpawnInput("Команда 1");
            SpawnInput("Команда 2");

            UpdateButtons();
            Debug.Log("[TeamSetupView] ✓ Start завершён");
        }

        private void SpawnInput(string defaultName = "")
        {
            var field = Instantiate(_inputPrefab, _container);
            field.text = defaultName;
            _inputs.Add(field);
            Debug.Log($"[TeamSetupView] Спавн поля: «{defaultName}» всего={_inputs.Count}");
        }

        private void OnAddTeam()
        {
            if (_inputs.Count >= MaxTeams) { Debug.Log("[TeamSetupView] Максимум команд"); return; }
            SpawnInput($"Команда {_inputs.Count + 1}");
            UpdateButtons();
        }

        private void OnRemoveTeam()
        {
            if (_inputs.Count <= MinTeams) { Debug.Log("[TeamSetupView] Минимум команд"); return; }
            var last = _inputs[_inputs.Count - 1];
            _inputs.RemoveAt(_inputs.Count - 1);
            Destroy(last.gameObject);
            UpdateButtons();
            Debug.Log($"[TeamSetupView] Удалено поле, осталось={_inputs.Count}");
        }

        private void UpdateButtons()
        {
            _addTeamButton.interactable    = _inputs.Count < MaxTeams;
            _removeTeamButton.interactable = _inputs.Count > MinTeams;
        }

        private void OnStart()
        {
            Debug.Log("[TeamSetupView] OnStart нажат");
            var names = _inputs.Select(f => f.text.Trim()).ToList();
            Debug.Log($"[TeamSetupView] Имена: [{string.Join(", ", names)}]");

            if (names.Any(string.IsNullOrEmpty))
            {
                _errorText.text = "Введите названия всех команд.";
                Debug.LogWarning("[TeamSetupView] Есть пустые имена");
                return;
            }
            if (names.Distinct().Count() != names.Count)
            {
                _errorText.text = "Названия команд должны быть уникальными.";
                Debug.LogWarning("[TeamSetupView] Дублирующиеся имена");
                return;
            }

            _errorText.text = "";
            _session.StartSession(names);
            Debug.Log("[TeamSetupView] ✓ Сессия начата → GoToGame");
            _navigator.GoToGame();
        }

        private void OnBack()
        {
            Debug.Log("[TeamSetupView] OnBack → GoToMainMenu");
            _navigator.GoToMainMenu();
        }

        private void OnDestroy()
        {
            Debug.Log("[TeamSetupView] OnDestroy");
            if (_addTeamButton)    _addTeamButton.onClick.RemoveListener(OnAddTeam);
            if (_removeTeamButton) _removeTeamButton.onClick.RemoveListener(OnRemoveTeam);
            if (_startButton)      _startButton.onClick.RemoveListener(OnStart);
            if (_backButton)       _backButton.onClick.RemoveListener(OnBack);
        }
    }
}
