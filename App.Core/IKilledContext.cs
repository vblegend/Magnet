using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IKilledContext : IObjectContext
{

    Object Murderer { get; }

    DateTime KilledTime { get; }



}
