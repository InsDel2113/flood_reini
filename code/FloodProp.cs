using Sandbox;


public partial class FloodProp : Prop
{
	[Net]
	public Entity PropOwner { get; set; }
	[Net] public float PropHealth { get; set; } = 250f;

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		if ( PhysicsBody.Mass > 200f )
			PhysicsBody.Mass = 200f;
	}

	public override void TakeDamage( DamageInfo info )
	{
		PropHealth -= info.Damage;

		if ( PropHealth <= 0f )
		{
			OnKilled();
			base.TakeDamage( info );
		}
		base.ApplyDamageForces( info );
	}
}
