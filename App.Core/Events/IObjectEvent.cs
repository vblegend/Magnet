using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Events
{
    public interface IObjectEvent
    {
        void OnAttack();
        void OnHurt();
        void OnKilled();
        void OnDead();
        void OnResurrected();
        void OnEat();
        void OnOpened(Int32 item);
        void OnPutItem();
        void OnTakeItem();
        void OnEnterMap();
        void OnMoved();
        void OnLeaveMap();
        void OnGetItem();
        void OnLostItem();
        void OnAcceptTask();
        void OnCompleteTask();

    }
}
