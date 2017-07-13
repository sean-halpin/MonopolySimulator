using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MonopolySimulator.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum PositionType
    {
        go,
        property,
        communitychest,
        tax,
        railroad,
        chance,
        jail,
        utility,
        freeparking,
        gotojail
    }
}