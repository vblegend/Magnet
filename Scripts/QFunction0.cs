using App.Core;
using App.Core.Events;
using Magnet.Core;



[Script(nameof(QFunction0))]
public class QFunction0 : GameScript, IPlayerGameEvent
{
    [Function(null, "攻击事件")]
    void IPlayerGameEvent.OnAttack(IAttackContext ctx)
    {

    }

    [Function(null, "魔法攻击事件")]
    void IPlayerGameEvent.OnMagicAttack(IMagicAttackContext ctx)
    {

    }

    [Function(null, "受伤事件")]
    void IPlayerGameEvent.OnStruck(IStruckContext ctx)
    {

    }

    [Function(null, "复活事件")]
    void IPlayerGameEvent.OnResurrected(IResurrectedContext ctx)
    {

    }

    [Function(null, "击杀事件")]
    void IPlayerGameEvent.OnKill(IKillContext ctx)
    {

    }

    [Function(null, "死亡事件")]
    void IPlayerGameEvent.OnDead(IDeadContext ctx)
    {

    }

    [Function(null, "吃掉东西")]
    void IPlayerGameEvent.OnEat(IEatContext ctx)
    {

    }

    [Function(null, "打开东西")]
    void IPlayerGameEvent.OnOpen(IOpenContext ctx)
    {

    }

    [Function(null, "进入地图")]
    void IPlayerGameEvent.OnEnterMap(IEnterMapContext ctx)
    {

    }

    [Function(null, "移动了")]
    void IPlayerGameEvent.OnMoved(IMoveContext ctx)
    {

    }

    [Function(null, "离开地图")]
    void IPlayerGameEvent.OnLeaveMap(ILeaveMapContext ctx)
    {

    }

    [Function(null, "捡到物品")]
    void IPlayerGameEvent.OnPickUpItem(IPickUpItemContext ctx)
    {

    }

    [Function(null, "丢掉物品")]
    void IPlayerGameEvent.OnPickDropItem(IPickDropItemContext ctx)
    {

    }

    [Function(null, "穿上装备")]
    void IPlayerGameEvent.OnTakeOn(ITakeOnContext ctx)
    {

    }

    [Function(null, "取下装备")]
    void IPlayerGameEvent.OnTakeOff(ITakeOffContext ctx)
    {

    }

    [Function(null, "接受任务")]
    void IPlayerGameEvent.OnAcceptMission(IAcceptMissionContext ctx)
    {

    }

    [Function(null, "放弃任务")]
    void IPlayerGameEvent.AbortMission(IAbortMissionContext ctx)
    {

    }

    [Function(null, "完成任务")]
    void IPlayerGameEvent.OnCompleteMission(ICompleteMissionContext ctx)
    {

    }

    [Function(null, "等级提升")]
    void IPlayerGameEvent.OnLevelUp(ILevelUpContext ctx)
    {

    }


}
