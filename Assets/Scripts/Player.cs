using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	[HideInInspector]
	public static Transform playerTransform;

	public GameObject optionPrefab;
	public int maximumOptions;
	public float optionDistance;

	private GameObject[] options;
	private float oldPower;
	
	public uint lives;
	public uint bombs;
	public float power;
	public uint point;

	public float unfocusedMovementSpeed;
	public float focusedMovementSpeed;

	// Based on Hans Eysenck's ideas of temprament
	// Introversion vs Extraverison controls spread and power
	// Psychotism vs Neurotism controls speed and power

	// Extraversion
	// -- Hot Red
	// -- Homing
	// -- Weaker Shots
	// -- Wider Spread

	// Introversion
	// -- Cool Blue
	// -- Focused
	// -- Stronger shots
	// -- Tighter Spread

	// Psychotism 
	// -- High Power Shots
	// -- Slow Shot Speed

	// Neurotism
	// -- Low Power Shots
	// -- Fast Shot Speed

	// > 0 := Extraversion/Neurotism
	// < 0 := Intraversion/Psychotism
	// = 0 := balanced
	public float IntroExtraVersion;
	public float NeuroPsychOtism;

	[HideInInspector]
	public float FireRate;
	[HideInInspector]
	public float Homing;
	[HideInInspector]
	public float ShotDamage;
	[HideInInspector]
	public float Spread;

	//Note Spread is in terms of PI
	public float baseFireRate = 3;
	public float baseHoming = 0;
	public float baseShotDamage = 10;
	public float baseSpread = 0.5f;
	public float FireRateTraitScaling = 2;
	public float HomingTraitScaling = 1;
	public float ShotDamageTraitScaling = 6;
	public float SpreadTraitScaling = 0.5f;

	public PlayerHitboxHandler HitboxHandler;

	public bool invincible;
	[HideInInspector]
	public float invincibilityTimer;
	public float bombInvincibilityTime; //In Seconds

	private bool bombPressed;

	void Start () 
	{
		//Cache commonly accessed components of player
		playerTransform = transform;

		FireRate = baseFireRate + NeuroPsychOtism * FireRateTraitScaling;
		Homing = baseHoming + (IntroExtraVersion + 2) / 2 * HomingTraitScaling;
		ShotDamage = baseShotDamage + NeuroPsychOtism * ShotDamageTraitScaling;
		Spread = baseSpread + IntroExtraVersion * SpreadTraitScaling;

		options = new GameObject[maximumOptions];

		for (int i = 0; i < maximumOptions; i++)
		{
			GameObject option = (GameObject)Instantiate(optionPrefab, playerTransform.position, Quaternion.identity);
			option.transform.parent = playerTransform;
			option.SetActive(false);
			options[i] = option;
		}

		HitboxHandler.master = this;
	}

	void Update () 
	{
		//Movement
		float deltat = Time.deltaTime;
		bool focused = HitboxHandler.hitboxRenderer.enabled = Input.GetKey (Global.Control.Focus);
		Vector3 movementVector = Vector3.zero;
		if(Input.GetKey(Global.Control.Up))
		{
			movementVector.y += (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		if(Input.GetKey(Global.Control.Down))
		{
			movementVector.y -= (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		if(Input.GetKey(Global.Control.Left))
		{
			movementVector.x -= (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		if(Input.GetKey(Global.Control.Right))
		{
			movementVector.x += (focused) ? focusedMovementSpeed : unfocusedMovementSpeed;
		}
		playerTransform.localPosition += movementVector * deltat;

		if(Input.GetKey(Global.Control.Bomb))
		{
			if(!bombPressed)
			{
				Bomb ();
				bombPressed = true;
			}
		}
		else
		{
			if(bombPressed)
			{
				bombPressed = false;
			}
		}

		if(invincible)
		{
			invincibilityTimer -= Time.deltaTime;
			if(invincibilityTimer <= 0)
			{
				invincibilityTimer = 0;
				invincible = false;
			}
		}

		for(int i = 0; i < (int)power; i++)
		{
			float distance = (focused) ? optionDistance * (2f/3f) : optionDistance;
			options[i].SetActive(true);
			float angle = -(Mathf.PI / (power + 1)) * (i + 1);
			Vector3 position = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
			options[i].transform.localPosition = position;
		}
		for(int i = (int)power; i < options.Length; i++)
		{
			options[i].SetActive(false);
		}
	}

	private void Bomb()
	{
		invincible = true;
		invincibilityTimer = bombInvincibilityTime;
	}

	public void Die()
	{
		if(lives <= 0)
		{
			Global.gameState = GameState.Game_Over;
		}
		lives--;
		GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
		foreach(GameObject go in pickups)
		{
			go.GetComponent<Pickup>().autoCollect = false;
		}
	}
}
