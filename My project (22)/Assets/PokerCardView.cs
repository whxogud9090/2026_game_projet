using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PokerCardView : MonoBehaviour, IPointerClickHandler
{
    public int CardId { get; private set; }
    public bool IsFront { get; private set; }
    public bool IsMatched { get; private set; }

    private PokerMemoryGame game;
    private Image clickArea;
    private Image backImage;
    private Image frontImage;
    private Button button;
    private Sprite backSprite;
    private Sprite faceSprite;
    private Coroutine flipCoroutine;

    public void Setup(PokerMemoryGame owner, int cardId, Sprite back, Sprite face)
    {
        game = owner;
        CardId = cardId;
        backSprite = back;
        faceSprite = face;
        IsMatched = false;

        clickArea = GetComponent<Image>();
        clickArea.color = new Color(1f, 1f, 1f, 0f);
        clickArea.raycastTarget = true;

        backImage = CreateSideImage("BackImage", backSprite);
        frontImage = CreateSideImage("FrontImage", faceSprite);

        button = GetComponent<Button>();
        button.targetGraphic = clickArea;
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => game.SelectCard(this));

        ApplySide(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && button.interactable && game != null)
        {
            game.SelectCard(this);
        }
    }

    public void Flip(bool showFront)
    {
        if (IsMatched)
        {
            return;
        }

        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }

        flipCoroutine = StartCoroutine(FlipRoutine(showFront));
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsFront = true;
        button.interactable = false;
        frontImage.color = new Color32(210, 255, 210, 255);
    }

    private IEnumerator FlipRoutine(bool showFront)
    {
        const float halfDuration = 0.12f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        ApplySide(showFront);

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
        flipCoroutine = null;
    }

    private void ApplySide(bool showFront)
    {
        IsFront = showFront;
        backImage.enabled = !showFront;
        frontImage.enabled = showFront;
        backImage.color = Color.white;
        frontImage.color = Color.white;
    }

    private Image CreateSideImage(string objectName, Sprite sprite)
    {
        Transform existing = transform.Find(objectName);
        GameObject sideObject = existing != null
            ? existing.gameObject
            : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        sideObject.transform.SetParent(transform, false);

        RectTransform rect = sideObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image sideImage = sideObject.GetComponent<Image>();
        sideImage.sprite = sprite;
        sideImage.preserveAspect = true;
        sideImage.raycastTarget = false;
        sideImage.color = Color.white;
        return sideImage;
    }
}
