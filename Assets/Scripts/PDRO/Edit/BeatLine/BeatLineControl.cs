using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatLineControl : MonoBehaviour
{
    public Image beatLineImage;
    public Text beatText, beatStepText;

    public float CurrentBeat;

    public void Init(int beat, int xifenyin, int dijixifen)
    {
        CurrentBeat = beat + 1f * dijixifen / xifenyin;
        
        if (dijixifen == 0)
        {
            beatText.text = beat.ToString();

            beatStepText.text = null;

            beatLineImage.color = Color.white;
        }
        else
        {
            beatText.text = null;

            beatStepText.text = $"{dijixifen}/{xifenyin}";

            beatLineImage.color = dijixifen % 2 == 0 ? new Color(0f, 0.67f, 1f, 1f) : new Color(1f, 0.33f, 0f, 1f);
        }
    }

}
