namespace MedResearchService.Entities
{
    public class MedResearch
    {
        public string Name { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string State { get; set; }
        public int ParticipantsCount { get; set; }
        public string[] ParticipantsRefs { get; set; }
    }
}
