using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

public class ServerDoc
{
    public bool IsLogin { get; private set; }

    public ServerUser User { get; private set; }

    public ServerUserInfo UserInfo { get; private set; }

    public void Clear()
    {
        IsLogin = false;
        this.User = null;
    }

    public void ReceiveUser(ServerUser u, ServerUserInfo i)
    {
        this.User = u;
        this.UserInfo = i;
        this.IsLogin = true;
        this.User.day = (uint)Math.Max(u.day, 1);
    }
}


public interface UserAsset
{
    /// <summary>
    /// All Clover
    /// </summary>
    int AllClover { get; set; }

    /// <summary>
    /// Clover Timestamp (가장 최근 충전 시간) 
    /// </summary>
    long CloverTs { get; set; }

    /// <summary>
    /// All Coin
    /// </summary>
    int AllCoin { get; set; }

    /// <summary>
    /// All Star
    /// </summary>
    int Star { get; set; }
    
    /// <summary>
    /// All Wing
    /// </summary>
    int AllWing { get; set; }

    /// <summary>
    /// All Ticket
    /// </summary>
    int AllTicket { get; set; }
}

[System.Serializable]
public class ServerUserAsset : UserAsset
{
    [JsonIgnore]
    public int AllClover
    {
        get { return (int) (clover + fclover); }
        set { }
    }

    [JsonIgnore]
    public long CloverTs
    {
        get { return cloverTs; }
        set { }
    }

    [JsonIgnore]
    public long FreePlayTs
    {
        get { return freePlayTs; }
        set { }
    }

    [JsonIgnore]
    public int AllCoin
    {
        get { return (int) (coin + fcoin); }
        set { }
    }

    [JsonIgnore]
    public int Star
    {
        get { return (int) star; }
        set { }
    }

    [JsonIgnore]
    public int AllWing
    {
        get { return (int) (wing + fwing); }
        set { }
    }

    [JsonIgnore]
    public int AllTicket
    {
        get { return (int)ticket; }
        set { }
    }

    /*JSON PROPS*/
    public uint clover;
    public uint fclover;
    public long cloverTs;
    public long freePlayStartTs;
    public long freePlayTs;
    public long maxQuitTimeInFreePlayTs;

    public uint coin;
    public uint fcoin;

    public uint star;

    public uint level;
    public ulong exp;
    public uint flower;

    public uint wing;
    public uint fwing;
    public long wingTs;

    public uint expBall;

    public uint ticket;

    public override string ToString()
    {
        return string.Format(
            "[UserAsset] clover:{0},{1}  coin:{2} {3} star:{4}, exp:{7} star:{8} flower:{9} wing:{10} fwing:{11}", clover,
            fclover,
            coin, fcoin, star, 0, 0, exp, star, flower, wing, fwing);
    }

    //public void CopyFrom(ServerUserAsset source)
    //{
    //    ObjectCopier.CopyAllTo(source, this);
    //}

    // for offline mode
    //public bool ClientConsumeClover(int qty)
    //{
    //    return ServerLogics.ConsumeAsset(ref fclover, ref clover, (uint) qty);
    //}

    //public bool ClientConsumeCoin(int qty)
    //{
    //    return ServerLogics.ConsumeAsset(ref fcoin, ref coin, (uint)qty);
    //}

    //public bool ClientConsumeStar(int qty)
    //{
    //    return ServerLogics.ConsumeAsset(ref star, (uint)qty);
    //}

    //public bool ClientConsumeTicket(int qty)
    //{
    //    return ServerLogics.ConsumeAsset(ref ticket, (uint)qty);
    //}

    //public bool ClientConsumeItem(int itemSeq, int qty)
    //{
    //    var val = ServerLogics.GetItemConsumeValue(itemSeq);
    //    return ServerLogics.ConsumeAsset(ref fcoin, ref coin, (uint)(qty * val));
    //}

    // 보상으로 얻은 재화들 적용
    //public void ClientRewardAsset(Dictionary<int, uint> r)
    //{
    //    foreach (var kv in r)
    //    {
    //        switch ((RewardType)kv.Key)
    //        {
    //            case RewardType.clover:
    //                fclover = (uint)Math.Clamp((int)(fclover + kv.Value), 0, ServerLogics.Limit_Clover);
    //                break;

