using UnityEngine;
using System.Collections;

public class KalmanState{

	public float q;
	public float r;
	public float x;
	public float p;
	public float k;
	
	public KalmanState(float q, float r, float x, float p, float k){
		this.q = q;
		this.r = r;
		this.x = x;
		this.p = p;
		this.k = k;
	}

	public void kalman_update(float measurement){
		this.p = this.p + this.q;
		
		this.k = this.p / (this.p + this.r);
		this.x = this.x + this.k * (measurement - this.x);
		this.p = (1 - this.k) * this.p;
	}
}
