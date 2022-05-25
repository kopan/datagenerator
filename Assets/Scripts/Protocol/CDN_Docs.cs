
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Protocol;

    // from CdnContens(Column:data) <- DynamicContents(DB)

    public class LoginCndConsts
    {
        public const int ReadyItemCount = 6;
        public const int InGameItemCount = 6;
    }

    public enum EGiftBoxType
    {
        S,
        M,
        B,
    }
    public enum CdnMinMax
    {
        Min,
        Max,
    }

    public class LoginCdn
    {
        public int ID;
        public string Tag;

        // 나중에 지울 예정 -> 어떻게 바꿀지 결정뒤에 
        public int titleImageVer;
        public int sendCloverEventVer;
        public int RankEventVer;
        public int StageRankVer;

    /*
        public int ChapterVer;
        public int CostumeVer;
        public int DayVer;
        public int EventChapterVer;
        public int HousingVer;
        public int HousingProgressVer;
        public int LoginEventVer;
        public int MaterialSpawnVer;
        public int PackageVer;


        public int SpecialEventVer;

        public int StageRankGroupVer;
        public int MissionVer;


        public int StickerVer;
        public int QuestVer;
        public int QuestProgressVer;
        public int GiftBoxVer;
        public int ItemRandomVer;
        public int GroupRandomVer;
        public int MaterialRandomVer;

        public int StageVer;
        public int OfflineVer;
        public int MiscVer;
        public int titleImageVer;
        public int crossPromotionVer; // for line CrossPromotion 용 버전

        public int inviteVer;
        [JsonProperty("rever")]
        public int rankEventVer;
        [JsonProperty("srver")]
        public int stageRankVer;
        public int adventureVer;
        */

   // public List<int> MiscOpts;

        public int OpenChapter;
        public int OpenMission;
        public List<int> AllClearRewards;       // [꽃3보상, 별꽃보상]

        public int TimeMissionCost;
        public int ContinueSale;
        public int ContinueCost;
        public List<int> ContinueCosts;
        public int ContinueMax;
        public int EventContinueMax;

        public int EmergencyResetCacheAssetV;
        public int EmergencyResetCacheImageV;
        public int EmergencyResetCacheStageV;

        public int ReBoot;                      // 리붓되는 타임: 분

        public int ReadyItemSale;
        public List<int> ReadyItems;

        // 1 : normal item sale, 2 : adventure item sale, 3 : all item sale
        public int InGameItemSale;
        public List<int> InGameItems;

        //[JsonProperty("jss")]
        public int jewelSale;
        public List<int> jewelCounts;
        public List<int> jewelBonuses;

        // [JsonConverter(typeof(UriConverter))] 
        public List<List<string>> jewelPrices;      // PlayStore에 로그인 안된 유저는 가격정보를 못가져 오기에 가상의 가격 정보 표시 UI용

        public int coinSale;
        public List<int> coinCounts;
        public List<int> coinBonuses;

        public int clover5Sale;
        public int cloverPrice;
        public int cloverFreeTimeSale;
        public int cloverFreeTimePrice;
        public int cloverFreePlayT;

        public int SaleNpc;
        public long giftBoxNpcEndTime;  // time stamp
        public string probabilityInfo;
        public int shopResourceId;
        public int giftBoxSale;     // 0, 1
        public List<int> normalGiftBoxPrice;  // [price, sale]
        public List<int> specialGiftBoxPrice; // [price, sale]
        public List<int> premiumGiftBoxPrice; // [price, sale]

        public int skipStageComment;
        public int skipStampReward;

        public List<List<int>> CItem1;              // CItemType1_1은 CItemsType1[Type,Count]
        public List<List<int>> CItem2;
        public List<List<int>> CItem3;

        public int CoinEventRatio;
        public long CoinEventTs;

        public int collaboIndex;
        public long collaboStartTs;
        public long collaboEndTs;

        public List<int> GbDurations;

        public List<List<int>> GbCoinMinMax;

        public List<List<int>> GbCloverMinMax;      // 0은 없은  [null, 1, 2]

        public int GiftBoxDuration(EGiftBoxType t)
        {
            return GbDurations[(int)t];
        }
        public List<int> GiftBoxCoin(EGiftBoxType t)
        {
            return GbCoinMinMax[(int)t];
        }
        public List<int> GiftBoxClover(EGiftBoxType t)
        {
            return GbCloverMinMax[(int)t];
        }

        public int EnableInvite;
        [JsonProperty("pir")]
        public List<Reward> PerInviteRewards;
        [JsonProperty("pire")]
        public int PerInviteRewardEvent; // 0, 1 : off, on

        public int LoginOffset;

        [JsonProperty("sldl")]
        public List<List<int>> StageLevelDownList; // [[fromStage, NormalFailCount, PaidUserFailCount, Difficult], ...]

        //	[JsonProperty("wp")]
        public int wingPrice;
        //	[JsonProperty("_ws")]
        public int wingSale;

        [JsonProperty("cwp")]
        public int coinWingPrice;
        [JsonProperty("cws")]
        public int coinWingSale;

        [JsonProperty("acs")]
        public int AdvContinueSale;
        [JsonProperty("acp")]
        public int AdvContinuePrice;

        [JsonProperty("ngi")]
        public int normalGachaId;
        [JsonProperty("ngp")]
        public int normalGachaPrice;
        [JsonProperty("ngs")]
        public int normalGachaSale;
        [JsonProperty("pgi")]
        public int premiumGachaId;
        [JsonProperty("pgp")]
        public int premiumGachaPrice;
        [JsonProperty("pgs")]
        public int premiumGachaSale;

        public List<int> GachaProducts;

        public override string ToString()
        {
            return string.Format("[LoginCdn] ({0}) Tag:{1} ContinueSale:{2}", ID, Tag, ContinueSale);
        }
    }
   
    public class CdnStage
    {
        public int stage;
        public int stageEx;

        public int star;
        public int score;

        public string custom;

        public override string ToString()
        {
            return string.Format("[CdnStage] stage:{0} ex:{1} star{2}", stage, stageEx, star);
        }
    }

    public class Notice
    {
        [JsonProperty("idx")]
        public int id = 0;
        [JsonProperty("nIdx")]
        public int noticeIndex = 0;
        [JsonProperty("sTs")]
        public long startTs = 0;
        [JsonProperty("eTs")]
        public long endTs = 0;

        public int type = 0;

        [JsonProperty("url")]
        public string url = "";
        [JsonProperty("depth")]
        public int depth = 0;
        [JsonProperty("csh")]
        public int country = 0;
        [JsonProperty("os")]
        public int os = 0;
        [JsonProperty("sprites")]
        public List<NoticeSprite> noticeSprite;
        [JsonProperty("video")]
        public List<NoticeVideo> video;

        public override string ToString()
        {
            return string.Format("[Notice] id:{0} noticeIndex:{1}", id, noticeIndex);
        }
    }

    public partial class NoticeSprite
    {
        public string filename;
        public List<int> intervals;
        public List<float> position;
        public List<int> size;



        public int GetWeight()
        {
            return size[0];
        }

        public int GetHeight()
        {
            return size[1];
        }
    }

    public partial class NoticeVideo
    {
        public string filename;
        public List<float> position;
        public List<int> size;



        public int GetWeight()
        {
            return size[0];
        }

        public int GetHeight()
        {
            return size[1];
        }
    }



