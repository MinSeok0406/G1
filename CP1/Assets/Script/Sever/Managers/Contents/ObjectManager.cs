﻿using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    //public MyPlayerControl MyPlayer { get; set; }
    public MyPlayerControl MyPlayer { get; set; }
    public Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    /*public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }*/

    public void Add(PlayerInfo info, bool myPlayer = false)
    {
        /*if (MyPlayer != null && MyPlayer.Id == info.ObjectId)
            return;

        if (_objects.ContainsKey(info.ObjectId))
            return;*/

        //GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        if (myPlayer)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
            go.name = info.Name;
            _objects.Add(info.PlayerId, go);

            MyPlayer = go.GetComponent<MyPlayerControl>();
            MyPlayer.Id = info.PlayerId;
            MyPlayer.PosInfo = info.PosInfo;
            //MyPlayer.SyncPos();
        }
        else
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Player");
            go.name = info.Name;
            _objects.Add(info.PlayerId, go);

            Player pc = go.GetComponent<Player>();
            pc.Id = info.PlayerId;
            pc.PosInfo = info.PosInfo;
            //pc.SyncPos();
        }
    }

    public void Remove(int id)
    {
        if (MyPlayer != null && MyPlayer.Id == id)
            return;

        if (_objects.ContainsKey(id) == false)
            return;

        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject FindCreature(Vector3 cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            Player player = obj.GetComponent<Player>();
            if (player == null)
                continue;

            if (player.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public GameObject Find(Vector3 cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            Player p = obj.GetComponent<Player>();
            if (p == null)
                continue;

            if (p.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    /*public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }*/

    public void RemoveMyPlayer()
    {
        if (MyPlayer == null)
            return;

        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);
        _objects.Clear();
        MyPlayer = null;
    }
}
