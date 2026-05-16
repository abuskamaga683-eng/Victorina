using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class AdminPanelView : MonoBehaviour
    {
        [Header("Контейнер куда спавнятся строки вопросов")]
        [SerializeField] private Transform _listContainer;

        [Header("Префаб одной строки вопроса в списке")]
        [SerializeField] private AdminQuestionRow _rowPrefab;

        [Header("Дропдаун фильтра по секторам")]
        [SerializeField] private TMP_Dropdown _sectorFilter;

        [Header("Кнопка Добавить новый вопрос")]
        [SerializeField] private Button _addButton;

        [Header("Кнопка Назад в главное меню")]
        [SerializeField] private Button _backButton;

        [Header("Панель редактора вопроса")]
        [SerializeField] private QuestionEditorView _editor;

        [Header("ScrollRect списка вопросов (для прокрутки вниз после добавления)")]
        [SerializeField] private ScrollRect _listScroll;

        private IQuestionRepository _questionRepo;
        private ISceneNavigator     _navigator;

        private readonly List<AdminQuestionRow> _rows = new();
        private int  _filterSectorIndex = -1;
        private bool _ready;

        private void Start()
        {
            Debug.Log("[AdminPanelView] Start");
            if (_listContainer == null) Debug.LogError("[AdminPanelView] _listContainer == null!");
            if (_sectorFilter  == null) Debug.LogError("[AdminPanelView] _sectorFilter == null!");
            if (_addButton     == null) Debug.LogError("[AdminPanelView] _addButton == null!");
            if (_backButton    == null) Debug.LogError("[AdminPanelView] _backButton == null!");
            if (_editor        == null) Debug.LogError("[AdminPanelView] _editor == null!");

            try { _questionRepo = GameBootstrapper.Services.Resolve<IQuestionRepository>(); }
            catch (System.Exception e) { Debug.LogError($"[AdminPanelView] ОШИБКА Resolve IQuestionRepository: {e.Message}"); }
            try { _navigator    = GameBootstrapper.Services.Resolve<ISceneNavigator>(); }
            catch (System.Exception e) { Debug.LogError($"[AdminPanelView] ОШИБКА Resolve ISceneNavigator: {e.Message}"); }

            _addButton.onClick.AddListener(OnAdd);
            _backButton.onClick.AddListener(OnBack);
            _sectorFilter.onValueChanged.AddListener(OnFilterChanged);

            // Колбэки устанавливаем один раз — редактор всегда знает куда звонить
            _editor.SetCallbacks(onRefresh: Refresh, onClose: OnEditorClosed);

            SetEditorVisible(false);
            Victorina.Services.SceneNavigator.OnAdminShown += Activate;
            _ready = true;
            Debug.Log("[AdminPanelView] ✓ Start завершён");
            Activate();
        }

        private void OnEnable()
        {
            Debug.Log("[AdminPanelView] OnEnable");
            if (_ready) Activate();
        }

        private void Activate()
        {
            Debug.Log("[AdminPanelView] Activate");
            SetEditorVisible(false);
            PopulateSectorFilter();
            // По умолчанию устанавливаем фильтр на "Общая база" (индекс 1 в дропдауне, т.к. 0 = "Все категории")
            // Но для логики фильтрации: 0 = "Все", 1 = "Общая база"(0), 2 = "Блиц"(1), 3 = "Черный ящик"(2)
            // Поэтому устанавливаем значение 1, что соответствует _filterSectorIndex = 0
            _sectorFilter.value = 1; // "Общая база"
            Refresh();
        }

        private void PopulateSectorFilter()
        {
            _sectorFilter.onValueChanged.RemoveListener(OnFilterChanged);
            _sectorFilter.ClearOptions();
            var opts = new List<TMP_Dropdown.OptionData> { new("Все категории") };
            for (int i = 0; i < PoolInfo.Count; i++)
                opts.Add(new TMP_Dropdown.OptionData(PoolInfo.Names[i]));
            _sectorFilter.AddOptions(opts);
            _sectorFilter.onValueChanged.AddListener(OnFilterChanged);
            Debug.Log("[AdminPanelView] Фильтр категорий заполнен");
        }

        private void OnFilterChanged(int index)
        {
            // index 0 = "Все категории" → _filterSectorIndex = -1
            // index 1+ = конкретная категория → _filterSectorIndex = index - 1
            _filterSectorIndex = index == 0 ? -1 : index - 1;
            
            // Дополнительная защита: если индекс вышел за пределы, сбрасываем на "все"
            if (_filterSectorIndex != -1 && (_filterSectorIndex < 0 || _filterSectorIndex >= PoolInfo.Count))
            {
                Debug.LogWarning($"[AdminPanelView] OnFilterChanged: невалидный индекс {_filterSectorIndex}, сброс на -1");
                _filterSectorIndex = -1;
            }
            Debug.Log($"[AdminPanelView] Фильтр: index={index} sectorIndex={_filterSectorIndex}");
            Refresh();
        }

        private void Refresh()
        {
            Debug.Log($"[AdminPanelView] Refresh START — фильтр: {_filterSectorIndex}");
            try
            {
                foreach (var row in _rows) Destroy(row.gameObject);
                _rows.Clear();

                if (_questionRepo == null)
                {
                    Debug.LogError("[AdminPanelView] Refresh: _questionRepo == null");
                    return;
                }

                List<Question> questions;
                if (_filterSectorIndex == -1)
                {
                    Debug.Log("[AdminPanelView] Запрос GetAll()");
                    questions = _questionRepo.GetAll();
                }
                else
                {
                    var safeIndex = Mathf.Clamp(_filterSectorIndex, 0, PoolInfo.Count - 1);
                    Debug.Log($"[AdminPanelView] Запрос GetByPool({safeIndex})");
                    questions = _questionRepo.GetByPool(safeIndex);
                }

                if (questions == null)
                {
                    Debug.LogWarning("[AdminPanelView] Refresh: questions == null");
                    return;
                }

                Debug.Log($"[AdminPanelView] ✓ Получено вопросов: {questions.Count}");

                foreach (var q in questions)
                {
                    var row = Instantiate(_rowPrefab, _listContainer);
                    row.Setup(q, OnEdit, OnDelete);
                    _rows.Add(row);
                }
                Debug.Log($"[AdminPanelView] Refresh END — отрисовано {_rows.Count} строк");

                // Принудительно перестраиваем layout чтобы скролл и размеры обновились
                if (_listContainer is RectTransform rt)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                Canvas.ForceUpdateCanvases();

                // Прокрутить список вниз через кадр (чтобы layout успел пересчитаться)
                StartCoroutine(ScrollToBottomNextFrame());
            }
            catch (System.Exception e) { Debug.LogError($"[AdminPanelView] ОШИБКА Refresh: {e.Message}\n{e.StackTrace}"); }
        }

        private void OnAdd()
        {
            Debug.Log("[AdminPanelView] OnAdd");
            int pool = _filterSectorIndex == -1 ? 0 : Mathf.Clamp(_filterSectorIndex, 0, PoolInfo.Count - 1);
            Debug.Log($"[AdminPanelView] Добавляем в категорию {pool} ({PoolInfo.Names[pool]})");
            SetEditorVisible(true);
            _editor.OpenForCreateWithCategory(pool);
        }

        private void OnEdit(Question q)
        {
            Debug.Log($"[AdminPanelView] OnEdit id={q.Id}");
            SetEditorVisible(true);
            _editor.OpenForEdit(q);
        }

        private void OnDelete(Question q)
        {
            Debug.Log($"[AdminPanelView] OnDelete вопрос id={q.Id}");
            try { _questionRepo.Delete(q.Id); Debug.Log($"[AdminPanelView] ✓ Вопрос id={q.Id} удалён"); }
            catch (System.Exception e) { Debug.LogError($"[AdminPanelView] ОШИБКА Delete: {e.Message}"); }
            Refresh();
        }

        private void OnEditorClosed()
        {
            Debug.Log("[AdminPanelView] OnEditorClosed → скрываем редактор, обновляем список");
            SetEditorVisible(false);
            Refresh();
        }

        private IEnumerator ScrollToBottomNextFrame()
        {
            yield return null; // ждём один кадр — layout пересчитается
            if (_listScroll != null)
                _listScroll.verticalNormalizedPosition = 0f; // 0 = низ
        }

        private void SetEditorVisible(bool visible)
        {
            _editor.SetVisible(visible);
        }

        private void OnBack() { Debug.Log("[AdminPanelView] OnBack → GoToMainMenu"); _navigator.GoToMainMenu(); }

        private void OnDestroy()
        {
            Debug.Log("[AdminPanelView] OnDestroy");
            if (_addButton)    _addButton.onClick.RemoveListener(OnAdd);
            if (_backButton)   _backButton.onClick.RemoveListener(OnBack);
            if (_sectorFilter) _sectorFilter.onValueChanged.RemoveListener(OnFilterChanged);
            Victorina.Services.SceneNavigator.OnAdminShown -= Activate;
        }
    }
}
