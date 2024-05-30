using System.ComponentModel.DataAnnotations;
using Azure.Maps.Search;

namespace TinyAgents.Maps.Azure;

public sealed class GetPositionsRequest(string address, bool extendedPostalCodes = true)
{
    private const int DefaultResultSize = 5;

    [Required] public string Address { get; } = address;

    private bool ExtendedPostalCodes { get; } = extendedPostalCodes;

    internal SearchAddressOptions GetOptions()
    {
        var options = new SearchAddressOptions
        {
            CountryFilter = ["AU"],
            Top = DefaultResultSize
        };

        if (ExtendedPostalCodes) options.ExtendedPostalCodesFor = [SearchIndex.Geographies];

        return options;
    }
}