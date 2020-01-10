using NetEscapades.Configuration.Validation;

namespace Bot
{
    public class FugattiConfiguration : IValidatable
    {
        public string BaseUrl { get; set; }

        public string Token { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.BaseUrl))
            {
                throw new SettingsValidationException(nameof(FugattiConfiguration), nameof(this.BaseUrl), "must be a non-empty string");
            }

            if (string.IsNullOrWhiteSpace(this.Token))
            {
                throw new SettingsValidationException(nameof(FugattiConfiguration), nameof(this.Token), "must be a non-empty string");
            }
        }
    }
}
