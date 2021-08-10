using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

[Library( "flood_ini", Title = "Flood" )]
partial class FloodGame : Game
{
	public FloodGame()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new FloodHud();
		}
		Global.PhysicsSubSteps = 2;
		// consider tweaking if we have any physics issues
		// but realistically we want this to perform as good as possible
	}

	public static FloodGame Instance
	{
		get => Current as FloodGame;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		FloodSimulate();
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new FloodPlayer();
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	/* 
	 * Down here I'm putting all the Flood stuff
	 I don't see any point in splitting this into a different file - it's plenty clean
	 */
	#region Round variables
	public enum Round
	{
		PreGame,
		Fight,
		PostGame
	}
	[Net]
	public Round CurrentRound { get; set; } = Round.PreGame;
	[Net]
	public int RoundTime { get; set; } = 0;

	public int PreGameTime = 60;
	public int FightTime = 60;
	public int PostGameTime = 7;
	#endregion

	#region Water variables
	public Entity Water;
	public Vector3 WaterBasePosition;
	public float WaterHeight = 375f; // how high the water goes up to
	public bool ForceCreateWater = true; // create the water if the map doesn't already have it
	#endregion

	#region Logic
	void FloodSimulate()
	{
		if ( !IsServer ) // Should we run this on the client? I don't know!
			return;
		SetupWater();
		if ( Water == null )
			return;
		FloodWorld();
	}
	int retryCounter = 0;
	void SetupWater() // We find the water and set up base positions in here
	{
		if ( Water != null )
			return;
		if ( retryCounter == 9 && ForceCreateWater )
		{
			var water = new WaterSea();
			float lowestPoint = 0f;
			foreach ( var entity in Entity.All ) // find lowest entity
				if ( entity.Position.z < lowestPoint )
					lowestPoint = entity.Position.z;
			if ( lowestPoint < WaterHeight / 2 ) // don't want it too low now
				lowestPoint += WaterHeight / 2;
			// theoretically this logic is wrong but it works

			water.Position = new Vector3(water.Position.x, water.Position.y, lowestPoint);
			Log.Info( "Force created water. Set ForceCreateWater to false in Game.cs to disable this " );
		}
		if ( retryCounter == 10 )
		{
			Log.Info( "Failed to find water after 10 retries. Please find a map with env_sea in it" );
			retryCounter++;
		}
		if ( retryCounter > 10 )
			return;
		Log.Info( "Finding water" );
		foreach ( var entity in Entity.All )
		{
			if ( entity.ClassInfo.Name == "env_sea" )
			{
				Water = entity;
				Log.Info( "Water found!" );
				WaterBasePosition = Water.Position;
			}
		}
		retryCounter++;
	}
	void FloodWorld() // makes the water go up and down/flood
	{
		switch ( CurrentRound )
		{
			case Round.PreGame:
				if ( WaterBasePosition.z < Water.Position.z )
					Water.Position -= Vector3.Up;
				break;
			case Round.Fight:
				if ( Water.Position.z < WaterHeight )
					Water.Position += Vector3.Up;
				break;
			case Round.PostGame:
				if ( WaterBasePosition.z < Water.Position.z )
					Water.Position -= Vector3.Up;
				break;
		}
	}

	public void OnSecond() // Second tick
	{
		RoundTime++;
		switch ( CurrentRound )
		{
			case Round.PreGame:
				if ( RoundTime > PreGameTime )
				{
					CurrentRound = Round.Fight;
					RoundTime = 0;
					Log.Info( "Setting RoundState to Fight" );
				}
				break;
			case Round.Fight:
				if ( RoundTime > FightTime )
				{
					CurrentRound = Round.PostGame;
					RoundTime = 0;
					Log.Info( "Setting RoundState to PostGame" );
				}
				break;
			case Round.PostGame:
				if ( RoundTime > PostGameTime )
				{
					CurrentRound = Round.PreGame;
					RoundTime = 0;
					Log.Info( "Setting RoundState to PreGame" );
				}
				break;
		}
	}

	[ServerCmd( "spawn" )]
	public static void Spawn( string modelname )
	{
		if ( FloodGame.Instance.CurrentRound != Round.PreGame )
		{
			ChatBox.Say( "You cannot spawn things during play phases!" );
			return;
		}
		var owner = ConsoleSystem.Caller?.Pawn;

		if ( ConsoleSystem.Caller == null )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		var ent = new FloodProp();
		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );
		ent.SetModel( modelname );
		ent.Position = tr.EndPos - Vector3.Up * ent.CollisionBounds.Mins.z;
		ent.PropOwner = owner;

		var floodPlayer = owner as FloodPlayer;
		var cost = (int)ent.PhysicsBody.Mass * 2; // mass based cost
		if ( cost > 1000 )
			cost = 1000;
		cost = ((cost) / 50) * 50; // round to nearest 50
		if ( cost > floodPlayer.Money )
		{
			ChatBox.Say( $"That costs too much! (Cost: {cost})" );
			ent.Delete();
			return;
		}
		floodPlayer.Money -= cost;
	}

	[ServerCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		if ( FloodGame.Instance.CurrentRound != Round.PreGame )
		{
			ChatBox.Say( "You cannot spawn things during play phases!" );
			return;
		}
		var owner = ConsoleSystem.Caller.Pawn;

		if ( owner == null )
			return;

		var attribute = Library.GetAttribute( entName );

		if ( attribute == null || !attribute.Spawnable )
			return;

		var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 200 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		var ent = Library.Create<Entity>( entName );
		if ( ent is BaseCarriable && owner.Inventory != null )
		{
			var floodPlayer = owner as FloodPlayer;
			int cost = 200;
			if ( cost > floodPlayer.Money )
			{
				ChatBox.Say( $"That costs too much! (Cost: {cost})" );
				return;
			}
			floodPlayer.Money -= cost;
			if ( owner.Inventory.Add( ent, true ) )
				return;
		}

		ent.Position = tr.EndPos;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) );
	}
	#endregion

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
		_ = StartSecondTimer();
	}
	public async Task StartSecondTimer()
	{
		while ( true )
		{
			await Task.DelaySeconds( 1 );
			OnSecond();
		}
	}
}
