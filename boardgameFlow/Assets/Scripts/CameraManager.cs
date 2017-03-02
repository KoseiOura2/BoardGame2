using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public float MASS_SIZE     = 0.5f;
	public float INTERVAL_SIZE = 0.5f;
    public float CAMERA_ANGLE_Y = 30.0f;
    public float CAMERA_ANGLE_Z = 30.0f;

	private Vector3 _camera_pos = Vector3.zero;
	private Vector3 _view_point = Vector3.zero;

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
		_view_point = Vector3.Lerp ( first_player.transform.position, last_player.transform.position, 0.5f );

		float view_x = last_player.transform.position.x + ( camera_width / 2 );
		_camera_pos = _view_point + Vector3.forward * -10;

		// カメラから注視点までの距離を計算
		float camera_field = Camera.main.fieldOfView;
		float distance = camera_width / 2 /  Mathf.Tan( camera_field / 4 );

		// カメラの座標を計算
        float adjust = 3.0f;
		_camera_pos.x += adjust;
		_camera_pos.y = _camera_pos.y + distance / Mathf.Sin( CAMERA_ANGLE_Y ) / ( adjust / 1.5f );
		_camera_pos.z = _camera_pos.z - distance * Mathf.Cos( CAMERA_ANGLE_Z ) + ( adjust * 2 );
		Camera.main.transform.position =  _camera_pos;

        float z = Camera.main.transform.rotation.z;
		// カメラの回転を計算
		Camera.main.transform.LookAt( _view_point );
        Camera.main.transform.Rotate( new Vector3( Camera.main.transform.rotation.x,
                                                   Camera.main.transform.rotation.y, 
                                                   4.0f ) );
	}

	/// <summary>
	/// rayを飛ばす
	/// </summary>
	public void pointToRay( ) {
		// rayの方向を計算
		Vector3 dir = _view_point - _camera_pos;
		// rayを作成
		Ray ray = new Ray( _camera_pos, dir ); 
		// rayの距離を計算
		float dis = Vector3.Distance( _view_point, _camera_pos );

		// rayにぶつかったオブジェクトに対して処理をする
		RaycastHit[ ] hits = Physics.RaycastAll( ray, dis );
		foreach ( RaycastHit hit in hits ) {
			if ( hit.collider.gameObject != null &&
				hit.collider.gameObject.tag == "BackgroundObj" ) {
				GameObject obj = hit.collider.gameObject;
				swichOffRenderer( ref obj );
			}
		}
	
	}

	/// <summary>
	/// rayが当たったオブジェクトのレンダラーをOFF
	/// </summary>
	/// <param name="coll_obj">Coll object.</param>
	private void swichOffRenderer( ref GameObject coll_obj ) {
		coll_obj.GetComponent< BackgroundObj >( ).notRend( );
	}
}