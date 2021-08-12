namespace Sandbox.Tools
{
	[Library( "tool_remover", Title = "Remover", Description = "Remove entities", Group = "construction" )]
	public partial class RemoverTool : BaseTool
	{
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( !Input.Pressed( InputButton.Attack1 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.HitLayer( CollisionLayer.Debris )
					.Run();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				if ( tr.Entity is Player )
					return;

				if ( !OwnershipChecks( Owner.GetClientOwner().SteamId, tr.Entity ) )
					return;

				CreateHitEffects( tr.EndPos );

				if ( tr.Entity.IsWorld )
					return;

				var FloodPlayer = Owner as FloodPlayer;
				FloodPlayer.Money += FloodGame.Instance.GetCostOfProp( (FloodProp)tr.Entity );
				tr.Entity.Delete();

				var particle = Particles.Create( "particles/physgun_freeze.vpcf" );
				particle.SetPosition( 0, tr.Entity.Position );
			}
		}
	}
}
