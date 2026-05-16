using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Victorina.Core.Models;

namespace Victorina.UI
{
    public class ScoreboardView : MonoBehaviour
    {
        [Header("Вьюпорт (маска прокрутки)")]
        [SerializeField] private RectTransform _viewport;

        [Header("Контент (сюда спавнятся строки команд)")]
        [SerializeField] private RectTransform _content;

        [Header("Префаб строки с именем и очками команды")]
        [SerializeField] private GameObject _scorePrefab;

        private readonly List<TextMeshProUGUI> _rows = new();
        private Coroutine _marquee;

        public void Highlight(int teamIndex)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (i == teamIndex)
                {
                    _rows[i].text =
                        _rows[i].text.Replace(
                            "#142337CC",
                            "#C58A18FF");
                }
                else
                {
                    _rows[i].text =
                        _rows[i].text.Replace(
                            "#C58A18FF",
                            "#142337CC");
                }
            }
        }

        public void Refresh(IReadOnlyList<Team> teams)
        {
            Debug.Log($"[ScoreboardView] Refresh: команд={teams.Count}");

            if (_scorePrefab == null) { Debug.LogError("[ScoreboardView] _scorePrefab == null!"); return; }
            if (_content     == null) { Debug.LogError("[ScoreboardView] _content == null!"); return; }

            foreach (var row in _rows) Destroy(row.gameObject);
            _rows.Clear();

            foreach (var team in teams)
            {
                try
                {
                    var go  = Instantiate(_scorePrefab, _content);
                    var row = go.GetComponent<TextMeshProUGUI>();
                    if (row == null) { Debug.LogError("[ScoreboardView] Префаб не содержит TextMeshProUGUI!"); continue; }
                    row.text =
                        $"<mark=#142337CC padding=\"18,8,18,8\">" +
                        $"<color=#CFF8FF><b>{team.Name}</b></color>" +
                        $"   " +
                        $"<color=#FFD54F><b>{team.Score}</b></color>" +
                        $"</mark>";
                    row.fontSize = 28;

                    row.alignment =
                        TextAlignmentOptions.Center;

                    row.fontStyle =
                        FontStyles.Bold;

                    row.outlineWidth = 0.18f;

                    row.outlineColor =
                        new Color32(0, 0, 0, 180);

                    row.margin =
                        new Vector4(25, 8, 25, 8); 
                    _rows.Add(row);
                    Debug.Log($"[ScoreboardView] ✓ Строка добавлена: {team.Name} {team.Score}");
                }
                catch (System.Exception e) { Debug.LogError($"[ScoreboardView] ОШИБКА создания строки: {e.Message}"); }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

            if (_marquee != null) StopCoroutine(_marquee);
            _marquee = StartCoroutine(MarqueeLoop());
        }

        private IEnumerator MarqueeLoop()
        {
            yield return null;
            yield return null;

            if (_viewport == null) { Debug.LogError("[ScoreboardView] _viewport == null в MarqueeLoop!"); yield break; }

            float contentW  = _content.rect.width;
            float viewportW = _viewport.rect.width;
            Debug.Log($"[ScoreboardView] MarqueeLoop: contentW={contentW} viewportW={viewportW}");

            if (contentW <= viewportW)
            {
                _content.anchoredPosition = Vector2.zero;
                Debug.Log("[ScoreboardView] MarqueeLoop: контент помещается, скролл не нужен");
                yield break;
            }

            float overflow = contentW - viewportW;
            const float speed = 80f;
            const float pause = 1.5f;
            Debug.Log($"[SceneBuilder] MarqueeLoop: overflow={overflow}, запускаю скролл");

            while (true)
            {
                _content.anchoredPosition = Vector2.zero;
                yield return new WaitForSeconds(pause);

                float x = 0f;
                while (x < overflow)
                {
                    x = Mathf.MoveTowards(x, overflow, speed * Time.deltaTime);
                    _content.anchoredPosition = new Vector2(-x, 0f);
                    yield return null;
                }

                yield return new WaitForSeconds(pause);
            }
        }
    }
}
