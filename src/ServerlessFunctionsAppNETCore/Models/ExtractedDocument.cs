using System.Collections.Generic;

namespace ServerlessFunctionsAppNETCore.Models
{
    public class ExtractedDocument
    {
        public ExtractedDocument(string parentUrl)
        {
            ParentUrl = parentUrl;
            ChildUrls = new List<string>();
        }

        public string ParentUrl { get; set; }

        public List<string> ChildUrls { get; set; }
    }
}
