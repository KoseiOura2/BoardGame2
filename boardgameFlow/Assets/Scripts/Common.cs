﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace Common {
    public enum PROGRAM_MODE {
		MODE_NO_CONNECT,
        MODE_ONE_CONNECT,
		MODE_TWO_CONNECT,
    };
    
	public enum GAME_PLAY_MODE {
		MODE_NORMAL_PLAY,
		MODE_PLAYER_SELECT,
	};

	/// <summary>
	/// ホストかクライアントか
	/// </summary>
	public enum SERVER_STATE {
		STATE_NONE,
		STATE_HOST,
		STATE_CLIANT,
	};

	/// <summary>
	/// シーン
	/// </summary>
	public enum SCENE {
		SCENE_CONNECT,
		SCENE_TITLE,
		SCENE_GAME,
		SCENE_FINISH,
	};

	/// <summary>
	/// メインゲームのフロー
	/// </summary>
	public enum MAIN_GAME_PHASE {
		GAME_PHASE_NO_PLAY,
		GAME_PHASE_DICE,
		GAME_PHASE_MOVE_CHARACTER,
		GAME_PHASE_DRAW_CARD,
		GAME_PHASE_BATTLE,
		GAME_PHASE_RESULT,
		GAME_PHASE_EVENT,
		GAME_PHASE_FINISH,
	};

    public enum GAME_STAGE {
        NORMAL,
        BONUS
    }

    /// <summary>
    /// 現在のプレイヤーの行動順
    /// </summary>
    public enum PLAYER_ORDER {
		PLAYER_ONE,
		PLAYER_TWO,
		MAX_PLAYER_NUM,
		NO_PLAYER = -1
	}

	/// <summary>
	/// プレイヤーの順位
	/// </summary>
	public enum PLAYER_RANK {
		NO_RANK,
		RANK_FIRST,
		RANK_SECOND,
	}

	/// <summary>
	/// プレイヤーの勝敗結果
	/// </summary>
	public enum BATTLE_RESULT {
		BATTLE_RESULT_NONE,
		WIN,
		LOSE,
		DRAW,
	};

	/// <summary>
	/// マス調整
	/// </summary>
	public enum MASS_ADJUST {
		NO_ADJUST,
		ADVANCE,
		BACK,
	};

	/// <summary>
	/// スペシャルカード　効果
	/// </summary>
	public enum SPECIAL_LIST {
		ENHANCE_TYPE_DRAW,
		NO_DATA
	}

    public enum CARD_TYPE {
        CARD_TYPE_NONE_TYPE,
        CARD_TYPE_ONCE_ENHANCE,
        CARD_TYPE_ONCE_WEAKEN,
        CARD_TYPE_CONTUNU_ENHANCE,
        CARD_TYPE_INSURANCE,
        CARD_TYPE_UNAVAILABLE,
    };

    public enum MASS_TYPE {
        MASS_NONE,            //0
        MASS_START,           //1
        MASS_GOAL,            //2
        MASS_NORMAL,          //3
        MASS_DENGER,          //4
        MASS_EVENT,           //5
    }

    public enum EVENT_TYPE {
        EVENT_NONE,           //0
        EVENT_START,          //1
        EVENT_GOAL,           //2
        EVENT_DRAW,           //3
        EVENT_MOVE,           //4
        EVENT_TRAP_ONE,       //5
        EVENT_TRAP_TWO,       //6
        EVENT_WORP,           //7
        EVENT_CHANGE,         //8
		EVENT_DISCARD,        //9
    }

    public enum PARTICLE_TYPE {
        PARTICLE_NONE,
        PARTICLE_OCEANCURRENT,
        PARTICLE_SPIRAL,
        PARTICLE_FIREWORKS1,
        PARTICLE_FIREWORKS2,
        PARTICLE_BUBBLE,
        MAX_PARTICLE_NUM
    }

	public enum FIELD_ENVIRONMENT {
		SHOAL_FIELD,
		OPEN_SEA_FIELD,
		DEEP_SEA_FIELD,
		FIELD_ENVIRONMENT_NUM,
		NO_FIELD,
	};

    public enum BGM_TYPE {
		BGM_NONE,
		BGM_FIELD,
		BGM_TILTE,
		BGM_GAME_END2,
	};

	public enum SE_TYPE {
		SE_NONE,
		SE_BUFF,
		SE_BUTTLE,
		SE_CHOICE1,
		SE_CHOICE2,
		SE_DRAW,
		SE_FISH1,
		SE_FISH2,
		SE_MASS,
		SE_PHASE,
		SE_TRESURE,
		SE_RESULT,
	};

	public enum VOICE_TYPE {
		VOICE_NONE,
	};

    /// <summary>
    /// 通信で送受信するフィールド側のデータ
    /// </summary>
    public struct NETWORK_FIELD_DATA {
        public bool[ ] send_status;
        public int player_num;
        public int[ ] hand_num;
        public int[ ] player_power;
        public SCENE scene;
		public MAIN_GAME_PHASE main_game_phase;
		public bool change_scene;
		public bool change_phase;
		public int[ ] card_list_one;
		public int[ ] card_list_two;
		public bool[ ] send_card;
		public BATTLE_RESULT[ ] result_player;
        public EVENT_TYPE[ ] event_type;
		public bool send_result;
        public int[ ] mass_count;
        public bool game_finish;
	};

	/// <summary>
	/// 通信で送受信するプレイヤー側のデータ
	/// </summary>
	public struct NETWORK_PLAYER_DATA {
		public bool changed_scene;
		public bool changed_phase;
		public int dice_value;
		public bool ready;
		public int player_power;
        public int hand_num;
		public int[ ] used_card_list;
		public int[ ] turned_card_list;
		public bool battle_ready;
		public MASS_ADJUST mass_adjust;
        public bool ok_event;
        public bool connect_ready;
        public bool go_title;
        public bool start_game;
        public bool finish_game;
	};

	/// <summary>
	/// 座標データ
	/// </summary>
	public struct POSS_DATA {
		public int index;	// インデックス
		public uint x;	// X座標
		public uint y;	// Y座標
		public uint z;	// Z座標
        public MASS_TYPE mass_type; //マスタイプ
        public EVENT_TYPE event_type; //マスタイプ
		public int nomalValue; //値１
		public int trapValue; //値２
		public string environment; //環境情報
        public int cardId;
    }

	/// <summary>
	/// ファイルデータ
	/// </summary>
	public struct FILE_DATA {
		public POSS_DATA[ ] mass; // マス配列
	}

	/// <summary>
	/// プレイヤーのデータ
	/// </summary>
	public struct PLAYER_DATA {
        public int id;
		public GameObject obj;
		public PLAYER_RANK rank;
		public int advance_count;	//プレイヤーの進んでいる回数
		public BATTLE_RESULT battle_result;
		public int draw;
		public int power;
        public EVENT_TYPE event_type;
        public bool on_move;
        public GAME_STAGE stage;
    }

    /// <summary>
    /// カードデータ
    /// </summary>
    public struct CARD_DATA {
        public int id;
        public string name;
        public CARD_TYPE enchant_type;
        public int enchant_value;
        public int special_value;
        public int rarity;
        public CARD_DATA( int id, string name, CARD_TYPE enchant_type, int enchant_value, int special_value, int rarity ) {
            this.id = id;
            this.name = name;
            this.enchant_type = enchant_type;
            this.enchant_value = enchant_value;
            this.special_value = special_value;
            this.rarity = rarity;
        }
    }

}