
using Newtonsoft.Json;
using System.Collections.Generic;
 

namespace Protocol
{
	public class BaseResp
	{
		public int code = (int)ServerError.UnknownError;
        [JsonProperty("err")]
        public string error;

		public BaseResp()
		{
			 
		}

		public BaseResp(ServerError code)
		{
			this.code = (int) code;
		}

		public BaseResp(int code, string msg)
		{
			this.code = code;
			this.error = msg;
		}

		public bool IsSuccess
		{
			get { return code == (int) ServerError.Success; }
		}

		public bool IsNetworkError 
		{
			get { return code == (int) ServerError.NetworkError; }
		}
		
		public int Error
		{
			get { return code; }
		}

		public string Message
		{
			get { return error; }
		}

		public override string ToString() {
			return string.Format("[server] RESP code:{0} error:{1} ", code, error);
		}
	}

	public class StringResp : BaseResp
	{
		public string text;
	}


	public class LoginResp : BaseResp
	{
		public int vsn;
		public LoginCdn cdn;

		public ServerUser user;
		public ServerUserInfo userInfo;
		
		public bool newUser;
		public long serverTime;
		public long serverTimeMs;
		public string tick;
		public int shard;
		public string pid; // debug
		public string salt;

		public int messageCnt;
		public List<ServerUserChapter> chapterOpen;
		public List<ServerEventChapter> eventChapterOpen;
		public ServerUserItem userItem;

		public override string ToString() {
			return string.Format("<LoginResp> vsn:{0} user:{1} items:{2} chpaterC:{3}  ", 
				vsn, user, userItem, chapterOpen.Count );
		}
	}

	public class LoginUserDataResp : BaseResp
	{
		public int day;
		public int haveInviteVer;
		public int rankPoint;
		public int coinGachaCount;
		public int jewelGachaCount;

		public List<ServerUserMission> missionOpen;
		public List<ServerUserQuest> questOpen;
		public List<ServerUserMaterialSpawnProgress> materialSpawn;
		public List<ServerUserHousingItem> housingItem;
		public List<ServerUserHousingSelected> housingList;
		public List<ServerUserGiftBox> giftboxes;
		public List<ServerUserMaterial> materials;
		public List<ServerUserSendCloverCoolTime> cloverCoolTimes;
		public List<ServerUserToy> toys;
		public List<ServerUserStamp> stamps;
		public List<Notice> notice;
		public List<ServerUserShopPackage> shopPackages;
		public List<ServerUserSpecialEvent> specialEvents;
		public List<int> customRewards;
		public List<ServerUserEventSticker> stickerEvents;
		public List<ServerUserCostume> costumes;
		public List<ServerRankStage> rankStage;
		public List<ServerUserCompletePackage> completePackages;
		public ServerStageRank stageRank;
		public ServerUserBlossomEvent blossomEvent;
        public List<ServerUserMail> inbox;

	}

	public class UserAssetResp : BaseResp
	{
		public ServerUserAsset userAsset;
	}

	public class ServerTimeResp : BaseResp
	{
		public long serverTime;
		public long serverTimeMs;
		public string tick;
	}

	public class GameStartResp : BaseResp
	{
		public int isEvent;
		public int eventIdx;
		public int stage;
		public string gameKey;
		public int[] items;

		public ServerUserItem userItem;
		public ServerUserStage userStage;
		public ServerEventStage eventStage;
		public ServerRankStage rankStage;
		
		public ServerUserAsset userAsset;
		public List<ServerUserQuest> userQuests;
	}

	public class GameItemResp : BaseResp
	{
		public int item;

		public ServerUserItem userItem;
		public ServerUserAsset userAsset;
	}

	public class GameActionResp : BaseResp
	{
		public int isEvent;
		public int eventIdx;
		public int stage;
		  

		public ServerUserStage userStage;
		public ServerEventStage eventStage;
		public ServerRankStage rankStage;
		public ServerEventChapter eventChapter;
		
		public ServerUserAsset userAsset;

		public override string ToString()
		{
			return string.Format("[GameActionResp] {0}, asset:{1}, stageData:{2}", stage, userAsset, userStage);
		}
	}

	public class UserRefreshResp : BaseResp
	{
//		public List<ServerUserMaterialSpawnProgress> materialSpawn;
		public int isRefresh;
	}
		
	public class GameClearResp : BaseResp
	{
		public int isEvent;
        public int type;
		public int eventIdx;
		public int chapter;
		public int stage;
		public int nextStage;
		public int allStageClear;
        public int isFirst;
		
		public int flowerLevel;
		public int score;
		public int gainStar;
		public int gainClover;
		public bool levelUp;
		public ServerUserAsset userAsset;						// MUST
		
		public ServerUserStage userStage;	
		public ServerEventStage eventStage;						// MUST
		public ServerUserChapter userChapter; //  				// MUST if first clear)
		public ServerEventChapter eventChapter; 				// option may be New
		public ServerUserChapter newChapter; //  				// option
		public ServerUserSpecialEvent specialEvent;
		public ServerRankStage rankStage;
		public ServerStageRank stageRank;
		
