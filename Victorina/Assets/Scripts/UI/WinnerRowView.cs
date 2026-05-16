using TMPro;
using UnityEngine;

namespace Victorina.UI
{
    public class WinnerRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private TMP_Text _gameText;
        [SerializeField] private TMP_Text _team1Text;
        [SerializeField] private TMP_Text _team2Text;
        [SerializeField] private TMP_Text _scoreText;

        public void Setup(
            string date,
            string game,
            string team1,
            string team2,
            string score)
        {
            _dateText.text = date;
            _gameText.text = game;
            _team1Text.text = team1;
            _team2Text.text = team2;
            _scoreText.text = score;
        }
    }
}