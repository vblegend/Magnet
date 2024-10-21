
namespace App.Core.Events
{
    public interface IPlayerGameEvent
    {
        /// <summary>
        /// 魔法攻击事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnMagicAttack(IMagicAttackContext ctx);

        /// <summary>
        /// 攻击事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnAttack(IAttackContext ctx);

        /// <summary>
        /// 受伤事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnStruck(IStruckContext ctx); 

        /// <summary>
        /// 击杀事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnKill(IKillContext ctx);

        /// <summary>
        /// 死亡事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnDead( IDeadContext ctx);

        /// <summary>
        /// 复活事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnResurrected(IResurrectedContext ctx);

        /// <summary>
        /// 吃东西事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnEat(IEatContext ctx);

        /// <summary>
        /// 打开东西事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnOpen(IOpenContext ctx);

        /// <summary>
        /// 穿上装备事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnTakeOn(ITakeOnContext ctx);

        /// <summary>
        /// 脱下装备事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnTakeOff(ITakeOffContext ctx);

        /// <summary>
        /// 进入地图事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnEnterMap(IEnterMapContext ctx);

        /// <summary>
        /// 对象移动事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnMoved(IMoveContext ctx);

        /// <summary>
        /// 离开地图事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnLeaveMap(ILeaveMapContext ctx);

        /// <summary>
        /// 捡起物品事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnPickUpItem(IPickUpItemContext ctx);

        /// <summary>
        /// 丢弃物品事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnPickDropItem(IPickDropItemContext ctx);

        /// <summary>
        /// 接受任务事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnAcceptMission(IAcceptMissionContext ctx);


        /// <summary>
        /// 放弃任务事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void AbortMission(IAbortMissionContext ctx);


        /// <summary>
        /// 完成任务事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnCompleteMission(ICompleteMissionContext ctx);

        /// <summary>
        /// 等级提升事件脚本
        /// </summary>
        /// <param name="ctx"></param>
        void OnLevelUp(ILevelUpContext ctx);
    }
}
