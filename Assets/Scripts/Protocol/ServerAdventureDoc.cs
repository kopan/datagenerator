using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ServerAdventureStageBase
{
    public int chapter;
    [JsonProperty("st")]
    public int stage;
	
    [JsonProperty("c")]
    public int continue_;
    [JsonProperty("p")]
    public int play;
    [JsonProperty("f")]
    public int fail;
    [JsonProperty("fg")]
    public int flag;
}

public class ServerUserAdventureAnimal
{
    [JsonProperty("ani_id")]
    public int animalId;
    public int grade;
    public int level;
    public int exp;
    public int Overlap;
}

public class ServerUserAdventureDeck
{
    [JsonProperty("deck_id")]
    public int deckId;

    public List<int> animals;
}

public class ServerUserAdventureStage : ServerAdventureStageBase
{
    [JsonProperty("mc")]
    public int missionClear;
}

public class ServerUserAdventureChapter
{
    public int chapter;
    public int state;
}

public class ServerUserAdventureLobbyAnimal
{
    [JsonProperty("ani_id")]
    public int animalId;

    public long get_reward_ts; // 받을 수 있는 시간
}