		public int addMail;										// option
		public ServerUserItem userItem;
		public List<ServerUserStamp> newStamps;
		public List<ServerUserToy> newToys;						// option
		public List<ServerUserGiftBox> giftBoxes;				// option (all)
		public List<ServerUserMaterial> newMaterials;			// option
		public List<ServerUserQuest> newQuests;				// option
		public ServerUserCostume newCostume;
		public ServerUserBlossomEvent blossomEvent;

		public ServerUserAdventureAnimal newAnimal;

		public string actionId; // option, for Line CrossPromotion
		
		public bool HasToy {
			get { return newToys != null && newToys.Count > 0; }
		}
		public ServerUserToy GetFirstToy() {
			if (newToys != null && newToys.Count > 0) {
				return newToys[0];
			}
			return null;
		}
	}
	

	public class OfflineGameResp : BaseResp
	{
	}

	public class UserPrePurchaseResp : BaseResp
	{
		public string orderId;
	}
	public class UserPostPurchaseResp : BaseResp
	{
		public ServerUserAsset userAsset;
        public int purchaseCnt;
    }
	

	public class UserMissionsResp : BaseResp
	{
		public List<ServerUserMission> userMissions;

	}
	public class UserStagesAllResp : BaseResp
	{
		
		public List<ServerUserStage> userStages;
		public List<ServerEventStage> eventStages;
		public List<ServerRankStage> rankStages;
	}
	
	public class UserQuestsResp : BaseResp
	{
		public List<ServerUserQuest> userQuests;
	}

	public class UserQuestsGetRewardResp : BaseResp
	{
		public List<ServerUserQuest> userQuests;
		public List<ServerUserGiftBox> userGiftBoxes;
		public ServerUserAsset userAsset;
		public ServerUserStamp userStamp;
		public ServerUserItem userItem;
		public List<ServerUserMaterial> userMaterial;
		public ServerUserCostume userCostume;
		public List<ServerUserToy> toy;
		public int addMail;
		public bool levelUp;
		public ServerUserHousingItem userHousing;
		public ServerUserAdventureAnimal userAnimal;
	}

	public class UserHousingResp : BaseResp
	{
		public List<ServerUserHousingSelected> userHousings;
	}

	public class UserHousingItemResp : BaseResp
	{
		public List<ServerUserHousingItem> userHousingItems;
	}

	public class UserStampResp : BaseResp
	{
		public ServerUserStamp userStamp;
		public ServerUserAsset userAsset;
		public int addMail;
	}

	public class ApplyMissionResp : BaseResp
	{
		public int day;
		public int missionCnt;
		
		public ServerUserMission mission;
		public List<ServerUserMission> missionOpen;
		public ServerUserAsset userAsset;
		public List<ServerUserQuest> userQuests;
		public List<ServerUserMaterialSpawnProgress> updateSpawn;
		public List<ServerUserHousingSelected> selectedItem;
	}

	public class TimeMissionResp : BaseResp
	{

		public ServerUserMission mission;
		public ServerUserAsset userAsset;
	}

	public class OpenGiftBoxResp : BaseResp
	{
		public long id;
		public ServerUserAsset userAsset;
		public ServerUserItem userItem;
		public List<ServerUserMaterial> userMaterial;
	}

	public class BuyGiftBoxResp : BaseResp
	{
		public ServerUserAsset userAsset;
		public ServerUserItem userItem;
		public List<ServerUserMaterial> userMaterial;
		public List<Reward> giftBox;
	}

	public class UserMessageCountResp : BaseResp
	{
		public int msgCnt;
	}
	
	public class UserInboxResp : BaseResp
	{
		public List<ServerUserMail> inbox;
	}

	public class UserReceiveMailResp : BaseResp
	{
		public long receiveIdx;
		public ServerUserAsset userAsset;
		public List<ServerUserGiftBox> userGiftBoxes;
		public ServerUserStamp userStamp;
		public ServerUserItem userItem;
		public List<ServerUserMaterial> userMaterial;
		public List<ServerUserToy> userToys;
		public List<ServerUserMail> inbox;
		public ServerUserCostume userCostume;
		public ServerUserHousingItem userHousing;
		public ServerUserAdventureAnimal userAdvAnimal;
		public bool levelUp;
	}

	public class MaterialHarvestResp : BaseResp
	{
		public List<ServerUserMaterial> updatedMaterials;
		public List<int> spawnIndexs;
		public List<ServerUserQuest> userQuests;
	}
	
	public class CombineMaterialResp : BaseResp
	{
		public List<ServerUserHousingItem> newItem;
		public List<ServerUserMaterial> materials;
	}

	public class MaterialRespawnResp : BaseResp
	{
		public List<ServerUserMaterialSpawnProgress> spawnMaterial;
	} 

