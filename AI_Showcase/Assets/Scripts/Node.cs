using UnityEngine;
using System.Collections;

public enum TerrainState{
	Start,
	End,
	Walkable,
	Unwalkable,
	Swamp
}

public class Node : MonoBehaviour {
	public GameObject parent = null;
	public int F,G,H;
	private TerrainState state;
	private float terrainMult = 1.0f;


	public void setG(int g){
		this.G = g;
	}
	public void setH(int h){
		this.H = h;
	}
	public void calcF(){
		this.F = this.G+this.H;
	}
	public void setParent(GameObject newParent){
		this.parent = newParent;
	}
	public void setTerrain(TerrainState newState){
		this.state = newState;

		Color newCol = Color.black;
		if(this.state == TerrainState.Start)
			newCol = Color.green;
		else if(this.state == TerrainState.End)
			newCol = Color.red;
		else if(this.state == TerrainState.Unwalkable)
			newCol = Color.blue;
		else if(this.state == TerrainState.Swamp){
			newCol = new Color(0.0f,0.75f,0.0f,0.0f);
			terrainMult = 1.5f;
		}
		else{
			newCol = Color.white;
		}
		GetComponent<Renderer>().material.color = newCol;
	}


	public int getG(){
		return this.G;
	}
	public int getH(){
		return this.H;
	}
	public int getF(){
		return this.F;
	}
	public GameObject getParent(){
		return this.parent;
	}
	public TerrainState getTerrain(){
		return this.state;
	}
	public float getTerrainMult(){
		return terrainMult;
	}
}
