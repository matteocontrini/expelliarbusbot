using NetEscapades.Configuration.Validation;

namespace Data
{
    public class DatabaseConfiguration : IValidatable
    {
        public string GtfsDataPath { get; set; }

        public string BotDataPath { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.GtfsDataPath))
            {
                throw new SettingsValidationException(nameof(DatabaseConfiguration), nameof(this.GtfsDataPath), "must be a non-empty string");
            }

            if (string.IsNullOrWhiteSpace(this.BotDataPath))
            {
                throw new SettingsValidationException(nameof(DatabaseConfiguration), nameof(this.BotDataPath), "must be a non-empty string");
            }
        }
    }
}
