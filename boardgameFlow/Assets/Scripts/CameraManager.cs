﻿using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public float MASS_SIZE     = 0.5f;
	public float INTERVAL_SIZE = 0.5f;
    public float CAMERA_ANGLE_Y = 30.0f;
    public float CAMERA_ANGLE_Z = 30.0f;

	// Use this for initialization
	void Start( ) {
	
	}
	
	// Update is called once per frame
	void Update( ) {
	
	}

	/// <summary>
	/// Moves the camera position.
	/// </summary>
	/// <param name="first_player">First player.</param>
	/// <param name="last_player">Last player.</param>
	public void moveCameraPos( GameObject first_player, GameObject last_player ) {
		int mass_num     = 6;
		int interval_num = mass_num + 1;

		// カメラの横幅を出す
		float camera_width = ( MASS_SIZE * mass_num ) + ( INTERVAL_SIZE * interval_num ) + ( first_player.transform.position.x - last_player.transform.position.x );
        // カメラの注視点を計算
        Vector3 center = Vector3.Lerp ( first_player.transform.position, last_player.transform.position, 0.5f );

		float view_x = last_player.transform.position.x + ( camera_width / 2 );
		Vector3 view_point = center + Vector3.forward * -10;

		// カメラから注視点までの距離を計算
		float camera_field = Camera.main.fieldOfView;
		float distance = camera_width / 2 /  Mathf.Tan( camera_field / 4 );

		// カメラの座標を計算
        float adjust = 3.0f;
        view_point.x += adjust;
        view_point.y = view_point.y + distance / Mathf.Sin( CAMERA_ANGLE_Y ) / ( adjust / 1.5f );
        view_point.z = view_point.z - distance * Mathf.Cos( CAMERA_ANGLE_Z ) + ( adjust * 2 );
		Camera.main.transform.position =  view_point;

        float z = Camera.main.transform.rotation.z;
		// カメラの回転を計算
		Camera.main.transform.LookAt( center );
        Camera.main.transform.Rotate( new Vector3( Camera.main.transform.rotation.x,
                                                   Camera.main.transform.rotation.y, 
                                                   4.0f ) );
	}
}