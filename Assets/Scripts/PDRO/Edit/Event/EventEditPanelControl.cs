using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PDRO.Data;
using PDRO.Utils.Singleton;
using PDRO.Utils;

public class EventEditPanelControl : MonoSingleton<EventEditPanelControl>
{
    void UpdateCruve()
    {
        //防止内存泄露，销毁上一张
        Destroy(CruveRawImage.texture);
        //更新波形图
        CruveRawImage.texture = GetEaseCruve(); //
    }

    // 缓动值2波形图！
    public Texture2D GetEaseCruve()
    {
        int width = 150;       // 这个是最后生成的Texture2D图片的宽度
        int height = 90;       // 这个是最后生成的Texture2D图片的高度

        BaseEventData data = IsColor ? ColorData : NumData;

        float[] value = new float[width];

        for (var i = 0; i < value.Length; i++)
        {
            value[i] = EaseUtility.Evaluate(data.EasingType, i, width - 1, data.StartEasingRange, data.EndEasingRange);
        }

        Color backgroundColor = Color.black;
        Color cruveColor = Color.yellow;
        Texture2D texture = new Texture2D(width, height);

        Color zeroColor = new Color(0f, 1f, 1f, 1f);
        Color oneColor = new Color(1f, 0f, 0f, 1f);

        Color[] blank = new Color[width * height];
        for (int i = 0; i < blank.Length; ++i)
        {
            blank[i] = backgroundColor;
        }
        texture.SetPixels(blank, 0);


        for (int i = 0; i < value.Length; ++i)
        {
            var endY = 10f + 70f * value[i];
            for (int y = 0; y <= endY; y++)
            {
                if (IsColor)
                {
                    texture.SetPixel(i, y, Color.LerpUnclamped(ColorData.StartValue, ColorData.EndValue, value[i]));
                }
                else
                {
                    texture.SetPixel(i, y, cruveColor);
                }
            }
        }

        //在0和1的位置画两条线
        for (var i = 0; i < width; i++)
        {
            texture.SetPixel(i, 10, IsColor ? ColorData.StartValue : zeroColor);
            texture.SetPixel(i, 80, IsColor ? ColorData.EndValue : oneColor);
        }

        texture.Apply();
        return texture;
    }

    public InputField StartTimeInput, EndTimeInput, StartValueInput, EndValueInput, EasingIDInput;

    public InputField SR, SG, SB, SA, ER, EG, EB, EA;
    public Dropdown EasingTypeDropdown;

    public Text Title;

    public GameObject ColorSVEdit, ColorEVEdit, NumSVEdit, NumEVEdit;

    public Button DeleteButton, ClosePanelButton;

    public Slider StartEasingRangeSlider, EndEasingRangeSlider;
    public RawImage CruveRawImage;

    public bool IsColor => ID > 9;
    public int ID;
    public NumEventData NumData;
    public ColorEventData ColorData;

    protected override void OnAwake()
    {
        DeleteButton.onClick.AddListener(DeleteEvent);
        ClosePanelButton.onClick.AddListener(ClosePanel);

        StartTimeInput.onEndEdit.AddListener(OnEndEditStartTime);
        EndTimeInput.onEndEdit.AddListener(OnEndEditEndTime);

        StartValueInput.onEndEdit.AddListener(OnEndEditStartValue);
        EndValueInput.onEndEdit.AddListener(OnEndEditEndValue);

        StartEasingRangeSlider.onValueChanged.AddListener(OnStartEasingRangeChange);
        EndEasingRangeSlider.onValueChanged.AddListener(OnEndEasingRangeChange);

        
        SA.onEndEdit.AddListener(OnEndEditSA);

        EA.onEndEdit.AddListener(OnEndEditEA);

        EasingTypeDropdown.onValueChanged.AddListener(OnEndEditEasingType);
        EasingIDInput.onEndEdit.AddListener(OnChangeEasingID);
    }

    private float NewStartEasingRange, NewEndEasingRange;

    void OnStartEasingRangeChange(float value)
    {
        NewStartEasingRange = value;
    }

    void OnEndEasingRangeChange(float value)
    {
        NewEndEasingRange = value;
    }

