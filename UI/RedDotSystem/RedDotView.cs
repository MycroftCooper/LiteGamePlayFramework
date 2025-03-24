using UnityEngine;
using UnityEngine.UI;

namespace LitePlayQuickFramework.UI.RedDotSystem {
    public class RedDotView :MonoBehaviour, IRedDotUI {
        public bool showNumber;
        public Sprite redDotSprite;
        public Color imgColor;
        public Color textColor;
        private Image _image;
        private Text _text;

        private void Awake() {
            _image = transform.GetComponent<Image>();
            _image.sprite = redDotSprite;
            _image.color = imgColor;
            
            _text = transform.Find("Text").GetComponent<Text>();
            _text.color = textColor;
            _text.enabled = showNumber;

            if (string.IsNullOrEmpty(redDotKey)) {
                Debug.LogError($"IRedDotUI> [{gameObject.name}] has no redDotKey!]");
            }
        }
        
        public string redDotKey;
        public string RedDotKey => redDotKey;
        public void SetRedDotState(bool active, int dotCount) {
            _text.text = dotCount.ToString();
            gameObject.SetActive(active);
        }
    }
}