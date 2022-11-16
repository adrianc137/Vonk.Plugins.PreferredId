using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using Hl7.FhirPath;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Vonk.Core.Context;
using Vonk.Core.ElementModel;
using Vonk.Core.Repository;
using Vonk.Fhir.R4;
using Task = System.Threading.Tasks.Task;

namespace Vonk.Plugins.PreferredId
{
    public class PreferredIdService
    {
        private readonly IAdministrationSearchRepository _searchRepository;
        private readonly ILogger<PreferredIdService> _logger;
        private readonly IStructureDefinitionSummaryProvider _schemaProvider;

        public PreferredIdService(
            ILogger<PreferredIdService> logger,
            IAdministrationSearchRepository searchRepository,
            IStructureDefinitionSummaryProvider schemaProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _searchRepository = searchRepository;
            _schemaProvider = schemaProvider;
        }

        public async Task PreferredIdGET(IVonkContext vonkContext)
        {
            _logger.LogTrace("Entering GET method.");

            // get id and type parameters
            //var id = context.Arguments.ResourceIdArgument().ArgumentValue;
            //var type = context.Arguments.ResourceTypeArguments().GetArguments("type"); // ??

            var id = "http://hl7.org/fhir/sid/us-ssn";
            var type = "oid";

            await GetResult(vonkContext, id, type);
        }

        // POST [base]/NamingSystem/$preferred-id
        public async Task PreferredIdPOST(IVonkContext vonkContext)
        {
            _logger.LogTrace("Entering GET method.");

            var (request, _, response) = vonkContext.Parts();

            if (!request.GetRequiredPayload(response, out var resource))
            {
                _logger.LogWarning("POST request contains no valid payload.");
                return;
            }

            var parameters = resource.ToTypedElement(_schemaProvider);
            var idNode = parameters.Select("children().where($this.name = 'id')").FirstOrDefault();
            var typeNode = parameters.Select("children().where($this.name = 'type')").FirstOrDefault();

            var idValue = idNode?.ChildString("value");
            var typeValue = typeNode?.ChildString("value");

            await GetResult(vonkContext, idValue, typeValue);
        }

        private async Task GetResult(IVonkContext vonkContext, string id, string type)
        {
            if (!ValidIncomingRequest(vonkContext, id, type))
            {
                return;
            }

            // Assuming this is how you search for a id+type within the NamingSystem resource type
            var searchResult = await GetSearchResultAsync(vonkContext, id, type);

            if (searchResult.TotalCount > 1)
            {
                vonkContext.Response.HttpResult = StatusCodes.Status409Conflict;
                vonkContext.Response.Outcome.AddIssue(VonkIssue.DUPLICATE_ID,
                    "There was more than 1 result found for the provided id and type values.");
                return;
            }

            var response = new Parameters();
            var result = searchResult.SingleOrDefault();
            var parameterComponent = result != null
                ? new Parameters.ParameterComponent
                {
                    Name = "result",
                    Value = new FhirString(
                        "result.GetValue()") // how to get the actual value from this SearchResult object?
                }
                : new Parameters.ParameterComponent();
            response.Parameter.Add(parameterComponent);

            vonkContext.Response.HttpResult = StatusCodes.Status200OK;
            vonkContext.Response.Payload = response.ToIResource();
        }

        private bool ValidIncomingRequest(IVonkContext vonkContext, string id, string type)
        {
            
            // validate id and type
            if (string.IsNullOrWhiteSpace(id))
            {
                vonkContext.Response.HttpResult = StatusCodes.Status400BadRequest;
                vonkContext.Response.Outcome.AddIssue(VonkIssue.INVALID_REQUEST, "The given parameter 'id' is not a valid string value.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                vonkContext.Response.HttpResult = StatusCodes.Status400BadRequest;
                vonkContext.Response.Outcome.AddIssue(VonkIssue.INVALID_REQUEST, "The given parameter 'id' is not a valid string value.");
                return false;
            }

            return true;
        }

        private async Task<SearchResult> GetSearchResultAsync(IVonkContext vonkContext, string id, string type)
        {
            var args = new ArgumentCollection(
                new Argument(ArgumentSource.Internal, ArgumentNames.resourceType, Constants.NamingSystem),
                new Argument(ArgumentSource.Internal, "_id", id),
                new Argument(ArgumentSource.Internal, "_type", type));

            // Tried both Latest and History, same outcome - 0 results
            var options = SearchOptions.History(vonkContext.ServerBase, vonkContext.Request.Interaction, vonkContext.InformationModel);

            return await _searchRepository.Search(args, options);
        }

        private bool IsIdValid(string id)
        {
            /*
             * The server parses the provided id to see what type it is (mary a URI, an OID as a URI, a plain OID, or a v2 table 0396 code).
             * If the server can't tell what type of identifier it is, it can try it as multiple types.
             * It is an error if more than one system matches the provided identifier
             */

            // AA: Don;t know what "mary a URI" means;
            // regardless, I would have to spend time going through the specs and creating regex for each of these types
            // The type provided for the id would be used to validate against the correct format
            throw new NotImplementedException();
        }
    }
}
