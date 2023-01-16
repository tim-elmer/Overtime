namespace Overtime.Model
{
    public class Configuration
    {
        /// <summary>
        /// The path to the database file.
        /// </summary>
        public string Database { get; set; } = "./overtime.sqlite";

        /// <summary>
        /// Your Discord token for the application.
        /// </summary>
        /// <remarks>Sensitive (duh).</remarks>
        public string Token { get; set; } = string.Empty;
    }
}