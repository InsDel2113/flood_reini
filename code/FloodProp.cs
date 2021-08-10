using Sandbox;


public partial class FloodProp : Prop
{
	[Net]
	public Entity PropOwner { get; set; }

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
	}

	protected override ModelPropData GetModelPropData()
	{
		var model = GetModel();

		ModelPropData defaultData = new ModelPropData();

		if ( model != null && !model.IsError && model.HasPropData() )
		{
			defaultData = model.GetPropData();
			defaultData.Health = defaultData.Health * 0.85f;
			return defaultData;
		}

		defaultData.Health = -1;
		defaultData.ImpactDamage = 10;
		if ( PhysicsGroup != null )
		{
			defaultData.ImpactDamage = PhysicsGroup.Mass / 10;
		}
		defaultData.MinImpactDamageSpeed = 500;
		return defaultData;
	}
}
