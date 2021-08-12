using Sandbox;


public partial class FloodProp : Prop
{
	[Net]
	public ulong PropOwner { get; set; }
	[Net] public float PropHealth { get; set; } = 250f;

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		if ( PhysicsBody.Mass > 125f ) // shits too heavy
			PhysicsBody.Mass = 125f;
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( FloodGame.Instance.CurrentRound != FloodGame.Round.Fight )
			return;
		PropHealth -= info.Damage;

		if ( PropHealth <= 0f )
		{
			OnKilled();
			base.TakeDamage( info );
		}
		base.ApplyDamageForces( info );
	}
}
