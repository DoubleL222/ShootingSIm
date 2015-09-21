using UnityEngine;
using System.Collections;

public class WController : SixenseHand {
	public void Update(){
		if ( m_controller == null )
		{
			m_controller = SixenseInput.GetController( m_hand );
		}
	}
}
