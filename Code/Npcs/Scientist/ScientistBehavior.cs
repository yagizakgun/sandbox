using Sandbox.Npcs.Layers;
using Sandbox.Npcs.Schedules;

namespace Sandbox.Npcs.Behaviors;

public class ScientistBehavior : Behavior
{
	private Vector3? _lastTarget;
	private TimeSince _timeSinceLostVision;

	public override ScheduleBase Run()
	{
		//
		// Update last known position if we can see a target
		//
		if ( Npc.Senses.VisibleTargets.Any() )
		{
			var visible = Npc.Senses.GetNearestVisible();
			if ( visible.IsValid() )
			{
				_lastTarget = visible.WorldPosition;
				_timeSinceLostVision = 0;
			}
		}

		//
		// Is someone in our face?
		//
		if ( Npc.Senses.DistanceToNearest <= Npc.Senses.PersonalSpace && Npc.Senses.Nearest.IsValid() )
		{
			var flee = GetSchedule<ScientistFleeSchedule>();
			flee.Source = Npc.Senses.Nearest;
			return flee;
		}

		if ( Npc.Senses.VisibleTargets.Any() )
		{
			var visible = Npc.Senses.GetNearestVisible();
			if ( visible.IsValid() )
			{
				var investigate = GetSchedule<ScientistInvestigateSchedule>();
				investigate.Target = visible;
				return investigate;
			}
		}

		if ( _lastTarget.HasValue && _timeSinceLostVision < 10f )
		{
			var search = GetSchedule<ScientistSearchSchedule>();
			search.Target = _lastTarget.Value;
			return search;
		}

		_lastTarget = null;
		return GetSchedule<ScientistIdleSchedule>();
	}
}
