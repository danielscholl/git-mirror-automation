using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Helpers
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Fetches results from an api endpoint and if said endpoint returns
        /// RFC'd next links then keeps following them (and aggregating the results).
        /// https://www.w3.org/wiki/LinkHeader
        /// </summary>
        public static async Task<T[]> GetPaginatedAsync<T>(this HttpClient httpClient, string url, CancellationToken cancellationToken)
        {
            string? nextLink = null;
            var items = new List<T>();
            do
            {
                var response = await httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                var results = await JsonSerializer.DeserializeAsync<T[]>(await response.Content.ReadAsStreamAsync(), JsonSettings.Default);
                items.AddRange(results);

                if (response.Headers.TryGetValues("Link", out var values))
                {
                    nextLink = values
                        .Select(v =>
                        {
                            // content as per spec: https://developer.github.com/v3/#link-header
                            // Link: <https://api.github.com/user/repos?page=3&per_page=100>; rel="next", < https://api.github.com/user/repos?page=50&per_page=100>; rel="last", ...
                            // only care for rel="next" link

                            if (string.IsNullOrEmpty(v) ||
                                !v.Contains(","))
                                return null;

                            foreach (var hyperlinkSections in v.Split(','))
                            {
                                if (!hyperlinkSections.Contains(";"))
                                    continue;
                                var parts = hyperlinkSections.Split(';');
                                if (!parts[1].Trim().Equals("rel=\"next\"", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                return parts[0].Trim().TrimStart('<').TrimEnd('>');
                            }
                            return null;
                        })
                        .Where(x => x != null)
                        .FirstOrDefault();
                    if (nextLink != null)
                    {
                        url = nextLink;
                        if (url.StartsWith(httpClient.BaseAddress.ToString(), StringComparison.OrdinalIgnoreCase))
                            url = url.Substring(httpClient.BaseAddress.ToString().Length);
                    }
                }
            }
            while (nextLink != null);

            return items.ToArray();
        }
    }
}
