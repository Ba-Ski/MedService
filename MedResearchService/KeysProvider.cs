using System.Collections.Generic;
using System.Linq;
using MedResearchService;
using MedResearchService.Entities;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;
using Newtonsoft.Json;

namespace MedResearchService
{
    public class KeysProvider: IKeyProvider
    {
        public List<Key> Keys { get; private set; }
        public Dictionary<string, Contract> Contracts { get; private set; }
        public Dictionary<string, Dictionary<string, int>> UsersContracts { get; set; }

        public ContractMethod JoinMethod { get; private set;  }
        public ContractMethod FinishMethod { get; private set; }
        public ContractMethod isTrialFinished { get; private set; }
        public ContractMethod PatientExist { get; private set; }

        public Key GetKeys(string pubId)
        {
            return Keys.FirstOrDefault(k => k.PublicKey == pubId);
        }
            
        public KeysProvider()
        {
            JoinMethod = new ContractMethod()
            {
                Signature = "joinTrial",
                ParamsTypes = new[] { "string" }
            };

            FinishMethod = new ContractMethod()
            {
                Signature = "finishTrial",
                ParamsTypes = new[] { "string" }
            };

            isTrialFinished = new ContractMethod()
            {
                Signature = "isTrialFinished",
                ParamsTypes = new string[0]
            };

            PatientExist = new ContractMethod()
            {
                Signature = "patientExist",
                ParamsTypes = new[] { "string" }
            };

            using (var file = System.IO.File.OpenText(@"C:\Users\baski\Projects\hakaton\keys.json"))
            {
                var serializer = new JsonSerializer();
                Keys = (List<Key>)serializer.Deserialize(file, typeof(List<Key>));
            }

            Contracts = new Dictionary<string, Contract>()
            {
                {"0x709607A142ee8BF71743c701147f0f00D8525C7C", new Contract()
                {
                    Name = "LSD research",
                    ContractId = "0x803e2A5462E42B9168D8AB5125348f5227560FAE",
                    Description = "Does LSD make our life better? Does LSD progrmmists work better? Do you have some change?",
                    State = ContractState.PENDING,
                    Params = new [] {"Mood", "Creativity", "Happyness", "Kittens"},
                    MaxCount = 2,
                    CurrentCount = 0,
                    MaxDays = 2
                
                }},
                {"0x3c65B4AbdFB08fC9725011fC8e4CC8D242fA5621", new Contract()
                {
                    Name = "Super hero powers",
                    ContractId = "0xBe4768e2a9B70A462fB0B2cd6720b5753E991278",
                    Description = "Could medecine give us superhuman powers? World will be safer and calmly with super heroes! Who protect us from villaims? Who will bring us 13th salary? Only super heroes! Join us!",
                    State = ContractState.PENDING,
                    Params = new [] {"Strength", "Speed", "Inteligence", "Agility"},
                    MaxCount = 2,
                    CurrentCount = 0,
                    MaxDays = 2
                }}
            };

            UsersContracts = new Dictionary<string, Dictionary<string, int>>();

            foreach (var key in Keys)
            {
                UsersContracts.Add(key.PublicKey, new Dictionary<string, int>());
            }

        }
    }
}

public enum ContractState
{
    JOINED,
    ACTIVE,
    PENDING,
    FINISHED
}

public class Contract
{
    public string ContractId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ContractState State { get; set; }
    public string[] Params { get; set; }
    public int MaxCount { get; set; }
    public int CurrentCount { get; set; }  
    public int MaxDays { get; set; }
}

public class ContractMethod
{
    public string Signature { get; set; }
    public string[] ParamsTypes { get; set; }
}

public interface IKeyProvider
{
    ContractMethod PatientExist { get; }
    ContractMethod JoinMethod { get; }
    ContractMethod FinishMethod { get; }
    ContractMethod isTrialFinished { get; }
    Dictionary<string, Dictionary<string, int>> UsersContracts { get; set; }
    List<Key> Keys { get; }
    Dictionary<string, Contract> Contracts { get; }
    Key GetKeys(string pubId);
}

