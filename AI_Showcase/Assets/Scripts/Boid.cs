using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boid : MonoBehaviour {
	public string tag = "Enviroment";
	private Vector3 bVelocity;

	private GameObject gObj; 
	private GameObject[] enviroment;

	// Use this for initialization
	void Start () {
		enviroment = GameObject.FindGameObjectsWithTag(tag);
		gObj = this.gameObject;
		bVelocity = new Vector3(Random.Range(BoidManager.instance.minVelocity,BoidManager.instance.maxVelocity),0.0f,Random.Range(BoidManager.instance.minVelocity,BoidManager.instance.maxVelocity));
	}
	
	// Update is called once per frame
	void Update () {
		//Vector3 stuff = objectAvoidance(enviroment,gObj);
		List<GameObject> closeBoids = new List<GameObject>();

		for(int i = 0; i < BoidManager.instance.boidList.Count; i++){
			if(Vector3.Distance(gObj.transform.position,BoidManager.instance.boidList[i].transform.position) < BoidManager.instance.boidRadius){
				closeBoids.Add(BoidManager.instance.boidList[i]);
			}
		}

		if(BoidManager.instance.bSeparation)
			bVelocity += separation(closeBoids,gObj)/4;
		
		if(BoidManager.instance.bAliegnment)
			bVelocity += aliegnment(closeBoids,gObj)/4; 

		if(BoidManager.instance.bCohesion)
			bVelocity += cohesion(BoidManager.instance.boidList,gObj)/100;
		
		if(BoidManager.instance.bRandom)
			bVelocity += new Vector3(Random.Range(-1.0f,1.0f),0,Random.Range(-1.0f,1.0f))/10;

		if(bVelocity.magnitude > BoidManager.instance.maxMag)
			bVelocity = bVelocity.normalized * BoidManager.instance.maxMag;
		
		gObj.transform.position += bVelocity * Time.deltaTime;
		gObj.transform.forward = bVelocity.normalized;
		gObj.transform.position += warpAround(gObj.transform.position);
	}


	Vector3 separation(List<GameObject> closeBoids, GameObject curBoid){
		Vector3 comb = Vector3.zero;
		int count = 0;

		for(int i = 0; i < closeBoids.Count; i++){
			float d = Vector3.Distance(curBoid.transform.position,closeBoids[i].transform.position);
			if(d > 0 && d < BoidManager.instance.boidSepDist){
				Vector3 difference = curBoid.transform.position-closeBoids[i].transform.position;
				difference.Normalize();
				difference /= d;
				comb += difference;
				count++;
			}
		}
		if(count > 0){
			comb /= count;
		}

		return comb.normalized;
	}

	Vector3 aliegnment(List<GameObject> closeBoids, GameObject curBoid){
		Vector3 comb = Vector3.zero;
		int count = 0;

		for(int i = 0; i < closeBoids.Count; i++){
			float d = Vector3.Distance(curBoid.transform.position,closeBoids[i].transform.position);
			if(d > 0 && d < BoidManager.instance.boidRadius){
				comb += closeBoids[i].GetComponent<Boid>().getVelocity();
				comb /= d;
				count++;
			}
		}
		if(count > 0){
			comb /= count;
		}

		return comb.normalized;
	}

	Vector3 cohesion(List<GameObject> closeBoids, GameObject curBoid){
		Vector3 comb = Vector3.zero;
		int count = 0;

		for(int i = 0; i < closeBoids.Count; i++){
			float d = Vector3.Distance(curBoid.transform.position,closeBoids[i].transform.position);
			if(d > 0.0f && d < BoidManager.instance.boidRadius){
				comb += closeBoids[i].transform.position;
				count++;
			}
		}
		if(count > 0){
			comb /= count;
		}

		return (comb-curBoid.transform.position).normalized;
	}

	Vector3 warpAround(Vector3 boidPos){
		Vector3 tmp = Vector3.zero;

		if(boidPos.x < BoidManager.instance.minX){
			tmp += new Vector3(BoidManager.instance.xDist,0,0);
		}
		else if(boidPos.x > BoidManager.instance.maxX){
			tmp += new Vector3(-BoidManager.instance.xDist,0,0);
		}
		/*if(boidPos.y < BoidManager.instance.minY){
			tmp += new Vector3(0,BoidManager.instance.yDist,0);
		}
		else if(boidPos.y > BoidManager.instance.maxY){
			tmp += new Vector3(0,-BoidManager.instance.yDist,0);
		}*/
		if(boidPos.z < BoidManager.instance.minZ){
			tmp += new Vector3(0,0,BoidManager.instance.zDist);
		}
		else if(boidPos.z > BoidManager.instance.maxZ){
			tmp += new Vector3(0,0,-BoidManager.instance.zDist);
		}
		return tmp;
	}

	Vector3 getVelocity(){
		return this.bVelocity;
	}


	Vector3 objectAvoidance(GameObject[] envArr, GameObject boid){
		Vector3 boidPos = boid.transform.position;
		Vector3 boidRot = boid.transform.rotation.eulerAngles;
		Vector3 returnAngle = Vector3.zero;

		//Debug.Log(envArr.Length);
		for(int i = 0; i < envArr.Length; i++){
			if( Vector3.Distance(boidPos,envArr[i].transform.position) < BoidManager.instance.minDistEnv){
				Vector3 envAngle = envArr[i].transform.rotation.eulerAngles;
				Debug.Log(Vector3.Angle(boidRot, envAngle));
			}				
		}
		return returnAngle;
	}
}
