using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace OpenApiSwagger
{
    public static class CustomConventions
    {
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]//match with the action method starting with Insert..
        public static void Insert(
         [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)] //match any input
         [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
        object model)
        {

        }
    }
}
