using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace Victorina.UI
{
    /// <summary>
    /// Добавь на любой объект в сцене.
    /// Выбери цвет в Inspector.
    /// Правой кнопкой по компоненту → "Применить цвет ко всем кнопкам".
    /// </summary>
    public class ButtonColorHelper : MonoBehaviour
    {
        [Header("Цвет всех кнопок в сцене")]
        [SerializeField] private Color _buttonColor = new Color(0x58 / 255f, 0xA9 / 255f, 0xD5 / 255f, 1f);

        [ContextMenu("Применить цвет ко всем кнопкам")]
        public void ApplyToAll()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int count = 0;

            foreach (var btn in buttons)
            {
                // 1. Ищем Image: сначала на самом объекте, потом targetGraphic, потом в детях
                var img = btn.GetComponent<Image>();
                if (img == null) img = btn.targetGraphic as Image;
                if (img == null) img = btn.GetComponentInChildren<Image>();

                if (img != null)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(img, "Button Color");
#endif
                    img.color = _buttonColor;
                }

                // 2. ColorBlock — normalColor чтобы тинт совпадал
#if UNITY_EDITOR
                Undo.RecordObject(btn, "Button Color");
#endif
                var cb = btn.colors;
                cb.normalColor = _buttonColor;
                btn.colors = cb;

                count++;
            }

            Debug.Log($"[ButtonColorHelper] Окрашено кнопок: {count}  цвет=#{ColorUtility.ToHtmlStringRGB(_buttonColor)}");

#if UNITY_EDITOR
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
        }
    }
}
