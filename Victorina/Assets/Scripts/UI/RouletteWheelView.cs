using System;
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
    public class RouletteWheelView : MonoBehaviour
    {
        [Header("Трансформ колеса (тот что вращается)")]
        [SerializeField] private RectTransform _wheelTransform;

        [Header("RawImage колеса (шейдер рисует сюда)")]
        [SerializeField] private RawImage _wheelImage;

        [Header("Шейдер колеса (перетащи Custom/RouletteWheel)")]
        [SerializeField] private Shader _wheelShader;

        [Header("Цвет — обычные вопросы (Общая база)")]
        [SerializeField] private Color _colorGeneral  = new Color(0.25f, 0.55f, 1.00f);

        [Header("Цвет — Блиц")]
        [SerializeField] private Color _colorBlitz    = new Color(1.00f, 0.80f, 0.10f);

        [Header("Цвет — Чёрный ящик")]
        [SerializeField] private Color _colorBlackBox = new Color(0.15f, 0.15f, 0.15f);

        [Header("Цвет цифры на секторе")]
        [SerializeField] private Color _labelColor = Color.white;

        [Header("Размер шрифта цифры на секторе")]
        [SerializeField] private float _labelFontSize = 18f;

        [Header("Радиус цифр (0 = центр, 1 = внешний край колеса)")]
        [Range(0f, 1f)]
        [SerializeField] private float _labelRadiusFraction = 0.82f;

        private Material        _wheelMat;
        private ISessionService _session;

        private readonly List<GameObject> _labels = new();

        private void Start()
        {
            Debug.Log("[RouletteWheelView] Start");
            if (_wheelTransform == null) Debug.LogError("[RouletteWheelView] Трансформ колеса не назначен!");
            if (_wheelImage     == null) Debug.LogError("[RouletteWheelView] RawImage не назначен!");

            try { _session = GameBootstrapper.Services.Resolve<ISessionService>(); }
            catch (System.Exception e) { Debug.LogError($"[RouletteWheelView] Resolve ISessionService: {e.Message}"); }

            Debug.Log("[RouletteWheelView] ✓ Start завершён");
        }

        public void RefreshWheel()
        {
            Debug.Log("[RouletteWheelView] RefreshWheel");

            // Ленивая инициализация
            if (_session == null)
            {
                try
                {
                    _session =
                        GameBootstrapper.Services.Resolve<ISessionService>();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(
                        $"[RouletteWheelView] Resolve ISessionService: {e.Message}"
                    );
                }
            }

            if (_wheelImage == null)
            {
                Debug.LogError(
                    "[RouletteWheelView] RefreshWheel: _wheelImage не назначен!"
                );

                return;
            }

            if (_session == null)
            {
                Debug.LogWarning(
                    "[RouletteWheelView] RefreshWheel: session == null"
                );

                return;
            }

            if (_wheelMat == null)
            {
                if (_wheelShader == null)
                {
                    Debug.LogError(
                        "[RouletteWheelView] Шейдер не назначен!"
                    );

                    return;
                }

                _wheelMat = new Material(_wheelShader);
                _wheelImage.material = _wheelMat;
            }

            var colors = new Vector4[100];
            var counts = new float[100];

            int totalSectors = 0;

            // Считаем количество секторов
            for (int pool = 0; pool < PoolInfo.Count; pool++)
            {
                int poolCount =
                    _session.GetAvailableQuestionCount(pool);

                if (poolCount <= 0)
                    continue;

                // Блиц всегда = 1 сектор
                if (PoolInfo.IsBlitz(pool))
                    totalSectors += 1;
                else
                    totalSectors += poolCount;
            }

            // Заполняем цвета
            int si = 0;

            for (int pool = 0; pool < PoolInfo.Count; pool++)
            {
                int poolCount =
                    _session.GetAvailableQuestionCount(pool);

                if (poolCount <= 0)
                    continue;

                Color c = PoolColor(pool);

                // БЛИЦ = ОДИН сектор
                if (PoolInfo.IsBlitz(pool))
                {
                    colors[si] =
                        new Vector4(c.r, c.g, c.b, 1f);

                    counts[si] = poolCount;

                    si++;

                    Debug.Log(
                        $"[RouletteWheelView] Блиц: 1 сектор, вопросов={poolCount}"
                    );
                }
                else
                {
                    // Обычные вопросы:
                    // 1 вопрос = 1 сектор
                    for (int j = 0; j < poolCount; j++)
                    {
                        colors[si] =
                            new Vector4(c.r, c.g, c.b, 1f);

                        counts[si] = 0;

                        si++;
                    }

                    Debug.Log(
                        $"[RouletteWheelView] Пул {pool}: секторов={poolCount}"
                    );
                }
            }

            Rect rect =
                _wheelImage.rectTransform.rect;

            float aspect =
                rect.height > 0
                ? rect.width / rect.height
                : 1f;

            _wheelMat.SetInt(
                "_SectorCount",
                Mathf.Max(totalSectors, 1)
            );

            _wheelMat.SetFloat("_Aspect", aspect);

            _wheelMat.SetVectorArray(
                "_SectorColors",
                colors
            );

            _wheelMat.SetFloatArray(
                "_QuestionCounts",
                counts
            );

            _wheelMat.SetFloat(
                "_LabelHalfSize",
                0f
            );

            BuildLabels(totalSectors);

            Debug.Log(
                $"[RouletteWheelView] ✓ Всего секторов: {totalSectors}"
            );
        }

        // ── TMP-метки ──────────────────────────────────────────────────────────

        private void BuildLabels(int totalSectors)
        {
            // Удаляем старые метки
            foreach (var go in _labels)
                if (go != null) Destroy(go);
            _labels.Clear();

            if (totalSectors <= 0 || _wheelTransform == null) return;

            // Радиус колеса в пикселях × фракция → метки у внешнего края
            float wheelRadius = Mathf.Min(_wheelTransform.rect.width, _wheelTransform.rect.height) * 0.5f;
            float labelRadius = wheelRadius * _labelRadiusFraction;

            float sectorAngle = 360f / totalSectors;

            for (int i = 0; i < totalSectors; i++)
            {
                // Угол центра сектора (0 = вверх, по часовой — как в шейдере)
                float angleDeg = (i + 0.5f) * sectorAngle;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                // Позиция в локальном пространстве _wheelTransform
                Vector2 pos = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad)) * labelRadius;

                var go = new GameObject($"SectorLabel_{i}", typeof(RectTransform));
                go.transform.SetParent(_wheelTransform, false);

                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = pos;
                rt.sizeDelta        = new Vector2(40f, 40f);
                // Поворачиваем метку так, чтобы цифра «смотрела» от центра наружу
                rt.localEulerAngles = new Vector3(0f, 0f, -angleDeg);

                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text      = (i + 1).ToString();
                tmp.fontSize  = _labelFontSize;
                tmp.color     = _labelColor;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = false;

                _labels.Add(go);
            }

            Debug.Log($"[RouletteWheelView] Создано {_labels.Count} меток");
        }

        // ── Цвета по пулу ──────────────────────────────────────────────────────

        private Color PoolColor(int pool) => pool switch
        {
            0 => _colorGeneral,
            1 => _colorBlitz,
            2 => _colorBlackBox,
            _ => Color.grey
        };

        // ── Анимация ───────────────────────────────────────────────────────────

        public void SpinTo(int targetIndex, int totalSectors, Action onComplete)
        {
            Debug.Log($"[RouletteWheelView] SpinTo: targetIndex={targetIndex} total={totalSectors}");
            StartCoroutine(SpinAnimation(targetIndex, totalSectors, onComplete));
        }

        private IEnumerator SpinAnimation(int targetIndex, int totalSectors, Action onComplete)
        {
            float sectorAngle = 360f / totalSectors;

            // Точный угол центра нужного сектора (нормализован в [0, 360))
            float targetFinal = ((targetIndex * sectorAngle + sectorAngle / 2f) % 360f + 360f) % 360f;

            float startZ = _wheelTransform.localEulerAngles.z;
            startZ = ((startZ % 360f) + 360f) % 360f;

            // Сколько нужно докрутить назад (по часовой) чтобы попасть в targetFinal
            float delta = startZ - targetFinal;
            if (delta < 0f) delta += 360f;

            // Добавляем лишние обороты для красоты
            float extraSpins = 360f * UnityEngine.Random.Range(5, 10);
            float finalDelta = -(extraSpins + delta);

            float duration = 4f;
            float elapsed  = 0f;

            Debug.Log($"[RouletteWheelView] Анимация: сектор={targetIndex} targetFinal={targetFinal} startZ={startZ} delta={delta}");

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 4f);
                _wheelTransform.localEulerAngles = new Vector3(0f, 0f, startZ + finalDelta * t);
                yield return null;
            }

            // Жёсткий снэп строго в центр сектора
            _wheelTransform.localEulerAngles = new Vector3(0f, 0f, targetFinal);

            Debug.Log($"[RouletteWheelView] ✓ Снэп в центр. Угол={targetFinal}");
            onComplete?.Invoke();
        }
    }
}
