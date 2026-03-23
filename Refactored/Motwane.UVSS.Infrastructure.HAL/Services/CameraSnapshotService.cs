using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Motwane.UVSS.HAL
{
    public class CameraSnapshotService : ICameraSnapshotService
    {
        private const string anprUrl = "https://192.168.4.56/cpapi/snapshot.cgi";
        private const string driverUrl = "https://admin:sefthS$2702@192.168.4.57/cpapi/snapshot.cgi";

        private const string username = "admin";
        private const string password = "sefthS$2702";

        private HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(username, password),
                ServerCertificateCustomValidationCallback = (s, c, ch, e) => true
            };

            return new HttpClient(handler);
        }

        public async Task<byte[]> CaptureAnprAsync()
        {
            var client = CreateClient();
            var response = await client.GetAsync(anprUrl);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> CaptureDriverAsync()
        {
            var client = CreateClient();
            var response = await client.GetAsync(driverUrl);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}