namespace Application.Secrets
{
    public interface ISecretClient
    {
        public Task<string> GetSecretAsync(string key, CancellationToken? token = null);
    }
}
