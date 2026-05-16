using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Models;

namespace Victorina.UI
{
    public class AdminQuestionRow : MonoBehaviour
    {
        [Header("Текст вопроса")]
        [SerializeField] private TMP_Text _questionText;

        [Header("Кнопка Редактировать вопрос")]
        [SerializeField] private Button _editButton;

        [Header("Кнопка Удалить вопрос")]
        [SerializeField] private Button _deleteButton;

public void Setup(Question question,
            Action<Question> onEdit, Action<Question> onDelete)
        {
            _questionText.text = question.Text;

            _editButton.onClick.RemoveAllListeners();
            _editButton.onClick.AddListener(() => onEdit(question));

            _deleteButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.AddListener(() => onDelete(question));
        }

        public void ApplyHeight()
        {
        }
    }
}
