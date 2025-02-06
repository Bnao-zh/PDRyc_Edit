using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using System.Linq;
using UnityEngine.UI;
using PDRO.Utils.Singleton;
using PDRO.Utils;

public class EditEventManager : MonoSingleton<EditEventManager>
{
    public EditEventControl EditEventPrefab;
    public RectTransform EditEventTransform;
    public List<EditEventControl> EditEvents = new();

    public int EditEventLayerIndex = 0;
    public int EditEventPosXIndex = 0;

    public void OnUpdate()
    {
        if (EditManager.Instance.CurrentEditType is EditManager.EditType.Events)
        {
            EditEventTransform.gameObject.SetActive(true);
        }
        else
        {
            EditEventTransform.gameObject.SetActive(false);
            return;
        }

        GetIndex = 0;

        // 一共有多少种事件的索引
        var eventTypeNum = EditManager.Instance.EditChartObjectIndex == -1 ? 7 : 11;

        for (var i = 0; i < eventTypeNum; i++)
        {
            if (i < 8)
            {
                var events = GetNumEventsFromID(i);

                for (var j = 0; j < events.Count; j++)
                {
                    var ev = events[j];

                    var dis = (EditManager.Instance.Time2Beat(ev.StartTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
                    var endDis = (EditManager.Instance.Time2Beat(ev.EndTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;

                    if (endDis < -400) continue;
                    if (dis > 1200f) break;

                    RenderEditEvent(ev, dis, endDis, i);
                }
            }
            else if (i > 9)
            {
                var events = GetColorEventsFromID(i);

                for (var j = 0; j < events.Count; j++)
                {
                    var ev = events[j];

                    var dis = (EditManager.Instance.Time2Beat(ev.StartTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
                    var endDis = (EditManager.Instance.Time2Beat(ev.EndTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;

                    if (endDis < -400) continue;
                    if (dis > 1200f) break;

                    RenderEditEvent(ev, dis, endDis, i);
                }
            }

        }

        for (var i = GetIndex; i < EditEvents.Count; i++)
        {
            EditEvents[i].gameObject.SetActive(false);
        }

        UpdateInput();
    }

    public List<NumEventData> GetNumEventsFromID(int id)
    {
        if (EditManager.Instance.EditChartObjectIndex == -1)
        {
            var layer = EditManager.Instance.EditingChart.Camera.Events[EditEventLayerIndex];
            switch (id)
            {
                case 0:
                    return layer.MoveXEvents;
                case 1:
                    return layer.MoveYEvents;
                case 2:
                    return layer.MoveZEvents;
                case 3:
                    return layer.RotateXEvents;
                case 4:
                    return layer.RotateYEvents;
                case 5:
                    return layer.RotateZEvents;
                case 6:
                    return layer.FovEvents;
                default:
                    throw new System.Exception("我传的什么jb");
            }
        }
        else//Track的
        {
            var layer = EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Events[EditEventLayerIndex];
            switch (id)
            {
                case 0:
                    return layer.MoveXEvents;
                case 1:
                    return layer.MoveYEvents;
                case 2:
                    return layer.MoveZEvents;
                case 3:
                    return layer.RotateXEvents;
                case 4:
                    return layer.RotateYEvents;
                case 5:
                    return layer.RotateZEvents;
                case 6:
                    return layer.NoteSpeedEvents;
                case 7:
                    return layer.UpLengthEvents;
                default:
                    throw new System.Exception("或许你该去颜色区域");
            }
        }
    }

    public List<ColorEventData> GetColorEventsFromID(int id)
    {
        if (EditManager.Instance.EditChartObjectIndex == -1) throw new System.Exception("你不该来这儿的");

        var layer = EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Events[EditEventLayerIndex];
        switch (id)
        {
            case 10:
                return layer.UpSurfaceColorEvents;
            default:
                throw new System.Exception("头好痒，要长脑子了吗");
        }
    }


    void RenderEventByEventData(NumEventData ev, int posXid)
    {
        var dis = (EditManager.Instance.Time2Beat(ev.StartTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
        var endDis = (EditManager.Instance.Time2Beat(ev.EndTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;

        RenderEditEvent(ev, dis, endDis, posXid);
    }

    void RenderEventByEventData(ColorEventData ev, int posXid)
    {
        var dis = (EditManager.Instance.Time2Beat(ev.StartTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
        var endDis = (EditManager.Instance.Time2Beat(ev.EndTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;

        RenderEditEvent(ev, dis, endDis, posXid);
    }

    void RenderEditEvent(ColorEventData ev, float dis, float endDis, int posXid)
    {
        var ins = GetEditNote();
        ins.Init(ev, posXid);
        ins.gameObject.SetActive(true);

        if (dis < -400f) dis = -400f;
        if (endDis > 1200f) endDis = 1200f;

        ins.transform.localPosition = new Vector3(-325f + 50f * posXid, (dis + endDis) * 0.5f, 0f);
        ins.Rect.sizeDelta = new Vector2(50f, Mathf.Max(endDis - dis, 40f));
    }

    void RenderEditEvent(NumEventData ev, float dis, float endDis, int posXid)
    {
        var ins = GetEditNote();
        ins.Init(ev, posXid);
        ins.gameObject.SetActive(true);

        if (dis < -400f) dis = -400f;
        if (endDis > 1200f) endDis = 1200f;

        ins.transform.localPosition = new Vector3(-325f + 50f * posXid, (dis + endDis) * 0.5f, 0f);
        ins.Rect.sizeDelta = new Vector2(50f, Mathf.Max(endDis - dis, 40f));
    }

    private int GetIndex;

    EditEventControl GetEditNote()
    {
        GetIndex++;

        if (GetIndex > EditEvents.Count)
        {
            var ins = Instantiate(EditEventPrefab, EditEventTransform);
            EditEvents.Add(ins);
        }

        return EditEvents[GetIndex - 1];
    }

    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddEvent(Input.mousePosition);
        }
    }

    void AddEvent(Vector2 mousePos)
    {
        var posX = (mousePos.x - EditEventTransform.position.x) / (EditEventTransform.position.x / 400f);

        Debug.Log($"{mousePos.x},{EditEventTransform.position.x}");

        if (Mathf.Abs(posX) > 350f) return;

        var id = Mathf.RoundToInt((posX + 325f) / 50f);

        if (EditManager.Instance.EditChartObjectIndex == -1)
        {
            //camera
            if (id > 6) return;
        }
        else
        {
            //track
            if (id > 10||id == 8||id == 9) return;

        }

        var beatLine = BeatLineManager.Instance.CurrentBeatLines
            .OrderBy(x => Mathf.Abs(x.transform.position.y - mousePos.y)).First();

        var nowTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

        if (id > 9)
        {
            //颜色事件区域

            //自动填充开始时间的事件值
            var temp = GameplayUtility.GetColorFromColorEvent(nowTime, GetColorEventsFromID(id));

            StartCoroutine(ColorEventEndTimeEdit(new ColorEventData
            {
                StartValue = temp,
                EndValue = temp,
                StartTime = nowTime,
                EndTime = nowTime,
                EasingType = PDRO.Utils.EaseUtility.Ease.Linear,
                StartEasingRange = 0f,
                EndEasingRange = 1f
            }, id));
        }
        else
        {
            //普通事件区域

            //自动填充开始时间的事件值
            var temp = GameplayUtility.GetValueFromTrackEvent(nowTime, GetNumEventsFromID(id));

            StartCoroutine(NumEventEndTimeEdit(new NumEventData
            {
                StartValue = temp,
                EndValue = temp,
                StartTime = nowTime,
                EndTime = nowTime,
                EasingType = PDRO.Utils.EaseUtility.Ease.Linear,
                StartEasingRange = 0f,
                EndEasingRange = 1f
            }, id));
        }
    }

    IEnumerator NumEventEndTimeEdit(NumEventData ev, int id)
    {
        while (!Input.GetKeyUp(KeyCode.E))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            ev.EndTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (ev.EndTime < ev.StartTime)
            {
                ev.StartTime = ev.EndTime;
            }
            RenderEventByEventData(ev, id);

            yield return null;
        }

        GetNumEventsFromID(id).Add(ev);
        GetNumEventsFromID(id).Sort();
        EditManager.Instance.Reload(false);
    }

    IEnumerator ColorEventEndTimeEdit(ColorEventData ev, int id)
    {
        while (!Input.GetKeyUp(KeyCode.E))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            ev.EndTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (ev.EndTime < ev.StartTime)
            {
                ev.StartTime = ev.EndTime;
            }

            RenderEventByEventData(ev, id);

            yield return null;
        }

        GetColorEventsFromID(id).Add(ev);
        GetColorEventsFromID(id).Sort();
        EditManager.Instance.Reload(false);
    }


    //两种四个开始结束时间的拖动

    //给外部用的
    public void DragStartTime(NumEventData data, int id)
    {
        StartCoroutine(DragStartTimeCoroutine(data, id));
    }

    public void DragStartTime(ColorEventData data, int id)
    {
        StartCoroutine(DragStartTimeCoroutine(data, id));
    }

    public void DragEndTime(NumEventData data, int id)
    {
        StartCoroutine(DragEndTimeCoroutine(data, id));
    }

    public void DragEndTime(ColorEventData data, int id)
    {
        StartCoroutine(DragEndTimeCoroutine(data, id));
    }

    //拖动开始时间
    IEnumerator DragStartTimeCoroutine(NumEventData data, int id)
    {
        EditEventManager.Instance.GetNumEventsFromID(id).Remove(data);

        while (!Input.GetKeyUp(KeyCode.S))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            data.StartTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (data.StartTime > data.EndTime)
            {
                data.EndTime = data.StartTime;
            }

            RenderEventByEventData(data, id);

            yield return null;
        }

        GetNumEventsFromID(id).Add(data);
        GetNumEventsFromID(id).Sort();
        EditManager.Instance.Reload(false);
    }

    IEnumerator DragStartTimeCoroutine(ColorEventData data, int id)
    {
        EditEventManager.Instance.GetColorEventsFromID(id).Remove(data);

        while (!Input.GetKeyUp(KeyCode.S))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            data.StartTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (data.StartTime > data.EndTime)
            {
                data.EndTime = data.StartTime;
            }

            RenderEventByEventData(data, id);

            yield return null;
        }

        GetColorEventsFromID(id).Add(data);
        GetColorEventsFromID(id).Sort();
        EditManager.Instance.Reload(false);
    }

    IEnumerator DragEndTimeCoroutine(NumEventData data, int id)
    {
        EditEventManager.Instance.GetNumEventsFromID(id).Remove(data);

        while (!Input.GetKeyUp(KeyCode.D))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            data.EndTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (data.EndTime < data.StartTime)
            {
                data.StartTime = data.EndTime;
            }

            RenderEventByEventData(data, id);

            yield return null;
        }

        GetNumEventsFromID(id).Add(data);
        GetNumEventsFromID(id).Sort();
        EditManager.Instance.Reload(false);
    }

    IEnumerator DragEndTimeCoroutine(ColorEventData data, int id)
    {
        EditEventManager.Instance.GetColorEventsFromID(id).Remove(data);

        while (!Input.GetKeyUp(KeyCode.D))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            data.EndTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (data.EndTime < data.StartTime)
            {
                data.StartTime = data.EndTime;
            }

            RenderEventByEventData(data, id);

            yield return null;
        }

        GetColorEventsFromID(id).Add(data);
        GetColorEventsFromID(id).Sort();
        EditManager.Instance.Reload(false);
    }


}
