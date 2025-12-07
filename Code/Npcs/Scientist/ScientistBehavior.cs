using Sandbox.Npcs.Layers;
using Sandbox.Npcs.Schedules;

namespace Sandbox.Npcs.Behaviors;

public class ScientistBehavior : Behavior
{
	[Property] public float SightRange { get; set; } = 512f;
	[Property] public float PersonalSpace { get; set; } = 128f;
	[Property] public TagSet TargetTags { get; set; } = ["player"];

	private Vector3? _lastTarget;
	private TimeSince _timeSinceLostVision;

	protected override void OnEnabled()
	{
		var senses = AddLayer<SensesLayer>();
		senses.SightRange = SightRange;
		senses.PersonalSpace = PersonalSpace;
		senses.TargetTags = TargetTags;

		AddLayer<LocomotionLayer>();
		AddLayer<LookAtLayer>();
	}

	public override ScheduleBase Run()
	{
		var senses = Layer<SensesLayer>();

		//
		// Update last known position if we can see a target
		//
		if ( senses.VisibleTargets.Any() )
		{
			var visible = senses.GetNearestVisible();
			if ( visible.IsValid() )
			{
				_lastTarget = visible.WorldPosition;
				_timeSinceLostVision = 0;
			}
		}

		//
		// Is someone in our face?
		//
		if ( senses.DistanceToNearest <= senses.PersonalSpace && senses.Nearest.IsValid() )
		{
			var flee = Schedule<ScientistFleeSchedule>();
			flee.Source = senses.Nearest;
			return flee;
		}

		if ( senses.VisibleTargets.Any() )
		{
			var visible = senses.GetNearestVisible();
			if ( visible.IsValid() )
			{
				var investigate = Schedule<ScientistInvestigateSchedule>();
				investigate.Target = visible;
				return investigate;
			}
		}

		if ( _lastTarget.HasValue && _timeSinceLostVision < 10f )
		{
			var search = Schedule<ScientistSearchSchedule>();
			search.Target = _lastTarget.Value;
			return search;
		}

		_lastTarget = null;
		return Schedule<ScientistIdleSchedule>();
	}
}
