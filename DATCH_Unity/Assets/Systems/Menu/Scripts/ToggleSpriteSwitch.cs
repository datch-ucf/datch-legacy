using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSpriteSwitch : MonoBehaviour
{

    private Toggle toggle;
    private Image image;
    private Sprite originalSprite;
    private Sprite originalHighlightedSprite;
    public Sprite selectedSprite;
    public Sprite selectedHighlightedSprite;
    // Use this for initialization
    void Start()
    {
        toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();
        originalSprite = image.sprite;
        originalHighlightedSprite = toggle.spriteState.highlightedSprite;

        toggle.onValueChanged.AddListener(SwapSprites);
        
        // Call it in start to make sure you have proper spirtes setup
        SwapSprites(toggle.isOn);
    }

    public void SwapSprites(bool value)
    {
        SpriteState ss = toggle.spriteState;
        
        Image targetImage = toggle.targetGraphic as Image;
        if (value)
        {
            image.sprite = selectedSprite;
            if (selectedHighlightedSprite != null)
                ss.highlightedSprite = selectedHighlightedSprite;
        }
        else
        {
            image.sprite = originalSprite;
            if (selectedHighlightedSprite != null)
                ss.highlightedSprite = originalHighlightedSprite;
        }
        toggle.targetGraphic = targetImage;
        toggle.spriteState = ss;
    }
}
