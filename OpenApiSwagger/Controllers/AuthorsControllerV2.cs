using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenApiSwagger.Entities;
using OpenApiSwagger.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenApiSwagger.Controllers
{
    [ApiController]
    [Produces("application/json",
        "application/xml")]
    [Route("api/v{version:apiVersion}/authors")]//URI Versioning
    [ApiVersion("2.0")]//explicit versioning
    public class AuthorsControllerV2 : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsControllerV2(
            IAuthorRepository authorsRepository,
            IMapper mapper)
        {
            _authorsRepository = authorsRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get an Authors (V2)
        /// </summary>
        /// <returns>An ActionResult of type IEnumerable of Authors</returns>
        /// <response code ="200" >Returns the list of Authors</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
            return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
        }
    }
}
