using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goat : MonoBehaviour {

    private Player[] players;
    // Use this for initialization
    void Start () {
        players = FindObjectsOfType<Player>();
    }

	// Update is called once per frame
	void Update () {
        float x = 0;
		foreach (Player player in players)
        {
            x += player.transform.position.x/players.Length;
        }
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
	}
}
