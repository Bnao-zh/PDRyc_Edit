using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Utils.Singleton;
using PDRO.Data.ScriptableObject;
using PDRO.Data;

public class DataManager : MonoSingleton<DataManager>
{
    public MusicDataObject[] MusicDataObjects;
    public Dictionary<int, MusicData> MusicDataDic = new();
    protected override void OnAwake()
    {
        //初始化字典喵
        foreach (MusicDataObject obj in MusicDataObjects)
        {
            var data = obj.CurrentData;

            MusicDataDic.Add(data.ID, data);
        }
    }

    private int LastLoadID = -1;
    private AudioClip LastLoadClip;
    public AudioClip GetAudioClipByID(int id)
    {
        //如果是重新加载就直接返回
        if (id == LastLoadID)
        {
            return LastLoadClip;
        }

        //Debug.Log($"{id},{LastLoadID}");

        //如果加载过而且要加载其他的就先卸载;
        if (id != -1)
        {
            Resources.UnloadAsset(LastLoadClip);
        }

        //加载其他的
        if (!MusicDataDic.ContainsKey(id))
        {
            throw new System.Exception("未找到id所对应的歌曲数据");
        }

        LastLoadClip = Resources.Load<AudioClip>($"MusicData/{id}/Audio");
        if (LastLoadClip == null)
        {
            throw new System.Exception("加载了个寂寞");
        }
        LastLoadID = id;
        return LastLoadClip;
    }

    public bool HasMusicID(int id)
    {
        return MusicDataDic.ContainsKey(id);
    }

    public MusicData GetMusicDataByID(int id)
    {
        if (!MusicDataDic.ContainsKey(id))
        {
            throw new System.Exception("未找到id所对应的歌曲数据");
        }

        return MusicDataDic[id];
    }

    public MusicData GetNowMusicData()
    {
        if (LastLoadID == -1) throw new System.Exception("还没加载找不到喵");

        return GetMusicDataByID(LastLoadID);
    }

}
