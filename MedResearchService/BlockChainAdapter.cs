using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MedResearchService.Entities;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;
using Newtonsoft.Json;
using Nethereum.ABI;
using Nethereum.ABI.Model;

namespace MedResearchService
{
    public class BlockChainAdapter: IBlockChainAdapter
    {
        private static readonly HttpClient Client = new HttpClient();
        private const string StorageURL = "http://10.177.0.202:9191/storage/";
        private const string ContractsURL = "http://10.177.0.202:9191/contracts/";

        public BlockChainAdapter()
        {

        }

        public async Task<string> GetIndicateAsync(string id)
        {
            var uri = new Uri(StorageURL + id);
            var response = await Client.GetAsync(uri);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }

        public async Task<string> GetIndicatesAsync(string id)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(StorageURL),
                Method = HttpMethod.Get,
                Headers = {{"originator-ref", id }}
            };
            
            var response = await Client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }

        public async Task<string> GetResearches(string id)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(StorageURL),
                Method = HttpMethod.Get,
                Headers = { { "originator-ref", id } }
            };

            var response = await Client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }

        public async Task<bool> CreateUserIndications(Dictionary<string,string> indications, string id)
        {
            var json = JsonConvert.SerializeObject(indications);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(StorageURL),
                Method = HttpMethod.Post,
                Headers = { { "originator-ref", id } },
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await Client.SendAsync(request);

            return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
        }


        public async Task<bool> CreateResearch(MedResearch research, string id)
        {
            var json = JsonConvert.SerializeObject(research);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(StorageURL),
                Method = HttpMethod.Post,
                Headers = { { "originator-ref", id } },
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await Client.SendAsync(request);

            return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
        }

        public async Task<bool> InteractWithContract(string pubKey, string privKey, string contractId, string sig)
        {
            var json = JsonConvert.SerializeObject(new
            {
                modifyState = true,
                contractInvocation = sig
            });

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(ContractsURL + contractId),
                Method = HttpMethod.Put,
                Headers =
                {
                    { "Authorization", privKey },
                    {"accept", "application/json"}
                },
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            var response = await Client.SendAsync(request);

            return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
        }

        public async Task<string> GetStringFromContract(string pubKey, string privKey, string contractId, string sig)
        {
                var json = JsonConvert.SerializeObject(new
            {
                modifyState = false,
                contractInvocation = sig
            });

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(ContractsURL + contractId),
                Method = HttpMethod.Put,
                Headers =
                {
                    { "originator-ref", pubKey },
                    {"accept", "application/json"}
                },
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            var response = await Client.SendAsync(request);
            using (var content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }

        public string CreateMethodSig(string functionSignature, string[] paramsTypes, string[] arguments)
        {
            if (string.IsNullOrEmpty(functionSignature))
            {
                return null;
            }

            var parameters = paramsTypes.Select(t => new Parameter(t)).ToArray();
            var func = new FunctionABI(functionSignature, false) {InputParameters = parameters};
            var sig = new Nethereum.ABI.FunctionEncoding.FunctionCallEncoder().EncodeRequest(func.Sha3Signature, parameters, arguments);

            return sig;
        }
        public bool Decode(string hash)
        {
            var val = JsonConvert.DeserializeObject<Dictionary<string, string>>(hash)["contractResponse"];
            var a = new Nethereum.ABI.Decoders.BoolTypeDecoder();
            return a.Decode(Encoding.UTF8.GetBytes(val));
        }
    }


    public interface IBlockChainAdapter
    {
        bool Decode(string hash);
        Task<bool> InteractWithContract(string pubKey, string privKey, string contractId, string sig);
        Task<string> GetStringFromContract(string pubKey, string privKey, string contractId, string sig);
        string CreateMethodSig(string functionSignature, string[] paramsTypes, string[] arguments);
        Task<bool> CreateResearch(MedResearch research, string id);
        Task<bool> CreateUserIndications(Dictionary<string, string> indications, string id);
        Task<string> GetIndicateAsync(string id);
        Task<string> GetIndicatesAsync(string id);
    }
}