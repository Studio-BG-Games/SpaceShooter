using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Forever;
using ModelCore;
using Services;
using Services.Inputs;
using UnityEngine;
using UnityEngine.Events;

public class C_ShipByPlayer : MonoBehaviour
{
    public LabelGoSo PlayerLabel;
    public Runner Runner;
    public InputReciverMove InputMove;
    public EntityRef PlayerEntityRef;
    private XYMover _xyMover;

    public UnityEvent<Entity> Inited;

    private void Start()
    {
        var player = EntityAgregator.Instance.Select(x => x.Label.IsAlias(PlayerLabel));
        if (player == null)
        {
            Debug.LogWarning("No player entity", this);
            return;
        }
        
        HandlerZMover(player.Select<ZMover>(x => true));
        HandlerXYMover(player.Select<XYMover>(x => true));
        
        InputMove.Move += OnMove;
        PlayerEntityRef.Init(player);
        Inited.Invoke(player);
    }

    private void HandlerZMover(ZMover mover)
    {
        Runner.followSpeed = mover.Speed;
        mover.Changed+=()=>Runner.followSpeed = mover.Speed;
    }

    private void HandlerXYMover(XYMover mover)
    {
        _xyMover = mover;
    }

    private void OnMove(Vector2 obj) => _xyMover.Move(Runner, obj);
}
