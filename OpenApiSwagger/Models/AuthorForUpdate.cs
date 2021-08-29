using System.ComponentModel.DataAnnotations;

namespace OpenApiSwagger.Models
{
    /// <summary>
    /// An author for update with FirstName and LastName
    /// </summary>
    public class AuthorForUpdate
    {
        /// <summary>
        /// FirstName of the author
        /// </summary>
        [Required] //This creates an asterick symbol in spec doc
        [MaxLength(150)]      
        public string FirstName { get; set; }
        /// <summary>
        /// LastName of the author
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string LastName { get; set; }
    }
}
