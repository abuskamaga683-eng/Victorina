using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Victorina.UI
{
    public class TeamTurnView : MonoBehaviour
    {
        [Header("Текст состояния")]
        [SerializeField] private TMP_Text _statusText;

        [Header("Фон панели")]
        [SerializeField] private Image _background;

        [Header("Цвет ожидания")]
        [SerializeField]
        private Color _idleColor =
            new Color(0f, 0.6f, 1f);

        [Header("Цвет чёрного ящика")]
        [SerializeField]
        private Color _blackBoxColor =
    new Color(0.1f, 0.1f, 0.1f);

        [Header("Цвет обсуждения")]
        [SerializeField]
        private Color _discussionColor =
            new Color(1f, 0.6f, 0f);

        [Header("Цвет успеха")]
        [SerializeField]
        private Color _successColor =
            new Color(0f, 1f, 0.4f);

        [Header("Цвет блица")]
        [SerializeField]
        private Color _blitzColor =
            new Color(1f, 0.2f, 0.2f);

        private void Start()
        {
            ShowIdle();
        }

        public void ShowIdle()
        {
            SetState(
                "КРУТИТЕ РУЛЕТКУ",
                _idleColor
            );
        }

        public void ShowBlackBox()
        {
            SetState(
                "ЧЁРНЫЙ ЯЩИК!",
                _blackBoxColor
            );
        }

        public void ShowDiscussion()
        {
            SetState(
                "ИДЁТ ОБСУЖДЕНИЕ",
                _discussionColor
            );
        }

        public void ShowSuccess()
        {
            SetState(
                "ОТВЕТ ПРИНЯТ",
                _successColor
            );
        }

        public void ShowBlitz()
        {
            SetState(
                "БЛИЦ-ВОПРОС",
                _blitzColor
            );
        }

        private void SetState(string text, Color color)
        {
            if (_statusText)
                _statusText.text = text;

            if (_background)
                _background.color = color;
        }
    }
}