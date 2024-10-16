using App.Core;
using System;


public interface IKilledContext : IObjectContext
{

    Object Murderer { get; }

    DateTime KilledTime { get; }





}
