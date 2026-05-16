using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class QuestionEditorView : MonoBehaviour
    {
        [Header("Визуальная панель редактора (тот объект, у которого Save/Cancel кнопки дети)")]
        [SerializeField] private GameObject _panel;

        [Header("Поле ввода текста вопроса")]
        [SerializeField] private TMP_InputField _questionInput;

        [Header("Поле ввода текста ответа")]
        [SerializeField] private TMP_InputField _answerInput;

        [Header("Кнопка Сохранить вопрос")]
        [SerializeField] private Button _saveButton;

        [Header("Кнопка Отмена / закрыть редактор")]
        [SerializeField] private Button _cancelButton;

        [Header("Текст ошибки валидации")]
        [SerializeField] private TMP_Text _errorText;

        private IQuestionRepository _questionRepo;
        private Question            _editingQuestion;
        private Action              _onClose;    // закрыть редактор + обновить список
        private Action              _onRefresh;  // только обновить список (редактор остаётся открытым)
        private bool                _isCreating;
        private bool                _isOpen;
        private int                 _preselectedPoolIndex;

        private void Awake()
        {
            Debug.Log("[QuestionEditorView] Awake");
            if (_questionInput == null) Debug.LogError("[QuestionEditorView] _questionInput == null!");
            if (_answerInput   == null) Debug.LogError("[QuestionEditorView] _answerInput == null!");
            if (_saveButton    == null) Debug.LogError("[QuestionEditorView] _saveButton == null!");
            if (_cancelButton  == null) Debug.LogError("[QuestionEditorView] _cancelButton == null!");
            if (_errorText     == null) Debug.LogError("[QuestionEditorView] _errorText == null!");

            if (_saveButton)   _saveButton.onClick.AddListener(OnSave);
            if (_cancelButton) _cancelButton.onClick.AddListener(OnCancel);

            ApplyPanelVisible(false);
        }

        // ── Видимость ──────────────────────────────────────────────────────────

        public void SetVisible(bool visible)
        {
            ApplyPanelVisible(visible);
        }

        private void ApplyPanelVisible(bool visible)
        {
            if (_panel == null) return;
            _panel.SetActive(visible);
        }

        // ── Колбэки ────────────────────────────────────────────────────────────

        /// <summary>
        /// Устанавливается один раз из AdminPanelView при старте.
        /// onRefresh — вызывается после каждого сохранения нового вопроса.
        /// onClose   — вызывается при отмене или сохранении редактирования.
        /// </summary>
        public void SetCallbacks(Action onRefresh, Action onClose)
        {
            _onRefresh = onRefresh;
            _onClose   = onClose;
            Debug.Log("[QuestionEditorView] SetCallbacks ✓");
        }

        // ── Repo ───────────────────────────────────────────────────────────────

        private void EnsureRepo()
        {
            if (_questionRepo != null) return;
            try { _questionRepo = GameBootstrapper.Services.Resolve<IQuestionRepository>(); }
            catch (Exception e) { Debug.LogError($"[QuestionEditorView] Resolve IQuestionRepository: {e.Message}"); }
        }

        // ── Открытие ───────────────────────────────────────────────────────────

        /// <summary>
        /// Создание нового вопроса.
        /// onRefresh — вызывается после каждого сохранения (обновляет список, редактор остаётся открытым).
        /// onClose   — вызывается при отмене (закрывает редактор).
        /// </summary>
        public void OpenForCreateWithCategory(int poolIndex)
        {
            Debug.Log($"[QuestionEditorView] OpenForCreate pool={poolIndex}");
            EnsureRepo();

            _isCreating           = true;
            _isOpen               = true;
            _preselectedPoolIndex = Mathf.Clamp(poolIndex, 0, PoolInfo.Count - 1);
            _editingQuestion      = null;

            ClearFields();
            Debug.Log($"[QuestionEditorView] ✓ Режим создания, категория={_preselectedPoolIndex} ({PoolInfo.Names[_preselectedPoolIndex]})");
        }

        public void OpenForEdit(Question question)
        {
            Debug.Log($"[QuestionEditorView] OpenForEdit id={question.Id} pool={question.PoolIndex}");
            EnsureRepo();

            _isCreating      = false;
            _isOpen          = true;
            _editingQuestion = question;

            if (_errorText     != null) _errorText.text     = "";
            if (_questionInput != null) _questionInput.text = question.Text;
            if (_answerInput   != null) _answerInput.text   = question.Answer;

            Debug.Log($"[QuestionEditorView] ✓ Режим редактирования, категория={question.PoolIndex}");
        }

        private void ClearFields()
        {
            if (_errorText     != null) _errorText.text     = "";
            if (_questionInput != null) _questionInput.text = "";
            if (_answerInput   != null) _answerInput.text   = "";
        }

        // ── Сохранение ─────────────────────────────────────────────────────────

        private void OnSave()
        {
            try
            {
                Debug.Log("[QuestionEditorView] OnSave");
                EnsureRepo();

                if (_questionRepo == null) { Debug.LogError("[QuestionEditorView] репозиторий не найден!"); return; }
                if (_questionInput == null) { Debug.LogError("[QuestionEditorView] _questionInput не назначен!"); return; }

                string text   = _questionInput.text.Trim();
                string answer = _answerInput != null ? _answerInput.text.Trim() : "";

                if (string.IsNullOrEmpty(text))
                {
                    if (_errorText != null) _errorText.text = "Текст вопроса не может быть пустым.";
                    Debug.LogWarning("[QuestionEditorView] Пустой текст — отмена");
                    return;
                }

                if (_editingQuestion != null)
                {
                    // ── Редактирование ─────────────────────────────────────────
                    _editingQuestion.Text   = text;
                    _editingQuestion.Answer = answer;
                    _questionRepo.Update(_editingQuestion);
                    Debug.Log($"[QuestionEditorView] ✓ Обновлён id={_editingQuestion.Id} pool={_editingQuestion.PoolIndex} text='{text}'");

                    _isOpen = false;
                    _onClose?.Invoke(); // закрыть редактор + обновить список
                }
                else
                {
                    // ── Создание нового ────────────────────────────────────────
                    int pool = _preselectedPoolIndex;
                    _questionRepo.Add(new Question { PoolIndex = pool, Text = text, Answer = answer });
                    Debug.Log($"[QuestionEditorView] ✓ Создан pool={pool} ({PoolInfo.Names[pool]}) text='{text}'");

                    // Сначала обновляем список, потом чистим поля
                    Debug.Log($"[QuestionEditorView] Вызов _onRefresh (null={_onRefresh == null})");
                    _onRefresh?.Invoke();
                    ClearFields();
                    if (_questionInput != null) _questionInput.Select();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[QuestionEditorView] ИСКЛЮЧЕНИЕ OnSave: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }
        }

        private void OnCancel()
        {
            Debug.Log("[QuestionEditorView] OnCancel");
            _isOpen = false;
            ClearFields();
            _onClose?.Invoke();
        }

        private void OnDestroy()
        {
            if (_saveButton)   _saveButton.onClick.RemoveListener(OnSave);
            if (_cancelButton) _cancelButton.onClick.RemoveListener(OnCancel);
        }
    }
}
