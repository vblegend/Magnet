using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Events
{
	public interface IAttackContext
	{
        IMap Map { get; }
        Point Position { get; }
        IObjectContext Target { get; }
    }

	public interface IMagicAttackContext
	{
        IMap Map { get; }
        Point Position { get; }
        IObjectContext Target { get; }
    }
	public interface IStruckContext
	{
        IMap Map { get; }
        Point Position { get; }
        IObjectContext Target { get; }
    }

	public interface IKillContext
	{
        IMap Map { get; }
        IObjectContext Target { get; }
    }

	public interface IDeadContext
	{
        IMap  Map { get; }
        IObjectContext Killer { get; }
    }

	public interface IResurrectedContext
	{

	}

	public interface IEatContext
	{
        Int32 ItemId { get; }
    }

	public interface IOpenContext
	{
		Int32 ItemId {  get; }
	}

	public interface ITakeOnContext
	{
        Int32 Position { get; }
    }

	public interface ITakeOffContext
	{
        Int32 Position { get; }
    }
	public interface IEnterMapContext
	{
        IMap Map { get; }
        Point Position { get; }

    }
	public interface IMoveContext
	{
        Point Origin { get; }
        Point Current { get; }
    }
	public interface ILeaveMapContext
	{
        IMap Map { get; }
        Point Position { get; }
    }

	public interface IPickUpItemContext
	{
        IMap Map { get; }
		Point Position { get; }

    }
	public interface IPickDropItemContext
	{
        IMap Map { get; }

        Point Position { get; }
    }

	public interface IAcceptMissionContext
	{

	}

	public interface IAbortMissionContext
    {

	}

	public interface ICompleteMissionContext
	{

	}
	public interface ILevelUpContext
	{
        Int32 Level { get; }
    }


	
}