    //            case RewardType.coin:
    //                fcoin = (uint)Math.Clamp((int)(fcoin + kv.Value), 0, ServerLogics.Limit_Coin);
    //                break;

    //            case RewardType.star:
    //                star = (uint)Math.Clamp((int)(star + kv.Value), 0, ServerLogics.Limit_Star);
    //                break;

    //            case RewardType.flower:
    //                flower = (uint)Math.Clamp((int)(flower + kv.Value), 0, ServerLogics.Limit_Flower);
    //                break;
    //        }
    //    }
    //}

}

[System.Serializable]
public class ServerUser : ServerUserAsset
{
    public long uid;

    public uint chapter;
    public uint stage;
    public uint day;
    public int missionCnt;
    public int questProg;


    public int blockReview;

    public int toy;
    public int rankPoint;
    public int inviteCnt; // 초대 전송 횟수,
    public int sendCloverCnt; // 클로버 송부 횟수
    public int purchaseCnt; // 구매 횟수
    public long purchaseTs; // 최근 구매 시간

    public string name;
    public long loginTs;

    public DateTime createdAt;

    //public ServerUser Clone()
    //{
    //    return ObjectCopier.Clone<ServerUser>(this);
    //}

    public override string ToString()
    {
        return string.Format("[User] {0} chaper:{1} stage:{2} day:{3} mission:{4}", base.ToString(), chapter, stage,
            day, missionCnt);
    }
}

public class ServerUserInfo
{
    public string name;
    public int blockReview;
    public int appVer;
    public int haveInviteVer;
    public int rankPoint;
    public int coinGachaCount;
    public int jewelGachaCount;
}

public class ServerEventChapter
{
    public int eventIndex;
    public int groupState;
    public int stage;

    public override string ToString()
    {
        return string.Format("[ServerEventChapter] {0}: {1} stage:{2}", eventIndex, groupState, stage);
    }
}

public class ServerStageBase
{
    [JsonProperty("st")] public int stage;


    [JsonProperty("c")] public int continue_;
    [JsonProperty("p")] public int play;
    [JsonProperty("f")] public int fail;
    [JsonProperty("sc")] public int score;
    [JsonProperty("fl")] public int flowerLevel;
}

public class ServerEventStage : ServerStageBase
{
    [JsonProperty("ei")] public int eventIdx;

    [JsonProperty("mac")] public int materialCnt;

    public ServerEventStage(int aEventIdx, int aStage)
    {
        eventIdx = aEventIdx;
        base.stage = aStage;
    }
}

public class ServerRankStage : ServerStageBase
{
    public enum MatchState
    {
        Matching = 0,
        Matched,
        End,
    }

    [JsonProperty("ei")] public int eventIdx;

    [JsonProperty("ms")] public int matchState = 0;

    [JsonProperty("mul")] public string matchedUserList = "";

    public ServerRankStage(int _eventIdx, int _event)
    {
        eventIdx = _eventIdx;
        base.stage = _event;
    }
}

public class ServerStageRank
{
    public enum MatchState
    {
        Matching = 0,
        Matched,
        End,
    }

    [JsonProperty("ei")] public int eventIdx;

    [JsonProperty("gi")] public int groupId;

    public List<int> scores;

    [JsonProperty("ts")] public int totalScore;

    [JsonProperty("mc")] public int missionClear = 0;

    [JsonProperty("ms")] public int matchState = 0;

    [JsonProperty("mul")] public string matchedUserList = "";
}

public enum EChapterClearState
{
    None,
    All_3Star,
    All_4Star,
    All_5Star,
}

public class ServerUserChapter
{
    public enum ClearStateConst
    {
        Offset = 2,
        BeginLv = 3,
    }


    public int chapter;
    public int state = 0;
    public int clearState = 0; // 0, 1, 2, 3
    public int missionState = 0; // 0, 1
    public int isGetBlueFlowerReward = 0;

    public override string ToString()
    {
        return string.Format("[ServerUserChapter] {0}: {1}", chapter, state);
    }
}

public class ServerUserStage : ServerStageBase
{
    [JsonProperty("ex")] public int stageEx;


    [JsonProperty("mc")] public int missionClear;

    [JsonProperty("m1")] public int mprog1;
    [JsonProperty("m2")] public int mprog2;


