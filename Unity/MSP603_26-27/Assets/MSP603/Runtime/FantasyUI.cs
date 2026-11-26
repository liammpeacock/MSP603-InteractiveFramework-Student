using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MSP603.TowerDefense
{
    public static class FantasyUI
    {
        public static readonly Color Gold = new(1f, .82f, .38f);

        // The supplied production PNGs were exported on large transparent artboards.
        // These normalized visible bounds keep runtime controls aligned with their art
        // without requiring students to reimport or modify the source artwork.
        private static readonly Dictionary<string, Rect> VisibleSpriteBounds = new()
        {
            { "UI_Button_Large.png", new Rect(.003f, .303f, .994f, .451f) },
            { "UI_Button_Small.png", new Rect(.005f, .261f, .989f, .537f) },
            { "UI_Button_Square.png", new Rect(.197f, .077f, .607f, .859f) },
            { "UI_HUD_Frame.png", new Rect(.008f, .354f, .984f, .327f) },
            { "UI_Slider_Track", new Rect(.010f, .425f, .980f, .160f) },
            { "UI_Slider_Fill", new Rect(.021f, .391f, .955f, .209f) },
            { "UI_Slider_Handle", new Rect(.130f, .066f, .740f, .902f) },
            { "UI_TowerSelection_Frame.png", new Rect(.007f, .222f, .987f, .596f) },
            { "UI_Toggle_Off", new Rect(.046f, .198f, .908f, .645f) }
        };

        public static Sprite Sprite(string name, Vector4 border = default)
        {
            Texture2D texture = Resources.Load<Texture2D>($"FantasyUI/{name}");
            if (texture == null) return null;

            Rect normalized = VisibleSpriteBounds.TryGetValue(name, out Rect bounds) ? bounds : new Rect(0, 0, 1, 1);
            Rect pixels = new(normalized.x * texture.width, normalized.y * texture.height,
                normalized.width * texture.width, normalized.height * texture.height);
            return UnityEngine.Sprite.Create(texture, pixels, new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
        }

        public static Canvas Canvas(string name = "Fantasy UI Canvas")
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject events = new("EventSystem");
                events.SetActive(false);
                events.AddComponent<EventSystem>();
                InputSystemUIInputModule inputModule = events.AddComponent<InputSystemUIInputModule>();
                ConfigureUiInput(inputModule, InputSystem.actions);
                events.SetActive(true);
            }
            GameObject go = new(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1536, 1024);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            return canvas;
        }

        private static void ConfigureUiInput(InputSystemUIInputModule inputModule, InputActionAsset actions)
        {
            if (actions == null)
                throw new InvalidOperationException("The project-wide InputSystem_Actions asset is not configured.");

            inputModule.actionsAsset = actions;
            inputModule.point = Reference(actions, "UI/Point");
            inputModule.leftClick = Reference(actions, "UI/Click");
            inputModule.rightClick = Reference(actions, "UI/RightClick");
            inputModule.middleClick = Reference(actions, "UI/MiddleClick");
            inputModule.scrollWheel = Reference(actions, "UI/ScrollWheel");
            inputModule.move = Reference(actions, "UI/Navigate");
            inputModule.submit = Reference(actions, "UI/Submit");
            inputModule.cancel = Reference(actions, "UI/Cancel");
            inputModule.trackedDevicePosition = Reference(actions, "UI/TrackedDevicePosition");
            inputModule.trackedDeviceOrientation = Reference(actions, "UI/TrackedDeviceOrientation");
        }

        private static InputActionReference Reference(InputActionAsset actions, string actionPath)
        {
            return InputActionReference.Create(actions.FindAction(actionPath, true));
        }

        public static RectTransform Image(Transform parent, string name, string sprite, Vector2 min, Vector2 max, bool sliced = false)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.sprite = Sprite(sprite);
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = !sliced;
            image.raycastTarget = false;
            return rt;
        }

        public static Text Label(Transform parent, string name, string value, int size, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.fontStyle = FontStyle.Bold; text.alignment = alignment;
            text.color = Gold; text.resizeTextForBestFit = true; text.resizeTextMinSize = 12; text.resizeTextMaxSize = size;
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(12, 8); rt.offsetMax = new Vector2(-12, -8);
            return text;
        }

        public static Button Button(Transform parent, string name, string label, string icon, Action action)
        {
            Button button = ButtonSurface(parent, name, string.IsNullOrEmpty(label)
                ? "UI_Button_Square.png"
                : "UI_Button_Large.png", action);
            GameObject go = button.gameObject;
            Image image = go.GetComponent<Image>();
            // Text-and-icon actions use the same wide production frame. The square
            // asset is reserved for icon-only controls such as the HUD pause button.
            bool iconOnly = string.IsNullOrEmpty(label);
            Transform existingIcon = go.transform.Find("Icon");
            if (existingIcon != null) UnityEngine.Object.Destroy(existingIcon.gameObject);
            Transform existingLabel = go.transform.Find("Label");
            if (existingLabel != null) UnityEngine.Object.Destroy(existingLabel.gameObject);
            if (icon != null)
            {
                RectTransform iconRt = Image(go.transform, "Icon", icon,
                    iconOnly ? new Vector2(.2f, .2f) : new Vector2(.06f, .18f),
                    iconOnly ? new Vector2(.8f, .8f) : new Vector2(.25f, .82f));
                iconRt.GetComponent<Image>().preserveAspect = true;
            }
            Text text = Label(go.transform, "Label", label, 29);
            text.gameObject.SetActive(!iconOnly);
            RectTransform tr = text.rectTransform; tr.anchorMin = icon == null ? Vector2.zero : new Vector2(.25f, 0); tr.offsetMin = new Vector2(8, 8);
            return button;
        }

        public static Button ButtonSurface(Transform parent, string name, string sprite, Action action)
        {
            Button authored = StudentAuthoringTemplates.InstantiateUiSource<Button>(name, parent);
            GameObject go = authored != null ? authored.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            if (authored == null) go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = Sprite(sprite);
            image.type = UnityEngine.UI.Image.Type.Simple;
            Button button = go.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.targetGraphic = image;
            button.onClick.AddListener(() => action());
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, .9f, .65f);
            colors.pressedColor = new Color(.72f, .62f, .45f);
            button.colors = colors;
            return button;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 position)
        { rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(.5f, .5f); rt.sizeDelta = size; rt.anchoredPosition = position; }

        public static Slider Slider(Transform parent, string name, float value, Action<float> changed)
        {
            Slider authored = StudentAuthoringTemplates.InstantiateUiSource<Slider>(name, parent);
            GameObject go = authored != null ? authored.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Slider));
            if (authored == null) go.transform.SetParent(parent, false);
            while (go.transform.childCount > 0) UnityEngine.Object.Destroy(go.transform.GetChild(0).gameObject);
            Image hitArea = go.GetComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;
            Image(go.transform, "Track", "UI_Slider_Track", new Vector2(0, .35f), new Vector2(1, .65f), true);
            RectTransform fillArea = Image(go.transform, "Fill Area", "UI_Slider_Fill", new Vector2(.02f, .35f), new Vector2(.98f, .65f), true);
            RectTransform handle = Image(go.transform, "Handle", "UI_Slider_Handle", new Vector2(0, .1f), new Vector2(.12f, .9f));
            Slider slider = go.GetComponent<Slider>(); slider.interactable = true; slider.minValue = 0f; slider.maxValue = 100f; slider.wholeNumbers = false;
            slider.fillRect = fillArea; slider.handleRect = handle; slider.targetGraphic = handle.GetComponent<Image>(); slider.value = Mathf.Clamp01(value) * 100f;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(v => changed(v / 100f)); return slider;
        }

        public static Toggle Toggle(Transform parent, string name, string label, bool value, Action<bool> changed)
        {
            Toggle authored = StudentAuthoringTemplates.InstantiateUiSource<Toggle>(name, parent);
            GameObject go = authored != null ? authored.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Toggle));
            if (authored == null) go.transform.SetParent(parent, false);
            while (go.transform.childCount > 0) UnityEngine.Object.Destroy(go.transform.GetChild(0).gameObject);
            Image hitArea = go.GetComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;
            RectTransform background = Image(go.transform, "State", "UI_Toggle_Off", new Vector2(0f, .12f), new Vector2(.22f, .88f));
            RectTransform check = Image(background, "Checkmark", "Icon_Mute", new Vector2(.18f, .18f), new Vector2(.82f, .82f));
            Text text = Label(go.transform, "Label", label, 22, TextAnchor.MiddleLeft);
            text.rectTransform.anchorMin = new Vector2(.25f, 0f);
            Toggle toggle = go.GetComponent<Toggle>();
            toggle.interactable = true;
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.isOn = value;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(v => changed(v));
            return toggle;
        }
    }
}
