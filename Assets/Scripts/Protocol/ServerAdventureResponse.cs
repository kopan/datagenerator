using System.Collections.Generic;
using Newtonsoft.Json;

namespace Protocol
{
    public class AdventureInitResp : BaseResp
    {
        public ServerUserAsset userAsset;

        public List<ServerUserAdventureAnimal> userAdvAnimals;
        public List<ServerUserAdventureDeck> userAdvDeck;
        public List<ServerUserAdventureStage> userAdvStages;
        public List<ServerUserAdventureChapter> userAdvChapters;
        public List<ServerUserAdventureLobbyAnimal> userAdvLobbyAnimals;

        public CdnAdventureMeta contentsMeta;
        public Dictionary<int, CdnAdventureAnimal> contentsAnimals;
        public Dictionary<int, CdnAdventureChapter> contentsChapters;
        public Dictionary<int, Dictionary<int, CdnAdventureStage>> contentsStages;
        public Dictionary<int, CdnAdventureGachaProduct> contentsGachaProduct;

        public int coinGachaCount;
        public int jewelGachaCount;
    }

    public class AdventureGameStartResp : BaseResp
    {
        public string gameKey;
        public ServerUserAsset userAsset;

        public ServerUserAdventureStage userAdvStage;
    }

    public class AdventureGameClearResp : BaseResp
    {
        public ServerUserAsset userAsset;

        public ServerUserAdventureStage userAdvStage;
        public ServerUserAdventureChapter userAdvChapter;

        public List<ServerUserAdventureAnimal> userAnimals;
        public List<ServerUserGiftBox> giftBoxes; // option (all)

        public Dictionary<int, List<Reward>> boxes; // b_type, 박스에서 떨어지는 아이템 목록

        public bool levelUp;

        public int addMail;
        public List<Reward> rewards;

        public List<ServerUserToy> userToy;
        public List<ServerUserMaterial> userMaterial;
        public ServerUserItem items;
    }


    public class AdventureGameFailResp : BaseResp
    {
        public ServerUserAsset userAsset;

        public ServerUserAdventureStage userAdvStage;
        public List<ServerUserAdventureAnimal> userAnimals;

        public int addMail;
        public List<Reward> rewards;

        public List<ServerUserToy> userToy;
        public List<ServerUserMaterial> userMaterial;
        public ServerUserItem items;
    }

    public class AdventureGameCancelResp : BaseResp
    {
        public ServerUserAdventureStage userAdvStage;
    }

    public class AdventureAnimalLevelUpResp : BaseResp
    {
        public ServerUserAsset userAsset;

        public ServerUserAdventureAnimal userAnimal;
    }

    public class AdventureSetDeckResp : BaseResp
    {
        public ServerUserAdventureDeck UserAdvDeck;
    }

    public class AdventureAddAnimalResp : BaseResp
    {
        public ServerUserAdventureAnimal animal;
    }

    public class AdventureGameContinueResp : BaseResp
    {
        public ServerUserAsset userAsset;
        public ServerUserAdventureStage userAdvStage;
    }

    public class AdventureGetChapterClearRewardResp : BaseResp
    {
        public ServerUserAsset userAsset;
        public ServerUserAdventureChapter userAdvChapter;

        public int addMail;
        public List<Reward> rewards;

        public List<ServerUserToy> userToy;
        public List<ServerUserMaterial> userMaterial;
        public ServerUserItem items;
    }

    public class AdventureRegisterLobbyAnimalResp : BaseResp
    {
        public ServerUserAdventureLobbyAnimal UserLobbyAnimal;
    }
    
    public class AdventureUnRegisterLobbyAnimalResp : BaseResp
    {
        public List<ServerUserAdventureLobbyAnimal> UserLobbyAnimals;
    }

    public class AdventureChangeLobbyAnimalResp : BaseResp
    {
        public ServerUserAdventureLobbyAnimal UserLobbyAnimal;
    }

    public class AdventureGetLobbyAnimalRewardResp : BaseResp
    {
        public ServerUserAsset userAsset;
        public ServerUserAdventureLobbyAnimal UserLobbyAnimal;

        public bool levelUp;
        
        public int addMail;
        public List<Reward> rewards;
        
        public List<ServerUserToy> userToy;
        public List<ServerUserMaterial> userMaterial;
        public ServerUserItem items;
    }

    public class AdventureContentsReFlashResp : BaseResp
    {
        public Dictionary<int, CdnAdventureChapter> contentsChapters;
        public Dictionary<int, Dictionary<int, CdnAdventureStage>> contentsStages;
    }
}
