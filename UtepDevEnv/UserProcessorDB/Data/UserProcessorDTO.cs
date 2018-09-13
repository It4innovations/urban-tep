using System;

namespace UserProcessorDB.Data
{
    public class UserProcessorDTO
    {
        public string FullProcessorName { get; set; }
        public long NamespaceInfoId { get; set; }
        public long TemplateId { get; set; }
        public string GeoserverName { get; set; }
        public string UserName { get; set; }
        public string ProcessorName { get; set; }
        public string Version { get; set; }
        public DateTime CreationalTime { get; set; }
        public bool IsActive { get; set; }
    }
}
