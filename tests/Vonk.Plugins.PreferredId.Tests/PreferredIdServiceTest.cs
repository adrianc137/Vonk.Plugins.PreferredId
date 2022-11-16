using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Vonk.Core.Common;
using Vonk.Core.Context;
using Vonk.Core.Repository;
using Vonk.Fhir.R4;
using Task = System.Threading.Tasks.Task;

namespace Vonk.Plugins.PreferredId.Tests
{
    public class PreferredIdServiceTest
    {
        private readonly PreferredIdService _preferredIdService;
        private readonly Mock<ILogger<PreferredIdService>> _loggerMock = new();
        private readonly Mock<IAdministrationSearchRepository> _searchRepositoryMock = new();
        private readonly VonkMockContext _mockContext = new();
        private Mock<IStructureDefinitionSummaryProvider> _schemaProviderMock = new();

        public PreferredIdServiceTest()
        {
            //_schemaProviderMock.Setup(x=>x.Provide(It.IsAny<string>())).Returns(xxx) // ?? No idea what this should return
            _preferredIdService = new PreferredIdService(_loggerMock.Object, _searchRepositoryMock.Object, _schemaProviderMock.Object);
        }

        [Fact]
        public async Task PreferredId_ValidGETRequest_ShouldReturnResult()
        {
            // Arrange
            var searchResult = CreateSearchResult();
            _searchRepositoryMock.Setup(sr => sr.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>()))
                .ReturnsAsync(searchResult);

            var requestArguments = CreateValidRequestArguments();

            _mockContext.Arguments.AddArguments(requestArguments);
            _mockContext.RequestFake.Method = "GET";

            // Act
            await _preferredIdService.PreferredIdGET(_mockContext);

            // Assert
            _mockContext.Response.HttpResult.Should().Be(StatusCodes.Status200OK);
            _mockContext.Response.Payload.Should().NotBeNull();
        }

        [Fact]
        public async Task PreferredId_ValidPOSTRequest_ShouldReturnResult()
        {

            /*
             * System.FormatException
type (at Cannot locate type information for type 'Parameters')
   at Hl7.Fhir.ElementModel.TypedElementOnSourceNode.buildRootPosition(String type)
   at Hl7.Fhir.ElementModel.TypedElementOnSourceNode..ctor(ISourceNode source, String type, IStructureDefinitionSummaryProvider provider, TypedElementSettings settings)
   at Hl7.Fhir.ElementModel.SourceNodeExtensions.ToTypedElement(ISourceNode node, IStructureDefinitionSummaryProvider provider, String type, TypedElementSettings settings)
             */

            // Arrange
            var searchResult = CreateSearchResult();
            _searchRepositoryMock.Setup(sr => sr.Search(It.IsAny<IArgumentCollection>(), It.IsAny<SearchOptions>()))
                .ReturnsAsync(searchResult);

            var requestArguments = CreateValidRequestArguments();

            _mockContext.Arguments.AddArguments(requestArguments);
            _mockContext.RequestFake.Method = "POST";
            _mockContext.RequestFake.Payload = new RequestPayload(true, CreateValidRequestParameters().ToIResource()); 

            // Act
            await _preferredIdService.PreferredIdPOST(_mockContext);

            // Assert
            _mockContext.Response.HttpResult.Should().Be(StatusCodes.Status200OK);
            _mockContext.Response.Payload.Should().NotBeNull();
        }

        // TODO implement invalid requests that end in 400

        private SearchResult CreateSearchResult()
        {
            /* Return
             * "resourceType" : "Parameters",
                "parameter": [
                    {
                        "name" : "result",
                        "valueString" : "2.16.840.1.113883.4.1"
                    }
    ]
             */
            var resource = SourceNode.Resource("resourceType", "Parameters");
            resource.Add(SourceNode.Valued("result", "2.16.840.1.113883.4.1"));
            return new SearchResult(new List<IResource> { resource.ToIResource(VonkConstants.Model.FhirR4) }, 1, 1);
        }

        private Parameters CreateValidRequestParameters()
        {
            var parameters = new Parameters();

            var idParameterComponent = new Parameters.ParameterComponent
            {
                Name = "id",
                Value = new FhirString("111.222")
            };
            var typeParameterComponent = new Parameters.ParameterComponent
            {
                Name = "type",
                Value = new FhirString("oid")
            };

            parameters.Parameter.Add(idParameterComponent);
            parameters.Parameter.Add(typeParameterComponent);

            return parameters;
        }

        private IArgumentCollection CreateValidRequestArguments()
        {
            var arguments = new List<Argument>
            {
                new(ArgumentSource.Path, ArgumentNames.resourceType, Constants.NamingSystem),
                new(ArgumentSource.Query, "id", "111.222"),
                new(ArgumentSource.Query, "type", "oid")
            };

            return new ArgumentCollection(arguments);
        }

        private IArgumentCollection CreateInvalidRequestArguments()
        {
            var arguments = new List<Argument>
            {
                new(ArgumentSource.Path, ArgumentNames.resourceType, Constants.NamingSystem),
                new(ArgumentSource.Query, "id", string.Empty),
                new(ArgumentSource.Query, "type", "oid")
            };

            return new ArgumentCollection(arguments);
        }
    }
}
