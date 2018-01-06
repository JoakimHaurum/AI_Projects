using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour {
	public static BoidManager instance = null;
	public bool bSeparation,bAliegnment,bCohesion,bRandom;
	public string boidTag = "Boids";
	public GameObject boidPrefab;
	[HideInInspector] public List<GameObject> boidList = new List<GameObject>();

	public float minDistEnv = 4.0f;
	public float boidRadius = 6.0f;
	public float boidSepDist = 2.0f;
	public float minVelocity = -5.0f;
	public float maxVelocity = 5.0f;
	public float maxMag = 2.0f;
	public float minX,maxX,minY,maxY,minZ,maxZ;
	[HideInInspector] public float xDist,yDist,zDist;


	void Awake () {
		// The following code makes the game manager a singleton. I.e. there can only be one instance in the game at any given time. 
		// This is a good thing as the game manager keeps track of score etc. and therefor there shouldnt be more than one
		if ( instance == null){
			instance = this;		//If no game managers exists, this game object becomes the game manager.
		}
		//If instance already exists and it's not this:
		else if(instance != this){
			Destroy (gameObject);	 //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
		}

		DontDestroyOnLoad(gameObject); //when changing scenes Unity normally destorys all gameobject. However sincethe game manager keeps track of score it should not be destoyed when new scenes are loaded
	}

	void Start(){
		GameObject[] tmp = GameObject.FindGameObjectsWithTag(boidTag);
		for(int i = 0; i < tmp.Length; i++)
			boidList.Add(tmp[i]);
		xDist = maxX-minX;
		yDist = maxY-minY;
		zDist = maxZ-minZ;
		bSeparation = true;
		bAliegnment = true;
		bCohesion = true;
		bRandom = true;

	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.B)){
			GameObject nBoid = Instantiate(boidPrefab,new Vector3(Random.Range(0.0f,1.0f),0.0f,Random.Range(0.0f,1.0f)),Quaternion.identity) as GameObject;
			boidList.Add(nBoid);
		}

		if(Input.GetKeyDown(KeyCode.U))
			bSeparation = !bSeparation;
		
		if(Input.GetKeyDown(KeyCode.I))
			bAliegnment = !bAliegnment;
		
		if(Input.GetKeyDown(KeyCode.O))
			bCohesion = !bCohesion;
		
		if(Input.GetKeyDown(KeyCode.P))
			bRandom = !bRandom;
	}

}
