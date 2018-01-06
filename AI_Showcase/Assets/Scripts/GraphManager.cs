using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Point{
	public int x;
	public int y;
}

public class GraphManager : MonoBehaviour {

	public GameObject nodePrefab, playerPrefab;
	public int gridX, gridY;
	public Point start = new Point();
	public Point end = new Point();
	public int orthogonalScore = 10;
	public int diagonalScore = 14;
	public bool Manhatten, Diagonal, Dijkstra;

	private GameObject[,] nodes;
	private List<GameObject> openList = new List<GameObject>();
	private List<GameObject> closedList = new List<GameObject>();
	private List<Vector3> pathList = new List<Vector3>();
	private int[] xCon = new int[8]{1,1,0,-1,-1,-1,0,1};
	private int[] yCon = new int[8]{0,-1,-1,-1,0,1,1,1};
	private string[,] nodeState;
	private GameObject startPoint;
	private GameObject endPoint;

	private bool endInClosed = false;



	// Use this for initialization
	void Start () {
		Manhatten = true;
		Diagonal = false;
		Dijkstra = false;
		nodes = new GameObject[gridX,gridY];
		nodeState = new string[gridX,gridY];

		setupBoard();
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.P))
			AStar();
		if(Input.GetKeyDown(KeyCode.C)){
			resetBoard();
		}
	}

	void setupBoard(){
		openList.Clear();
		closedList.Clear();
		for(int x = 0; x < gridX; x++){
			for(int y = 0; y < gridY; y++){
				GameObject newNode = Instantiate(nodePrefab,new Vector3(x,0,y),Quaternion.identity) as GameObject;

				nodeState[x,y] = "U";
				nodes[x,y] = newNode;
				setupTerrain(newNode,x,y);

			}
		}
	}

	void resetBoard(){
		openList.Clear();
		closedList.Clear();
		for(int x = 0; x < gridX; x++){
			for(int y = 0; y < gridY; y++){
				GameObject newNode = nodes[x,y];

				nodeState[x,y] = "U";
				setupTerrain(newNode,x,y);
			}
		}
	}

	void setupTerrain(GameObject newNode, int x, int y){
		if(x == start.x && y == start.y){
			newNode.GetComponent<Node>().setTerrain(TerrainState.Start);
			newNode.name = "Start";
			startPoint = newNode;
			startPoint.GetComponent<Node>().setG(0);
			addToOpenlist(newNode,x,y);
		}
		else if(x == end.x && y == end.y){
			newNode.GetComponent<Node>().setTerrain(TerrainState.End);
			endPoint = newNode;
			newNode.name = "End";
		}
		else if (x > 14 && x < 34 && y > 14 && y < 34){
			newNode.GetComponent<Node>().setTerrain(TerrainState.Unwalkable);
			newNode.name = "Wall";
		}
		else if(x < 34 && x >= 0 && y >= 34 && y < gridY){
			newNode.GetComponent<Node>().setTerrain(TerrainState.Swamp);
			newNode.name = "Swamp";
		}

		else{
			newNode.GetComponent<Node>().setTerrain(TerrainState.Walkable);
			newNode.name = "Flat";
		}
	}


	void AStar(){
		bool sortOpenList = true;
		endInClosed = false;

		startPoint.GetComponent<Node>().setH(calcHeuristic(startPoint.transform.position,endPoint.transform.position));
		startPoint.GetComponent<Node>().calcF();

		while(openList.Count != 0 && endInClosed != true){	// as long as open list is not empty and end point isnt in the closed list
			// Open list = objects that have been discoreved but need to be investigated
			// closed list = objects which have already been evaluated

			if(sortOpenList){
				sortListByF();
				sortOpenList = false;
			}

			GameObject current = openList[0];
			Node curNode = current.GetComponent<Node>();
			int curNodeX = (int)current.transform.position.x;
			int curNodeY = (int)current.transform.position.z;

			addToClosedList(current,curNodeX,curNodeY);
			if(curNode.getTerrain() == TerrainState.End)
				endInClosed = true;


			for(int i = 0; i < 8; i++){	//Checks all neighbours of the current object (8-connectivity)
				int neighbourX = curNodeX+xCon[i];
				int neighbourY = curNodeY+yCon[i];

				if(neighbourX >= 0 && neighbourX < gridX && neighbourY >= 0 && neighbourY < gridY){
					GameObject neighbour = nodes[neighbourX,neighbourY];
					Node neighbourNode = neighbour.GetComponent<Node>();

					if(neighbourNode.getTerrain() != TerrainState.Unwalkable){	//If the player can move onto the neighbour object
						int newG = curNode.getG();

						if(neighbourX == curNodeX || neighbourY == curNodeY)	//calculate G based on whether it is a diagonal neighbour or not
							newG += (int)(orthogonalScore*neighbourNode.getTerrainMult());
						else
							newG += (int)(diagonalScore*neighbourNode.getTerrainMult());

						if(nodeState[neighbourX,neighbourY] == "U"){	//If the neighbour node is unassigned i.e. not in the openList or closedList, then include it in the openlist
							neighbour.GetComponent<Node>().setParent(current);
							addToOpenlist(neighbour,neighbourX,neighbourY);

							neighbourNode.setH(calcHeuristic(neighbour.transform.position, endPoint.transform.position));
							neighbourNode.setG(newG);
							neighbourNode.calcF();
							sortOpenList = true;
						}
						else if(nodeState[neighbourX,neighbourY] == "O"){ //if the neigbour is already in the openList, but we just found a shorter way to it, then update its parent node and its score
							if(newG < neighbourNode.getG()){
								neighbour.GetComponent<Node>().setParent(current);
								neighbourNode.setG(newG);
								neighbourNode.calcF();
								sortOpenList = true;
							}
						}
					}
				}				
			}
		}
		if(endInClosed){	//retrack the path and highlight it in yellow
			GameObject path = endPoint;
			while(path.GetComponent<Node>().getParent() != startPoint && path.GetComponent<Node>().getParent() != null ){
				GameObject parentPath = path.GetComponent<Node>().getParent();
				parentPath.GetComponent<Renderer>().material.color = Color.yellow;
				path = parentPath;
			}

			path = endPoint;
			while(path.GetComponent<Node>().getParent() != null ){	//add the path to the PatthList
				pathList.Add(path.transform.position);
				GameObject parentPath = path.GetComponent<Node>().getParent();
				path = parentPath;
			}
			Debug.Log(pathList.Count);
			StartCoroutine(movePlayer()); //move a sphere along the found path
		}
	}


	IEnumerator movePlayer(){
		Debug.Log("Player");
		GameObject player = Instantiate(playerPrefab,pathList[pathList.Count-1]+Vector3.up,Quaternion.identity) as GameObject;

		for(int i = pathList.Count-1; i > 0; i--){
			float startTime = Time.time;
			while(Time.time - startTime <= 1.0f){
				player.transform.position = Vector3.Lerp(pathList[i]+Vector3.up,pathList[i-1]+Vector3.up,Time.time - startTime);
				yield return new WaitForFixedUpdate();
			}
		}
	}

	void addToOpenlist(GameObject i, int x, int y){
		openList.Add(i);
		nodeState[x,y] = "O";
		if(i.GetComponent<Node>().getTerrain() != TerrainState.Unwalkable && i.GetComponent<Node>().getTerrain() != TerrainState.End && i.GetComponent<Node>().getTerrain() != TerrainState.Start)
			i.GetComponent<Renderer>().material.color = Color.grey;
	}
	void addToClosedList(GameObject i, int x, int y){
		openList.Remove(i);
		closedList.Add(i);
		nodeState[x,y] = "C";
		if(i.GetComponent<Node>().getTerrain() != TerrainState.Unwalkable && i.GetComponent<Node>().getTerrain() != TerrainState.End && i.GetComponent<Node>().getTerrain() != TerrainState.Start)
			i.GetComponent<Renderer>().material.color = Color.black;
	}
	void sortListByF(){	//Sorts the openlist by their F value (combined distance and heuristics) in increasing order 
		openList = openList.OrderBy(x => x.GetComponent<Node>().getF()).ToList();
//		foreach(GameObject t in openList){
//			Debug.Log(t.GetComponent<Node>().getF());
//		}
	}


	int calcHeuristic(Vector3 curNodePos, Vector3 endNodePos){
		int H = 0;

		if(Manhatten)
			H = manhattenH(curNodePos, endNodePos);
		if(Diagonal)
			H = diagH(curNodePos, endNodePos);
		if(Dijkstra)
			H = dijkstraH();

		return H;

	}

	int manhattenH(Vector3 curNodePos, Vector3 endNodePos){
		float manhattenDist = (int)(Mathf.Abs(curNodePos.x-endNodePos.x)+Mathf.Abs(curNodePos.y-endNodePos.y)+Mathf.Abs(curNodePos.z-endNodePos.z));
		return (int)(manhattenDist*orthogonalScore);
	}

	int diagH(Vector3 curNodePos, Vector3 endNodePos){
		float diagDist = 0;
		float xDist = Mathf.Abs(curNodePos.x-endNodePos.x);
		float zDist = Mathf.Abs(curNodePos.z-endNodePos.z);

		if(xDist > zDist)
			diagDist = diagonalScore*zDist + orthogonalScore*(xDist-zDist);
		else
			diagDist = diagonalScore*xDist + orthogonalScore*(zDist-xDist);

		return (int)diagDist;
	}

	int dijkstraH(){
		return 0;
	}
}
