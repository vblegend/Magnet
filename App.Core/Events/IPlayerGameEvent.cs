using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Events
{
    public interface IPlayerGameEvent
    {
        void OnMagicAttack();
        void OnAttack();
        void OnStruck(); 
        void OnKilled();
        void OnDead();
        void OnResurrected();
        void OnEat();
        void OnOpened(Int32 item);
        void OnTakeOn();
        void OnTakeOff();
        void OnEnterMap();
        void OnMoved();
        void OnLeaveMap();
        void OnPickUpItem();
        void OnPickDropItem();
        void OnAcceptTask();
        void OnCompleteTask();
        void OnLevelUp();
    }
}