    public ServerUserStage()
    {
    }

    public ServerUserStage(int aStage, int aStageEx)
    {
        base.stage = aStage;
        stageEx = aStageEx;
    }


    public override string ToString()
    {
        return string.Format("[ServerUserStage] stage:{0}: stageex:{1}  score:{2}", stage, stageEx, score);
    }
}

public class ServerUserItem
{
    public const int MaxReadyItems = 6;
    public const int MaxInGameItems = 4;
    public const int MaxItemSlot = 15;
    private const int StartAdventureIdx = 11;
    private const int StartGameItemIdx = 6;

    public List<int> items = new List<int>(MaxItemSlot);

    public int GetItem(int idx)
    {
        return items[idx];
    }

    public int ReadyItem(int seq)
    {
        if (items == null)
        {
//			Debug.LogWarning("ReadyItem : User Item Array Is Null .......");
            return 0;
        }

        return items[seq];
    }

    public int InGameItem(int seq)
    {
        return items[seq + StartGameItemIdx];
    }

    public int AdventureItem(int seq)
    {
        return items[seq + StartAdventureIdx];
    }

    //public List<int> gameItems;

    public override string ToString()
    {
        return string.Join(",", items.Select(x => x.ToString()).ToArray());
    }
}

public class ServerUserMission
{
    [JsonProperty("i")] public int idx;

    [JsonProperty("cc")] public int clearCount;
    [JsonProperty("ct")] public long clearTime;
    [JsonProperty("st")] public int state;

    public override string ToString()
    {
        return string.Format("[ServerUserMisssion] Index:{0} Count:{1} State:{2}", idx, clearCount, state);
    }
}

public class ServerUserQuest
{
    [JsonProperty("idx")] public int index = 0; // 미션 인텍스
    public int type = 0;
    public int state = 0; // 0.진행중(받은상태)> 1.퀘스트는 완료> 2.보상 확인해서 퀘스트 사라짐
    public long timer = 0; // 해당시간까지 완료 못하면 미션 fail
    public long exTimer = 0; // 확장 타이머
    public int level = 0;
    public int targetCount = 0;
    public int prog1 = 0; // 달성도
    public int prog2 = 0;
    public int prog3 = 0;
    public string info;
    public long valueTime1 = 0;
    public List<Reward> rewardList;

    public override string ToString()
    {
        return string.Format("[ServerUserQuest] Index:{0} State:{1} Info:{2}", index, state, info);
    }
}

public class ServerUserHousingSelected
{
    [JsonProperty("idx")] public int index = 0;
    [JsonProperty("mdl")] public int selectModel = 0;

    public override string ToString()
    {
        return string.Format("[ServerUserHousing] Index:{0} selectModel:{1}", index, selectModel);
    }
}

public class ServerUserHousingItem
{
    [JsonProperty("idx")] public int index = 0;
    [JsonProperty("mdl")] public int modelIndex = 0;

    [JsonProperty("iOp")] public int isOpen = 0;
    [JsonProperty("atv")] public int active = 0;

    public override string ToString()
    {
        return string.Format("[ServerUserHousingItem] Index:{0} modelIndex:{1}", index, modelIndex);
    }
}

public class ServerUserMaterialSpawnProgress
{
    [JsonProperty("idx")] public int spawnIndex = 0;
    [JsonProperty("ts")] public long spawnTs = 0;
    [JsonProperty("ptn")] public int position = 0;
    [JsonProperty("mIdx")] public int materialIndex = 0;
    [JsonProperty("mCnt")] public int materialCount = 0;

    public override string ToString()
    {
        return string.Format("[ServerUserMaterialSpawnProgress] spawnIndex:{0}", spawnIndex);
    }
}

public class Reward
{
    public int type;
    public int value;
}

public class ServerUserGiftBox
{
    public long index;
    public int type;
    public long openTimer;
    public List<Reward> rewardList;

    public override string ToString()
    {
        return string.Format("[ServerUserGiftBox] Index:{0} Type:{1}", index, type);
    }
}

public class ServerUserMail
{
    public long index;
    public long fuid;
    public string fUserKey;
    public long ts;
    public int type;
    public int mtype;
    public int value;
    public int textKey;
    public string text;

