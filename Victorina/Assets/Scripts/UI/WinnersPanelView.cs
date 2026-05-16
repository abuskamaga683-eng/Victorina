using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Interfaces;
using Victorina.Core.Models;
using Victorina.Data;
using Victorina.Infrastructure;

namespace Victorina.UI
{
    public class WinnersPanelView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private ScrollRect _scrollRect;
        [Header("TABLE")]
        [SerializeField] private Transform _content;

        [SerializeField] private GameObject _rowPrefab;

        private ISceneNavigator _navigator;

        private WinnerRepository _repo;

        private void Start()
        {
            _navigator = GameBootstrapper.Services.Resolve<ISceneNavigator>();

            var db = GameBootstrapper.Services.Resolve<IDatabaseService>();

            _repo = new WinnerRepository(db);

            _backButton.onClick.AddListener(OnBack);

            Victorina.Services.SceneNavigator.OnWinnersShown += Load;

            Load();
        }

        private void Load()
        {
            foreach (Transform child in _content)
                Destroy(child.gameObject);

            List<WinnerInfo> games = _repo.GetAll();

            Debug.Log($"WINNERS COUNT = {games.Count}");

            foreach (var game in games)
            {
                var row = Instantiate(_rowPrefab, _content);

                var ui = row.GetComponent<WinnersRowUI>();

                ui.DateText.text =
                    $"<color=#CFCFCF>{game.Date}</color>";

                ui.WinnerText.text =
                    $"<b>Победитель:</b> <color=#4FE3D5>{game.WinnerName}</color>";

                var teamsText = "<b>Команды:</b>\n";
                teamsText += "<color=#888888>────────────</color>\n\n";

                foreach (var team in game.Teams)
                {
                    teamsText += $"• <color=#FFFFFF>{team}</color> очков\n";
                }

                ui.TeamsText.text = teamsText;
            }
        }

        private void OnBack()
        {
            _navigator.GoToMainMenu();
        }

        private void OnDestroy()
        {
            if (_backButton)
                _backButton.onClick.RemoveListener(OnBack);

            Victorina.Services.SceneNavigator.OnWinnersShown -= Load;
        }
    }
}