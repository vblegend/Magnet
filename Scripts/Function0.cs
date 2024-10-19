using App.Core;
using App.Core.Events;
using Magnet.Core;
using System;


namespace Scripts
{
    [Script(nameof(Function0))]
    public class Function0 : MyScript, IObjectEvent
    {

        [Function(null, "攻击事件")]
        public void OnAttack()
        {
            Script<ScriptA>().Login(null);
            this.PRINT("Attack Event");
        }


        [Function(null, "受伤事件")]
        public void OnHurt()
        {

        }


        [Function(null, "击杀事件")]
        public void OnKilled()
        {
  
        }

        [Function(null, "死亡事件")]
        public void OnDead()
        {
            // Resurrected 
        }

        [Function(null, "复活事件")]
        public void OnResurrected()
        {
           
        }

        /// <summary>
        /// 在吃东西
        /// </summary>
        [Function(null, "吃掉东西")]
        public void OnEat()
        {
            GIVE(MAKE(10024).Alias("Hello").Quality(100).Count(5));
        }



        [Function(null, "打开东西")]
        public void OnOpened(Int32 item)
        {

        }


        [Function(null, "穿上装备")]
        public void OnPutItem()
        {

        }

        [Function(null, "取下装备")]
        public void OnTakeItem()
        {

        }




        [Function(null, "进入地图")]
        public void OnEnterMap()
        {

        }


        [Function(null, "移动了")]
        public void OnMoved()
        {

        }


        [Function(null, "离开地图")]
        public void OnLeaveMap()
        {

        }



        [Function(null, "获得物品")]
        public void OnGetItem()
        {

        }


        [Function(null, "丢失物品")]
        public void OnLostItem()
        {

        }

        [Function(null, "接受任务")]
        public void OnAcceptTask()
        {

        }

        [Function(null, "完成任务")]
        public void OnCompleteTask()
        {

        }


    }
}
