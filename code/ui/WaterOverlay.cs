using Sandbox;
using Sandbox.UI;

// TODO: Replace this with a proper screen overlay & refract shader when it's fixed
	public class WaterOverlay : Panel
	{
		public WaterOverlay()
		{
			StyleSheet.Load( "/Code/UI/WaterOverlay.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			void SetOpacity( float opacity )
			{
				Style.Opacity = opacity;
				Style.Dirty();
			}
			
			if ( Local.Pawn is Player player )
			{
				if ( player.WaterLevel.Fraction < 0.9f )
				{
					SetOpacity( 0 );
					return;
				}
				
				if ( player.Camera is Camera camera )
				{
					var rayStart = camera.Pos;
					var rayDir = camera.Rot.Forward * 1f + (camera.Rot.Up * 48f);
					var rayEnd = rayStart + rayDir;
					
					if ( Trace.Ray( rayStart, rayEnd ).HitLayer( CollisionLayer.Water ).Run().Hit )
					{
						SetOpacity( 1 );
						return;
					}
					
					SetOpacity( 0 );
				}
			}
		}
	}
