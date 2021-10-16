using AutoMapper;
using OpenApiSwagger.Models;
using OpenApiSwagger.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OpenApiSwagger.Attributes;

namespace OpenApiSwagger.Controllers
{
    [Produces("application/json","application/xml")]
    [Route("api/authors/{authorId}/books")]
    [ApiController]
    public class BooksController : ControllerBase
    { 
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesDefaultResponseType] 
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
        Guid authorId )
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId); 
            return Ok(_mapper.Map<IEnumerable<Book>>(booksFromRepo));
        }
        /// <summary>
        /// Get a book by Id
        /// </summary>
        /// <param name="authorId"></param>
        /// <param name="bookId"></param>
        /// <returns>An ActionResult of Book</returns>
        /// <response code= "200">Returns the requested Book</response> // to change the default description of the status code
        // Expected status codes
        //[ProducesResponseType(StatusCodes.Status200OK)] OR
        [HttpGet("{bookId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Book))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Produces("application/vnd.marvin.book+json")]
        //[RequestHeaderMatchesMediaType(HeaderNames.Accept,
        //    "application/json",
        //    "application/vnd.marvin.book+json")]
        public async Task<IActionResult> GetBook(Guid authorId, Guid bookId)
        {
            if (! await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Book>(bookFromRepo));
        }

        //*******************************************************************
        [HttpPost()]
        [Consumes("application/json","application/xml", "application/vnd.marvin.bookforcreation+json")]
        [RequestHeaderMatchesMediaType(CommonConstants.contentType,
            "application/json", "application/vnd.marvin.bookforcreation+json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity,
            Type = typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary))]      
        public async Task<ActionResult<Book>> CreateBook(
            Guid authorId,
            [FromBody] BookForCreation bookForCreation)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }

        //*******************************************************************

        [HttpPost()]
        [Consumes("application/json", "application/xml", "application/vnd.marvin.bookforcreationwithamountofpages+json")]
        [RequestHeaderMatchesMediaType(CommonConstants.contentType,
             "application/vnd.marvin.bookforcreationwithamountofpages+json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity,
            Type = typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary))]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Book>> CreateBookWithAmountOfPages(
            Guid authorId,
            [FromBody] BookForCreationWithAmountOfPages bookForCreation)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }

        private class CommonConstants
        {
            public const string contentType = "HeaderNames.ContentType";

        }
    }
}
