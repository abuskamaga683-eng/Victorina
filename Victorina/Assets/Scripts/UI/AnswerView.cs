using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Victorina.UI
{
    public class AnswerView : MonoBehaviour
    {
        [Header("Текст ответа")]
        [SerializeField] private TMP_Text _answerText;

        [Header("Кнопка (клик = правильный ответ)")]
        [SerializeField] private Button _button;

        public void Setup(string answer, Action onClicked)
        {
            Debug.Log($"[AnswerView] Setup: {answer}");
            if (_answerText == null) _answerText = GetComponentInChildren<TMP_Text>();
            if (_button     == null) _button     = GetComponent<Button>();

            _answerText.text = answer;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                Debug.Log($"[AnswerView] Нажат ответ: {answer}");
                onClicked?.Invoke();
            });
        }

        private void OnDestroy()
        {
            Debug.Log("[AnswerView] OnDestroy");
            if (_button) _button.onClick.RemoveAllListeners();
        }
    }
}