    void ApplyEasingRange()
    {
        if (Input.GetMouseButtonUp(0))//鼠标抬起才相应
        {
            if (IsColor)
            {
                if (ColorData.StartEasingRange != NewStartEasingRange || ColorData.EndEasingRange != NewEndEasingRange)
                {
                    ColorData.StartEasingRange = NewStartEasingRange;
                    ColorData.EndEasingRange = NewEndEasingRange;
                    OnEditValue(false, true);
                }
            }
            else
            {
                if (NumData.StartEasingRange != NewStartEasingRange || NumData.EndEasingRange != NewEndEasingRange)
                {
                    NumData.StartEasingRange = NewStartEasingRange;
                    NumData.EndEasingRange = NewEndEasingRange;
                    OnEditValue(false, true);
                }
            }
        }
    }

    public void ShowPanel(NumEventData data, int id)
    {
        this.gameObject.SetActive(true);

        NumData = data;
        ColorData = null;
        ID = id;

        StartTimeInput.text = (Mathf.RoundToInt(NumData.StartTime * 1000f)).ToString();
        EndTimeInput.text = (Mathf.RoundToInt(NumData.EndTime * 1000f)).ToString();
        StartValueInput.text = NumData.StartValue.ToString();
        EndValueInput.text = NumData.EndValue.ToString();

        EasingTypeDropdown.value = (int)NumData.EasingType;
        EasingIDInput.text = ((int)NumData.EasingType).ToString();

        StartEasingRangeSlider.value = NewStartEasingRange = NumData.StartEasingRange;
        EndEasingRangeSlider.value = NewEndEasingRange = NumData.EndEasingRange;

        ColorSVEdit.SetActive(false);
        ColorEVEdit.SetActive(false);
        NumSVEdit.SetActive(true);
        NumEVEdit.SetActive(true);

        UpdateCruve();

        Title.text = "事件编辑";
    }

    public void ShowPanel(ColorEventData data, int id)
    {
        this.gameObject.SetActive(true);

        ColorData = data;
        NumData = null;
        ID = id;

        StartTimeInput.text = (Mathf.RoundToInt(ColorData.StartTime * 1000f)).ToString();
        EndTimeInput.text = (Mathf.RoundToInt(ColorData.EndTime * 1000f)).ToString();

        SA.text = (Mathf.RoundToInt(ColorData.StartValue.a * 255f)).ToString();

        EA.text = (Mathf.RoundToInt(ColorData.EndValue.a * 255f)).ToString();

        EasingTypeDropdown.value = (int)ColorData.EasingType;
        EasingIDInput.text = ((int)ColorData.EasingType).ToString();

        StartEasingRangeSlider.value = NewStartEasingRange = ColorData.StartEasingRange;
        EndEasingRangeSlider.value = NewEndEasingRange = ColorData.EndEasingRange;

        ColorSVEdit.SetActive(true);
        ColorEVEdit.SetActive(true);
        NumSVEdit.SetActive(false);
        NumEVEdit.SetActive(false);

        UpdateCruve();

        Title.text = "颜色事件编辑";
    }

