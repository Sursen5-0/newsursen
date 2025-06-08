namespace Domain.Interfaces.ExternalClients
{
    public interface ISecretClient
    {
        /// <summary>
        /// Asynchronously retrieves a secret value for the specified key.
        /// </summary>
        /// <param name="key">The name of the secret to retrieve.</param>
        /// <param name="token">An optional cancellation token for the request.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with the secret value as a <see cref="string"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the secret is not found, the client is unauthorized, the service is unavailable, or an unhandled HTTP status code is returned.
        /// </exception>
        public Task<string> GetSecretAsync(string key, CancellationToken? token = null);
    }
}
