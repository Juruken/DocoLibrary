using Nancy;

namespace DocoLibrary
{
    public class IndexModule : NancyModule
    {
        private class IndexModel 
        {
            public string Id { get; set; }
        }

        private readonly IndexModel m_Model;

        public IndexModule()
        {
            m_Model = new IndexModel
            {
                Id = Amazon.Util.EC2InstanceMetadata.InstanceId ?? string.Empty
            };

            Get["/"] = _ => Main();
        }

        public object Main()
        {
            return View["index", m_Model];
        }
    }
}