    public override string ToString()
    {
        return string.Format("[ServerUserMail] Index:{0} Type:{1}", index, type);
    }
}

public class ServerUserMaterial
{
    [JsonProperty("idx")] public int index;
    [JsonProperty("cnt")] public int count;

    public override string ToString()
    {
        return string.Format("[ServerUserMaterial] Index:{0} Count:{1}", index, count);
    }
}

public class ServerUserSendCloverCoolTime
{
    public string fUserKey;
    public long sendCoolTime;
    public long sendCount;

    public override string ToString()
    {
        return string.Format("[ServerUserCloverCoolTime] fUserKey:{0}", fUserKey);
    }
}

public class ServerUserReqCloverCoolTime
{
    public string fUserKey;
    public long reqCoolTime;

    public override string ToString()
    {
        return string.Format("[ServerUserRequestCloverCoolTime] fUserKey:{0}", fUserKey);
    }
}

public class ServerUserInvitedFriend
{
    public string fUserKey;
    public long expiredAt;

    public override string ToString()
    {
        return string.Format("[ServerUserInvitedFriend] fUserKey:{0}", fUserKey);
    }
}

public class ServerUserToy
{
    [JsonProperty("idx")] public int index = 0;

    public override string ToString()
    {
        return string.Format("[ServerUserToy] index:{0}", index);
    }
}

public class ServerUserStamp
{
    [JsonProperty("idx")] public int index;
    public int reward; // 1 = done
    public long expireTs;
    public int type; // 타입 : 0은 기본, 1은 등급있는거
    public int grade; // 등급 (별 한개부터 다섯개까지)

    public override string ToString()
    {
        return string.Format("[ServerUserStamp] index:{0} expire:{1} reward:{2} type:{3} grade:{4} ", index, expireTs,
            reward, type, grade);
    }
}

public class ServerUserSpecialEvent
{
    public int eventIndex;
    public int progress;
    public int rewardSection;
    public int done;
}

public class ServerUserProfileLookup
{
    public string nickName;
    public int stage;
    public int flowerCnt;
    public int day;
    public List<int> toys;
}

public class CdnMaterial
{
    public int index = 0;
    public int count = 0;
}

public class ServerImageLink
{
    public int idx;
    public int priority;
    public string link;
    public string image;
}

public class ServerUserShopPackage
{
    public int idx;
    public int vsn;
    public string sku;
    public DateTime createdAt;

    public override string ToString()
    {
        return string.Format("[ServerUserShopPackage] index:{0} vsn:{1} sku:{2}", idx, vsn, sku);
    }
}

public class ServerUserCompletePackage
{
    public int idx;
}

public class ServerUserEventQuest : ServerUserQuest
{
    [JsonProperty("end_ts")] public int endTs = 0;

    [JsonProperty("event_index")] public int eventIndex = 0;

    [JsonProperty("target_count")] public int targetCount = 0;
}

public class ServerUserEventSticker
{
    [JsonProperty("event_index")] public int eventIndex = 0;

    [JsonProperty("end_ts")] public int endTs = 0;

    public int state = 0;
    public int prog = 0; // 달성도
}

public class ServerUserCostume
{
    public int char_id = 0;
    public int costume_id = 0;
    public int is_equip = 0;

    //public int GetCostumeIndex()
    //{
    //    foreach (var costume in ServerContents.Costumes)
    //    {
    //        if (costume.Value.char_id == char_id && costume.Value.costume_id == costume_id)
    //        {
    //            return costume.Value.idx;
    //        }
    //    }

    //    return 0;
    //}
}

public class ServerUserLoginEvent
{
    public int loginEventCnt;
    public Reward loginEventReward;
}

public class ServerUserCoupon
{
    public int coupon_type;
    public string coupon;
    public string owner_mid;
}

public class ServerUserBlossomEvent
{
    public int idx = 0;
    public int flowerType = 0; // 3, 4, 5 : 흰, 하늘, 태양  꽃
    public int state = 0;
    public int prog = 0;
    public int targetCount = 0;
}

public class QuestSpawn
{
    public int index;
    public int level;   // 값은 있지만 쓰지는 않을 것. Quest 클래스 안에 있는 값을 쓰면 됌
    public int extends; // 확장 의도로 만들어놓은 듯. 현재 사용하지 않음
}
