using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;

public class ParticleManager : MonoBehaviour {

    // パーティクル関係
    private const float OCEAN_CURRENT_STOP_TIME    = 60.0f;
    private const float OCEAN_CURRENT_DESTROY_TIME = 90.0f;
    private const float SPIRAL_TIME_ONE            = 15.0f;
    private const float SPIRAL_TIME_TWO            = 60.0f;
    private const float SPIRAL_TIME_THREE          = 62.0f;
    private const float SPIRAL_TIME_FOUR           = 200.0f;
    private const float GOAL_PARTICLE_WAIT_TIME    = 30.0f;

    [ SerializeField ]
	private GameObject _particle;
    private GameObject _fireworks_1;
    private GameObject _fireworks_2;
	[ SerializeField ]
	private float _particle_time = 0;

    private int _particle_phase = 0;

    private int _goal_time = 0;
    private int _particle_init_time = 0;
    private int _particle_init_count = 0;
    private List<GameObject> _particle_obj = new List<GameObject> { };

    private PARTICLE_TYPE _particle_type;
    private bool _particle_end = false;

	// Use this for initialization
	void Start () {
	
	}

    public void init( ) {
        loadParticle( );
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void loadParticle( ) {
        _fireworks_1 = Resources.Load< GameObject >( "Prefabs/Effect_02" );
        _fireworks_2 = Resources.Load< GameObject >( "Prefabs/Effect_07" );
    }

    public void setParticle( GameObject particle ) {
        _particle = particle;
    }

    public GameObject getParticle() {
        return _particle;
    }

    public void particleUpdate( ) {
        // パーティクルの更新
        switch( _particle_type ) {
            case PARTICLE_TYPE.PARTICLE_OCEANCURRENT:
				_particle_time++;
				if( _particle_time > OCEAN_CURRENT_STOP_TIME ) {
                    //　パーティクルの停止
					_particle.GetComponent< ParticleEmitter >( ).emit = false;
				}
				if( _particle_time > OCEAN_CURRENT_DESTROY_TIME ) {
                    // パーティクルの削除
					_particle_time = 0.0f;
					_particle = null;
					_particle_phase = 0;
				}
                break;
            case PARTICLE_TYPE.PARTICLE_SPIRAL:
				_particle_time++;
                if ( _particle_phase == 3 ) {
					_particle_end = true;
					_particle_time = 0.0f;
					_particle = null;
                    _particle_phase = 0;
                }
                break;
            case PARTICLE_TYPE.PARTICLE_FIREWORKS:
                if(_particle_init_time > GOAL_PARTICLE_WAIT_TIME) {
                    _particle_init_time = 0;
                    int rand = Random.Range(0, 2);
                    GameObject road_particle = ( rand == 1 ) ? _fireworks_1 : _fireworks_2;
                    _particle_obj.Add((GameObject)Instantiate(road_particle, new Vector3( Random.Range( 90, 110 ), Random.Range( 100, 105 ), 165f ),Quaternion.identity));
                    _particle_obj[_particle_init_count].transform.parent = GameObject.Find( "Canvas" ).transform;
                    _particle_init_count++;
                }
                _particle_init_time++;
                break;
        }
    }

    public void setParticleType( PARTICLE_TYPE particle_type ) {
        _particle_type = particle_type;
    }

	public void particlePhaseUpdate( ) {
		if( _particle_time == 0 ) {
			_particle_phase = 0;
		} else if ( SPIRAL_TIME_ONE < _particle_time && _particle_time < SPIRAL_TIME_TWO ) {
			_particle_phase = 1;
		} else if (  SPIRAL_TIME_TWO < _particle_time && _particle_time< SPIRAL_TIME_THREE ) {
			_particle_phase = 2;
		} else if ( _particle_time > SPIRAL_TIME_FOUR ) {
			_particle_phase = 3;
		} else {
            _particle_phase = 4;
        }
	}

    public int isParticlePhase( ) {
        return _particle_phase;
    }

    public bool isParticleEnd( ) {
        return _particle_end;
    }

    public void resetParticleEnd( ) {
        _particle_end  = false;
    }

    public void enableParticle( ) {
        for(int i = 0; i < _particle_obj.Count; i++) {
            if ( !_particle_obj[ i ].GetComponentInChildren<ParticleSystem>().IsAlive() )
                    _particle_obj[ i ].SetActive( false );
        }
    }

    public void deleteParticle( ) {
        for(int i = 0; i < _particle_obj.Count; i++) {
            Destroy(_particle_obj[ i ]);
        }
		_particle_obj.Clear( );
    }

    public void refreshParticle( ){
        _particle = null;
        _particle_end = false;
        _particle_time = 0;
        _particle_phase = 0;
        _particle_type = PARTICLE_TYPE.PARTICLE_NONE;
    }
}

