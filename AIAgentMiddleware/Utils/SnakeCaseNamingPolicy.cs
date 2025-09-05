using System.Text.Json;

namespace AIAgentMiddleware.Utils
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                        builder.Append('_');
                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
    }
}
