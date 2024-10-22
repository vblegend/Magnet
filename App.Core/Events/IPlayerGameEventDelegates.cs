using System;

namespace App.Core.Events
{

    /// <summary>
    /// 魔法攻击事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnMagicAttackHandler(IMagicAttackContext ctx);

    /// <summary>
    /// 攻击事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnAttackHandler(IAttackContext ctx);

    /// <summary>
    /// 受伤事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnStruckHandler(IStruckContext ctx);

    /// <summary>
    /// 击杀事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnKillHandler(IKillContext ctx);

    /// <summary>
    /// 死亡事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnDeadHandler(IDeadContext ctx);

    /// <summary>
    /// 复活事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnResurrectedHandler(IResurrectedContext ctx);

    /// <summary>
    /// 吃东西事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnEatHandler(IEatContext ctx);

    /// <summary>
    /// 打开东西事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnOpenHandler(IOpenContext ctx);

    /// <summary>
    /// 穿上装备事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnTakeOnHandler(ITakeOnContext ctx);

    /// <summary>
    /// 脱下装备事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnTakeOffHandler(ITakeOffContext ctx);

    /// <summary>
    /// 进入地图事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnEnterMapHandler(IEnterMapContext ctx);

    /// <summary>
    /// 对象移动事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnMovedHandler(IMoveContext ctx);

    /// <summary>
    /// 离开地图事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnLeaveMapHandler(ILeaveMapContext ctx);

    /// <summary>
    /// 捡起物品事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnPickUpItemHandler(IPickUpItemContext ctx);

    /// <summary>
    /// 丢弃物品事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnPickDropItemHandler(IPickDropItemContext ctx);

    /// <summary>
    /// 接受任务事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnAcceptMissionHandler(IAcceptMissionContext ctx);


    /// <summary>
    /// 放弃任务事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void AbortMissionHandler(IAbortMissionContext ctx);


    /// <summary>
    /// 完成任务事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnCompleteMissionHandler(ICompleteMissionContext ctx);

    /// <summary>
    /// 等级提升事件脚本
    /// </summary>
    /// <param name="ctx"></param>
    public delegate void OnLevelUpHandler(ILevelUpContext ctx);
}
