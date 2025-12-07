using Sandbox.Npcs.Layers;
using Sandbox.Npcs.Schedules;

namespace Sandbox.Npcs.Behaviors;

public class ScientistBehavior : Behavior
{
	[Property] public float SightRange { get; set; } = 512f;
	[Property] public float PersonalSpace { get; set; } = 128f;
	[Property] public TagSet TargetTags { get; set; } = ["player"];

	protected SensesLayer _senses;

	protected override void OnEnabled()
	{
		var senses = AddLayer<SensesLayer>();

		// configure senses
		senses.SightRange = SightRange;
		senses.PersonalSpace = PersonalSpace;
		senses.TargetTags = TargetTags;

		AddLayer<LocomotionLayer>();
		AddLayer<LookAtLayer>();

		_senses = senses;
	}

	public override ScheduleBase QuerySchedule()
	{
		if ( _senses.DistanceToNearest <= _senses.PersonalSpace && _senses.Nearest.IsValid() )
		{
			return new ScientistFleeSchedule( _senses.Nearest );
		}

		if ( _senses.VisibleTargets.Count > 0 )
		{
			var visiblePlayer = _senses.GetNearestVisible();
			if ( visiblePlayer.IsValid() )
			{
				return new ScientistInvestigateSchedule( visiblePlayer );
			}
		}

		if ( _senses.Nearest.IsValid() && _senses.DistanceToNearest <= _senses.SightRange )
		{
			return new ScientistSearchSchedule( _senses.Nearest.WorldPosition );
		}

		return new ScientistIdleSchedule();
	}
}
