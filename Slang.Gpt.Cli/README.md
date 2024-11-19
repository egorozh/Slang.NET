# Slang.Net - GPT

Use GPT to automatically translate your app at compile time.

Currently, only the [OpenAI API](https://platform.openai.com/docs/) is supported.

## Getting Started

### Install slang-gpt-cli:

```bash
dotnet tool install -g slang.gpt.cli --prerelease
```

Then add the following configuration in your slang.json:

```json
{
  "base_culture": "ru",
  "gpt": {
    "model": "gpt-4o-mini",
    "description": "Showcase for Slang.Net.Gpt"
  }
}
```

Let's run this:

```bash
slang-gpt <csproj> --target=ru --api-key=<api-key>
```

## Configuration

| Key                | Type     | Usage                            | Required | Default             |
|--------------------|----------|----------------------------------|----------|---------------------|
| `model`            | `string` | Model name                       | YES      |                     |
| `max_input_length` | `int`    | Max input characters per request | NO       | (inferred by model) |
| `temperature`      | `double` | Temperature parameter for GPT    | NO       | (API default)       |
| `description`      | `string` | App description                  | YES      |                     |

## Command line arguments

| Argument         | Description              | Required | Default                |
|------------------|--------------------------|----------|------------------------|
| `--target=`      | Target language          | NO       | (all existing locales) |
| `--api-key=`     | API key                  | YES      |                        |
| `-f` / `--full`  | Skip partial translation | NO       | (partial translation)  |
| `-d` / `--debug` | Write chat to file       | NO       | (no chat output)       |

## Models

| Model name          | Provider | Context length | Cost per 1k input token | Cost per 1k output token |
|---------------------|----------|----------------|-------------------------|--------------------------|
| `gpt-3.5-turbo`     | Open AI  | 4096           | $0.0005                 | $0.0015                  |
| `gpt-3.5-turbo-16k` | Open AI  | 16384          | $0.003                  | $0.004                   |
| `gpt-4`             | Open AI  | 8192           | $0.03                   | $0.06                    |
| `gpt-4-turbo`       | Open AI  | 64000          | $0.01                   | $0.03                    |
| `gpt-4o`            | Open AI  | 128000         | $0.005                  | $0.015                   |
| `gpt-4o-mini`       | Open AI  | 128000         | $0.00015                | $0.0006                  |

1k tokens = 750 words (English)

## GPT context length

Each model has a different context length. Try to avoid exceeding it as the model starts to "forget".

Luckily, slang_gpt supports splitting the input into multiple requests.

The `max_input_length` is optional and defaults to some heuristic.

If you work with less common languages and the model starts to forget, try to reduce the `max_input_length`.

Alternatively, you can also use a model with a larger context length.

## Partial translation

By default, slang_gpt will only translate missing keys to reduce costs.

You may add the `--full` flag to translate all keys.

```bash
<directory with slang-gpt>/slang-gpt <csproj filepath> --target=ru --api-key=<open-ai-gpt-api-key> --full
```

To avoid a specific subset of keys from being translated, you may add the `ignoreGpt` modifier to the key:

```json
{
  "key1": "This will be translated",
  "key2(ignoreGpt)": {
    "key3": "This will be ignored"
  }
}
```

## Target language

By default, slang_gpt will translate to all existing locales.

You may add the `--target` flag to translate to a specific locale. This may be useful if you want to translate to a new locale.

Additionally, you may also use predefined language sets (keep in mind that English must be the base locale):

**By GDP (Gross Domestic Product):**

| Flag              | Languages                                                           |
|-------------------|---------------------------------------------------------------------|
| `--target=gdp-3`  | `["zh-Hans", "es", "ja"]`                                           |
| `--target=gdp-5`  | `["zh-Hans", "es", "ja", "de", "fr"]`                               |
| `--target=gdp-10` | `["zh-Hans", "es", "ja", "de", "fr", "pt", "ar", "it", "ru", "ko"]` |

**By region and population:**

| Flag             | Languages                                                      |
|------------------|----------------------------------------------------------------|
| `--target=eu-3`  | `["de", "fr", "it"]`                                           |
| `--target=eu-5`  | `["de", "fr", "it", "es", "pl"]`                               |
| `--target=eu-10` | `["de", "fr", "it", "es", "pl", "ro", "nl", "cs", "el", "sv"]` |
