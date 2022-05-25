 
using System.Collections.Generic;

namespace Protocol {

    public class BaseReq
    {
        public int shard = -1;
        public long uid = 0;
        public string skey;
    }

	public class LoginReq : BaseReq {
		//public int platform;			// Editr, Guest, Line, Kakao, facebook, google+
		public int os = 0;				// Editor = 0, AOS = 11, IOS = 8 

		public int authProvider;		// AuthProviderGuest = 2, AuthProviderLINE = 4
		public string userKey = "";
		public string userName = "";
		public string providerKey = "";
		public string token = "";
		public string device = "";

        public string platform;
        public string version;
        public string appVer;
    }

	[System.Serializable]
	public class GameBaseReq : BaseReq
	{
		public long ts;
		public int chapter;
		public int stage;
		public int stageEx	= 0;
		public int isEvent	= 0;
		public int eventIdx	= 0;		// 이벤트 스테이지 경우 이벤트 번호 또는 랭킹 모드 일 경우의 이벤트 번호
		public int type = 0;
	}
	
	[System.Serializable]
	public class GameStartReq : GameBaseReq
	{
		
		public int[] items;
	}

	[System.Serializable]
	public class GameFailReq : GameBaseReq
	{
		public int playSec;
		public int gameCoin;
		public int gameScore;
        public int missionClear = 0;

        public List<int> gameRemains;		
		public List<int> items;
        public List<int> missions;

        public string gameKey;	// 
	}
	
	[System.Serializable]
	public class GameClearReq : GameBaseReq
	{
		public int playSec;		
		public int gameCoin;
		public int gameScore;
		public int gameFlower;
		public int gameTurn;
		public int allStageClear = 0;		// 0 => none, 1 => 모든 스테이지 꽃3, 2 => 모든 스테이지 별 
		public int missionClear = 0;		// 1 => clear
		
		public List<int> items;
		public List<int> missions; //
		public string gameKey;

		public int eventMaterial;			// for event Stage DB update
		public List<List<int>> gainMaterials;	// [1001(재료), 1(수량)],[1002,2]...
		public List<int> gainEventItems; 		// [1(eventIndex), 30(ea)] // (예) 초컬릿 이벤트 30개 획득
		
		public List<int> paidItems;
	}

	[System.Serializable]
	public class GameContinueReq : GameBaseReq
	{
	}
	
	[System.Serializable]
	public class GameItemReq : GameBaseReq
	{
		public int itemIndex;	
	}
	
	public class AssetReg
	{
		public int clover;
		public int star;
		public int coin;
		public int jewel;
	}

	[System.Serializable]
	public class MultiRequestFUserKey
	{
		public List<string> fUserKeyList;
	}
}


