using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using UnityEngine.UI;

public class FrameRateLimiter : MonoBehaviour
{
public int targetFrameRate = -1;
}

public class EditEventControl : MonoBehaviour
{
    public int ID;
    public RectTransform Rect;
    public Text SVText, EVText;
    public Image SVImage, EVImage;
    public Button CurrentButton;

    public bool IsColor;
    public NumEventData NumData;
    public ColorEventData ColorData;

    void Awake()
    {
        CurrentButton.onClick.AddListener(TryShowPanel);
    }

    public void Init(NumEventData data, int id)
    {
        IsColor = false;

        NumData = data;
        ID = id;

        SVImage.gameObject.SetActive(false);
        EVImage.gameObject.SetActive(false);

        SVText.gameObject.SetActive(true);
        EVText.gameObject.SetActive(true);

        SVText.text = NumData.StartValue.ToString();
        EVText.text = NumData.EndValue.ToString();
        SVText.color = new Color(1f,1f,1f,1f);
        EVText.color = new Color(1f,1f,1f,1f);
    }

    public void Init(ColorEventData data, int id)
    {
        IsColor = true;

        ColorData = data;
        ID = id;
        /*
        SVText.gameObject.SetActive(false);
        EVText.gameObject.SetActive(false);

        SVImage.gameObject.SetActive(true);
        EVImage.gameObject.SetActive(true);

        SVImage.color = ColorData.StartValue;
        EVImage.color = ColorData.EndValue;
        */
        SVImage.gameObject.SetActive(true);
        EVImage.gameObject.SetActive(true);

        SVText.gameObject.SetActive(true);
        EVText.gameObject.SetActive(true);

        SVImage.color = new Color(1f, 1f, 1f, ColorData.StartValue.a);
        EVImage.color = new Color(1f, 1f, 1f, ColorData.EndValue.a);

        SVText.text = ColorData.StartValue.a.ToString();
        EVText.text = ColorData.EndValue.a.ToString();

        SVText.color = new Color(1f,0.3f,0f,1f);
        EVText.color = new Color(1f,0.3f,0f,1f);
    }

    void Update()
    {
        if (EventEditPanelControl.Instance.NumData == NumData || EventEditPanelControl.Instance.ColorData == ColorData)
        {
            CurrentButton.image.color = Color.black;
        }
        else
        {
            CurrentButton.image.color = new Color(1f, 1f, 1f, 0.6f);
        }
    }

    void TryShowPanel()
    {
        if (Input.GetMouseButton(1))
        {
            if (ID < 10)
            {
                EditEventManager.Instance.GetNumEventsFromID(ID).Remove(NumData);
            }
            else
            {
                EditEventManager.Instance.GetColorEventsFromID(ID).Remove(ColorData);
            }

            //毕竟删除东西是不影响排序的
            EditManager.Instance.Reload(false);
        }
        else
        {
            if (IsColor)
            {
                EventEditPanelControl.Instance.ShowPanel(ColorData, ID);
            }
            else
            {
                EventEditPanelControl.Instance.ShowPanel(NumData, ID);
            }

        }
    }

}
