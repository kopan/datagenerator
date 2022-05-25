using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static partial class GameData
{
    public static readonly int maxFlowerLevel = 3;
    public static readonly int blueFlowerLevel = 4;
    public static readonly int rankPerFlower = 10;
    public static readonly int cloverChargeTime = (60 * 30);
    public static readonly int MaxClover = 5;
    public static readonly int StageConsumeClover = 1;
}

public enum TypeMissionState
{
    Inactive,
    Active,
    Clear,
}

public enum GameType
{
    NORMAL = 0,
    EVENT,
    RANK,
    ADVENTURE,
}

public enum GameItems
{
		ready_item_1    = 0,
		ready_item_2    = 1,
		ready_item_3    = 2,
		ready_item_4    = 3,
		ready_item_5    = 4,
		ready_item_6    = 5,
		ingame_item_1   = 6,
		ingame_item_2   = 7,
		ingame_item_3   = 8,
		ingame_item_4   = 9,
		continue_count  = 10,
		adv_item_1      = 11,
		adv_item_2      = 12,
		adv_item_3      = 13,
        MAX             = 14
}

public enum QuestMissionType
{
    None,
    Star,                               // 별모으기
    WhiteFlower,                        // 흰꽃모으기
    Clover,                     // 3.클로버사용
    CollectMaterial,            // 4.재료수집
    LineBomb,                   // 5.라인폭탄 만들기
    DoubleBomb,                 // 6.더블폭탄 만들기
    RainbowBomb,                // 7.레인보우폭탄 만들기
    MixBomb,                    // 8.폭탄조합 하기
    SendClover,                 // 9.클로버전송
    SendStamp,                  // 10.스템프전송
    InviteFriend,               // 11.친구초대 (InviteBase : 초대장) 초대하기 미션
    Login,                      // 12.로그인
    BlueFlower,                 // 13.파란꽃모으기
    NewStageClear,              // 14.신규 스테이지 클리어 ( 통상, 이벤트 스테이지를 첫 클리어 했을 경우 )

    ChapterCandy = 1000,        // 사탕찾기
    ChapterAllFlower,           // 꽃 피우기
    ChapterDuck,                // 오리찾기
    ChapterStarStone,           // 별돌깨기

    OpenWhiteFlowers = 2000,    // 기간한정 흰 꽃 n(100)개 피우기 이벤트
    OpenBlueFlowers,            // 기간한정 하늘 꽃 n(100)개 피우기 이벤트
}

public enum CostumeGetType
{
    None,
    ExchangeCoin,
    EventReward,
    ExchangeTicket,
}

// 컴파일 확인용
//static public class Global
//{
//    static public string StreamingAssetsPathForFile
//    {
//        get
//        {
//            return "";
//        }
//    }
//}