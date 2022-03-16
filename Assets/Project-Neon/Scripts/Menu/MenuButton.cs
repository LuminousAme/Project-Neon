using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class MenuButton : MonoBehaviour
{
    public static int fontIndex = 0;
    int currentFontIndex = 0;

    [SerializeField] bool alwaysFont1 = false;
    [SerializeField] TMP_Text unselectedText;
    [SerializeField] TMP_Text selectedText;
    [SerializeField] TMP_Text clickedText;
    [SerializeField] TMP_Text unlitText;
    int currentlyOn = 3;
    [HideInInspector] public bool hovering = false, lightOff = true;
    public bool neverOff = false;
    bool clicked = false, clickedDelayOneFrame = false;

    public UnityEvent onClick = new UnityEvent();

    public struct FontCollection
    {
        public TMP_FontAsset unselectedFont;
        public TMP_FontAsset selectedFont;
        public TMP_FontAsset clickedFont;
        public TMP_FontAsset unlitFont;
    }

    static FontCollection[] fonts = new FontCollection[2];
    static bool inited = false;

    private void Start()
    {
        if(!inited)
        {
            fonts[0].unselectedFont = Resources.Load<TMP_FontAsset>("Neon Sign Red");
            fonts[0].selectedFont = Resources.Load<TMP_FontAsset>("Neon Sign Blue");
            fonts[0].clickedFont = Resources.Load<TMP_FontAsset>("Neon Sign Yellow");
            fonts[0].unlitFont = Resources.Load<TMP_FontAsset>("Neon Sign Unlit");

            fonts[1].unselectedFont = Resources.Load<TMP_FontAsset>("Regular Text Red");
            fonts[1].selectedFont = Resources.Load<TMP_FontAsset>("Regular Text blue");
            fonts[1].clickedFont = Resources.Load<TMP_FontAsset>("Regular Text Yellow");
            fonts[1].unlitFont = Resources.Load<TMP_FontAsset>("Regular Text Unlit");
            inited = true;
        }

        changeFont();
        UnlitActive();
        lightOff = true;
    }

    private void Update()
    {
        //handle font settings change
        if(fontIndex != currentFontIndex)
        {
            changeFont();
        }

        //handle if a button is being hovered over
        if (hovering && !clicked && currentlyOn != 1)
        {
            SelectedActive();
            selectedText.GetComponent<LeanTweenHelper>().BeginAll();
        }
        //handle if it's not
        if (!hovering && !clicked && (!lightOff || neverOff))
        {
            UnselectedActive();
        }
        else if (!hovering && !clicked && lightOff)
        {
            UnlitActive();
        }

        clickedDelayOneFrame = clicked;
    }

    public void OnHover()
    {
        hovering = true;
    }

    public void OnStopHover()
    {
        hovering = false;
    }

    public void OnClick()
    {
        ClickedActive();
        clicked = true;
        onClick.Invoke();
    }

    void changeFont()
    {   
        if(alwaysFont1 && currentFontIndex != 1)
        {
            unselectedText.font = fonts[1].unselectedFont;
            selectedText.font = fonts[1].selectedFont;
            clickedText.font = fonts[1].clickedFont;
            unlitText.font = fonts[1].unlitFont;
            currentFontIndex = 1;
        }
        else if (!alwaysFont1 && fontIndex < fonts.Length)
        {
            unselectedText.font = fonts[fontIndex].unselectedFont;
            selectedText.font = fonts[fontIndex].selectedFont;
            clickedText.font = fonts[fontIndex].clickedFont;
            unlitText.font = fonts[fontIndex].unlitFont;
            currentFontIndex = fontIndex;
        }
        else if (!alwaysFont1) fontIndex = currentFontIndex;
    }

    void UnselectedActive()
    {
        unselectedText.gameObject.SetActive(true);
        selectedText.gameObject.SetActive(false);
        clickedText.gameObject.SetActive(false);
        unlitText.gameObject.SetActive(false);
        currentlyOn = 0;
    }

    void SelectedActive()
    {
        unselectedText.gameObject.SetActive(false);
        selectedText.gameObject.SetActive(true);
        clickedText.gameObject.SetActive(false);
        unlitText.gameObject.SetActive(false);
        currentlyOn = 1;
    }

    void ClickedActive()
    {
        unselectedText.gameObject.SetActive(false);
        selectedText.gameObject.SetActive(false);
        clickedText.gameObject.SetActive(true);
        unlitText.gameObject.SetActive(false);
        currentlyOn = 2;
    }

    void UnlitActive()
    {
        unselectedText.gameObject.SetActive(false);
        selectedText.gameObject.SetActive(false);
        clickedText.gameObject.SetActive(false);
        unlitText.gameObject.SetActive(true);
        currentlyOn = 3;
    }

    public void UnClick()
    {
        clicked = false;
    }

    public bool GetClicked()
    {
        return clickedDelayOneFrame;
    }
}