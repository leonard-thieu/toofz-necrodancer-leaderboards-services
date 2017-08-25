using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;

namespace toofz.Services
{
    public abstract class ArgsParser<TOptions, TSettings>
        where TOptions : Options, new()
        where TSettings : ISettings
    {
        /// <summary>
        /// Gets the description of a property decorated with <see cref="SettingsDescriptionAttribute"/>.
        /// </summary>
        /// <param name="type">The type to search for the property on.</param>
        /// <param name="propName">The name of the property.</param>
        /// <returns>
        /// The description of the property or null if the property is not decorated with <see cref="SettingsDescriptionAttribute"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propName"/> cannot be null.
        /// </exception>
        /// <exception cref="AmbiguousMatchException">
        /// More than one of the requested attributes was found.
        /// </exception>
        /// <exception cref="TypeLoadException">
        /// A custom attribute type cannot be loaded.
        /// </exception>
        protected static string GetDescription(Type type, string propName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var propertyInfo = type.GetProperty(propName);
            var descAttr = propertyInfo.GetCustomAttribute<SettingsDescriptionAttribute>();

            return descAttr?.Description;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ArgsParser{TOptions, TSettings}"/> class.
        /// </summary>
        /// <param name="inReader">The <see cref="TextReader"/> to read input with.</param>
        /// <param name="outWriter">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="errorWriter">The <see cref="TextWriter"/> to write errors to.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="inReader"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="outWriter"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="errorWriter"/> cannot be null.
        /// </exception>
        protected ArgsParser(
            TextReader inReader,
            TextWriter outWriter,
            TextWriter errorWriter)
        {
            InReader = inReader ?? throw new ArgumentNullException(nameof(inReader));
            OutWriter = outWriter ?? throw new ArgumentNullException(nameof(outWriter));
            ErrorWriter = errorWriter ?? throw new ArgumentNullException(nameof(errorWriter));
        }

        /// <summary>
        /// The <see cref="TextReader"/> to read input with.
        /// </summary>
        protected TextReader InReader { get; }
        /// <summary>
        /// The <see cref="TextWriter"/> to write output to.
        /// </summary>
        protected TextWriter OutWriter { get; }
        /// <summary>
        /// The <see cref="TextWriter"/> to write errors to.
        /// </summary>
        protected TextWriter ErrorWriter { get; }

        /// <summary>
        /// The file name of the entry assembly.
        /// </summary>
        protected abstract string EntryAssemblyFileName { get; }

        /// <summary>
        /// Parses arguments into settings and saves them.
        /// </summary>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="settings">The settings object.</param>
        /// <returns>
        /// Zero, if parsing was successful. Non-zero if there was an error while parsing.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="args"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settings"/> cannot be null.
        /// </exception>
        public int Parse(string[] args, TSettings settings)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var optionSet = new OptionSet();
            var options = new TOptions();

            OnParsing(settings.GetType(), optionSet, options);

            try
            {
                var extraArgs = optionSet.Parse(args);
                if (extraArgs.Any())
                {
                    var first = extraArgs.First();
                    throw new OptionException($"'{first}' is not a valid option.", first);
                }
            }
            catch (OptionException ex)
            {
                ErrorWriter.WriteLine($"{EntryAssemblyFileName}: {ex.Message}");
                WriteUsage(optionSet);

                return 1;
            }

            if (options.ShowHelp)
            {
                WriteUsage(optionSet);

                return 0;
            }

            OnParsed(options, settings);

            settings.Save();

            return 0;
        }

        /// <summary>
        /// When overridden in a derived class, adds additional options to parse.
        /// </summary>
        /// <param name="settingsType">The type of the settings object.</param>
        /// <param name="optionSet">The <see cref="OptionSet"/> object.</param>
        /// <param name="options">The object to stored parsed options into.</param>
        protected virtual void OnParsing(Type settingsType, OptionSet optionSet, TOptions options)
        {
            optionSet.Add("help", "Shows usage information.", h => options.ShowHelp = h != null);
            optionSet.Add("interval=", GetDescription(settingsType, nameof(ISettings.UpdateInterval)), (TimeSpan interval) => options.UpdateInterval = interval);
            optionSet.Add("delay=", GetDescription(settingsType, nameof(ISettings.DelayBeforeGC)), (TimeSpan delay) => options.DelayBeforeGC = delay);
        }

        /// <summary>
        /// When overridden in a derived class, applies settings.
        /// </summary>
        /// <param name="options">The parsed options object.</param>
        /// <param name="settings">The settings object.</param>
        protected virtual void OnParsed(TOptions options, TSettings settings)
        {
            #region UpdateInterval

            if (options.UpdateInterval != null)
            {
                settings.UpdateInterval = options.UpdateInterval.Value;
            }

            #endregion

            #region DelayBeforeGC

            if (options.DelayBeforeGC != null)
            {
                settings.DelayBeforeGC = options.DelayBeforeGC.Value;
            }

            #endregion
        }

        /// <summary>
        /// Writes formatted usage information.
        /// </summary>
        /// <param name="options">
        /// The <see cref="OptionSet"/> to write usage information for.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> cannot be null.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// An option has an unsupported <see cref="OptionValueType"/>.
        /// </exception>
        void WriteUsage(OptionSet options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            using (var indentedTextWriter = new IndentedTextWriter(OutWriter, "  "))
            {
                indentedTextWriter.WriteLine();
                indentedTextWriter.WriteLine($"Usage: {EntryAssemblyFileName} [options]");
                indentedTextWriter.WriteLine();

                indentedTextWriter.WriteLine("options:");
                indentedTextWriter.Indent++;

                var maxPrototypeLength = options.Max(option =>
                {
                    switch (option.OptionValueType)
                    {
                        case OptionValueType.None:
                            return option.Prototype.Length;
                        case OptionValueType.Optional:
                            return option.Prototype.Length - 1 + "[=VALUE]".Length;
                        case OptionValueType.Required:
                            return option.Prototype.Length + "VALUE".Length;
                        default:
                            throw new NotSupportedException($"Unknown {nameof(OptionValueType)}: '{option.OptionValueType}'.");
                    }
                });
                foreach (var option in options)
                {
                    switch (option.OptionValueType)
                    {
                        case OptionValueType.None:
                            indentedTextWriter.WriteLine($"--{{0,-{maxPrototypeLength}}}  {option.Description}", option.Prototype);
                            break;
                        case OptionValueType.Optional:
                            indentedTextWriter.WriteLine($"--{{0,-{maxPrototypeLength}}}  {option.Description}", option.Prototype.TrimEnd(':') + "[=VALUE]");
                            break;
                        case OptionValueType.Required:
                            indentedTextWriter.WriteLine($"--{{0,-{maxPrototypeLength}}}  {option.Description}", option.Prototype + "VALUE");
                            break;
                        default:
                            // Unreachable. THe previous code block would have thrown already and OptionValueType should be treated as immutable.
                            break;
                    }
                }

                indentedTextWriter.Indent--;
            }
        }
    }
}
