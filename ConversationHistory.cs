
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Alterna
{
    public class AssigneePerson
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string id { get; set; }
        public string accountId { get; set; }
        public string personSource { get; set; }
        public string sourceId { get; set; }
        public object sourceUrl { get; set; }
        public object sourceData { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        public object nickname { get; set; }
        public string displayName { get; set; }
        public string personType { get; set; }
        public string authorizationRole { get; set; }
        public string email { get; set; }
        public object phone { get; set; }
        public string teamId { get; set; }
        public string teamName { get; set; }
        public object avatar { get; set; }
        public object metadata { get; set; }
    }

    public class ContextPerson
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string id { get; set; }
        public string accountId { get; set; }
        public string personSource { get; set; }
        public string sourceId { get; set; }
        public object sourceUrl { get; set; }
        public object sourceData { get; set; }
        public object firstName { get; set; }
        public object lastName { get; set; }
        public object username { get; set; }
        public object nickname { get; set; }
        public string displayName { get; set; }
        public string personType { get; set; }
        public string authorizationRole { get; set; }
        public object email { get; set; }
        public object phone { get; set; }
        public object teamId { get; set; }
        public object teamName { get; set; }
        public object avatar { get; set; }
        public object metadata { get; set; }
    }

    public class DisplayNameTranslations
    {
        public string fr { get; set; }
    }

    public class EndPerson
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string id { get; set; }
        public string accountId { get; set; }
        public string personSource { get; set; }
        public string sourceId { get; set; }
        public object sourceUrl { get; set; }
        public object sourceData { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        public object nickname { get; set; }
        public string displayName { get; set; }
        public string personType { get; set; }
        public string authorizationRole { get; set; }
        public string email { get; set; }
        public object phone { get; set; }
        public string teamId { get; set; }
        public string teamName { get; set; }
        public object avatar { get; set; }
        public object metadata { get; set; }
    }

    public class Participant
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string state { get; set; }
        public object createdTimestamp { get; set; }
        public long? joinedTimestamp { get; set; }
        public long? activationTimestamp { get; set; }
        public long? offboardingTimestamp { get; set; }
        public long? leftTimestamp { get; set; }
        public string leftReason { get; set; }
        public object leftComment { get; set; }
        public object conversationRating { get; set; }
        public bool isHidden { get; set; }
        public bool conversationStarred { get; set; }
        public Person person { get; set; }
    }

    public class Person
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string id { get; set; }
        public string accountId { get; set; }
        public string personSource { get; set; }
        public string sourceId { get; set; }
        public object sourceUrl { get; set; }
        public object sourceData { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        public object nickname { get; set; }
        public string displayName { get; set; }
        public string personType { get; set; }
        public string authorizationRole { get; set; }
        public string email { get; set; }
        public object phone { get; set; }
        public string teamId { get; set; }
        public string teamName { get; set; }
        public string avatar { get; set; }
        public object metadata { get; set; }
    }

    public class Recipient
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string id { get; set; }
        public string displayName { get; set; }
        public DisplayNameTranslations displayNameTranslations { get; set; }
        public string avatar { get; set; }
        public string accountId { get; set; }
    }

    public class ConversationHistory
    {
        [JsonProperty("$_type")]
        public string _type { get; set; }
        public string id { get; set; }
        public Recipient recipient { get; set; }
        public AssigneePerson assigneePerson { get; set; }
        public ContextPerson contextPerson { get; set; }
        public EndPerson endPerson { get; set; }
        public List<Participant> participants { get; set; }
        public object createdTimestamp { get; set; }
        public object onboardingTimestamp { get; set; }
        public long? activationTimestamp { get; set; }
        public long? assigneeJoinTimestamp { get; set; }
        public long? reboardingTimestamp { get; set; }
        public long? offboardingTimestamp { get; set; }
        public long? endTimestamp { get; set; }
        public long? lastMessageTimestamp { get; set; }
        public long? lastCompletedRecordingTimestamp { get; set; }
        public long? queuedTimestamp { get; set; }
        public string state { get; set; }
        public string initialEngagementType { get; set; }
        public string locale { get; set; }
        public string endReason { get; set; }
        public object tokboxSessionId { get; set; }
        public string conversationTemplateId { get; set; }
        public object externalMessengerChannelIconId { get; set; }
        public object externalMessengerChannelName { get; set; }
        public object topic { get; set; }
        public object sourceUrl { get; set; }
        public string scheduledTimestamp { get; set; }
        public string deletionTimestamp { get; set; }
        public string initialEngagementUrl { get; set; }
        public string awaitedPersonType { get; set; }
        public object awaitedPersonTypeChangeTimestamp { get; set; }
    }
}
