using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class Player : IEquatable<Player>, IComparable<Player>
{

	public string name;
	public int score;
	
	// Update is called once per frame
	public Player (string name){
		this.name = name;
		this.score = 0;
	}
	public bool Equals(Player p){
		if (this.name.Equals (p.name)) {
			return  true;
		} else {
			return false;
		}
	}
	public override bool Equals(object p){
		Player p1 = p as Player;
		if (this.name.Equals (p1.name)) {
			return true;
		} else
			return false;
	}
	public int CompareTo(Player p){
		return this.score-p.score;

	}
}
