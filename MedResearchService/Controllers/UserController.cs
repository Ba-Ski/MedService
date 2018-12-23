using Microsoft.AspNetCore.Mvc;

namespace MedResearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IKeyProvider _keyProvider;


        public UserController(IKeyProvider keyProvider)
        {
            _keyProvider = keyProvider;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<bool> Get(string id)
        {
            var key = _keyProvider.GetKeys(id);
            return key != null;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