    public void OnEndEditSR(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.StartValue.r = to01;
        }
        OnEditValue(false, true);
    }

    public void OnEndEditSG(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.StartValue.g = to01;
        }
        OnEditValue(false, true);
    }

    public void OnEndEditSB(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.StartValue.b = to01;
        }
        OnEditValue(false, true);
    }

    public void OnEndEditSA(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.StartValue.a = to01;
        }
        SA.text = (Mathf.RoundToInt(ColorData.StartValue.a * 255f)).ToString();
        OnEditValue(false, true);
    }

    public void OnEndEditER(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.EndValue.r = to01;
        }
        ER.text = (Mathf.RoundToInt(ColorData.EndValue.r * 255f)).ToString();
        OnEditValue(false, true);
    }

    public void OnEndEditEG(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.EndValue.g = to01;
        }
        EG.text = (Mathf.RoundToInt(ColorData.EndValue.g * 255f)).ToString();
        OnEditValue(false, true);
    }

    public void OnEndEditEB(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.EndValue.b = to01;
        }
        EB.text = (Mathf.RoundToInt(ColorData.EndValue.b * 255f)).ToString();
        OnEditValue(false, true);
    }

    public void OnEndEditEA(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            ColorData.EndValue.a = to01;
        }
        EA.text = (Mathf.RoundToInt(ColorData.EndValue.a * 255f)).ToString();
        OnEditValue(false, true);
    }

    public void OnChangeEasingID(string value)
    {
        if (int.TryParse(value, out int id) && id > -1 && id < 31)
        {
            OnEndEditEasingType(id);
        }
    }

    public void OnEndEditEasingType(int value)
    {
        if (IsColor)
        {
            ColorData.EasingType = (EaseUtility.Ease)value;
        }
        else
        {
            NumData.EasingType = (EaseUtility.Ease)value;
        };
        EasingIDInput.text = value.ToString();
        EasingTypeDropdown.value = value;
        OnEditValue(false, true);
    }

    public void OnEndEditStartValue(string value)
    {
        if (IsColor)
        {

        }
        else
        {
            if (float.TryParse(value, out float sv))
            {
                NumData.StartValue = sv;
            }
            StartValueInput.text = NumData.StartValue.ToString();
        }

        OnEditValue();
    }

    public void OnEndEditEndValue(string value)
    {
        if (IsColor)
        {

        }
        else
        {
            if (float.TryParse(value, out float ev))
            {
                NumData.EndValue = ev;
            }
            EndValueInput.text = NumData.EndValue.ToString();
        }

        OnEditValue();
    }

    public void OnEndEditStartTime(string value)
    {
        if (int.TryParse(value, out int time))
        {
            if (IsColor)
            {
                ColorData.StartTime = time * 0.001f;
            }
            else
            {
                NumData.StartTime = time * 0.001f;
            }
        }

        if (IsColor)
        {
            StartTimeInput.text = (Mathf.RoundToInt(ColorData.StartTime * 1000f)).ToString();
        }
        else
        {
            StartTimeInput.text = (Mathf.RoundToInt(NumData.StartTime * 1000f)).ToString();
        }

        OnEditValue(true);
    }

    public void OnEndEditEndTime(string value)
    {
        if (int.TryParse(value, out int time))
        {
            if (IsColor)
            {
                ColorData.EndTime = time * 0.001f;
            }
            else
            {
                NumData.EndTime = time * 0.001f;
            }
        }

        if (IsColor)
        {
            EndTimeInput.text = (Mathf.RoundToInt(ColorData.EndTime * 1000f)).ToString();
        }
        else
        {
            EndTimeInput.text = (Mathf.RoundToInt(NumData.EndTime * 1000f)).ToString();
        }

        OnEditValue();
    }



    public void ClosePanel()
    {
        NumData = null;
        ColorData = null;
        this.gameObject.SetActive(false);
    }

    void DeleteEvent()
    {
        if (ID > 9)
        {
            EditEventManager.Instance.GetColorEventsFromID(ID).Remove(ColorData);
        }
        else
        {
            EditEventManager.Instance.GetNumEventsFromID(ID).Remove(NumData);
        }

        OnEditValue();
    }

    public void OnEditValue(bool sort = false, bool updCruve = false)
    {
        if (sort)
        {
            if (ID > 9)
            {
                EditEventManager.Instance.GetColorEventsFromID(ID).Sort();
            }
            else
            {
                EditEventManager.Instance.GetNumEventsFromID(ID).Sort();
            }
        }

        if (updCruve)
        {
            UpdateCruve();
        }

        EditManager.Instance.Reload(false);
    }

    void Update()
    {
        ApplyEasingRange();

        if (ID > 9)
        {
            //按下S拖动开始时间
            if (Input.GetKeyDown(KeyCode.S))
            {
                EditEventManager.Instance.DragStartTime(ColorData, ID);
            }
            //按下D拖动结束时间
            else if (Input.GetKeyDown(KeyCode.D))
            {
                EditEventManager.Instance.DragEndTime(ColorData, ID);
            }

            if (!EditEventManager.Instance.GetColorEventsFromID(ID).Contains(ColorData)) ClosePanel();
        }
        else
        {
            //按下S拖动开始时间
            if (Input.GetKeyDown(KeyCode.S))
            {
                EditEventManager.Instance.DragStartTime(NumData, ID);
            }
            //按下D拖动结束时间
            else if (Input.GetKeyDown(KeyCode.D))
            {
                EditEventManager.Instance.DragEndTime(NumData, ID);
            }

            if (!EditEventManager.Instance.GetNumEventsFromID(ID).Contains(NumData)) ClosePanel();
        }
    }
}
