using System;

namespace OpenApiSwagger.Models
{
    /// <summary>
    /// An author with Id, firstname, lastName fields
    /// </summary>

    public class Author
    {       
        /// <summary>
        /// Id of the Author
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// FirstName of the Author
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// LastName of the Author
        /// </summary>
        public string LastName { get; set; }
    }
}
