using App.Core;
using App.Core.Events;
using Magnet.Core;
using System;



[Script(nameof(QFunction0))]
public class QFunction0 : MyScript, IPlayerGameEvent
{
    [Function(null, "攻击事件")]
    public void OnAttack(IAttackContext ctx)
    {

    }

    [Function(null, "魔法攻击事件")]
    public void OnMagicAttack(IMagicAttackContext ctx)
    {

    }

    [Function(null, "受伤事件")]
    public void OnStruck(IStruckContext ctx)
    {

    }

    [Function(null, "复活事件")]
    public void OnResurrected(IResurrectedContext ctx)
    {

    }

    [Function(null, "击杀事件")]
    public void OnKill(IKillContext ctx)
    {

    }

    [Function(null, "死亡事件")]
    public void OnDead(IDeadContext ctx)
    {
    
    }

    [Function(null, "吃掉东西")]
    public void OnEat(IEatContext ctx)
    {
    
    }

    [Function(null, "打开东西")]
    public void OnOpen(IOpenContext ctx)
    {

    }

    [Function(null, "进入地图")]
    public void OnEnterMap(IEnterMapContext ctx)
    {
    
    }

    [Function(null, "移动了")]
    public void OnMoved(IMoveContext ctx)
    {

    }

    [Function(null, "离开地图")]
    public void OnLeaveMap(ILeaveMapContext ctx)
    {
    
    }

    [Function(null, "捡到物品")]
    public void OnPickUpItem(IPickUpItemContext ctx)
    {

    }

    [Function(null, "丢掉物品")]
    public void OnPickDropItem(IPickDropItemContext ctx)
    {
    
    }

    [Function(null, "穿上装备")]
    public void OnTakeOn(ITakeOnContext ctx)
    {
    
    }

    [Function(null, "取下装备")]
    public void OnTakeOff(ITakeOffContext ctx)
    {

    }

    [Function(null, "接受任务")]
    public void OnAcceptMission(IAcceptMissionContext ctx)
    {

    }

    [Function(null, "放弃任务")]
    public void AbortMission(IAbortMissionContext ctx)
    {

    }

    [Function(null, "完成任务")]
    public void OnCompleteMission(ICompleteMissionContext ctx)
    {

    }

    [Function(null, "等级提升")]
    public void OnLevelUp(ILevelUpContext ctx)
    {

    }


}
