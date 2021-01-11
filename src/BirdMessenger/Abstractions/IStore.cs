namespace BirdMessenger.Abstractions
{
    /// <summary>
    /// Stroe Interface
    /// </summary>
    public interface IStore
    {
        /// <summary>
        ///  Get Url by `fingerprint`
        /// </summary>
        /// <param name="fingerprint">fingerprint</param>
        /// <returns></returns>
        public string Get(string fingerprint);

        public void Set(string fingerprint, string url);

        public void Delete(string fingerprint);

        public void Close();

    }
}
