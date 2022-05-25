using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


    public class CdnAdventureMeta
    {
        public int expEvent;
        public int expEventScale;

        public int openChapter;
    }

    public class CdnAdventureAnimal
    {
        public int id;
        public int grade;
        public int level_up_cost;
        public int max_level;
        public int max_overlap;

        public int lobby_installable; // 설치 가능 유무 0, 1
        public int lobby_reward_cool_time; // 갱신되는 시간 ex) 14400 
        public List<Reward> lobby_rewards; // 보상 종류
    }

    public class CdnAdventureChapter
    {
        public int id;
        public int bossId;
        public List<Reward> mission_reward;
        public List<Reward> rewards;
        public long start_ts;
    }

    public class CdnAdventureStage
    {
        public int id;
        public int chapter;
        public int first_exp;
        public int normal_exp;
        public int mission;
        public List<Reward> rewards;
        public int first_drop_box_ratio;
        public int normal_drop_box_ratio;
        public Dictionary<int, int> drop_box_ratio;  // box type, box select ratio
        public Dictionary<int, List<int>> drop_boxes; // key: box type, value : [ratio, drop_box_Id] 
        public int stage_type;
    }

    public class CdnAdventureGachaProduct
    {
        public int product_id; // 프로덕트 번호 
        public int gacha_id; // 가챠id(이전과 동일)
        public int asset_type; // 재화 타입 coin : 2, dia : 3
        public int price; // 가격
        public int sale; // 세일 유무 on : 1, off 0
        public int collabo; // 0인 경우 콜라보가 아님 
        public int rate_up;  // 0인 경우 확률업이 아님
        public List<Reward> rewards;
        public long expired_at; // 만료시간, 0인 경우 무시
    }
