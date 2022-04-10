using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Forever;
using Dreamteck.Splines;
using Infrastructure;
using MC.Models;
using ModelCore;
using Services;
using Services.Inputs;
using UnityEngine;
using UnityEngine.Events;

public class C_ShipByPlayer : MonoBehaviour
{
    public LabelGoSo PlayerLabel;
    public LabelGoSo LevelMark;
    
    public Runner Runner;
    public InputReciverMove InputMove;
    public EntityRef PlayerEntityRef;
    private XYMover _xyMover;
    private XYClamp _xyClapm;

    public UnityEvent<Entity> Inited;
    private DataPathShip _shipData;
    private Entity _player;

    public void Init(Entity playerEntity)
    {
        _player = playerEntity;
        HandlerZMover(playerEntity.Select<ZMover>(x => true));
        HandlerXYMover(playerEntity.Select<XYMover>(x => true));

        _shipData = playerEntity.SelectOrCreate<DataPathShip>();
        _xyClapm = EntityAgregator.Instance.Select(x => x.Label.IsAlias(LevelMark)).Select<XYClamp>();
        
        InputMove.Move += OnMove;
        PlayerEntityRef.Init(playerEntity);
        Inited.Invoke(playerEntity);
        enabled = true;
    }

    private void Update()
    {
        if(_player==null) return;
        _shipData.OffsetInNomal = _xyClapm.GetNormal(Runner);
        _shipData.Progress = LevelGenerator.instance.LocalToGlobalPercent(Runner.result.percent, Runner.segment.index);
        Debug.Log("Persent: "+_shipData.Progress);
    }
    

    private void HandlerZMover(ZMover mover)
    {
        Runner.followSpeed = mover.Speed;
        mover.Changed+=()=>Runner.followSpeed = mover.Speed;
    }

    private void HandlerXYMover(XYMover mover) => _xyMover = mover;

    private void OnMove(Vector2 obj)
    {
        _xyMover.Move(Runner, obj);
        _xyClapm.Clamp(Runner);
    }
}
