using System.Collections.Generic;
using System.Linq;
using MedResearchService.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MedResearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResearcheController : ControllerBase
    {
        private readonly IBlockChainAdapter _blockChainAdapter;
        private readonly IKeyProvider _keyProvider;

        public ResearcheController(IBlockChainAdapter blockChainAdapter, IKeyProvider keyProvider)
        {
            _keyProvider = keyProvider;
            _blockChainAdapter = blockChainAdapter;
        }


        [Route("user/all")]
        [HttpGet]
        public ActionResult<string> GetAllIndicates()
        {
            var key = HttpContext.Request.Cookies["auth-key"];
            if (key == null)
            {
                return BadRequest();
            }

            return _blockChainAdapter.GetIndicatesAsync(key).Result;
        }

        [Route("user/join/{id}")]
        [HttpPost]
        public ActionResult<bool> JoinResearch(string id)
        {
            var key = HttpContext.Request.Cookies["auth-key"];
            if (key == null)
            {
                return BadRequest();
            }

            var keysInfo = _keyProvider.GetKeys(key);
            if (keysInfo == null)
            {
                return BadRequest();
            }

            if (!_keyProvider.Contracts.TryGetValue(id, out var contract))
            {
                return BadRequest();
            }
            var sig = _blockChainAdapter.CreateMethodSig(_keyProvider.JoinMethod.Signature, _keyProvider.JoinMethod.ParamsTypes, new []{key});
            var res = _blockChainAdapter.InteractWithContract(key, keysInfo.PrivateKey, id, sig).Result;

            if (!res)
            {
                return BadRequest();
            }

            if (_keyProvider.UsersContracts[key].ContainsKey(id))
            {
                return Ok();
            }

            _keyProvider.UsersContracts[key][id] = 0;
            var research = _keyProvider.Contracts[id];
            research.CurrentCount++;
            if (research.MaxCount == research.CurrentCount)
            {
                research.State = ContractState.ACTIVE;
            }


            return Ok();

        }


        [Route("user/{id}/all")]
        [HttpGet]
        public ActionResult<string> GetAllByUserId(string id)
        {
            return _blockChainAdapter.GetIndicatesAsync(id).Result;
        }

        [Route("user/add")]
        [HttpPost]
        public ActionResult<string> AddIndicationsByUserId([FromBody] Dictionary<string,string> indications)
        {
            var key = HttpContext.Request.Cookies["auth-key"];
            if (key == null)
            {
                return BadRequest();
            }

            if (!indications.TryGetValue("ResearchId", out var researchId))
            {
                BadRequest();
            }

            var res = _blockChainAdapter.CreateUserIndications(indications, key).Result;
            if (!res)
            {
                return BadRequest();
            }

            if (!_keyProvider.UsersContracts[key].ContainsKey(researchId))
            {
                BadRequest();
            }

            _keyProvider.UsersContracts[key][researchId]++;

            if (_keyProvider.UsersContracts[key][researchId] ==
                _keyProvider.Contracts[researchId].MaxDays)
            {
                var keysInfo = _keyProvider.GetKeys(key);
                if (keysInfo == null)
                {
                    return BadRequest();
                }
                var sig = _blockChainAdapter.CreateMethodSig(_keyProvider.FinishMethod.Signature, _keyProvider.FinishMethod.ParamsTypes, new string[0]);
                res = _blockChainAdapter.InteractWithContract(key, keysInfo.PrivateKey, researchId, sig).Result;
                if (!res)
                {
                    BadRequest();
                }

            }

            return Ok();
        }

        [Route("all")]
        [HttpGet]
        public ActionResult<List<Contract>> GetAllResearches()
        {
            var key = HttpContext.Request.Cookies["auth-key"];
            if (key == null)
            {
                return BadRequest();
            }

            var researches = new List<Contract>();
            foreach (var keyProviderContract in _keyProvider.Contracts.Values)
            {
                researches.Add(new Contract()
                {
                    Name = keyProviderContract.Name,
                    Description = keyProviderContract.Description,
                    Params = keyProviderContract.Params,
                    State = keyProviderContract.State == ContractState.PENDING &&
                            _keyProvider.UsersContracts[key].ContainsKey(keyProviderContract.ContractId)
                        ? ContractState.JOINED
                        : keyProviderContract.State,
                    ContractId = keyProviderContract.ContractId,
                    MaxCount = keyProviderContract.MaxCount,
                    CurrentCount = keyProviderContract.CurrentCount
                });
            }
            return researches;
        }
    }
}