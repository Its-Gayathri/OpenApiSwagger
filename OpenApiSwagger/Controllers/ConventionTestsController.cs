using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace OpenApiSwagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiConventionType(typeof(CustomConventions))]
    public class ConventionTestsController : ControllerBase
    {
        // GET: api/<ConventionTestsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ConventionTestsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ConventionTestsController>
        [HttpPost]
        //[ApiConventionMethod(typeof(CustomConventions),nameof(CustomConventions.Insert))] // custom convention
        public void InsertTest([FromBody] string value) //replaced Post with InsertTest--> in this case it requires custom convention
        {
        }

        // PUT api/<ConventionTestsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ConventionTestsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
