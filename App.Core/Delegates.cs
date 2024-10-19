using System;

namespace App.Core
{
    public delegate void OnAttackDelegate();
    public delegate void OnHurtDelegate();
    public delegate void OnKilledDelegate();
    public delegate void OnDeadDelegate();
    public delegate void OnEatDelegate();
    public delegate void OnOpenedDelegate(Int32 item);
    public delegate void OnPutItemDelegate();
    public delegate void OnTakeItemDelegate();
    public delegate void OnEnterMapDelegate();
    public delegate void OnMovedDelegate();
    public delegate void OnLeaveMapDelegate();
    public delegate void OnGetItemDelegate();
    public delegate void OnLostItemDelegate();
    public delegate void OnAcceptTaskDelegate();
    public delegate void OnCompleteTaskDelegate();
}
