using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FontManager : MonoBehaviour
{
    public static int fontIndex = 0;
    int currentFontIndex = 0;
    [SerializeField] bool alwaysFont1 = false;
    [SerializeField] bool textField = false;
    TMP_Text text;
    TMP_InputField inputField;
    int textColor = -1; //0 red font, 1 blue font, 2 yellow font, 3 dark font, 4 regular font

    public struct Fonts
    {
        public TMP_FontAsset redFont;
        public TMP_FontAsset blueFont;
        public TMP_FontAsset yellowFont;
        public TMP_FontAsset darkFont;
        public TMP_FontAsset regularFont;
    }

    static Fonts[] fonts = new Fonts[2];
    static bool inited = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!textField) text = this.GetComponent<TMP_Text>();
        else inputField = this.GetComponent<TMP_InputField>();

        if (!inited)
        {
            fonts[0].redFont = Resources.Load<TMP_FontAsset>("Neon Sign Red");
            fonts[0].blueFont = Resources.Load<TMP_FontAsset>("Neon Sign Blue");
            fonts[0].yellowFont = Resources.Load<TMP_FontAsset>("Neon Sign Yellow");
            fonts[0].darkFont = Resources.Load<TMP_FontAsset>("Neon Sign Unlit");
            fonts[0].regularFont = Resources.Load<TMP_FontAsset>("Blank Neon Sign Text");

            fonts[1].redFont = Resources.Load<TMP_FontAsset>("Regular Text Red");
            fonts[1].blueFont = Resources.Load<TMP_FontAsset>("Regular Text blue");
            fonts[1].yellowFont = Resources.Load<TMP_FontAsset>("Regular Text Yellow");
            fonts[1].darkFont = Resources.Load<TMP_FontAsset>("Regular Text Unlit");
            fonts[1].regularFont = Resources.Load<TMP_FontAsset>("Blank Regular Text");
            inited = true;
        }

        if(!textField)
        {
            if (text.font == fonts[0].redFont || text.font == fonts[1].redFont) textColor = 0;
            else if (text.font == fonts[0].blueFont || text.font == fonts[1].blueFont) textColor = 1;
            else if (text.font == fonts[0].yellowFont || text.font == fonts[1].yellowFont) textColor = 2;
            else if (text.font == fonts[0].darkFont || text.font == fonts[1].darkFont) textColor = 3;
            else if (text.font == fonts[0].regularFont || text.font == fonts[1].regularFont) textColor = 4;
        }
        else
        {
            if (inputField.fontAsset == fonts[0].redFont || inputField.fontAsset == fonts[1].redFont) textColor = 0;
            else if (inputField.fontAsset == fonts[0].blueFont || inputField.fontAsset == fonts[1].blueFont) textColor = 1;
            else if (inputField.fontAsset == fonts[0].yellowFont || inputField.fontAsset == fonts[1].yellowFont) textColor = 2;
            else if (inputField.fontAsset == fonts[0].darkFont || inputField.fontAsset == fonts[1].darkFont) textColor = 3;
            else if (inputField.fontAsset == fonts[0].regularFont || inputField.fontAsset == fonts[1].regularFont) textColor = 4;
        }


        ChangeFont();
    }

    // Update is called once per frame
    void Update()
    {
        //handle font settings change
        if (fontIndex != currentFontIndex)
        {
            ChangeFont();
        }
    }

    void ChangeFont()
    {
        if (alwaysFont1 && currentFontIndex != 1)
        {
            if(!textField)
            {
                if (textColor == 0) text.font = fonts[1].redFont;
                else if (textColor == 1) text.font = fonts[1].blueFont;
                else if (textColor == 2) text.font = fonts[1].yellowFont;
                else if (textColor == 3) text.font = fonts[1].darkFont;
                else if (textColor == 4) text.font = fonts[1].regularFont;
            }
            else
            {
                if (textColor == 0) inputField.fontAsset = fonts[1].redFont;
                else if (textColor == 1) inputField.fontAsset = fonts[1].blueFont;
                else if (textColor == 2) inputField.fontAsset = fonts[1].yellowFont;
                else if (textColor == 3) inputField.fontAsset = fonts[1].darkFont;
                else if (textColor == 4) inputField.fontAsset = fonts[1].regularFont;
            }

            currentFontIndex = 1;
        }
        else if (!alwaysFont1 && fontIndex < fonts.Length)
        {
            if(!textField)
            {
                if (textColor == 0) text.font = fonts[fontIndex].redFont;
                else if (textColor == 1) text.font = fonts[fontIndex].blueFont;
                else if (textColor == 2) text.font = fonts[fontIndex].yellowFont;
                else if (textColor == 3) text.font = fonts[fontIndex].darkFont;
                else if (textColor == 4) text.font = fonts[fontIndex].regularFont;
            }
            else
            {
                if (textColor == 0) inputField.fontAsset = fonts[fontIndex].redFont;
                else if (textColor == 1) inputField.fontAsset = fonts[fontIndex].blueFont;
                else if (textColor == 2) inputField.fontAsset = fonts[fontIndex].yellowFont;
                else if (textColor == 3) inputField.fontAsset = fonts[fontIndex].darkFont;
                else if (textColor == 4) inputField.fontAsset = fonts[fontIndex].regularFont;
            }

            currentFontIndex = fontIndex;
        }
        else if (!alwaysFont1) fontIndex = currentFontIndex;
    }

    public void SelectInputField(string input)
    {
        Vector3 scale = inputField.transform.localScale;
        scale.x = 1.2f;
        scale.y = 1.2f;
        inputField.transform.localScale = scale;
    }

    public void DeselectInputField(string input)
    {
        Vector3 scale = inputField.transform.localScale;
        scale.x = 1f;
        scale.y = 1f;
        inputField.transform.localScale = scale;
    }

    public void ChangeFontColor(int color)
    {
        textColor = color;
        currentFontIndex = -1; //force a font update
        ChangeFont();
    }
}