namespace Slang.Gpt.Domain.Models;

public enum GptProvider
{
    OpenAi
}

public static class GptModel
{
    /// <param name="Id">
    /// The id of this model.
    /// Will be sent to the GPT API.
    /// </param>
    /// <param name="Provider">The provider of this model.</param>
    /// <param name="DefaultInputLength">
    /// Each model has a limited context until this model starts to "forget".
    ///
    /// The default input length is calculated as follows:
    /// 1 token = 4 characters (English)
    /// input context = 1 / 3 of the model's context (Assuming 2x output context)
    /// Therefore, input_length = 1.33 * model_context
    /// </param>
    /// <param name="CostPer1KInputToken">The cost per input token in USD.</param>
    /// <param name="CostPer1KOutputToken">he cost per output token in USD.</param>
    public record GptModelInfo(
        string Id,
        GptProvider Provider,
        int DefaultInputLength,
        double CostPer1KInputToken,
        double CostPer1KOutputToken
    )
    {
        public double CostPerInputToken => CostPer1KInputToken / 1000;

        public double CostPerOutputToken => CostPer1KOutputToken / 1000;
    }

    internal static readonly GptModelInfo Gpt35Turbo = new("gpt-3.5-turbo", GptProvider.OpenAi,
        DefaultInputLength: 2000,
        CostPer1KInputToken: 0.0005,
        CostPer1KOutputToken: 0.0015);

    private static readonly GptModelInfo Gpt35Turbo16K = new("gpt-3.5-turbo-16k", GptProvider.OpenAi,
        DefaultInputLength: 8000,
        CostPer1KInputToken: 0.003,
        CostPer1KOutputToken: 0.004);

    private static readonly GptModelInfo Gpt4 = new("gpt-4", GptProvider.OpenAi,
        DefaultInputLength: 4000,
        CostPer1KInputToken: 0.03,
        CostPer1KOutputToken: 0.06);

    private static readonly GptModelInfo Gpt4Turbo = new("gpt-4-turbo", GptProvider.OpenAi,
        DefaultInputLength: 64000,
        CostPer1KInputToken: 0.01,
        CostPer1KOutputToken: 0.03);

    private static readonly GptModelInfo Gpt4O = new("gpt-4o", GptProvider.OpenAi,
        DefaultInputLength: 128000,
        CostPer1KInputToken: 0.005,
        CostPer1KOutputToken: 0.015);

    private static readonly GptModelInfo Gpt4OMini = new("gpt-4o-mini", GptProvider.OpenAi,
        DefaultInputLength: 128000,
        CostPer1KInputToken: 0.00015,
        CostPer1KOutputToken: 0.0006);

    public const int DefaultInputLength = 128000;

    public static readonly HashSet<GptModelInfo> Values =
    [
        Gpt35Turbo, Gpt35Turbo16K, Gpt4, Gpt4Turbo, Gpt4O, Gpt4OMini
    ];
}