	public class HousingFreeSelectResp : BaseResp
	{
		public List<ServerUserHousingItem> selectedItem;
		public List<ServerUserHousingSelected> newSelect;
	}

	public class HousingChangeResp : BaseResp
	{
		public List<ServerUserHousingSelected> changed; 
	}

	public class HousingBuyResp : BaseResp
	{
		public List<ServerUserHousingItem> buyItem;
		public ServerUserAsset userAsset;
	}

	public class SendCloverResp : BaseResp
	{
		public ServerUserSendCloverCoolTime cloverCoolTime;
		public List<ServerUserQuest> userQuests;
	}
	public class RequestCloverResp : BaseResp
	{
		public ServerUserReqCloverCoolTime cloverCoolTime;
	}
	
	public class RequestedCloverListResp : BaseResp
	{
		public List<ServerUserReqCloverCoolTime> cloverCoolTime;
	}

	public class ToySetResp : BaseResp
	{
		public int setIdx;
	}
	
	public class InvitedFriendsResp : BaseResp
	{
		public int haveInviteVer;
		public int inviteDayCnt;
		public int totalInviteCnt;
		public List<ServerUserInvitedFriend> invitedFriends;
	}
	
	public class InviteFriendResp : BaseResp
	{
		public int haveInviteVer;
		public int inviteDayCnt;
		public int totalInviteCnt;
		public ServerUserInvitedFriend inviteFriend;
		public List<ServerUserQuest> userQuests;
		public ServerUserAsset userAsset;
	}
	
	public class MultiInviteFriendResp : BaseResp
	{
		public int haveInviteVer;
		public int inviteDayCnt;
		public int totalInviteCnt;
		public List<ServerUserInvitedFriend> invitedFriends;
		public List<ServerUserQuest> userQuests;
		public ServerUserAsset userAsset;
	}

	public class ProfileLookupResp : BaseResp
	{
		public ServerUserProfileLookup profileLookup;
		public List<ServerUserHousingItem> housing;
	}

	public class OptionImageLinkResp : BaseResp
	{
		public List<ServerImageLink> links;
	}

	public class NGWordResp : BaseResp
	{
		public bool changed;
		public string text;
	}
	
	public class UserBuyShopPackageResp : BaseResp
	{
		public ServerUserAsset userAsset;
		public ServerUserItem userItem;
		public ServerUserShopPackage userPackage;
		public int messageCnt;
	}

	public class GetCompletePackageRewardResp : BaseResp
	{
		public ServerUserCompletePackage userCompletePack;
		public List<Reward> rewards;
		public int addMail;
	}

	public class CustomRewardResp : BaseResp
	{
		public int addMail;
		public int customReward;
	}

	public class StickerInfosResp : BaseResp
	{
		public List<ServerUserEventSticker> stickers;
		public List<ServerUserEventQuest> eventQuests;
	}

	public class StickerGetRewardResp : BaseResp
	{
		public List<ServerUserEventSticker> stickers;
	}

	public class CostumeBuyResp : BaseResp
	{
		public ServerUserCostume costume;
		public ServerUserAsset userAsset;
	}
	
	public class CostumeSetResp : BaseResp
	{
        public int charId;
        public int costumeId;
		public List<ServerUserCostume> costume;
	}

	public class LoginEventResp : BaseResp
	{
		public ServerUserLoginEvent LoginEvent;
	}

	public class GetBlueFlowerRewardResp : BaseResp
	{
		public ServerUserChapter chapter;
		public ServerUserHousingItem housing;
		public List<ServerUserMaterial> materials;
		public ServerUserAdventureAnimal userAnimal;
	}

	public class GetRankEventRewardResp : BaseResp
	{
		public ServerUserAsset userAsset;
		public List<ServerUserToy> toys;
		public int rank;
		public bool levelUp;
	}

	public class RankEventMatchResp : BaseResp
	{
		public ServerRankStage rankStage;
	}
	
	public class StageRankMatchResp : BaseResp
	{
		public ServerStageRank stageRank;
	}
	
	public class StageRankRewardResp : BaseResp
	{
		public ServerUserAsset userAsset;
		public List<ServerUserToy> toys;
		public int rank;
		public bool levelUp;
		public ServerStageRank stageRank;
	}

	public class GetCouponResp : BaseResp
	{
		public ServerUserCoupon coupon;
	}

	public class StartBlossomEventResp : BaseResp
	{
		public ServerUserBlossomEvent BlossomEvent;
	}

	public class PurchaseGachaResp : BaseResp
	{
		public ServerUserAsset userAsset;
		public ServerUserAdventureAnimal userAdvAnimal;
		
		public List<Reward> rewards;

		public int coinGachaCount;
		public int jewelGachaCount;
	}

	public class AdventureCanGachaResp : BaseResp
	{
		public bool canGacha;
	}
	
	public class AdventureCheckGachaProductResp : BaseResp
	{
		public bool canGacha;
	}
}
