using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.Features;
using Vonk.Core.Common;
using Vonk.Core.Context;

namespace Vonk.Plugins.PreferredId.Tests
{
    public class VonkMockContext: VonkBaseContext
    {
        public VonkMockContext()
        {
            RequestFake = new VonkFakeRequest();
            ResponseFake = new VonkFakeResponse();
            ArgumentsFake = new VonkFakeArguments();
            ArgumentsFake.Arguments = new ArgumentCollection();
        }

        public VonkFakeRequest RequestFake
        {
            get => _vonkRequest as VonkFakeRequest;
            set => _vonkRequest = value;
        }

        public VonkFakeResponse ResponseFake
        {
            get => _vonkResponse as VonkFakeResponse;
            set => _vonkResponse = value;
        }

        public VonkFakeArguments ArgumentsFake
        {
            get => _vonkArguments as VonkFakeArguments;
            set => _vonkArguments = value;
        }

        public override string InformationModel => VonkConstants.Model.FhirR4;
    }

    public class VonkFakeRequest : IVonkRequest
    {
        public string Path { get; }
        public string Method { get; set; }
        public string CustomOperation { get; }
        public VonkInteraction Interaction { get; }
        public RequestPayload Payload { get; set; }
    }

    public class VonkFakeResponse : IVonkResponse
    {
        public Dictionary<VonkResultHeader, string> Headers { get; }
        public int HttpResult { get; set; }
        public VonkOutcome Outcome { get; }
        public IResource Payload { get; set; }
    }

    public class VonkFakeArguments : IVonkArguments
    {
        public IArgumentCollection Arguments { get; set; }
    }
}
