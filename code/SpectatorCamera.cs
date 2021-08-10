using Sandbox;
using System;
public class SpectatorCamera : Camera
{
	Angles LookAngles;
	Vector3 MoveInput;

	Vector3 TargetPos;
	Rotation TargetRot;

	bool PivotEnabled;
	Vector3 PivotPos;
	float PivotDist;

	float MoveSpeed;
	float FovOverride = 0;

	float LerpMode = 0;

	/// <summary>
	/// On the camera becoming activated, snap to the current view position
	/// </summary>
	public override void Activated()
	{
		base.Activated();

		TargetPos = CurrentView.Position;
		TargetRot = CurrentView.Rotation;

		Pos = TargetPos;
		Rot = TargetRot;
		LookAngles = Rot.Angles();
		FovOverride = 80;

		DoFPoint = 0.0f;
		DoFBlurSize = 0.0f;
	}

	public override void Deactivated()
	{
		base.Deactivated();
	}

	public override void Update()
	{
		var player = Local.Client;
		if ( player == null )
		{
			return;
		}

		var tr = Trace.Ray( Pos, Pos + Rot.Forward * 4096 ).UseHitboxes().Run();

		FieldOfView = FovOverride;

		Viewer = null;
		{
			var lerpTarget = tr.EndPos.Distance( Pos );

			DoFPoint = lerpTarget;

			var pos = new Vector3( 100, 100 );
		}

		if ( PivotEnabled )
		{
			PivotMove();
		}
		else
		{
			FreeMove();
		}
	}

	public override void BuildInput( InputBuilder input )
	{

		MoveInput = input.AnalogMove;

		MoveSpeed = 1;
		if ( input.Down( InputButton.Run ) )
		{
			MoveSpeed = 5;
		}

		if ( input.Down( InputButton.Duck ) )
		{
			MoveSpeed = 0.2f;
		}

		if ( input.Down( InputButton.Slot1 ) )
		{
			LerpMode = 0.0f;
		}

		if ( input.Down( InputButton.Slot2 ) )
		{
			LerpMode = 0.5f;
		}

		if ( input.Down( InputButton.Slot3 ) )
		{
			LerpMode = 0.9f;
		}

		if ( input.Pressed( InputButton.Walk ) )
		{
			var tr = Trace.Ray( Pos, Pos + Rot.Forward * 4096 ).Run();
			PivotPos = tr.EndPos;
			PivotDist = Vector3.DistanceBetween( tr.EndPos, Pos );
		}

		if ( input.Down( InputButton.Use ) )
		{
			DoFBlurSize = Math.Clamp( DoFBlurSize + (Time.Delta * 3.0f), 0.0f, 100.0f );
		}

		if ( input.Down( InputButton.Menu ) )
		{
			DoFBlurSize = Math.Clamp( DoFBlurSize - (Time.Delta * 3.0f), 0.0f, 100.0f );
		}

		if ( input.Down( InputButton.Attack2 ) )
		{
			FovOverride += input.AnalogLook.pitch * (FovOverride / 30.0f);
			FovOverride = FovOverride.Clamp( 5, 150 );
			input.AnalogLook = default;
		}

		LookAngles += input.AnalogLook * (FovOverride / 80.0f);
		LookAngles.roll = 0;

		PivotEnabled = input.Down( InputButton.Walk );

		input.Clear();
		input.StopProcessing = true;
	}

	void FreeMove()
	{
		var mv = MoveInput.Normal * 300 * RealTime.Delta * Rot * MoveSpeed;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Pos = Vector3.Lerp( Pos, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
		Rot = Rotation.Slerp( Rot, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
	}

	void PivotMove()
	{
		PivotDist -= MoveInput.x * RealTime.Delta * 100 * (PivotDist / 50);
		PivotDist = PivotDist.Clamp( 1, 1000 );

		TargetRot = Rotation.From( LookAngles );
		Rot = Rotation.Slerp( Rot, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );

		TargetPos = PivotPos + Rot.Forward * -PivotDist;
		Pos = TargetPos;
	}
}
