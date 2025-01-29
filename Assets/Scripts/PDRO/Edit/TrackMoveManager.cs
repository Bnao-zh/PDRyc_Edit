using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Utils.Singleton;

public class TrackMoveManager : MonoSingleton<TrackMoveManager>
{
    public void UpMove(int ObjectID, bool reload = true)
    {
        if (ObjectID == -1)
        {
            Debug.LogError("不可以移动摄像机哦");
        }
        else
        {
            //检查是否为最上面那个
            if (ObjectID == 0)
            {
                Debug.LogError("不可以再上移了");
            }
            else//应该没别的问题了，开移！
            {
                Init();
                //先改父物体
                TryChangeFather(ObjectID, true);
                TryChangeFather(ObjectID - 1, false);

                //交换位置
                var tracks = EditManager.Instance.EditingChart.Tracks;
                (tracks[ObjectID], tracks[ObjectID - 1]) = (tracks[ObjectID - 1], tracks[ObjectID]);

                //重新加载
                if (reload)
                {
                    EditManager.Instance.Reload(false);
                }
            }
        }
    }

    public void DownMove(int ObjectID, bool reload = true)
    {
        if (ObjectID == -1)
        {
            Debug.LogError("不可以移动摄像机哦");
        }
        else
        {
            //检查是否为最下面那个
            if (ObjectID == EditManager.Instance.EditingChart.Tracks.Count - 1)
            {
                Debug.LogError("不可以再下移了");
            }
            else//应该没别的问题了，开移！
            {
                Init();
                //先改父物体
                TryChangeFather(ObjectID, false);
                TryChangeFather(ObjectID + 1, true);

                //交换位置
                var tracks = EditManager.Instance.EditingChart.Tracks;
                (tracks[ObjectID], tracks[ObjectID + 1]) = (tracks[ObjectID + 1], tracks[ObjectID]);

                //重新加载
                if (reload)
                {
                    EditManager.Instance.Reload(false);
                }
            }
        }
    }

    void Init()
    {
        ChangeFatherIDTrackList.Clear();
        ChangeTargetIDLineDict.Clear();
        ChangeEditIndex = false;
    }

    public List<int> ChangeFatherIDTrackList = new();
    bool ChangeEditIndex;

    public Dictionary<int, List<int>> ChangeTargetIDLineDict = new();

    //封装了一下的改某个以某个ID为父物件的玩意
    //现在又加入了改连线目标ID的功能
    void TryChangeFather(int id, bool isUp)
    {
        var chart = EditManager.Instance.EditingChart;

        //摄像机
        if (chart.Camera.FatherTrackIndex == id && !ChangeFatherIDTrackList.Contains(-1))
        {
            chart.Camera.FatherTrackIndex = NewID();
            ChangeFatherIDTrackList.Add(-1);
        }

        //轨道
        for (var i = 0; i < chart.Tracks.Count; i++)
        {
            //防止重复改变
            if (ChangeFatherIDTrackList.Contains(i)) continue;

            var track = chart.Tracks[i];

            if (track.FatherTrackIndex == id)
            {
                track.FatherTrackIndex = NewID();
                ChangeFatherIDTrackList.Add(i);
            }
        }

        //连线目标
        for (var i = 0; i < chart.Tracks.Count; i++)
        {
            var lines = chart.Tracks[i].Lines;

            for (var j = 0; j < lines.Count; j++)
            {
                var line = lines[j];

                if (line.TargetTrackIndex == id)
                {
                    //检查是否要移了

                    //如果这个轨道有line移过
                    if (ChangeTargetIDLineDict.ContainsKey(i))
                    {
                        //先拿到对应的List
                        var list = ChangeTargetIDLineDict[i];

                        //移过了
                        if (list.Contains(j))
                        {
                            continue;
                        }
                        else
                        {
                            line.TargetTrackIndex = NewID();
                            list.Add(j);
                        }
                    }
                    else
                    {
                        //没有就直接移顺便新建
                        var list = new List<int>();

                        line.TargetTrackIndex = NewID();
                        list.Add(j);

                        ChangeTargetIDLineDict.Add(i, list);
                    }
                }
            }

        }

        //编辑序号
        if (EditManager.Instance.EditChartObjectIndex == id && !ChangeEditIndex)
        {
            EditManager.Instance.EditChartObjectIndex = NewID();
            ChangeEditIndex = true;
        }

        int NewID()
        {
            return isUp ? id - 1 : id + 1;
        }
    }
}
