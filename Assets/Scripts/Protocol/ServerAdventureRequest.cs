using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Protocol
{
    public class AdventureGameStartReq
    {
        public int chapter;
        public int stage;
        public int type; // 탐험모드 게임 타입 기본은 0
    }

    public class AdventureGameClearReq
    {
        public int chapter;
        public int stage;
        public int coin;
        public int deckId;
        public int missionClear;

        public string gameKey;
        public Dictionary<int, int> dropBoxes; // box type, box count
        public int type; // 탐험모드 게임 타입 기본은 0
        public int turn; // 총 걸린 턴
        public int playSec; // 플레이한 시간
        public List<int> items; // 사용한 아이템
    }

    public class AdventureGameFailReq
    {
        public int chapter;
        public int stage;
        public int coin;
        public int deckId;
        public int type; // 탐험모드 게임 타입 기본은 0
        public int playSec; // 플레이한 시간
        public List<int> items; // 사용한 아이템
    }

    public class AdventureGameCancelReq
    {
        public int chapter;
        public int stage;
        public int type; // 탐험모드 게임 타입 기본은 0
        public int playSec; // 플레이한 시간
        public List<int> items; // 사용한 아이템
    }

    public class AdventureGameContinueReq
    {
        public int chapter;
        public int stage;
    }
}