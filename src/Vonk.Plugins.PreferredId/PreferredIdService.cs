using Microsoft.Extensions.Logging;
using Vonk.Core.Context;
using Vonk.Core.Repository;

namespace Vonk.Plugins.PreferredId
{
    public class PreferredIdService
    {
        private readonly IAdministrationSearchRepository _searchRepository;
        private readonly ILogger<PreferredIdService> _logger;


        public PreferredIdService(ILogger<PreferredIdService> logger/*, IAdministrationSearchRepository searchRepository*/)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //_searchRepository = searchRepository;
        }

        // GET [base]/NamingSystem/$preferred-id?id=http://ns.electronichealth.net.au/id/hi/ihi/1.0&type=oid
        public async Task PreferredIdGET(IVonkContext context)
        {
            _logger.LogDebug("Entering GET method.");

            // get id and type parameters
            var id = context.Arguments.ResourceIdArgument().ArgumentValue;
            var type = context.Arguments.ResourceTypeArguments().GetArguments("type");
        }

        // POST [base]/NamingSystem/$preferred-id
        public async Task PreferredIdPOST(IVonkContext context)
        {
            // context.Parts()

            // validate incoming request -> HTTP error response + OperationOutcome object
        }
    }
}
