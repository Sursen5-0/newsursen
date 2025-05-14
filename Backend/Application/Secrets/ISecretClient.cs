namespace Application.Secrets
{
    public interface ISecretClient
    {
        public string GetSecret(string key);
    }
}
