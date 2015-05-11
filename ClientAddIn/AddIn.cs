using ININ.InteractionClient.AddIn;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClientAddIn
{
    public class AddIn : ININ.InteractionClient.AddIn.QueueMonitor
    {
        private const string CtiClientId = "BCM1234"; //TODO: Get our own, or at least check if this is ok to use.
        private const int SAPPort = 36729;

        private ITraceContext _traceService;

        //Call attributes that we want to receive added/changed events on
        protected override IEnumerable<string> Attributes
        {
            get
            {
                return new[] {InteractionAttributes.State};
            }
        }

        //Other call attributes that we want to access later on. 
        protected override IEnumerable<string> SupportingAttributes
        {
            get
            {
                return new[] {InteractionAttributes.RemoteId, 
                            "EIC_CalledTn",
                            "EIC_CallIdKey"};
            }
        }

        protected override void InteractionAdded(IInteraction interaction)
        {
            if (interaction.GetAttribute(InteractionAttributes.State) == InteractionAttributeValues.State.Alerting ||
                interaction.GetAttribute(InteractionAttributes.State) == InteractionAttributeValues.State.Offering)
            {
                //if the call is alerting the agent, do a screen pop
                DoScreenPop(interaction);
            }
        }

        protected override void OnLoad(IServiceProvider serviceProvider)
        {
            base.OnLoad(serviceProvider);

            _traceService = (ITraceContext)serviceProvider.GetService(typeof(ITraceContext));
            _traceService.Always("SAP C4C addin loaded");
        }

        private string CreateQueryString(IInteraction interaction)
        {
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(String.Empty);

            //TODO: add BP, TicketID, SerialNo, etc from custom call attributes 
            // those attributes need to be added to the SupportingAttributes property above
            queryString.Add("CID", CtiClientId);
            queryString.Add("ANI", interaction.GetAttribute(InteractionAttributes.RemoteId));
            queryString.Add("DNIS", interaction.GetAttribute("EIC_CalledTn"));
            queryString.Add("ExternalReferenceID", interaction.GetAttribute("EIC_CallIdKey"));

            return queryString.ToString();
        }

        private void DoScreenPop(IInteraction interaction)
        {
            var url = String.Format("http://localhost:{0}/?{1}", SAPPort, CreateQueryString(interaction));

            _traceService.Status("Screen pop to " + url);

            WebRequest request = WebRequest.Create(url);
            request.Method = HttpMethod.Get.Method;
            request.GetResponse();
        }
    }

    
}
