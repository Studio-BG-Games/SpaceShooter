using System;
using System.Collections;
using System.Collections.Generic;
using DIContainer;
using Dreamteck.Forever;
using Dreamteck.Splines;
using Infrastructure;
using MC.Models;
using ModelCore;
using Services;
using Services.Inputs;
using Services.PauseManagers;
using Services.RecoveryManagers;
using Sirenix.Utilities;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

public class C_ShipByPlayer : MonoBehaviour
{
    public LabelGoSo LevelMark;
    
    public Runner Runner;
    public InputReciverMove InputMove;
    public EntityRef PlayerEntityRef;
    private XYMover _xyMover;
    private XYClamp _xyClapm;

    public UnityEvent<Entity> Inited;
    private DataPathShip _shipData;
    private Entity _player;

    public UltEvent OnRecovery;
    
    private ResolveSingle<PauseGame> _pauseGame = new ResolveSingle<PauseGame>();
    private ResolveSingle<RecoveryPlayerServices> _recovery = new ResolveSingle<RecoveryPlayerServices>();

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

        _recovery.Depence.Recovered += () => Recovery();
        _pauseGame.Depence.IsPause.Updated += x => Runner.enabled = !x;
    }

    private void Recovery()
    {
        Runner.enabled = true;
        OnRecovery.Invoke();
        _player.SelectAll<Health>().ForEach(x => x.Current = x.Max);
    }

    private void Update()
    {
        if(_player==null) return;
        _shipData.OffsetInNomal = _xyClapm.GetNormal(Runner);
        
        _shipData.Progress = LevelGenerator.instance.LocalToGlobalPercent(Runner.result.percent, Runner.segment.index);
    }
    

    private void HandlerZMover(ZMover mover)
    {
        Runner.followSpeed = mover.SpeedByDiraction;
        mover.Changed+=()=>Runner.followSpeed = mover.SpeedByDiraction;
    }

    private void HandlerXYMover(XYMover mover) => _xyMover = mover;

    private void OnMove(Vector2 obj)
    {
        if(_pauseGame.Depence.IsPause.Value) return;
        _xyMover.Move(Runner, obj);
        _xyClapm.Clamp(Runner);
    }
}
